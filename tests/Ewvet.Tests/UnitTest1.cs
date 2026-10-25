using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Ewvet.Tests;

public sealed class UnitTest1
{
    [Theory]
    [InlineData(new string[] { }, 2, "", "ewvet: missing command (decode, explain, check, encode or charsets)\n")]
    [InlineData(new string[] { "show" }, 2, "", "ewvet: unknown command \"show\"\n")]
    [InlineData(new string[] { "--help", "decode" }, 2, "", "ewvet: unknown command \"--help\"\n")]
    [InlineData(new string[] { "Decode" }, 2, "", "ewvet: unknown command \"Decode\"\n")]
    [InlineData(new string[] { "😀" }, 2, "", "ewvet: unknown command \"?\"\n")]
    [InlineData(new string[] { "--help" }, 0, "usage: ewvet decode [--strict] FILE\n       ewvet explain FILE\n       ewvet check FILE...\n       ewvet encode [--charset NAME] [--encoding auto|b|q] FILE\n       ewvet charsets [PREFIX]\n", "")]
    public void DispatchesOnlyCommandsAndSoleHelp(string[] argv, int expectedCode, string expectedOut, string expectedErr)
    {
        var io = new FakeIo();

        int code = Cli.Run(argv, io);

        Assert.Equal(expectedCode, code);
        Assert.Equal(expectedOut.Length == 0 ? [] : [expectedOut], io.OutCalls);
        Assert.Equal(expectedErr.Length == 0 ? [] : [expectedErr], io.ErrCalls);
        Assert.Empty(io.FileReads);
        Assert.Equal(0, io.StdinReads);
    }

    [Theory]
    [InlineData(new string[] { "show", "--all" }, "ewvet: unknown command \"show\"\n")]
    [InlineData(new string[] { "decode", "--all", "f" }, "ewvet: unknown option \"--all\"\n")]
    [InlineData(new string[] { "decode", "--strict=1", "f" }, "ewvet: unknown option \"--strict=1\"\n")]
    [InlineData(new string[] { "decode", "--" }, "ewvet: unknown option \"--\"\n")]
    [InlineData(new string[] { "explain", "--strict", "f" }, "ewvet: unknown option \"--strict\"\n")]
    [InlineData(new string[] { "check", "--help" }, "ewvet: unknown option \"--help\"\n")]
    [InlineData(new string[] { "charsets", "--charset=utf-8" }, "ewvet: unknown option \"--charset=utf-8\"\n")]
    [InlineData(new string[] { "encode", "--charset" }, "ewvet: --charset needs a value\n")]
    [InlineData(new string[] { "encode", "--encoding" }, "ewvet: --encoding needs a value\n")]
    [InlineData(new string[] { "decode", "--all", "--strict=1" }, "ewvet: unknown option \"--all\"\n")]
    [InlineData(new string[] { "encode", "--charset", "koi8-r" }, "ewvet: missing FILE\n")]
    [InlineData(new string[] { "decode", "f", "f" }, "ewvet: too many arguments\n")]
    [InlineData(new string[] { "charsets", "a", "b" }, "ewvet: too many arguments\n")]
    [InlineData(new string[] { "encode", "--encoding", "B", "--charset", "koi8-r", "f" }, "ewvet: unknown charset \"koi8-r\"\n")]
    [InlineData(new string[] { "encode", "--charset", "--encoding", "f" }, "ewvet: unknown charset \"--encoding\"\n")]
    [InlineData(new string[] { "encode", "--charset=", "f" }, "ewvet: unknown charset \"\"\n")]
    [InlineData(new string[] { "encode", "--encoding", "B", "f" }, "ewvet: invalid encoding \"B\" (expected auto, b or q)\n")]
    [InlineData(new string[] { "decode" }, "ewvet: missing FILE\n")]
    [InlineData(new string[] { "explain" }, "ewvet: missing FILE\n")]
    [InlineData(new string[] { "check" }, "ewvet: missing FILE\n")]
    [InlineData(new string[] { "encode" }, "ewvet: missing FILE\n")]
    public void ValidatesCommandOptionsArityAndValuesBeforeReading(string[] argv, string expectedErr)
    {
        var io = new FakeIo();

        int code = Cli.Run(argv, io);

        Assert.Equal(2, code);
        Assert.Empty(io.OutCalls);
        Assert.Equal([expectedErr], io.ErrCalls);
        Assert.Empty(io.FileReads);
        Assert.Equal(0, io.StdinReads);
    }

