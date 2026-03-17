use std::{ffi::{CString, c_char, CStr}, fs::File, io::ErrorKind, ptr::null_mut, path::Path};
use symphonia::core::{audio::Signal, errors::Error};
use symphonia::core::{formats::{FormatOptions, Track}};
use symphonia::core::io::MediaSourceStream;
use symphonia::core::meta::MetadataOptions;
use symphonia::core::probe::Hint;
use symphonia::core::codecs::{CODEC_TYPE_NULL, DecoderOptions};
use symphonia::core::formats::FormatReader;
use symphonia::core::audio::AudioBuffer;
use vorbis_rs::VorbisEncoderBuilder;

#[repr(C)]
struct FfiMessage {
    is_success: bool,
    error_message: *mut c_char,
}

impl FfiMessage {
    fn ok() -> Self {
        Self { is_success: true, error_message: null_mut(),}
    }

    fn fail(msg: String) -> Self {
        let c_str = CString::new(msg).unwrap_or_default();
        Self { is_success: false, error_message: c_str.into_raw() }
    }
}

#[derive(Debug)]
enum ConversionError {
    FileNotFound(String),
    SystemError(String),
    DecodeError(String),
    EncodeError(String),
    UnsupportedFormat(String),
    IoError(String),
}

fn load(input: &str) -> Result<(Box<dyn FormatReader>, Track), ConversionError> {
    let src = match File::open(input) {
        Ok(file) => file,
        Err(e) if e.kind() == ErrorKind::NotFound => return Err(ConversionError::FileNotFound(input.to_string())),
        Err(e) => return Err(ConversionError::IoError(e.to_string())),
    };

    let mss = MediaSourceStream::new(Box::new(src), Default::default());

    let meta_opts: MetadataOptions = Default::default();
    let fmt_opts: FormatOptions = Default::default();
    let mut hint = Hint::new();

    if let Some(extension) = Path::new(input).extension().and_then(|e| e.to_str()) {
        hint.with_extension(extension);
    }

    let probed = match symphonia::default::get_probe().format(&hint, mss, &fmt_opts, &meta_opts) {
        Ok(p) => p,
        Err(e) => return Err(ConversionError::UnsupportedFormat(format!("Failed to probe format: {}", e))),
    };

    let format = probed.format;

    let track = format.tracks()
        .iter()
        .find(|t| t.codec_params.codec != CODEC_TYPE_NULL)
        .cloned()
        .ok_or_else(|| ConversionError::UnsupportedFormat("No supported audio track found.".to_string()))?;

    Ok((format, track))
}

fn transcode(track: &Track, mut format: Box<dyn FormatReader>, output_path: &str) -> Result<(), ConversionError> {
    let dec_opts: DecoderOptions = Default::default();

    let mut decoder = match symphonia::default::get_codecs().make(&track.codec_params, &dec_opts) {
        Ok(d) => d,
        Err(e) => return Err(ConversionError::DecodeError(format!("Failed to create decoder: {}", e))),
    };
    
    let track_id = track.id;

    let codec_params = &track.codec_params;
    let sample_rate = codec_params.sample_rate.ok_or(ConversionError::UnsupportedFormat("Unknown sample rate".to_string()))?;
    let channels = codec_params.channels.ok_or(ConversionError::UnsupportedFormat("Unknown channel count".to_string()))?.count();

    let output_file = match File::create(output_path) {
        Ok(f) => f,
        Err(e) => return Err(ConversionError::IoError(format!("Failed to create output file: {}", e))),
    };

    let safe_sample_rate = std::num::NonZeroU32::new(sample_rate)
        .ok_or_else(|| ConversionError::UnsupportedFormat("Sample rate is zero".to_string()))?;
    
    let safe_channels = std::num::NonZeroU8::new(channels as u8)
        .ok_or_else(|| ConversionError::UnsupportedFormat("Channel count is zero".to_string()))?;

    let mut encoder = VorbisEncoderBuilder::new(safe_sample_rate, safe_channels, output_file)
        .map_err(|e| ConversionError::SystemError(format!("Vorbis init failed: {:?}", e)))?
        .build()
        .map_err(|e| ConversionError::SystemError(format!("Vorbis build failed: {:?}", e)))?;

    let mut conversion_buf: Option<AudioBuffer<f32>> = None;

    loop {
        let packet = match format.next_packet() {
            Ok(packet) => packet,
            Err(Error::IoError(e)) if e.kind() == ErrorKind::UnexpectedEof => {
                break; // leave the cycle if file is gone
            }
            Err(Error::ResetRequired) => {
                break;
            }
            Err(err) => {
                return Err(ConversionError::DecodeError(err.to_string()))
            }
        };

        while !format.metadata().is_latest() {
            format.metadata().pop();
        }

        if packet.track_id() != track_id {
            continue;
        }

        match decoder.decode(&packet) {
            Ok(decoded) => {
                if conversion_buf.is_none() {
                    let spec = *decoded.spec();
                    let duration = decoded.capacity() as u64;
                    conversion_buf = Some(AudioBuffer::<f32>::new(duration, spec));
                }

                if let Some(buf) = &mut conversion_buf {
                    buf.clear();
                    decoded.convert(buf);
                    let planes = buf.planes();
                    let plane_slices = planes.planes();

                    encoder.encode_audio_block(plane_slices)
                        .map_err(|e| ConversionError::EncodeError(format!("{:?}", e)))?;
                }
            }
            Err(Error::IoError(_)) => continue,
            Err(Error::DecodeError(_)) => continue,
            Err(err) => return Err(ConversionError::DecodeError(err.to_string()))
        }
    }

    encoder.finish().map_err(|e| ConversionError::EncodeError(format!("{:?}", e)))?;

    Ok(())
}

