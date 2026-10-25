namespace Ewvet.Tests;

public sealed class CheckerTests
{
    [Theory]
    [InlineData("EW001", "=?a?=", "malformed encoded word \"=?a?=\"")]
    [InlineData("EW002", "koi8-r", "unknown charset \"koi8-r\"")]
    [InlineData("EW003", "X", "unknown encoding \"X\"")]
    [InlineData("EW004", "", "invalid base64 text")]
    [InlineData("EW005", "", "base64 text is missing its padding")]
    [InlineData("EW006", "", "invalid quoted-printable text")]
    [InlineData("EW007", "utf-8", "bytes are not valid utf-8")]
    [InlineData("EW008", "76", "encoded word has 76 characters, the limit is 75")]
    [InlineData("EW009", "U+0001", "decoded text has the control character U+0001")]
    public void EveryDiagnosticCodeHasItsExactMessage(string code, string detail, string expected)
    {
        var controlCase = ("EW009", "U+0001");
        Assert.Equal("decoded text has the control character U+0001", Problems.Message(controlCase.Item1, controlCase.Item2));
        Assert.Equal(expected, Problems.Message(code, detail));
    }

    [Theory]
    [InlineData("EW002", "abcdefghijklmnopqrstuvwx", "unknown charset \"abcdefghijklmnopqrstuvwx\"")]
    [InlineData("EW002", "abcdefghijklmnopqrstuvwxy", "unknown charset \"abcdefghijklmnopqrstu...\"")]
    [InlineData("EW003", "", "unknown encoding \"\"")]
    [InlineData("EW003", "é😀\n", "unknown encoding \"???\"")]
    [InlineData("EW003", "\"", "unknown encoding \"\"\"")]
    [InlineData("EW007", "é😀\n", "bytes are not valid ???")]
    [InlineData("EW007", "é😀\u007F", "bytes are not valid ???")]
    public void MessageDetailsUseExactClippingSanitizationAndPlainRules(string code, string detail, string expected)
    {
        var unicodeDetail = ("EW003", "é😀\n");
        Assert.Equal("unknown encoding \"???\"", Problems.Message(unicodeDetail.Item1, unicodeDetail.Item2));
        Assert.Equal(expected, Problems.Message(code, detail));
    }

    [Theory]
    [InlineData("EW000", "unknown problem code \"EW000\"")]
    [InlineData("", "unknown problem code \"\"")]
    public void UnknownDiagnosticCodesThrowQuotedFailures(string code, string expected)
    {
        var failure = Assert.Throws<Failure>(() => Problems.Message(code, ""));

        Assert.Equal(expected, failure.Message);
    }

    [Theory]
    [InlineData("=?a?=", "f:1:1: EW001 malformed encoded word \"=?a?=\"\n1 problem in 1 file\n", 1)]
    [InlineData("=?koi8-r?Q?x?=", "f:1:1: EW002 unknown charset \"koi8-r\"\n1 problem in 1 file\n", 1)]
    [InlineData("=?utf-8?X?a?=", "f:1:1: EW003 unknown encoding \"X\"\n1 problem in 1 file\n", 1)]
    [InlineData("=?utf-8?B?S?=", "f:1:1: EW004 invalid base64 text\n1 problem in 1 file\n", 1)]
    [InlineData("=?utf-8?B?SGk?=", "f:1:1: EW005 base64 text is missing its padding\n1 problem in 1 file\n", 1)]
    [InlineData("=?utf-8?Q?=4?=", "f:1:1: EW006 invalid quoted-printable text\n1 problem in 1 file\n", 1)]
    [InlineData("=?utf-8?Q?=C3?=", "f:1:1: EW007 bytes are not valid utf-8\n1 problem in 1 file\n", 1)]
    [InlineData("=?utf-8?Q?aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa?=", "f:1:1: EW008 encoded word has 76 characters, the limit is 75\n1 problem in 1 file\n", 1)]
    [InlineData("=?utf-8?Q?=01?=", "f:1:1: EW009 decoded text has the control character U+0001\n1 problem in 1 file\n", 1)]
    public void ReportsOneExactLineForEachDiagnosticCode(string text, string expected, int expectedCode)
    {
        var output = Checker.Check(new List<Inputs.Source> { new("f", text) });

        Assert.Equal(expected, output.Out);
        Assert.Equal(expectedCode, output.Code);
    }

    [Fact]
    public void ReportsCodePointColumnsAndLfOnlyLineBoundaries()
    {
        (string Display, string Text)[] correctedCase = [("f", "😀 é\r =?a?=\n\t=?utf-8?Q?=4?=")];
        var correctedOutput = Checker.Check(new List<Inputs.Source>
        {
            new(correctedCase[0].Display, correctedCase[0].Text),
        });

        Assert.Equal("f:1:6: EW001 malformed encoded word \"=?a?=\"\nf:2:2: EW006 invalid quoted-printable text\n2 problems in 1 file\n", correctedOutput.Out);
        Assert.Equal(1, correctedOutput.Code);

        var carriageReturnCase = ("f", "😀 =?a?=\r=?utf-8?Q?=4?=");
        var carriageReturnOutput = Checker.Check(new List<Inputs.Source>
        {
            new(carriageReturnCase.Item1, carriageReturnCase.Item2),
        });

        Assert.Equal("f:1:3: EW001 malformed encoded word \"=?a?=\"\nf:1:9: EW006 invalid quoted-printable text\n2 problems in 1 file\n", carriageReturnOutput.Out);
        Assert.Equal(1, carriageReturnOutput.Code);
    }

