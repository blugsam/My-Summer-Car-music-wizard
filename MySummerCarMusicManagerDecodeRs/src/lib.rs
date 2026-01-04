use std::{ffi::{CString, c_char}, fs::File, io::ErrorKind, ptr::null_mut};
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

struct FileFinder;

impl FileFinder {
    fn find(input: &str) -> Result<(), ConversionError> {
        let input_path = std::path::Path::new(input);
        if !input_path.exists() {
            return Err(ConversionError::FileNotFound(input.to_string()));
        }
        if !input_path.is_file() {
           return Err(ConversionError::FileNotFound(format!("Path is not a file: {}", input)));
        }

        Ok(())
    }
}

struct FileLoader;

impl FileLoader {
    fn load(input: &str) -> Result<(Box<dyn FormatReader>, Track), ConversionError> {
        let src_result = std::fs::File::open(input);
        let src = match src_result {
            Ok(file) => file,
            Err(error) => return Err(ConversionError::IoError(error.to_string()))
        };

        let mss = MediaSourceStream::new(Box::new(src), Default::default());

        let meta_opts: MetadataOptions = Default::default();
        let fmt_opts: FormatOptions = Default::default();

        let hint = Hint::new();

        let probed_result = symphonia::default::get_probe().format(&hint, mss, &fmt_opts, &meta_opts);
        let probed = match probed_result {
            Ok(probed_result) => probed_result,
            Err(error) => return Err(ConversionError::UnsupportedFormat(error.to_string()))
        };

        let format = probed.format;

        let track = format.tracks()
            .iter()
            .find(|t| t.codec_params.codec != CODEC_TYPE_NULL)
            .cloned()
            .ok_or_else(|| ConversionError::UnsupportedFormat("Hasn't found proper track.".to_string()))?;

        Ok((format, track))

    }

}

struct FileTranscoder;

impl FileTranscoder {
    fn transcode(track: &Track, mut format: Box<dyn FormatReader>, output_path: &str) -> Result<(), ConversionError> {
        let dec_opts: DecoderOptions = Default::default();

        let decoder_result = symphonia::default::get_codecs().make(&track.codec_params, &dec_opts);
        let mut decoder = match decoder_result {
            Ok(decoder_result) => decoder_result,
            Err(error) => return Err(ConversionError::DecodeError(error.to_string()))
        };
        
        let track_id = track.id;

        let codec_params = &track.codec_params;
        let sample_rate = codec_params.sample_rate.ok_or(ConversionError::UnsupportedFormat("Unknown sample rate".to_string()))?;
        let channels = codec_params.channels.ok_or(ConversionError::UnsupportedFormat("Unknown channel count".to_string()))?.count();

        let output_file_result = File::create(output_path);
        let mut output_file = match output_file_result {
            Ok(output_file_result) => output_file_result,
            Err(error) => return Err(ConversionError::IoError(error.to_string()))
        };

        let mut encoder = VorbisEncoderBuilder::new(
            std::num::NonZeroU32::new(sample_rate).unwrap(),
            std::num::NonZeroU8::new(channels as u8).unwrap(),
            output_file
        )
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
                Err(Error::IoError(_)) => {
                    continue;
                }
                Err(Error::DecodeError(_)) => {
                    continue;
                }
                Err(err) => {
                    return Err(ConversionError::DecodeError(err.to_string()))
                }
            }
        }

        encoder.finish().map_err(|e| ConversionError::EncodeError(format!("{:?}", e)))?;

        Ok(())
    }

    fn perform_pipeline(input: &str, output: &str) -> Result<(), ConversionError> {
        FileFinder::find(input)?;
        let (reader, track) = FileLoader::load(input)?;
        FileTranscoder::transcode(&track, reader, output)?;
        Ok(())
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use core::error;
    use std::fs;
    use std::path::{Path, PathBuf};

    #[test]
    fn test_file_finding_on_success() {
        let mut path = PathBuf::from(env!("CARGO_MANIFEST_DIR"));
        path.push("misc");
        path.push("Erkki Armas Hokkanen Ad.flac");

        let result = FileFinder::find(path.to_str().unwrap());
        
        assert!(result.is_ok());
    }

    #[test]
    fn test_file_finding_on_failure() {
        let result = FileFinder::find("perkele/suomi.flac");

        assert!(
            matches!(result, Err(ConversionError::FileNotFound(_))),
            "Expected a FileNotFound error, but got {:?}", result
        );
    }

    #[test]
    fn test_file_loading_on_success() {
        let mut path = PathBuf::from(env!("CARGO_MANIFEST_DIR"));
        path.push("misc");
        path.push("Erkki Armas Hokkanen Ad.flac");

        let result = FileLoader::load(path.to_str().unwrap());

        match result {
            Ok((_reader, _track)) => {}
            Err(e) => panic!("FileLoader doesn't load a file: {:?}", e)
        }
    }

    #[test]
    fn test_transcoding_pipeline() {
        let mut input_path = PathBuf::from(env!("CARGO_MANIFEST_DIR"));
        input_path.push("misc");
        input_path.push("Erkki Armas Hokkanen Ad.flac");

        let mut output_path = std::env::temp_dir();
        output_path.push("output_test.ogg");

        if output_path.exists() {
            fs::remove_file(&output_path).unwrap();
        }

        match FileTranscoder::perform_pipeline(input_path.to_str().unwrap(), output_path.to_str().unwrap()) {
            Ok(()) => {},
            Err(e) => panic!("Error during transcoding: {:?}", e)
        }

        match fs::metadata(&output_path) {
            Ok(m) if m.len() > 0 => {}
            Ok(_) => {
                panic!("File exists but he is empty.");
            },
            Err(_) => panic!("Output file not found after transcoding.")
        }
    }
}