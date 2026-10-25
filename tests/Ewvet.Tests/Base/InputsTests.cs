namespace Ewvet.Tests;

public sealed class InputsTests
{
    [Fact]
    public void ReadReturnsSourceWithSanitizedDisplayAndOriginalPath()
    {
        var io = new FakeIo();
        io.Files["café😀.txt"] = [120];
        var inputs = new Inputs(io);

        var source = inputs.Read("café😀.txt");

        Assert.Equal("caf??.txt", source.Display);
        Assert.Equal("x", source.Text);
        Assert.Equal(["café😀.txt"], io.FileReads);
        Assert.Equal(0, io.StdinReads);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);

        var record = new Inputs.Source("f", "x");
        Assert.Equal("f", record.Display);
        Assert.Equal("x", record.Text);

        Assert.Equal("sample", new Failure("sample").Message);

        var output = new Output("x\n", 1);
        Assert.Equal("x\n", output.Out);
        Assert.Equal(1, output.Code);
    }

    [Theory]
    [InlineData(new byte[] { }, "")]
    [InlineData(new byte[] { 65 }, "A")]
    [InlineData(new byte[] { 195, 169 }, "é")]
    [InlineData(new byte[] { 226, 130, 172 }, "€")]
    [InlineData(new byte[] { 240, 159, 152, 128 }, "😀")]
    [InlineData(new byte[] { 244, 143, 191, 191 }, "\U0010FFFF")]
    [InlineData(new byte[] { 239, 187, 191, 97, 13, 10, 0, 10, 13 }, "\uFEFFa\r\n\0\n\r")]
    public void ReadDecodesValidFileUtf8(byte[] bytes, string expected)
    {
        var io = new FakeIo();
        io.Files["f"] = bytes;
        var source = new Inputs(io).Read("f");

        Assert.Equal("f", source.Display);
        Assert.Equal(expected, source.Text);
        Assert.Equal(["f"], io.FileReads);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    [Theory]
    [InlineData(new byte[] { }, "")]
    [InlineData(new byte[] { 120 }, "x")]
    [InlineData(new byte[] { 239, 187, 191, 0, 13, 10 }, "\uFEFF\0\r\n")]
    public void ReadDecodesValidStandardInputUtf8(byte[] bytes, string expected)
    {
        var io = new FakeIo { StdinBytes = bytes };
        var source = new Inputs(io).Read("-");

        Assert.Equal("<stdin>", source.Display);
        Assert.Equal(expected, source.Text);
        Assert.Equal(1, io.StdinReads);
        Assert.Empty(io.FileReads);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    [Theory]
    [InlineData(new byte[] { 128 })]
    [InlineData(new byte[] { 255 })]
    [InlineData(new byte[] { 192, 175 })]
    [InlineData(new byte[] { 224, 128, 175 })]
    [InlineData(new byte[] { 240, 128, 128, 175 })]
    [InlineData(new byte[] { 237, 160, 128 })]
    [InlineData(new byte[] { 244, 144, 128, 128 })]
    [InlineData(new byte[] { 195 })]
    [InlineData(new byte[] { 226, 130 })]
    [InlineData(new byte[] { 240, 159, 152 })]
    [InlineData(new byte[] { 195, 65 })]
    [InlineData(new byte[] { 254 })]
    [InlineData(new byte[] { 245, 128, 128, 128 })]
    [InlineData(new byte[] { 224, 160 })]
    [InlineData(new byte[] { 240, 144, 128 })]
    [InlineData(new byte[] { 226, 130, 65 })]
    [InlineData(new byte[] { 194, 127 })]
    public void ReadRejectsMalformedFileUtf8(byte[] bytes)
    {
        var io = new FakeIo();
        io.Files["f"] = bytes;

        var failure = Assert.Throws<Failure>(() => new Inputs(io).Read("f"));

        Assert.Equal("f: input is not valid UTF-8", failure.Message);
        Assert.Equal(["f"], io.FileReads);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    [Theory]
    [InlineData(new byte[] { 97, 255, 10 })]
    [InlineData(new byte[] { 128 })]
    [InlineData(new byte[] { 195 })]
    [InlineData(new byte[] { 240, 159, 152 })]
    public void ReadRejectsMalformedStandardInputUtf8(byte[] bytes)
    {
        var io = new FakeIo { StdinBytes = bytes };

        var failure = Assert.Throws<Failure>(() => new Inputs(io).Read("-"));

        Assert.Equal("<stdin>: input is not valid UTF-8", failure.Message);
        Assert.Equal(1, io.StdinReads);
        Assert.Empty(io.FileReads);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    [Fact]
    public void ReadTranslatesIoExceptionsAndSanitizesFailureDisplay()
    {
        var io = new FakeIo();
        io.FailingFiles.Add("nofile.txt");
        io.FailingFiles.Add("café.txt");
        io.ThrowOnStdinRead = true;
        var inputs = new Inputs(io);

        var fileFailure = Assert.Throws<Failure>(() => inputs.Read("nofile.txt"));
        var sanitizedFailure = Assert.Throws<Failure>(() => inputs.Read("café.txt"));
        var stdinFailure = Assert.Throws<Failure>(() => inputs.Read("-"));

        Assert.Equal("nofile.txt: cannot read the file", fileFailure.Message);
        Assert.Equal("caf?.txt: cannot read the file", sanitizedFailure.Message);
        Assert.Equal("<stdin>: cannot read the file", stdinFailure.Message);
        Assert.Equal(["nofile.txt", "café.txt"], io.FileReads);
        Assert.Equal(1, io.StdinReads);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    [Fact]
    public void ReadsRepeatedFileNamesSeparately()
    {
        var io = new FakeIo();
        io.Files["f"] = [120];
        var inputs = new Inputs(io);

        Assert.Equal(new Inputs.Source("f", "x"), inputs.Read("f"));
        Assert.Equal(new Inputs.Source("f", "x"), inputs.Read("f"));

        Assert.Equal(["f", "f"], io.FileReads);
        Assert.Equal(0, io.StdinReads);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    [Fact]
    public void ReadsStandardInputAtMostOnce()
    {
        var io = new FakeIo { StdinBytes = [120] };
        var inputs = new Inputs(io);

        Assert.Equal(new Inputs.Source("<stdin>", "x"), inputs.Read("-"));
        var failure = Assert.Throws<Failure>(() => inputs.Read("-"));

        Assert.Equal("standard input is named twice", failure.Message);
        Assert.Equal(1, io.StdinReads);
        Assert.Empty(io.FileReads);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    [Fact]
    public void FailedStandardInputAttemptsConsumeTheAllowance()
    {
        var ioExceptionIo = new FakeIo { ThrowOnStdinRead = true };
        var ioExceptionInputs = new Inputs(ioExceptionIo);
        var readFailure = Assert.Throws<Failure>(() => ioExceptionInputs.Read("-"));
        var afterReadFailure = Assert.Throws<Failure>(() => ioExceptionInputs.Read("-"));

        Assert.Equal("<stdin>: cannot read the file", readFailure.Message);
        Assert.Equal("standard input is named twice", afterReadFailure.Message);
        Assert.Equal(1, ioExceptionIo.StdinReads);
        Assert.Empty(ioExceptionIo.OutCalls);
        Assert.Empty(ioExceptionIo.ErrCalls);

        var invalidBytesIo = new FakeIo { StdinBytes = [97, 255, 10] };
        var invalidBytesInputs = new Inputs(invalidBytesIo);
        var decodingFailure = Assert.Throws<Failure>(() => invalidBytesInputs.Read("-"));
        var afterDecodingFailure = Assert.Throws<Failure>(() => invalidBytesInputs.Read("-"));

        Assert.Equal("<stdin>: input is not valid UTF-8", decodingFailure.Message);
        Assert.Equal("standard input is named twice", afterDecodingFailure.Message);
        Assert.Equal(1, invalidBytesIo.StdinReads);
        Assert.Empty(invalidBytesIo.OutCalls);
        Assert.Empty(invalidBytesIo.ErrCalls);
    }

    [Fact]
    public void EachInputsInstanceHasItsOwnStandardInputAllowance()
    {
        var io = new FakeIo { StdinBytes = [120] };
        var firstInputs = new Inputs(io);
        var secondInputs = new Inputs(io);

        Assert.Equal(new Inputs.Source("<stdin>", "x"), firstInputs.Read("-"));
        Assert.Equal(new Inputs.Source("<stdin>", "x"), secondInputs.Read("-"));

        Assert.Equal(2, io.StdinReads);
        Assert.Empty(io.FileReads);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    private sealed class FakeIo : IIo
    {
        public byte[] StdinBytes { get; set; } = [];

        public bool ThrowOnStdinRead { get; set; }

        public Dictionary<string, byte[]> Files { get; } = new(StringComparer.Ordinal);

        public HashSet<string> FailingFiles { get; } = new(StringComparer.Ordinal);

        public List<string> FileReads { get; } = [];

        public List<string> OutCalls { get; } = [];

        public List<string> ErrCalls { get; } = [];

        public int StdinReads { get; private set; }

        public byte[] ReadStdin()
        {
            StdinReads++;
            if (ThrowOnStdinRead)
            {
                throw new IOException();
            }

            return StdinBytes;
        }

        public byte[] ReadFile(string path)
        {
            FileReads.Add(path);
            if (FailingFiles.Contains(path))
            {
                throw new IOException();
            }

            return Files[path];
        }

        public void Out(string text)
        {
            OutCalls.Add(text);
        }

        public void Err(string text)
        {
            ErrCalls.Add(text);
        }
    }
}
