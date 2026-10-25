namespace Ewvet.Tests;

public sealed class CheckEncodeCharsetsTests
{
    [Fact]
    public void CheckRejectsMissingSourcesWithoutIo()
    {
        var io = new FakeIo();

        var failure = Assert.Throws<Failure>(() => CheckCommand.Run(
            new List<string>(),
            new HashSet<string>(),
            new Dictionary<string, string>(),
            new Inputs(io)));

        Assert.Equal("missing FILE", failure.Message);
        Assert.Empty(io.FileReads);
        Assert.Equal(0, io.StdinReads);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    [Theory]
    [InlineData("empty.txt", "", "ok, 0 encoded words in 1 file\n", 0)]
    [InlineData("a.txt", "=?utf-8?Q?a?= =?utf-8?Q?b?=", "ok, 2 encoded words in 1 file\n", 0)]
    [InlineData("-", "x", "ok, 0 encoded words in 1 file\n", 0)]
    public void CheckReturnsExactCheckerOutputForOneSource(string arg, string text, string expected, int expectedCode)
    {
        var io = new FakeIo { StdinBytes = System.Text.Encoding.UTF8.GetBytes(text) };
        if (arg != "-")
        {
            io.Files[arg] = text;
        }

        var output = CheckCommand.Run(
            new List<string> { arg },
            new HashSet<string>(),
            new Dictionary<string, string>(),
            new Inputs(io));

        Assert.Equal(expected, output.Out);
        Assert.Equal(expectedCode, output.Code);
        Assert.Equal(arg == "-" ? [] : [arg], io.FileReads);
        Assert.Equal(arg == "-" ? 1 : 0, io.StdinReads);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    [Fact]
    public void CheckReadsRepeatedFilesAndOrderedSources()
    {
        var repeatedIo = new FakeIo();
        repeatedIo.Files["a.txt"] = "=?utf-8?Q?a?= =?utf-8?Q?b?=";

        var repeatedOutput = CheckCommand.Run(
            new List<string> { "a.txt", "a.txt" },
            new HashSet<string>(),
            new Dictionary<string, string>(),
            new Inputs(repeatedIo));

        Assert.Equal("ok, 4 encoded words in 2 files\n", repeatedOutput.Out);
        Assert.Equal(0, repeatedOutput.Code);
        Assert.Equal(["a.txt", "a.txt"], repeatedIo.FileReads);
        Assert.Empty(repeatedIo.OutCalls);
        Assert.Empty(repeatedIo.ErrCalls);

        var orderedIo = new FakeIo();
        orderedIo.Files["a.txt"] = "=?utf-8?Q?a?= =?utf-8?Q?b?=";
        orderedIo.Files["b.txt"] = "=?utf-8?B?SGk?= =?utf-8*en?Q?x?=";

        var orderedOutput = CheckCommand.Run(
            new List<string> { "a.txt", "b.txt" },
            new HashSet<string>(),
            new Dictionary<string, string>(),
            new Inputs(orderedIo));

        Assert.Equal("b.txt:1:1: EW005 base64 text is missing its padding\n1 problem in 1 file\n", orderedOutput.Out);
        Assert.Equal(1, orderedOutput.Code);
        Assert.Equal(["a.txt", "b.txt"], orderedIo.FileReads);
        Assert.Empty(orderedIo.OutCalls);
        Assert.Empty(orderedIo.ErrCalls);
    }

    [Fact]
    public void CheckStopsAtReadFailureAndRejectsRepeatedStdin()
    {
        var fileIo = new FakeIo();
        fileIo.Files["b.txt"] = "=?utf-8?B?SGk?=";
        fileIo.FailingFiles.Add("nofile.txt");

        var readFailure = Assert.Throws<Failure>(() => CheckCommand.Run(
            new List<string> { "b.txt", "nofile.txt" },
            new HashSet<string>(),
            new Dictionary<string, string>(),
            new Inputs(fileIo)));

        Assert.Equal("nofile.txt: cannot read the file", readFailure.Message);
        Assert.Equal(["b.txt", "nofile.txt"], fileIo.FileReads);
        Assert.Empty(fileIo.OutCalls);
        Assert.Empty(fileIo.ErrCalls);

        var stdinIo = new FakeIo { StdinBytes = [120] };
        var stdinFailure = Assert.Throws<Failure>(() => CheckCommand.Run(
            new List<string> { "-", "-" },
            new HashSet<string>(),
            new Dictionary<string, string>(),
            new Inputs(stdinIo)));

        Assert.Equal("standard input is named twice", stdinFailure.Message);
        Assert.Equal(1, stdinIo.StdinReads);
        Assert.Empty(stdinIo.FileReads);
        Assert.Empty(stdinIo.OutCalls);
        Assert.Empty(stdinIo.ErrCalls);
    }

    [Fact]
    public void CheckPassesMalformedStdinBytesToStrictInputDecoder()
    {
        var io = new FakeIo { StdinBytes = [0x61, 0xFF, 0x0A] };

        var failure = Assert.Throws<Failure>(() => CheckCommand.Run(
            new List<string> { "-" },
            new HashSet<string>(),
            new Dictionary<string, string>(),
            new Inputs(io)));

        Assert.Equal("<stdin>: input is not valid UTF-8", failure.Message);
        Assert.Equal(1, io.StdinReads);
        Assert.Empty(io.FileReads);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    [Theory]
    [InlineData(new string[] { }, "koi8-r", "B", "missing FILE")]
    [InlineData(new string[] { "f", "f" }, "koi8-r", "B", "too many arguments")]
    [InlineData(new string[] { "f" }, "koi8-r", "B", "unknown charset \"koi8-r\"")]
    [InlineData(new string[] { "f" }, "", null, "unknown charset \"\"")]
    [InlineData(new string[] { "f" }, "é😀", null, "unknown charset \"??\"")]
    [InlineData(new string[] { "f" }, null, "B", "invalid encoding \"B\" (expected auto, b or q)")]
    [InlineData(new string[] { "f" }, null, "Q", "invalid encoding \"Q\" (expected auto, b or q)")]
    [InlineData(new string[] { "f" }, null, "AUTO", "invalid encoding \"AUTO\" (expected auto, b or q)")]
    [InlineData(new string[] { "f" }, null, "", "invalid encoding \"\" (expected auto, b or q)")]
    public void EncodeValidatesArityCharsetAndEncodingBeforeReading(string[] args, string? charset, string? encoding, string expectedMessage)
    {
        var io = new FakeIo();
        io.Files["f"] = "hello";
        var values = new Dictionary<string, string>();
        if (charset is not null)
        {
            values["--charset"] = charset;
        }

        if (encoding is not null)
        {
            values["--encoding"] = encoding;
        }

        var failure = Assert.Throws<Failure>(() => EncodeCommand.Run(
            args.ToList(),
            new HashSet<string>(),
            values,
            new Inputs(io)));

        Assert.Equal(expectedMessage, failure.Message);
        Assert.Empty(io.FileReads);
        Assert.Equal(0, io.StdinReads);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    [Theory]
    [InlineData("", null, null, "", 0)]
    [InlineData("hello\r\n", null, null, "hello\r\n", 0)]
    [InlineData("é", null, null, "=?utf-8?B?w6k=?=", 0)]
    [InlineData("é", "UTF8", "q", "=?utf-8?Q?=C3=A9?=", 0)]
    [InlineData("é", "Latin1", "q", "=?iso-8859-1?Q?=E9?=", 0)]
    [InlineData("é", "utf-8", "b", "=?utf-8?B?w6k=?=", 0)]
    [InlineData("€", "L9", "auto", "=?iso-8859-15?Q?=A4?=", 0)]
    public void EncodeReturnsExactLiteralConversions(string text, string? charset, string? encoding, string expected, int expectedCode)
    {
        var io = new FakeIo();
        io.Files["f"] = text;
        var values = new Dictionary<string, string>();
        if (charset is not null)
        {
            values["--charset"] = charset;
        }

        if (encoding is not null)
        {
            values["--encoding"] = encoding;
        }

        var output = EncodeCommand.Run(
            new List<string> { "f" },
            new HashSet<string>(),
            values,
            new Inputs(io));

        Assert.Equal(expected, output.Out);
        Assert.Equal(expectedCode, output.Code);
        Assert.Equal(["f"], io.FileReads);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    [Fact]
    public void EncodePropagatesConversionAndReadFailures()
    {
        var conversionIo = new FakeIo();
        conversionIo.Files["e.txt"] = "é";

        var conversionFailure = Assert.Throws<Failure>(() => EncodeCommand.Run(
            new List<string> { "e.txt" },
            new HashSet<string>(),
            new Dictionary<string, string> { ["--charset"] = "ASCII" },
            new Inputs(conversionIo)));

        Assert.Equal("U+00E9 cannot be written in us-ascii", conversionFailure.Message);
        Assert.Equal(["e.txt"], conversionIo.FileReads);
        Assert.Empty(conversionIo.OutCalls);
        Assert.Empty(conversionIo.ErrCalls);

        var readIo = new FakeIo();
        readIo.FailingFiles.Add("nofile.txt");

        var readFailure = Assert.Throws<Failure>(() => EncodeCommand.Run(
            new List<string> { "nofile.txt" },
            new HashSet<string>(),
            new Dictionary<string, string>(),
            new Inputs(readIo)));

        Assert.Equal("nofile.txt: cannot read the file", readFailure.Message);
        Assert.Equal(["nofile.txt"], readIo.FileReads);
        Assert.Empty(readIo.OutCalls);
        Assert.Empty(readIo.ErrCalls);
    }

    [Theory]
    [InlineData(new string[] { }, "NAME         ALIASES               BYTES\niso-8859-1   iso8859-1,l1,latin1   1\niso-8859-15  iso8859-15,l9,latin9  1\nus-ascii     ascii,us              1\nutf-8        utf8                  1 to 4\n", 0)]
    [InlineData(new string[] { "" }, "NAME         ALIASES               BYTES\niso-8859-1   iso8859-1,l1,latin1   1\niso-8859-15  iso8859-15,l9,latin9  1\nus-ascii     ascii,us              1\nutf-8        utf8                  1 to 4\n", 0)]
    [InlineData(new string[] { "L9" }, "NAME         ALIASES               BYTES\niso-8859-15  iso8859-15,l9,latin9  1\n", 0)]
    [InlineData(new string[] { "UTF" }, "NAME   ALIASES  BYTES\nutf-8  utf8     1 to 4\n", 0)]
    [InlineData(new string[] { "ascii" }, "NAME      ALIASES   BYTES\nus-ascii  ascii,us  1\n", 0)]
    [InlineData(new string[] { "zz" }, "no charset starts with \"zz\"\n", 1)]
    [InlineData(new string[] { "ZZ" }, "no charset starts with \"ZZ\"\n", 1)]
    [InlineData(new string[] { "Ｕ" }, "no charset starts with \"?\"\n", 1)]
    [InlineData(new string[] { "- " }, "no charset starts with \"- \"\n", 1)]
    public void CharsetsReturnsExactFilteredTablesAndMessages(string[] args, string expected, int expectedCode)
    {
        var io = new FakeIo();

        var output = CharsetsCommand.Run(
            args.ToList(),
            new HashSet<string>(),
            new Dictionary<string, string>(),
            new Inputs(io));

        Assert.Equal(expected, output.Out);
        Assert.Equal(expectedCode, output.Code);
        Assert.Empty(io.FileReads);
        Assert.Equal(0, io.StdinReads);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    [Fact]
    public void CharsetsRejectsExcessArgumentsWithoutIo()
    {
        var io = new FakeIo();

        var failure = Assert.Throws<Failure>(() => CharsetsCommand.Run(
            new List<string> { "a", "b" },
            new HashSet<string>(),
            new Dictionary<string, string>(),
            new Inputs(io)));

        Assert.Equal("too many arguments", failure.Message);
        Assert.Empty(io.FileReads);
        Assert.Equal(0, io.StdinReads);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    private sealed class FakeIo : IIo
    {
        public byte[] StdinBytes { get; set; } = [];

        public Dictionary<string, string> Files { get; } = new(StringComparer.Ordinal);

        public HashSet<string> FailingFiles { get; } = new(StringComparer.Ordinal);

        public List<string> FileReads { get; } = [];

        public List<string> OutCalls { get; } = [];

        public List<string> ErrCalls { get; } = [];

        public int StdinReads { get; private set; }

        public byte[] ReadStdin()
        {
            StdinReads++;
            return StdinBytes;
        }

        public byte[] ReadFile(string path)
        {
            FileReads.Add(path);
            if (FailingFiles.Contains(path))
            {
                throw new IOException();
            }

            return System.Text.Encoding.UTF8.GetBytes(Files[path]);
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