    [Theory]
    [InlineData("x=?utf-8?Q?a?= (=?utf-8?Q?b?=) =?utf-8?Q?a", "ok, 0 encoded words in 1 file\n", 0)]
    [InlineData("=?utf-8?B?AA?=", "f:1:1: EW005 base64 text is missing its padding\nf:1:1: EW009 decoded text has the control character U+0000\n2 problems in 1 file\n", 1)]
    [InlineData("=?utf-8?B?AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA?=", "f:1:1: EW005 base64 text is missing its padding\nf:1:1: EW008 encoded word has 78 characters, the limit is 75\nf:1:1: EW009 decoded text has the control character U+0000\n3 problems in 1 file\n", 1)]
    [InlineData("=?utf-8?Q?aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa?=", "ok, 1 encoded word in 1 file\n", 0)]
    [InlineData("=?utf-8?Q?aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa?=", "f:1:1: EW008 encoded word has 76 characters, the limit is 75\n1 problem in 1 file\n", 1)]
    public void ReportsCodesInReaderOrderAndChecksThe75CharacterBoundary(string text, string expected, int expectedCode)
    {
        var output = Checker.Check(new List<Inputs.Source> { new("f", text) });

        Assert.Equal(expected, output.Out);
        Assert.Equal(expectedCode, output.Code);
    }

    [Fact]
    public void SummariesPluralizeEmptyAndSingleSourceReports()
    {
        var noSources = Checker.Check(new List<Inputs.Source>());
        var emptySource = Checker.Check(new List<Inputs.Source> { new("f", "") });
        var emptyWord = Checker.Check(new List<Inputs.Source> { new("f", "=?utf-8?Q??=") });
        var oneWord = Checker.Check(new List<Inputs.Source> { new("f", "=?utf-8?Q?a?=") });
        var twoWords = Checker.Check(new List<Inputs.Source> { new("f", "=?utf-8?Q?a?= =?utf-8?Q?b?=") });

        Assert.Equal("ok, 0 encoded words in 0 files\n", noSources.Out);
        Assert.Equal(0, noSources.Code);
        Assert.Equal("ok, 0 encoded words in 1 file\n", emptySource.Out);
        Assert.Equal(0, emptySource.Code);
        Assert.Equal("ok, 1 encoded word in 1 file\n", emptyWord.Out);
        Assert.Equal(0, emptyWord.Code);
        Assert.Equal("ok, 1 encoded word in 1 file\n", oneWord.Out);
        Assert.Equal(0, oneWord.Code);
        Assert.Equal("ok, 2 encoded words in 1 file\n", twoWords.Out);
        Assert.Equal(0, twoWords.Code);
    }

    [Theory]
    [InlineData("a", "=?utf-8?Q?a?= =?utf-8?Q?b?=", "b", "=?utf-8?Q?c?= =?utf-8?Q?d?=", "ok, 4 encoded words in 2 files\n", 0)]
    [InlineData("a", "plain", "b", "=?utf-8?B?SGk?=", "b:1:1: EW005 base64 text is missing its padding\n1 problem in 1 file\n", 1)]
    [InlineData("same", "=?a?=", "same", "=?utf-8?X?a?=", "same:1:1: EW001 malformed encoded word \"=?a?=\"\nsame:1:1: EW003 unknown encoding \"X\"\n2 problems in 2 files\n", 1)]
    [InlineData("z", "=?utf-8?Q?=4?=", "a", "=?koi8-r?Q?x?=", "z:1:1: EW006 invalid quoted-printable text\na:1:1: EW002 unknown charset \"koi8-r\"\n2 problems in 2 files\n", 1)]
    [InlineData("same", "=?a?=", "same", "plain", "same:1:1: EW001 malformed encoded word \"=?a?=\"\n1 problem in 1 file\n", 1)]
    [InlineData("f", "=?a?= =?utf-8?X?a?=", "g", "", "f:1:1: EW001 malformed encoded word \"=?a?=\"\nf:1:7: EW003 unknown encoding \"X\"\n2 problems in 1 file\n", 1)]
    [InlineData("a", "plain", "b", "still plain", "ok, 0 encoded words in 2 files\n", 0)]
    [InlineData("left", "=?utf-8?Q?a?= =?utf-8?Q?b?=", "right", "=?utf-8?Q?c?=", "ok, 3 encoded words in 2 files\n", 0)]
    [InlineData("z", "=?utf-8?B?AA?=", "a", "=?a?=", "z:1:1: EW005 base64 text is missing its padding\nz:1:1: EW009 decoded text has the control character U+0000\na:1:1: EW001 malformed encoded word \"=?a?=\"\n3 problems in 2 files\n", 1)]
    public void SummariesPreserveSourceOrderAndCountProblematicEntries(string firstDisplay, string firstText, string secondDisplay, string secondText, string expected, int expectedCode)
    {
        var output = Checker.Check(new List<Inputs.Source>
        {
            new(firstDisplay, firstText),
            new(secondDisplay, secondText),
        });

        Assert.Equal(expected, output.Out);
        Assert.Equal(expectedCode, output.Code);
    }
}
