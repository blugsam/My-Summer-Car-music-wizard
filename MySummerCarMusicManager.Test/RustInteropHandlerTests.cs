using MySummerCarMusicManager.Infrastructure.Interop;

namespace MySummerCarMusicManager.Test;

public class RustInteropHandlerTests
{
    private readonly RustInteropHandler _handler;

    public RustInteropHandlerTests()
    {
        _handler = new RustInteropHandler();
    }

    [Fact]
    public void HandleConvert_FileNotFound_ThrowsInvalidOperationException_WithRustMessage()
    {
        // Arrange
        var fakeInput = "C:\\file.wav";
        var fakeOutput = "output.ogg";

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _handler.HandleConvert(fakeInput, fakeOutput));

        // Assert
        Assert.Contains("Transcoding error:", exception.Message);
        Assert.Contains("File not found", exception.Message);
    }

    private void ConvertFile(string inputPath)
    {
        var outputPath = "test_output.ogg";

        if (System.IO.File.Exists(outputPath))
        {
            System.IO.File.Delete(outputPath);
        }

        // Act
        _handler.HandleConvert(inputPath, outputPath);

        // Assert
        Assert.True(System.IO.File.Exists(outputPath), "File .ogg has not been created.");

        System.IO.File.Delete(outputPath);
    }

    [Fact]
    public void HandleConvert_ValidWav()
    {
        var inputPath = "Misc/administrator.wav";

        ConvertFile(inputPath);
    }

    [Fact]
    public void HandleConvert_ValidFlac()
    {
        var inputPath = "Misc/Erkki Armas Hokkanen Ad.flac";

        ConvertFile(inputPath);
    }
}
