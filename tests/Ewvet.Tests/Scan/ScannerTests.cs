namespace Ewvet.Tests;

public sealed class ScannerTests
{
    [Theory]
    [InlineData("")]
    [InlineData("hello")]
    [InlineData("x=?utf-8?Q?a?=")]
    [InlineData("=?=")]
    [InlineData("=?utf-8?Q?a?=b")]
    public void ScanReturnsNoOccurrencesWhenNoBoundedAttemptStarts(string input)
    {
        Assert.Empty(Scanner.Scan(Text.Cps(input)));
    }

    [Theory]
    [InlineData("=?utf-8?Q?a?=", 0, 13, "a", "", "", "utf-8", 'Q', null)]
    [InlineData("=??=", 0, 4, null, "EW001", "=??=", null, '\0', null)]
    [InlineData("=?utf-8?Q??=", 0, 12, "", "", "", "utf-8", 'Q', null)]
    [InlineData("😀 =?utf-8?Q?a?=", 2, 15, "a", "", "", "utf-8", 'Q', null)]
    [InlineData("=?utf-8?Q?a?==?utf-8?Q?b?=", 0, 26, null, "EW001", "=?utf-8?Q?a?==?utf-8?Q?b?=", null, '\0', null)]
    public void ScanPreservesOrderedOccurrenceAndReadingFields(
        string input,
        int expectedStart,
        int expectedEnd,
        string? expectedResult,
        string expectedCodes,
        string expectedDetails,
        string? expectedCharset,
        char expectedEncoding,
        string? expectedLanguage)
    {
        var occurrences = Scanner.Scan(Text.Cps(input));

        var occurrence = Assert.Single(occurrences);
        Assert.Equal(expectedStart, occurrence.Start);
        Assert.Equal(0, occurrence.Gap);
        Assert.Equal(expectedEnd, occurrence.Reading.End);
        Assert.Equal(expectedResult, occurrence.Reading.Result is null
            ? null
            : Text.Str(occurrence.Reading.Result, 0, occurrence.Reading.Result.Length));
        Assert.Equal(expectedCodes, string.Join(",", occurrence.Reading.Codes));
        Assert.Equal(expectedDetails, string.Join(",", occurrence.Reading.Details));
        Assert.Equal(expectedCharset, occurrence.Reading.Charset);
        Assert.Equal(expectedEncoding, occurrence.Reading.Enc);
        Assert.Equal(expectedLanguage, occurrence.Reading.Lang);
    }

    [Fact]
    public void ScanPreservesLanguageAndDecodedWarnings()
    {
        var language = Assert.Single(Scanner.Scan(Text.Cps("=?utf-8*en-US?Q?a?="))).Reading;
        Assert.Equal("en-US", language.Lang);
        Assert.Equal("utf-8", language.Charset);
        Assert.Equal('Q', language.Enc);
        Assert.Equal(new[] { 97 }, language.Result);

        var warning = Assert.Single(Scanner.Scan(Text.Cps("=?utf-8?B?SGk?="))).Reading;
        Assert.Equal(new[] { 72, 105 }, warning.Result);
        Assert.Equal(new[] { "EW005" }, warning.Codes);
        Assert.Equal(new[] { "" }, warning.Details);
        Assert.Equal("utf-8", warning.Charset);
        Assert.Equal('B', warning.Enc);
    }

