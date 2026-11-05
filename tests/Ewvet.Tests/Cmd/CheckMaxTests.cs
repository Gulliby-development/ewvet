namespace Ewvet.Tests;

public sealed class CheckMaxTests
{
    private const string Mail = "X-One: =?a?= =?koi8-r?Q?x?= =?utf-8?X?a?= =?utf-8?B?S?=\n";
    private const string Good = "X: =?utf-8?Q?a?=\n";
    private const string Two = "Y: =?koi8-r?Q?x?= =?utf-8?X?a?=\n";

    [Theory]
    [InlineData(
        new string[] { "check", "--max", "2", "mail.txt" },
        "mail.txt:1:8: EW001 malformed encoded word \"=?a?=\"\n"
            + "mail.txt:1:14: EW002 unknown charset \"koi8-r\"\n"
            + "... 2 more\n"
            + "4 problems in 1 file\n",
        1)]
    [InlineData(
        new string[] { "check", "--max=3", "mail.txt" },
        "mail.txt:1:8: EW001 malformed encoded word \"=?a?=\"\n"
            + "mail.txt:1:14: EW002 unknown charset \"koi8-r\"\n"
            + "mail.txt:1:29: EW003 unknown encoding \"X\"\n"
            + "... 1 more\n"
            + "4 problems in 1 file\n",
        1)]
    [InlineData(
        new string[] { "check", "--max", "4", "mail.txt" },
        "mail.txt:1:8: EW001 malformed encoded word \"=?a?=\"\n"
            + "mail.txt:1:14: EW002 unknown charset \"koi8-r\"\n"
            + "mail.txt:1:29: EW003 unknown encoding \"X\"\n"
            + "mail.txt:1:43: EW004 invalid base64 text\n"
            + "4 problems in 1 file\n",
        1)]
    [InlineData(
        new string[] { "check", "--max", "9999", "mail.txt" },
        "mail.txt:1:8: EW001 malformed encoded word \"=?a?=\"\n"
            + "mail.txt:1:14: EW002 unknown charset \"koi8-r\"\n"
            + "mail.txt:1:29: EW003 unknown encoding \"X\"\n"
            + "mail.txt:1:43: EW004 invalid base64 text\n"
            + "4 problems in 1 file\n",
        1)]
    [InlineData(
        new string[] { "check", "--max", "1", "mail.txt", "two.txt" },
        "mail.txt:1:8: EW001 malformed encoded word \"=?a?=\"\n"
            + "... 5 more\n"
            + "6 problems in 2 files\n",
        1)]
    [InlineData(
        new string[] { "check", "--max", "1", "--ignore", "EW001,EW002", "mail.txt" },
        "mail.txt:1:29: EW003 unknown encoding \"X\"\n"
            + "... 1 more\n"
            + "2 problems in 1 file\n",
        1)]
    [InlineData(
        new string[] { "check", "--max", "1", "--max", "3", "mail.txt" },
        "mail.txt:1:8: EW001 malformed encoded word \"=?a?=\"\n"
            + "mail.txt:1:14: EW002 unknown charset \"koi8-r\"\n"
            + "mail.txt:1:29: EW003 unknown encoding \"X\"\n"
            + "... 1 more\n"
            + "4 problems in 1 file\n",
        1)]
    [InlineData(new string[] { "check", "--quiet", "--max", "1", "mail.txt" }, "4 problems in 1 file\n", 1)]
    [InlineData(new string[] { "check", "--max", "1", "good.txt" }, "ok, 1 encoded word in 1 file\n", 0)]
    [InlineData(new string[] { "check", "--max", "1", "--ignore", "EW001,EW002,EW003,EW004", "mail.txt" }, "ok, 4 encoded words in 1 file\n", 0)]
    public void CheckListsAtMostMaxProblemsAndCountsAll(string[] argv, string expectedOut, int expectedCode)
    {
        var io = NewIo();

        int code = Cli.Run(argv, io);

        Assert.Equal(expectedCode, code);
        Assert.Equal([expectedOut], io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("00")]
    [InlineData("01")]
    [InlineData("10000")]
    [InlineData("-1")]
    [InlineData("+1")]
    [InlineData("1.5")]
    [InlineData("1e2")]
    [InlineData("")]
    [InlineData("two")]
    [InlineData(" 1")]
    public void CheckRejectsABadMax(string value)
    {
        var io = NewIo();

        int code = Cli.Run(["check", "--max=" + value, "mail.txt"], io);

        Assert.Equal(2, code);
        Assert.Empty(io.OutCalls);
        Assert.Equal(["ewvet: --max must be a whole number from 1 to 9999\n"], io.ErrCalls);
        Assert.Empty(io.FileReads);
    }

    [Fact]
    public void CheckChecksIgnoreBeforeMaxAndMaxBeforeTheFiles()
    {
        var io = NewIo();

        Assert.Equal(2, Cli.Run(["check", "--max", "0", "--ignore", "EW010", "mail.txt"], io));
        Assert.Equal(["ewvet: unknown problem code \"EW010\"\n"], io.ErrCalls);

        io = NewIo();
        Assert.Equal(2, Cli.Run(["check", "--max", "0", "missing.txt"], io));
        Assert.Equal(["ewvet: --max must be a whole number from 1 to 9999\n"], io.ErrCalls);
        Assert.Empty(io.FileReads);
    }

    [Fact]
    public void CheckNeedsAValueForMaxAndOtherCommandsRejectTheOption()
    {
        var io = NewIo();
        Assert.Equal(2, Cli.Run(["check", "--max"], io));
        Assert.Equal(["ewvet: --max needs a value\n"], io.ErrCalls);

        io = NewIo();
        Assert.Equal(2, Cli.Run(["decode", "--max", "1", "mail.txt"], io));
        Assert.Equal(["ewvet: unknown option \"--max\"\n"], io.ErrCalls);
    }

    private static FakeIo NewIo()
    {
        var io = new FakeIo();
        io.Files["mail.txt"] = System.Text.Encoding.UTF8.GetBytes(Mail);
        io.Files["good.txt"] = System.Text.Encoding.UTF8.GetBytes(Good);
        io.Files["two.txt"] = System.Text.Encoding.UTF8.GetBytes(Two);
        return io;
    }

    private sealed class FakeIo : IIo
    {
        public Dictionary<string, byte[]> Files { get; } = new(StringComparer.Ordinal);

        public List<string> FileReads { get; } = [];

        public List<string> OutCalls { get; } = [];

        public List<string> ErrCalls { get; } = [];

        public byte[] ReadStdin()
        {
            return [];
        }

        public byte[] ReadFile(string path)
        {
            FileReads.Add(path);
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