fn perform_pipeline(input: &str, output: &str) -> Result<(), ConversionError> {
    let (reader, track) = load(input)?;
    transcode(&track, reader, output)?;
    Ok(())
}

#[unsafe(no_mangle)]
extern "C" fn convert_audio_to_ogg(input: *const c_char, output: *const c_char) -> FfiMessage {
    if input.is_null() {
        return FfiMessage::fail("Input path pointer is NULL".to_string());
    }
    if output.is_null() {
        return FfiMessage::fail("Output path pointer is NULL".to_string());
    }

    let input_str = unsafe {
        match CStr::from_ptr(input).to_str() {
            Ok(s) => s,
            Err(_) => return FfiMessage::fail("Input path is not valid UTF-8".to_string()),
        }
    };

    let output_str = unsafe {
        match CStr::from_ptr(output).to_str() {
            Ok(s) => s,
            Err(_) => return FfiMessage::fail("Output path is not valid UTF-8".to_string()),
        }
    };

    match perform_pipeline(input_str, output_str) {
        Ok(_) => FfiMessage::ok(),
        Err(e) => {
            let error_msg = match e {
                ConversionError::FileNotFound(path) => format!("File not found: {}", path),
                ConversionError::IoError(msg) => format!("IO Error: {}", msg),
                ConversionError::UnsupportedFormat(msg) => format!("Format Error: {}", msg),
                ConversionError::DecodeError(msg) => format!("Decoding Error: {}", msg),
                ConversionError::EncodeError(msg) => format!("Encoding Error: {}", msg),
                ConversionError::SystemError(msg) => format!("System Error: {}", msg),
            };
            FfiMessage::fail(error_msg)
        }
    }
}

//free the error message from memory
#[unsafe(no_mangle)]
pub extern "C" fn free_ffi_message_error(ptr: *mut c_char) {
    if ptr.is_null() {
        return;
    }
    unsafe {
        let _ = CString::from_raw(ptr); 
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::fs;
    use std::path::{PathBuf};

    fn get_test_resource(filename: &str) -> PathBuf {
        let mut path = PathBuf::from(env!("CARGO_MANIFEST_DIR"));
        path.push("misc");
        path.push(filename);
        path
    }

    #[test]
    fn load_single_flac_file() -> Result<(), Box<dyn std::error::Error>> {
        let path = get_test_resource("Erkki Armas Hokkanen Ad.flac");
        
        let (_reader, _track) = load(path.to_str().unwrap())
            .expect("FileLoader failed to load flac");

        Ok(())
    }

    #[test]
    fn load_single_wav_file() -> Result<(), Box<dyn std::error::Error>> {
        let path = get_test_resource("administrator.wav");

        let (_reader, _track) = load(path.to_str().unwrap())
            .expect("FileLoader failed to load wav");

        Ok(())
    }

    fn transcode(input_file: &str) -> Result<(), Box<dyn std::error::Error>> {
        let input_path = get_test_resource(input_file);
        let output_path = std::env::temp_dir().join("output_test.ogg");

        if output_path.exists() {
            fs::remove_file(&output_path)?;
        }

        match perform_pipeline(input_path.to_str().unwrap(), output_path.to_str().unwrap()) {
            Ok(()) => {},
            Err(e) => panic!("Error during transcoding: {:?}", e)
        }

        assert!(output_path.exists(), "Output file should exist");
        let meta = fs::metadata(&output_path)?;
        assert!(meta.len() > 0, "Output file should not be empty");

        Ok(())
    }

    #[test]
    fn transcode_flac() -> Result<(), Box<dyn std::error::Error>> {
        transcode("Erkki Armas Hokkanen Ad.flac")
    }

    #[test]
    fn transcode_wav() -> Result<(), Box<dyn std::error::Error>> {
        transcode("administrator.wav")
    }
}