namespace Ewvet;

public sealed class DocsTests
{
    [Fact]
    public void IncludesRequiredSectionsAndDevelopmentInstructions()
    {
        string readme = ReadReadme();

        Assert.Contains("## Install", readme);
        Assert.Contains("## Usage", readme);
        Assert.Contains("## Commands", readme);
        Assert.Contains("## Exit codes", readme);
        Assert.Contains("## Development", readme);
        Assert.Contains("dotnet build", readme);
        Assert.Contains("dotnet run --project src/Ewvet -- COMMAND", readme);
        Assert.Contains("dotnet test", readme);
        Assert.Contains("dotnet format --verify-no-changes", readme);
        Assert.Contains(
            "In the examples, `ewvet` stands for `dotnet run --project src/Ewvet --`:",
            readme);
        Assert.Equal(
            1,
            readme.Split("In the examples, `ewvet` stands for", StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public void IncludesTheExactFiveCommandUsageBlock()
    {
        string readme = ReadReadme();
        const string usage = "usage: ewvet decode [--strict] FILE\n"
            + "       ewvet explain FILE\n"
            + "       ewvet check [--quiet] [--ignore CODES] FILE...\n"
            + "       ewvet encode [--charset NAME] [--encoding auto|b|q] FILE\n"
            + "       ewvet charsets [PREFIX]";

        Assert.Contains("```\n" + usage + "\n```", readme);
    }

    [Fact]
    public void ShowsRunnableDecodeAndExplainExamples()
    {
        string readme = ReadReadme();

        Assert.Contains("printf '' > decode.txt\newvet decode - < decode.txt", readme);
        Assert.Contains("Standard output: zero bytes. Exit status: 0.", readme);
        Assert.Contains("printf '' > explain.txt\newvet explain - < explain.txt", readme);
        Assert.Contains("AT  GAP  TOKEN  HOW  RESULT  CODES", readme);
        Assert.Contains("Exit status: 0.", readme);
    }

    [Fact]
    public void ShowsRunnableCheckAndEncodeExamplesWithLiteralOutput()
    {
        string readme = ReadReadme();

        Assert.Contains("printf '%s\\n' \\\n  'Subject: =?UTF-8?B?Y2Fmw6k=?= =?UTF-8?Q?_au_lait?='", readme);
        Assert.Contains(
            "mail.txt:3:8: EW001 malformed encoded word \"=?a?=\"\n"
                + "mail.txt:3:14: EW002 unknown charset \"koi8-r\"\n"
                + "mail.txt:3:29: EW003 unknown encoding \"X\"\n"
                + "mail.txt:3:43: EW004 invalid base64 text",
            readme);
        Assert.Contains(
            "printf 'caf\\303\\251 \\303\\274ber  nice =?x?= ok\\n\\303\\251\\n\\303\\251\\n' > e.txt\n"
                + "ewvet encode --charset=latin1 --encoding=q e.txt",
            readme);
        Assert.Contains(
            "=?iso-8859-1?Q?caf=E9_=FCber?=  nice =?iso-8859-1?Q?=3D=3Fx=3F=3D?= ok\n"
                + "=?iso-8859-1?Q?=E9?=\n"
                + "=?iso-8859-1?Q?=E9?=",
            readme);
    }

    [Fact]
    public void UsesPrintfCreatedInputsAndRealOutputCharacters()
    {
        string readme = ReadReadme();

        Assert.Contains("printf", readme);
        Assert.Contains("\\303\\251", readme);
        Assert.Contains("\\303\\274", readme);
        Assert.DoesNotContain("<U+", readme);
    }

    private static string ReadReadme()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            string path = Path.Combine(directory.FullName, "README.md");
            if (File.Exists(path))
            {
                return File.ReadAllText(path)
                    .Replace("\r\n", "\n", StringComparison.Ordinal)
                    .Replace('\r', '\n');
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find README.md in any parent of '{AppContext.BaseDirectory}'.");
    }
}
