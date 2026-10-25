using System.Text;

namespace Ewvet.Tests;

public sealed class DecodeExplainTests
{
    [Fact]
    public void ArityIsValidatedBeforeEitherAdapterReads()
    {
        var io = new FakeIo { FailOnAnyRead = true };
        var inputs = new Inputs(io);

        Assert.Equal("missing FILE", Assert.Throws<Failure>(() => DecodeCommand.Run([], new HashSet<string>(), new Dictionary<string, string>(), inputs)).Message);
        Assert.Equal("too many arguments", Assert.Throws<Failure>(() => DecodeCommand.Run(["f", "f"], new HashSet<string>(), new Dictionary<string, string>(), inputs)).Message);
        Assert.Equal("too many arguments", Assert.Throws<Failure>(() => DecodeCommand.Run(["-", "-"], new HashSet<string>(), new Dictionary<string, string>(), inputs)).Message);
        Assert.Equal("missing FILE", Assert.Throws<Failure>(() => ExplainCommand.Run([], new HashSet<string>(), new Dictionary<string, string>(), inputs)).Message);
        Assert.Equal("too many arguments", Assert.Throws<Failure>(() => ExplainCommand.Run(["f", "f"], new HashSet<string>(), new Dictionary<string, string>(), inputs)).Message);
        Assert.Equal("too many arguments", Assert.Throws<Failure>(() => ExplainCommand.Run(["-", "-"], new HashSet<string>(), new Dictionary<string, string>(), inputs)).Message);

        Assert.Empty(io.FileReads);
        Assert.Equal(0, io.StdinReads);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    [Theory]
    [InlineData("decode", "f", "hello", "hello")]
    [InlineData("decode", "-", "hello", "hello")]
    [InlineData("explain", "f", "hello", "AT  GAP  TOKEN  HOW  RESULT  CODES\n")]
    [InlineData("explain", "-", "hello", "AT  GAP  TOKEN  HOW  RESULT  CODES\n")]
    public void AdaptersReadOneSourceAndReturnWholeOutputWithoutWriting(string command, string path, string source, string expected)
    {
        var io = new FakeIo();
        if (path == "-")
        {
            io.StdinBytes = Encoding.UTF8.GetBytes(source);
        }
        else
        {
            io.Files["f"] = Encoding.UTF8.GetBytes(source);
        }

        var inputs = new Inputs(io);
        var output = command == "decode"
            ? DecodeCommand.Run([path], new HashSet<string>(), new Dictionary<string, string>(), inputs)
            : ExplainCommand.Run([path], new HashSet<string>(), new Dictionary<string, string>(), inputs);

        Assert.Equal(expected, output.Out);
        Assert.Equal(0, output.Code);
        if (path == "-")
        {
            Assert.Empty(io.FileReads);
            Assert.Equal(1, io.StdinReads);
        }
        else
        {
            Assert.Equal(["f"], io.FileReads);
            Assert.Equal(0, io.StdinReads);
        }

        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    [Fact]
    public void AdaptersPropagateReadFailuresMalformedUtf8AndRepeatedStdin()
    {
        var failedFileIo = new FakeIo();
        failedFileIo.FailingFiles.Add("café.txt");
        var failedFileInputs = new Inputs(failedFileIo);
        Assert.Equal("caf?.txt: cannot read the file", Assert.Throws<Failure>(() => DecodeCommand.Run(["café.txt"], new HashSet<string>(), new Dictionary<string, string>(), failedFileInputs)).Message);
        Assert.Equal("caf?.txt: cannot read the file", Assert.Throws<Failure>(() => ExplainCommand.Run(["café.txt"], new HashSet<string>(), new Dictionary<string, string>(), failedFileInputs)).Message);
        Assert.Equal(["café.txt", "café.txt"], failedFileIo.FileReads);

        var badDecodeIo = new FakeIo { StdinBytes = [0x61, 0xFF, 0x0A] };
        Assert.Equal("<stdin>: input is not valid UTF-8", Assert.Throws<Failure>(() => DecodeCommand.Run(["-"], new HashSet<string>(), new Dictionary<string, string>(), new Inputs(badDecodeIo))).Message);
        Assert.Equal(1, badDecodeIo.StdinReads);
        Assert.Empty(badDecodeIo.OutCalls);
        Assert.Empty(badDecodeIo.ErrCalls);

        var badExplainIo = new FakeIo { StdinBytes = [0x61, 0xFF, 0x0A] };
        Assert.Equal("<stdin>: input is not valid UTF-8", Assert.Throws<Failure>(() => ExplainCommand.Run(["-"], new HashSet<string>(), new Dictionary<string, string>(), new Inputs(badExplainIo))).Message);
        Assert.Equal(1, badExplainIo.StdinReads);
        Assert.Empty(badExplainIo.OutCalls);
        Assert.Empty(badExplainIo.ErrCalls);

        var decodeStdinIo = new FakeIo { StdinBytes = [0x78] };
        var decodeInputs = new Inputs(decodeStdinIo);
        Assert.Equal("x", DecodeCommand.Run(["-"], new HashSet<string>(), new Dictionary<string, string>(), decodeInputs).Out);
        Assert.Equal("standard input is named twice", Assert.Throws<Failure>(() => DecodeCommand.Run(["-"], new HashSet<string>(), new Dictionary<string, string>(), decodeInputs)).Message);
        Assert.Equal(1, decodeStdinIo.StdinReads);

        var explainStdinIo = new FakeIo { StdinBytes = [0x78] };
        var explainInputs = new Inputs(explainStdinIo);
        Assert.Equal("AT  GAP  TOKEN  HOW  RESULT  CODES\n", ExplainCommand.Run(["-"], new HashSet<string>(), new Dictionary<string, string>(), explainInputs).Out);
        Assert.Equal("standard input is named twice", Assert.Throws<Failure>(() => ExplainCommand.Run(["-"], new HashSet<string>(), new Dictionary<string, string>(), explainInputs)).Message);
        Assert.Equal(1, explainStdinIo.StdinReads);

        Assert.Empty(failedFileIo.OutCalls);
        Assert.Empty(failedFileIo.ErrCalls);
        Assert.Empty(decodeStdinIo.OutCalls);
        Assert.Empty(decodeStdinIo.ErrCalls);
        Assert.Empty(explainStdinIo.OutCalls);
        Assert.Empty(explainStdinIo.ErrCalls);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("hello", "hello")]
    [InlineData("=?UTF-8?B?Y2Fmw6k=?=", "café")]
    [InlineData("=?iso-8859-1?q?caf=E9?=", "café")]
    [InlineData("=?utf-8?Q??=", "")]
    [InlineData("=?utf-8?Q?a?= =?utf-8?Q?b?=", "ab")]
    [InlineData("=?utf-8?Q?a?= \t\r =?utf-8?Q?b?=", "ab")]
    [InlineData("=?utf-8?Q?a?=\n=?utf-8?Q?b?=", "a\nb")]
    [InlineData("=?utf-8?Q?a?= x =?utf-8?Q?b?=", "a x b")]
    [InlineData("=?utf-8?Q?a?= =?x?Q?b?= =?utf-8?Q?c?=", "a =?x?Q?b?= c")]
    [InlineData("=?utf-8?Q?=3D=3Futf-8=3FQ=3Fa=3F=3D?=", "=?utf-8?Q?a?=")]
    [InlineData("x=?utf-8?Q?a?=", "x=?utf-8?Q?a?=")]
    [InlineData("=?utf-8?Q?a?=\u00A0=?utf-8?Q?b?=", "=?utf-8?Q?a?=\u00A0=?utf-8?Q?b?=")]
    public void DecodeReturnsLiteralConversionsWithoutAddingLf(string source, string expected)
    {
        var io = new FakeIo();
        io.Files["f"] = Encoding.UTF8.GetBytes(source);

        var output = DecodeCommand.Run(["f"], new HashSet<string>(), new Dictionary<string, string>(), new Inputs(io));

        Assert.Equal(expected, output.Out);
        Assert.Equal(0, output.Code);
        Assert.Equal(["f"], io.FileReads);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    [Fact]
    public void DecodePreservesLiteralBomLineEndingsNulAndRejectsInvalidBytesAtInputBoundary()
    {
        var io = new FakeIo();
        io.Files["f"] = [0xEF, 0xBB, 0xBF, 0x68, 0x69, 0x0D, 0x0A, 0x00];

        var output = DecodeCommand.Run(["f"], new HashSet<string>(), new Dictionary<string, string>(), new Inputs(io));

        Assert.Equal("\uFEFFhi\r\n\0", output.Out);
        Assert.Equal(0, output.Code);
        Assert.Equal(["f"], io.FileReads);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    [Theory]
    [InlineData("=?utf-8?Q?a?=", "a", 0)]
    [InlineData("=?utf-8?Q?a", "=?utf-8?Q?a", 0)]
    [InlineData("=??=", "=??=", 1)]
    [InlineData("=?koi8-r?Q?a?=", "=?koi8-r?Q?a?=", 1)]
    [InlineData("=?utf-8?X?a?=", "=?utf-8?X?a?=", 1)]
    [InlineData("=?utf-8?B?S?=", "=?utf-8?B?S?=", 1)]
    [InlineData("=?utf-8?B?SGk?=", "Hi", 1)]
    [InlineData("=?utf-8?Q?=4?=", "=?utf-8?Q?=4?=", 1)]
    [InlineData("=?utf-8?Q?=C3?=", "=?utf-8?Q?=C3?=", 1)]
    [InlineData("=?utf-8?Q?=00?=", "\0", 1)]
    [InlineData("=?utf-8?Q?aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa?=", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", 0)]
    [InlineData("=?utf-8?Q?aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa?=", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", 1)]
    public void DecodeProblemMatrixReturnsTextWithAndWithoutStrict(string source, string expected, int strictCode)
    {
        var ordinaryIo = new FakeIo();
        ordinaryIo.Files["f"] = Encoding.UTF8.GetBytes(source);

        var ordinaryOutput = DecodeCommand.Run(["f"], new HashSet<string>(), new Dictionary<string, string>(), new Inputs(ordinaryIo));

        Assert.Equal(expected, ordinaryOutput.Out);
        Assert.Equal(0, ordinaryOutput.Code);
        Assert.Equal(["f"], ordinaryIo.FileReads);
        Assert.Empty(ordinaryIo.OutCalls);
        Assert.Empty(ordinaryIo.ErrCalls);

        var strictIo = new FakeIo();
        strictIo.Files["f"] = Encoding.UTF8.GetBytes(source);

        var strictOutput = DecodeCommand.Run(["f"], new HashSet<string> { "--strict" }, new Dictionary<string, string>(), new Inputs(strictIo));

        Assert.Equal(expected, strictOutput.Out);
        Assert.Equal(strictCode, strictOutput.Code);
        Assert.Equal(["f"], strictIo.FileReads);
        Assert.Empty(strictIo.OutCalls);
        Assert.Empty(strictIo.ErrCalls);
    }

    [Theory]
    [InlineData("", "AT  GAP  TOKEN  HOW  RESULT  CODES\n")]
    [InlineData("hello", "AT  GAP  TOKEN  HOW  RESULT  CODES\n")]
    [InlineData("=?UTF8*en-US?q?a?=", "AT   GAP  TOKEN                 HOW            RESULT  CODES\n1:1  0    \"=?UTF8*en-US?q?a?=\"  Q utf-8*en-US  \"a\"     -\n")]
    [InlineData("=?ASCII?b?QQ==?=", "AT   GAP  TOKEN               HOW         RESULT  CODES\n1:1  0    \"=?ASCII?b?QQ==?=\"  B us-ascii  \"A\"     -\n")]
    [InlineData("=??=", "AT   GAP  TOKEN   HOW   RESULT  CODES\n1:1  0    \"=??=\"  kept  -       EW001\n")]
    [InlineData("=?utf-8?B?AA?=", "AT   GAP  TOKEN             HOW      RESULT  CODES\n1:1  0    \"=?utf-8?B?AA?=\"  B utf-8  \"?\"     EW005,EW009\n")]
    [InlineData("=?utf-8?Q??=", "AT   GAP  TOKEN           HOW      RESULT  CODES\n1:1  0    \"=?utf-8?Q??=\"  Q utf-8  \"\"      -\n")]
    public void ExplainRendersCompleteLiteralCanonicalTables(string source, string expected)
    {
        var io = new FakeIo();
        io.Files["f"] = Encoding.UTF8.GetBytes(source);

        var output = ExplainCommand.Run(["f"], new HashSet<string>(), new Dictionary<string, string>(), new Inputs(io));

        Assert.Equal(expected, output.Out);
        Assert.Equal(0, output.Code);
        Assert.Equal(["f"], io.FileReads);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    [Theory]
    [InlineData("😀 =?utf-8?Q?a?=", "AT   GAP  TOKEN            HOW      RESULT  CODES\n1:3  0    \"=?utf-8?Q?a?=\"  Q utf-8  \"a\"     -\n")]
    [InlineData("\r =?utf-8?Q?a?=", "AT   GAP  TOKEN            HOW      RESULT  CODES\n1:3  0    \"=?utf-8?Q?a?=\"  Q utf-8  \"a\"     -\n")]
    [InlineData("x\n=?utf-8?Q?a?=", "AT   GAP  TOKEN            HOW      RESULT  CODES\n2:1  0    \"=?utf-8?Q?a?=\"  Q utf-8  \"a\"     -\n")]
    [InlineData("=?utf-8?Q?a?= =?utf-8?Q?b?=", "AT    GAP  TOKEN            HOW      RESULT  CODES\n1:1   0    \"=?utf-8?Q?a?=\"  Q utf-8  \"a\"     -\n1:15  1    \"=?utf-8?Q?b?=\"  Q utf-8  \"b\"     -\n")]
    [InlineData("=?utf-8?Q?a?=\n=?utf-8?Q?b?=", "AT   GAP  TOKEN            HOW      RESULT  CODES\n1:1  0    \"=?utf-8?Q?a?=\"  Q utf-8  \"a\"     -\n2:1  0    \"=?utf-8?Q?b?=\"  Q utf-8  \"b\"     -\n")]
    [InlineData("=?utf-8?Q?a?= =?x?Q?b?= =?utf-8?Q?c?=", "AT    GAP  TOKEN            HOW      RESULT  CODES\n1:1   0    \"=?utf-8?Q?a?=\"  Q utf-8  \"a\"     -\n1:15  0    \"=?x?Q?b?=\"      kept     -       EW002\n1:25  0    \"=?utf-8?Q?c?=\"  Q utf-8  \"c\"     -\n")]
    public void ExplainUsesCodePointLocationsAndRecordedGaps(string source, string expected)
    {
        var io = new FakeIo();
        io.Files["f"] = Encoding.UTF8.GetBytes(source);

        var output = ExplainCommand.Run(["f"], new HashSet<string>(), new Dictionary<string, string>(), new Inputs(io));

        Assert.Equal(expected, output.Out);
        Assert.Equal(0, output.Code);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    [Theory]
    [InlineData("=?utf-8?Q?=F0=9F=98=80?=", "AT   GAP  TOKEN                       HOW      RESULT  CODES\n1:1  0    \"=?utf-8?Q?=F0=9F=98=80?=\"  Q utf-8  \"?\"     -\n")]
    [InlineData("=?utf-8?Q?abcdefghijklmnopqrstuvwx?=", "AT   GAP  TOKEN                       HOW      RESULT                      CODES\n1:1  0    \"=?utf-8?Q?abcdefghijk...\"  Q utf-8  \"abcdefghijklmnopqrstuvwx\"  -\n")]
    [InlineData("=?utf-8?Q?abcdefghijklmnopqrstuvwxy?=", "AT   GAP  TOKEN                       HOW      RESULT                      CODES\n1:1  0    \"=?utf-8?Q?abcdefghijk...\"  Q utf-8  \"abcdefghijklmnopqrstu...\"  -\n")]
    [InlineData("=?utf-8?Q?aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa?=", "AT   GAP  TOKEN                       HOW      RESULT                      CODES\n1:1  0    \"=?utf-8?Q?aaaaaaaaaaa...\"  Q utf-8  \"aaaaaaaaaaaaaaaaaaaaa...\"  EW008\n")]
    [InlineData("=?utf-8?Q?aaaaaaaaaaaaaaaaaaaa=F0=9F=98=80bbbb?=", "AT   GAP  TOKEN                       HOW      RESULT                      CODES\n1:1  0    \"=?utf-8?Q?aaaaaaaaaaa...\"  Q utf-8  \"aaaaaaaaaaaaaaaaaaaa?...\"  -\n")]
    public void ExplainClipsTokenAndResultByCodePointBeforeSanitizing(string source, string expected)
    {
        var io = new FakeIo();
        io.Files["f"] = Encoding.UTF8.GetBytes(source);

        var output = ExplainCommand.Run(["f"], new HashSet<string>(), new Dictionary<string, string>(), new Inputs(io));

        Assert.Equal(expected, output.Out);
        Assert.Equal(0, output.Code);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    private sealed class FakeIo : IIo
    {
        public byte[] StdinBytes { get; set; } = [];

        public bool FailOnAnyRead { get; set; }

        public Dictionary<string, byte[]> Files { get; } = new(StringComparer.Ordinal);

        public HashSet<string> FailingFiles { get; } = new(StringComparer.Ordinal);

        public List<string> FileReads { get; } = [];

        public List<string> OutCalls { get; } = [];

        public List<string> ErrCalls { get; } = [];

        public int StdinReads { get; private set; }

        public byte[] ReadStdin()
        {
            StdinReads++;
            if (FailOnAnyRead)
            {
                throw new IOException();
            }

            return StdinBytes;
        }

        public byte[] ReadFile(string path)
        {
            FileReads.Add(path);
            if (FailOnAnyRead || FailingFiles.Contains(path))
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
