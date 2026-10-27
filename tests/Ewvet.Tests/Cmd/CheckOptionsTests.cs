namespace Ewvet.Tests;

public sealed class CheckOptionsTests
{
    private const string One = "X: =?koi8-r?Q?x?= =?utf-8?Q?a?=\n";
    private const string Good = "X: =?utf-8?Q?a?=\n";
    private const string Mail = "X-One: =?a?= =?koi8-r?Q?x?= =?utf-8?X?a?= =?utf-8?B?S?=\n";

    [Theory]
    [InlineData(new string[] { "check", "--quiet", "one.txt" }, "1 problem in 1 file\n", 1)]
    [InlineData(new string[] { "check", "--quiet", "good.txt" }, "ok, 1 encoded word in 1 file\n", 0)]
    [InlineData(new string[] { "check", "--quiet", "--quiet", "good.txt" }, "ok, 1 encoded word in 1 file\n", 0)]
    [InlineData(new string[] { "check", "--ignore", "EW002", "one.txt" }, "ok, 2 encoded words in 1 file\n", 0)]
    [InlineData(new string[] { "check", "--ignore=EW002,EW001", "one.txt", "good.txt" }, "ok, 3 encoded words in 2 files\n", 0)]
    [InlineData(new string[] { "check", "--quiet", "--ignore", "EW002", "one.txt" }, "ok, 2 encoded words in 1 file\n", 0)]
    [InlineData(new string[] { "check", "--ignore", "EW002,EW002", "one.txt" }, "ok, 2 encoded words in 1 file\n", 0)]
    [InlineData(
        new string[] { "check", "--ignore", "EW002,EW003,EW004,EW001", "mail.txt", "good.txt" },
        "ok, 5 encoded words in 2 files\n",
        0)]
    [InlineData(
        new string[] { "check", "--ignore", "EW002", "mail.txt" },
        "mail.txt:1:8: EW001 malformed encoded word \"=?a?=\"\n"
            + "mail.txt:1:29: EW003 unknown encoding \"X\"\n"
            + "mail.txt:1:43: EW004 invalid base64 text\n"
            + "3 problems in 1 file\n",
        1)]
    [InlineData(new string[] { "check", "--quiet", "--ignore", "EW002,EW001", "mail.txt" }, "2 problems in 1 file\n", 1)]
    [InlineData(
        new string[] { "check", "--ignore", "EW009", "good.txt", "one.txt" },
        "one.txt:1:4: EW002 unknown charset \"koi8-r\"\n1 problem in 1 file\n",
        1)]
    [InlineData(
        new string[] { "check", "--ignore", "EW002", "--ignore", "EW001", "one.txt" },
        "one.txt:1:4: EW002 unknown charset \"koi8-r\"\n1 problem in 1 file\n",
        1)]
    public void CheckHonorsQuietAndIgnore(string[] argv, string expectedOut, int expectedCode)
    {
        var io = NewIo();

        int code = Cli.Run(argv, io);

        Assert.Equal(expectedCode, code);
        Assert.Equal([expectedOut], io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    [Theory]
    [InlineData(new string[] { "check", "--ignore", "EW010", "one.txt" }, "ewvet: unknown problem code \"EW010\"\n")]
    [InlineData(new string[] { "check", "--ignore", "ew002", "one.txt" }, "ewvet: unknown problem code \"ew002\"\n")]
    [InlineData(new string[] { "check", "--ignore", "EW002,", "one.txt" }, "ewvet: unknown problem code \"\"\n")]
    [InlineData(new string[] { "check", "--ignore", "EW000", "missing.txt" }, "ewvet: unknown problem code \"EW000\"\n")]
    [InlineData(new string[] { "check", "--ignore=", "one.txt" }, "ewvet: --ignore needs a non-empty value\n")]
    [InlineData(new string[] { "check", "--ignore" }, "ewvet: --ignore needs a value\n")]
    [InlineData(new string[] { "check", "--ignore", "EW002" }, "ewvet: missing FILE\n")]
    [InlineData(new string[] { "check", "--quiet=1", "one.txt" }, "ewvet: unknown option \"--quiet=1\"\n")]
    [InlineData(new string[] { "decode", "--quiet", "one.txt" }, "ewvet: unknown option \"--quiet\"\n")]
    [InlineData(new string[] { "explain", "--ignore", "EW001", "one.txt" }, "ewvet: unknown option \"--ignore\"\n")]
    public void CheckRejectsBadOptionsBeforeReadingAnyFile(string[] argv, string expectedErr)
    {
        var io = NewIo();

        int code = Cli.Run(argv, io);

        Assert.Equal(2, code);
        Assert.Empty(io.OutCalls);
        Assert.Equal([expectedErr], io.ErrCalls);
        Assert.Empty(io.FileReads);
    }

    [Fact]
    public void HelpListsTheCheckOptions()
    {
        var io = NewIo();

        int code = Cli.Run(["--help"], io);

        Assert.Equal(0, code);
        Assert.Contains("       ewvet check [--quiet] [--ignore CODES] FILE...\n", io.OutCalls[0]);
    }

    private static FakeIo NewIo()
    {
        var io = new FakeIo();
        io.Files["one.txt"] = System.Text.Encoding.UTF8.GetBytes(One);
        io.Files["good.txt"] = System.Text.Encoding.UTF8.GetBytes(Good);
        io.Files["mail.txt"] = System.Text.Encoding.UTF8.GetBytes(Mail);
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