    [Theory]
    [InlineData("=?utf-8?Q?a?= =?utf-8?Q?b?=", "0,1")]
    [InlineData("=?utf-8?Q?a?=  \t =?utf-8?Q?b?=", "0,4")]
    [InlineData("=?utf-8?Q?a?=\r=?utf-8?Q?b?=", "0,1")]
    [InlineData("=?utf-8?Q?a?=\n=?utf-8?Q?b?=", "0,0")]
    [InlineData("=?utf-8?Q?a?=\r\n =?utf-8?Q?b?=", "0,0")]
    [InlineData("=?utf-8?Q?a?= x =?utf-8?Q?b?=", "0,0")]
    [InlineData("=?utf-8?Q?a?= =?x?Q?b?= =?utf-8?Q?c?=", "0,0,0")]
    [InlineData("=?utf-8?Q??= =?utf-8?Q?b?=", "0,1")]
    [InlineData("=?utf-8?Q?a?= =?utf-8?Q??=", "0,1")]
    [InlineData("=?utf-8?B?SGk?= =?utf-8?Q?=00?=", "0,1")]
    [InlineData("=?utf-8?Q?a?=\u00a0=?utf-8?Q?b?=", "0")]
    public void ScanComputesGapsOnlyBetweenImmediatelyAdjacentDecodedWords(string input, string expectedGaps)
    {
        var occurrences = Scanner.Scan(Text.Cps(input));

        Assert.Equal(expectedGaps, string.Join(",", occurrences.Select(occurrence => occurrence.Gap)));
    }

    [Theory]
    [InlineData("", 0, 1, 1)]
    [InlineData("é😀x", 2, 1, 3)]
    [InlineData("a\r\nb", 1, 1, 2)]
    [InlineData("a\r\nb", 2, 1, 3)]
    [InlineData("a\r\nb", 3, 2, 1)]
    [InlineData("a\rb", 2, 1, 3)]
    [InlineData("\n\n", 2, 3, 1)]
    [InlineData("\t\u0085\u2028", 3, 1, 4)]
    [InlineData("abc", 3, 1, 4)]
    public void LinesCountsCodePointsAndUsesOnlyLfBoundaries(string input, int index, int expectedLine, int expectedColumn)
    {
        var lines = new Lines(Text.Cps(input));

        Assert.Equal(expectedLine, lines.Line(index));
        Assert.Equal(expectedColumn, lines.Col(index));
    }

    [Theory]
    [InlineData("(\"a\\r\\nb\",1)", 1, 2)]
    [InlineData("(\"a\\r\\nb\",2)", 1, 3)]
    [InlineData("(\"a\\r\\nb\",3)", 2, 1)]
    [InlineData("(\"\\n\\n\",2)", 3, 1)]
    public void LinesMatchesExactEscapedLocationRows(string caseRow, int expectedLine, int expectedColumn)
    {
        var values = caseRow[1..^1].Split(',');
        var input = values[0][1..^1]
            .Replace("\\r", "\r", StringComparison.Ordinal)
            .Replace("\\n", "\n", StringComparison.Ordinal);
        var index = int.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture);
        var lines = new Lines(Text.Cps(input));

        Assert.Equal(expectedLine, lines.Line(index));
        Assert.Equal(expectedColumn, lines.Col(index));
    }

    [Theory]
    [InlineData("a", -1)]
    [InlineData("a", 2)]
    public void LinesRejectsOffsetsOutsideTheText(string input, int index)
    {
        var lines = new Lines(Text.Cps(input));

        Assert.Throws<ArgumentOutOfRangeException>(() => lines.Line(index));
        Assert.Throws<ArgumentOutOfRangeException>(() => lines.Col(index));
    }

    [Fact]
    public void GridRendersNoRowsAndAZeroCellRow()
    {
        Assert.Equal("", Grid.Render(new List<List<string>>()));
        Assert.Equal("\n", Grid.Render(new List<List<string>> { new() }));
    }

    [Theory]
    [InlineData("A", "A\n")]
    [InlineData("NAME|N;x|12", "NAME  N\nx     12\n")]
    [InlineData("A|B;long|x", "A     B\nlong  x\n")]
    [InlineData("😀|x;ab|y", "😀   x\nab  y\n")]
    [InlineData("a|b|c;aa|bb|cc", "a   b   c\naa  bb  cc\n")]
    [InlineData("a|", "a\n")]
    [InlineData("x  ", "x\n")]
    public void GridPadsColumnsAndRendersExactLfTerminatedRows(string literalRows, string expected)
    {
        var rows = literalRows
            .Split(';')
            .Select(row => row.Split('|', StringSplitOptions.None).ToList())
            .ToList();

        Assert.Equal(expected, Grid.Render(rows));
    }
}