    [Theory]
    [InlineData(new string[] { "decode", "-" }, "", 0, "", 1)]
    [InlineData(new string[] { "decode", "-" }, "=?utf-8?Q?a?=", 0, "a", 1)]
    [InlineData(new string[] { "decode", "-", "--strict", "--strict" }, "=?utf-8?B?SGk?=", 1, "Hi", 1)]
    [InlineData(new string[] { "decode", "-" }, "\uFEFFa\r\n\u0000", 0, "\uFEFFa\r\n\u0000", 1)]
    [InlineData(new string[] { "explain", "-" }, "", 0, "AT  GAP  TOKEN  HOW  RESULT  CODES\n", 1)]
    [InlineData(new string[] { "check", "-" }, "x", 0, "ok, 0 encoded words in 1 file\n", 1)]
    [InlineData(new string[] { "check", "-" }, "=?a?=", 1, "<stdin>:1:1: EW001 malformed encoded word \"=?a?=\"\n1 problem in 1 file\n", 1)]
    [InlineData(new string[] { "encode", "-" }, "é", 0, "=?utf-8?B?w6k=?=", 1)]
    [InlineData(new string[] { "encode", "--charset=koi8-r", "--charset", "Latin1", "--encoding=b", "--encoding", "q", "-" }, "é", 0, "=?iso-8859-1?Q?=E9?=", 1)]
    [InlineData(new string[] { "charsets", "zz" }, null, 1, "no charset starts with \"zz\"\n", 0)]
    [InlineData(new string[] { "charsets", "L9" }, null, 0, "NAME         ALIASES               BYTES\niso-8859-15  iso8859-15,l9,latin9  1\n", 0)]
    [InlineData(new string[] { "charsets" }, null, 0, "NAME         ALIASES               BYTES\niso-8859-1   iso8859-1,l1,latin1   1\niso-8859-15  iso8859-15,l9,latin9  1\nus-ascii     ascii,us              1\nutf-8        utf8                  1 to 4\n", 0)]
    [InlineData(new string[] { "charsets", "" }, null, 0, "NAME         ALIASES               BYTES\niso-8859-1   iso8859-1,l1,latin1   1\niso-8859-15  iso8859-15,l9,latin9  1\nus-ascii     ascii,us              1\nutf-8        utf8                  1 to 4\n", 0)]
    public void DispatchesAdaptersWithExactOutputAndOneWrite(string[] argv, string? stdinText, int expectedCode, string expectedOut, int expectedStdinReads)
    {
        var io = new FakeIo();
        if (stdinText is not null)
        {
            io.StdinBytes = Encoding.UTF8.GetBytes(stdinText);
        }

        int code = Cli.Run(argv, io);

        Assert.Equal(expectedCode, code);
        Assert.Equal(expectedOut.Length == 0 ? [] : [expectedOut], io.OutCalls);
        Assert.Empty(io.ErrCalls);
        Assert.Empty(io.FileReads);
        Assert.Equal(expectedStdinReads, io.StdinReads);
    }

    [Fact]
    public void ReadsSourcesInOrderAndTranslatesFailuresWithoutPartialOutput()
    {
        var missingIo = new FakeIo();
        missingIo.FailingFiles.Add("nofile.txt");
        Assert.Equal(2, Cli.Run(["check", "nofile.txt"], missingIo));
        Assert.Empty(missingIo.OutCalls);
        Assert.Equal(["ewvet: nofile.txt: cannot read the file\n"], missingIo.ErrCalls);
        Assert.Equal(["nofile.txt"], missingIo.FileReads);

        var sanitizedIo = new FakeIo();
        sanitizedIo.FailingFiles.Add("café.txt");
        Assert.Equal(2, Cli.Run(["decode", "café.txt"], sanitizedIo));
        Assert.Empty(sanitizedIo.OutCalls);
        Assert.Equal(["ewvet: caf?.txt: cannot read the file\n"], sanitizedIo.ErrCalls);
        Assert.Equal(["café.txt"], sanitizedIo.FileReads);

        var duplicateStdinIo = new FakeIo { StdinBytes = [120] };
        Assert.Equal(2, Cli.Run(["check", "-", "-"], duplicateStdinIo));
        Assert.Empty(duplicateStdinIo.OutCalls);
        Assert.Equal(["ewvet: standard input is named twice\n"], duplicateStdinIo.ErrCalls);
        Assert.Equal(1, duplicateStdinIo.StdinReads);
        Assert.Empty(duplicateStdinIo.FileReads);

        var orderedIo = new FakeIo();
        orderedIo.Files["f"] = [120];
        orderedIo.FailingFiles.Add("nofile.txt");
        Assert.Equal(2, Cli.Run(["check", "f", "nofile.txt"], orderedIo));
        Assert.Empty(orderedIo.OutCalls);
        Assert.Equal(["ewvet: nofile.txt: cannot read the file\n"], orderedIo.ErrCalls);
        Assert.Equal(["f", "nofile.txt"], orderedIo.FileReads);

        var repeatedFileIo = new FakeIo();
        repeatedFileIo.Files["f"] = [120];
        Assert.Equal(0, Cli.Run(["check", "f", "f"], repeatedFileIo));
        Assert.Equal(["ok, 0 encoded words in 2 files\n"], repeatedFileIo.OutCalls);
        Assert.Empty(repeatedFileIo.ErrCalls);
        Assert.Equal(["f", "f"], repeatedFileIo.FileReads);

        var negativePathIo = new FakeIo();
        negativePathIo.FailingFiles.Add("-5");
        Assert.Equal(2, Cli.Run(["decode", "-5"], negativePathIo));
        Assert.Empty(negativePathIo.OutCalls);
        Assert.Equal(["ewvet: -5: cannot read the file\n"], negativePathIo.ErrCalls);
        Assert.Equal(["-5"], negativePathIo.FileReads);

        var malformedIo = new FakeIo { StdinBytes = [97, 255, 10] };
        Assert.Equal(2, Cli.Run(["check", "-"], malformedIo));
        Assert.Empty(malformedIo.OutCalls);
        Assert.Equal(["ewvet: <stdin>: input is not valid UTF-8\n"], malformedIo.ErrCalls);
        Assert.Equal(1, malformedIo.StdinReads);
        Assert.Empty(malformedIo.FileReads);

        var conversionIo = new FakeIo();
        conversionIo.Files["e.txt"] = Encoding.UTF8.GetBytes("é");
        Assert.Equal(2, Cli.Run(["encode", "--charset", "us-ascii", "e.txt"], conversionIo));
        Assert.Empty(conversionIo.OutCalls);
        Assert.Equal(["ewvet: U+00E9 cannot be written in us-ascii\n"], conversionIo.ErrCalls);
        Assert.Equal(["e.txt"], conversionIo.FileReads);
    }

    [Fact]
    public void ProgramForwardsCliAndReadsNamedFilesWithFileReadAllBytes()
    {
        var io = new FakeIo { StdinBytes = [97] };

        int code = Cli.Run(["decode", "-"], io);

        Assert.Equal(0, code);
        Assert.Equal(["a"], io.OutCalls);
        Assert.Empty(io.ErrCalls);

        MethodInfo cliRun = typeof(Cli).GetMethod(nameof(Cli.Run), [typeof(IReadOnlyList<string>), typeof(IIo)])!;
        byte[] mainIl = typeof(Program).GetMethod(nameof(Program.Main))!.GetMethodBody()!.GetILAsByteArray()!;
        var mainCalls = new List<MethodBase>();
        for (int offset = 0; offset <= mainIl.Length - sizeof(int); offset++)
        {
            try
            {
                MethodBase? called = typeof(Program).Module.ResolveMethod(BitConverter.ToInt32(mainIl, offset));
                if (called is not null)
                {
                    mainCalls.Add(called);
                }
            }
            catch (ArgumentException)
            {
            }
            catch (BadImageFormatException)
            {
            }
        }

        Assert.Contains(cliRun, mainCalls);

        MethodInfo readAllBytes = typeof(File).GetMethod(nameof(File.ReadAllBytes), [typeof(string)])!;
        Type realIo = typeof(Program).GetNestedType("RealIo", BindingFlags.NonPublic)!;
        byte[] readFileIl = realIo.GetMethod("ReadFile")!.GetMethodBody()!.GetILAsByteArray()!;
        var readFileCalls = new List<MethodBase>();
        for (int offset = 0; offset <= readFileIl.Length - sizeof(int); offset++)
        {
            try
            {
                MethodBase? called = realIo.Module.ResolveMethod(BitConverter.ToInt32(readFileIl, offset));
                if (called is not null)
                {
                    readFileCalls.Add(called);
                }
            }
            catch (ArgumentException)
            {
            }
            catch (BadImageFormatException)
            {
            }
        }

        Assert.Contains(readAllBytes, readFileCalls);
    }

    [Fact]
    public void PropagatesNonFailureExceptionsWithoutWriting()
    {
        var sentinel = new InvalidOperationException("sentinel");
        var io = new FakeIo { StdinException = sentinel };

        var thrown = Assert.Throws<InvalidOperationException>(() => Cli.Run(["decode", "-"], io));

        Assert.Same(sentinel, thrown);
        Assert.Equal(1, io.StdinReads);
        Assert.Empty(io.FileReads);
        Assert.Empty(io.OutCalls);
        Assert.Empty(io.ErrCalls);
    }

    [Fact]
    public async Task ProcessReportsMalformedStandardInputAsExactBytes()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = AppContext.BaseDirectory,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "Ewvet.dll"));
        startInfo.ArgumentList.Add("check");
        startInfo.ArgumentList.Add("-");

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start ewvet.");
        using var stdout = new MemoryStream();
        using var stderr = new MemoryStream();
        Task stdoutDrain = process.StandardOutput.BaseStream.CopyToAsync(stdout);
        Task stderrDrain = process.StandardError.BaseStream.CopyToAsync(stderr);

        await process.StandardInput.BaseStream.WriteAsync(new byte[] { 97, 255, 10 });
        process.StandardInput.Close();

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        await process.WaitForExitAsync(timeout.Token);
        await Task.WhenAll(stdoutDrain, stderrDrain);

        Assert.Equal(2, process.ExitCode);
        Assert.Equal(Array.Empty<byte>(), stdout.ToArray());
        Assert.Equal(new byte[]
        {
            101, 119, 118, 101, 116, 58, 32, 60, 115, 116, 100, 105, 110, 62, 58, 32,
            105, 110, 112, 117, 116, 32, 105, 115, 32, 110, 111, 116, 32, 118, 97, 108,
            105, 100, 32, 85, 84, 70, 45, 56, 10
        }, stderr.ToArray());
    }

    private sealed class FakeIo : IIo
    {
        public byte[] StdinBytes { get; set; } = [];

        public Exception? StdinException { get; set; }

        public Dictionary<string, byte[]> Files { get; } = new(StringComparer.Ordinal);

        public HashSet<string> FailingFiles { get; } = new(StringComparer.Ordinal);

        public List<string> FileReads { get; } = [];

        public List<string> OutCalls { get; } = [];

        public List<string> ErrCalls { get; } = [];

        public int StdinReads { get; private set; }

        public byte[] ReadStdin()
        {
            StdinReads++;
            if (StdinException is not null)
            {
                throw StdinException;
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
