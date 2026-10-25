namespace Ewvet.Tests.Read;

public sealed class ReaderShapeTests
{
    [Fact]
    public void UnsafeIndicesAndOutOfRangeCallsReturnNull()
    {
        Assert.Null(Reader.Read(Text.Cps(string.Empty), 0));

        var text = Text.Cps("=?utf-8?Q?a?=");
        Assert.Null(Reader.Read(text, int.MinValue));
        Assert.Null(Reader.Read(text, -1));
        Assert.Null(Reader.Read(text, 13));
        Assert.Null(Reader.Read(text, text.Length));
        Assert.Null(Reader.Read(text, int.MaxValue));

        var equals = Text.Cps("=");
        Assert.Null(Reader.Read(equals, 0));
        Assert.Null(Reader.Read(equals, 1));
        Assert.Null(Reader.Read(equals, int.MaxValue));
    }

    [Theory]
    [InlineData("x=?utf-8?Q?a?=", 1)]
    [InlineData("(=?utf-8?Q?a?=)", 1)]
    [InlineData("\u00A0=?utf-8?Q?a?=", 1)]
    [InlineData("\u3000=?utf-8?Q?a?=", 1)]
    [InlineData("\uFEFF=?utf-8?Q?a?=", 1)]
    [InlineData("\u0085=?utf-8?Q?a?=", 1)]
    [InlineData("=?utf-8?Q?a?=b", 0)]
    [InlineData("=?utf-8?Q?a", 0)]
    [InlineData("=?=", 0)]
    [InlineData("=?utf-8?Q?a b?=", 0)]
    [InlineData("abc", 0)]
    [InlineData("=?utf-8?Q?a?=", 14)]
    public void NonblankPrefixesAndIncompleteTokensAreNotAttempts(string input, int at)
    {
        Assert.Null(Reader.Read(Text.Cps(input), at));
    }

    [Theory]
    [InlineData(" =?utf-8?Q?a?=", 1, 14)]
    [InlineData("\t=?utf-8?Q?a?=", 1, 14)]
    [InlineData("\r=?utf-8?Q?a?=", 1, 14)]
    [InlineData("\n=?utf-8?Q?a?=", 1, 14)]
    [InlineData("😀 =?utf-8?Q?a?=", 2, 15)]
    public void ASCIIBlanksPermitAttemptsAtCodePointOffsets(string input, int at, int expectedEnd)
    {
        var reading = Reader.Read(Text.Cps(input), at);

        Assert.NotNull(reading);
        Assert.Equal(expectedEnd, reading.End);
        Assert.Equal(new[] { 97 }, reading.Result);
        Assert.Empty(reading.Codes);
        Assert.Empty(reading.Details);
        Assert.Equal("utf-8", reading.Charset);
        Assert.Equal('Q', reading.Enc);
        Assert.Null(reading.Lang);
    }

    [Theory]
    [InlineData("=??=", 4)]
    [InlineData("=?a?=", 5)]
    [InlineData("=?a?b?=", 7)]
    [InlineData("=?a?b?c?d?=", 11)]
    [InlineData("=?utf-8?Q?a?b?=", 15)]
    [InlineData("=?utf-8*?Q?a?=", 14)]
    [InlineData("=?*en?Q?a?=", 11)]
    [InlineData("=?utf-8*e*n?Q?a?=", 17)]
    [InlineData("=?utf-8**en?Q?a?=", 17)]
    [InlineData("=?ut(f?Q?a?=", 12)]
    [InlineData("=?é?Q?a?=", 9)]
    [InlineData("=?utf-8*é?Q?a?=", 15)]
    public void MalformedBoundedShapesAreKeptWithTheirWholeToken(string input, int expectedEnd)
    {
        var reading = Reader.Read(Text.Cps(input), 0);

        Assert.NotNull(reading);
        Assert.Equal(expectedEnd, reading.End);
        Assert.Null(reading.Result);
        Assert.Equal(new[] { "EW001" }, reading.Codes);
        Assert.Equal(new[] { input }, reading.Details);
        Assert.Null(reading.Charset);
        Assert.Equal('\0', reading.Enc);
        Assert.Null(reading.Lang);
    }

    [Theory]
    [InlineData("=?ut(f?X?=G1?=", 14, "EW001", "=?ut(f?X?=G1?=")]
    [InlineData("=?koi8-r*en?X?=G1?=", 19, "EW002", "koi8-r")]
    [InlineData("=?utf-8*en?X?=G1?=", 18, "EW003", "X")]
    [InlineData("=?koi8-r?Q?a?=", 14, "EW002", "koi8-r")]
    [InlineData("=?utf_8?Q?a?=", 13, "EW002", "utf_8")]
    [InlineData("=?utf-16?Q?a?=", 14, "EW002", "utf-16")]
    [InlineData("=?a.b:c+d?Q?a?=", 15, "EW002", "a.b:c+d")]
    public void FailuresFollowSyntaxCharsetThenEncodingPrecedence(
        string input,
        int expectedEnd,
        string expectedCode,
        string expectedDetail)
    {
        var reading = Reader.Read(Text.Cps(input), 0);

        Assert.NotNull(reading);
        Assert.Equal(expectedEnd, reading.End);
        Assert.Null(reading.Result);
        Assert.Equal(new[] { expectedCode }, reading.Codes);
        Assert.Equal(new[] { expectedDetail }, reading.Details);
        Assert.Null(reading.Charset);
        Assert.Equal('\0', reading.Enc);
        Assert.Null(reading.Lang);
    }

    [Theory]
    [InlineData("=?UTF-8?Q?a?=", 13, 97, "utf-8", null)]
    [InlineData("=?ASCII?Q?a?=", 13, 97, "us-ascii", null)]
    [InlineData("=?Latin1?Q?=E9?=", 16, 233, "iso-8859-1", null)]
    [InlineData("=?L9?Q?=A4?=", 12, 8364, "iso-8859-15", null)]
    [InlineData("=?utf-8*en?Q?a?=", 16, 97, "utf-8", "en")]
    [InlineData("=?utf-8*en-US?Q?a?=", 19, 97, "utf-8", "en-US")]
    public void CharsetAliasesAndLanguageTagsProduceCanonicalMetadata(
        string input,
        int expectedEnd,
        int expectedCodePoint,
        string expectedCharset,
        string? expectedLanguage)
    {
        var reading = Reader.Read(Text.Cps(input), 0);

        Assert.NotNull(reading);
        Assert.Equal(expectedEnd, reading.End);
        Assert.Equal(new[] { expectedCodePoint }, reading.Result);
        Assert.Empty(reading.Codes);
        Assert.Empty(reading.Details);
        Assert.Equal(expectedCharset, reading.Charset);
        Assert.Equal('Q', reading.Enc);
        Assert.Equal(expectedLanguage, reading.Lang);
    }

    [Theory]
    [InlineData("=?utf-8?b?QQ==?=", 16, 65, 'B')]
    [InlineData("=?utf-8?q?a?=", 13, 97, 'Q')]
    public void EncodingLettersAreAcceptedCaseInsensitivelyAndNormalized(
        string input,
        int expectedEnd,
        int expectedCodePoint,
        char expectedEncoding)
    {
        var reading = Reader.Read(Text.Cps(input), 0);

        Assert.NotNull(reading);
        Assert.Equal(expectedEnd, reading.End);
        Assert.Equal(new[] { expectedCodePoint }, reading.Result);
        Assert.Empty(reading.Codes);
        Assert.Empty(reading.Details);
        Assert.Equal("utf-8", reading.Charset);
        Assert.Equal(expectedEncoding, reading.Enc);
        Assert.Null(reading.Lang);
    }

    [Theory]
    [InlineData("=?utf-8?X?a?=", 13, "X")]
    [InlineData("=?utf-8??a?=", 12, "")]
    [InlineData("=?utf-8?BQ?a?=", 14, "BQ")]
    [InlineData("=?utf-8?\u00A0?a?=", 13, "\u00A0")]
    public void InvalidEncodingKeepsTheOriginalPartAndClearsMetadata(
        string input,
        int expectedEnd,
        string expectedDetail)
    {
        var reading = Reader.Read(Text.Cps(input), 0);

        Assert.NotNull(reading);
        Assert.Equal(expectedEnd, reading.End);
        Assert.Null(reading.Result);
        Assert.Equal(new[] { "EW003" }, reading.Codes);
        Assert.Equal(new[] { expectedDetail }, reading.Details);
        Assert.Null(reading.Charset);
        Assert.Equal('\0', reading.Enc);
        Assert.Null(reading.Lang);
    }

    [Theory]
    [InlineData("=?utf-8?Q?a?= =?x?Q?b?=", 13, 97, "utf-8", 'Q')]
    [InlineData("=?utf-8?Q?a?==?utf-8?Q?b?=", 26, -1, null, '\0')]
    public void AttemptsConsumeExactlyOneBoundedToken(
        string input,
        int expectedEnd,
        int expectedCodePoint,
        string? expectedCharset,
        char expectedEncoding)
    {
        var reading = Reader.Read(Text.Cps(input), 0);

        Assert.NotNull(reading);
        Assert.Equal(expectedEnd, reading.End);
        Assert.Equal(expectedCharset, reading.Charset);
        Assert.Equal(expectedEncoding, reading.Enc);
        if (expectedCodePoint >= 0)
        {
            Assert.Equal(new[] { expectedCodePoint }, reading.Result);
            Assert.Empty(reading.Codes);
            Assert.Empty(reading.Details);
        }
        else
        {
            Assert.Null(reading.Result);
            Assert.Equal(new[] { "EW001" }, reading.Codes);
            Assert.Equal(new[] { input }, reading.Details);
        }
    }

    [Fact]
    public void EmptyPayloadIsDecodedAndLaterReadsDoNotChangeEarlierDiagnostics()
    {
        var emptyQ = Reader.Read(Text.Cps("=?utf-8?Q??="), 0);
        Assert.NotNull(emptyQ);
        Assert.Equal(12, emptyQ.End);
        Assert.NotNull(emptyQ.Result);
        Assert.Empty(emptyQ.Result);
        Assert.Empty(emptyQ.Codes);
        Assert.Empty(emptyQ.Details);
        Assert.Equal("utf-8", emptyQ.Charset);
        Assert.Equal('Q', emptyQ.Enc);
        Assert.Null(emptyQ.Lang);

        var emptyB = Reader.Read(Text.Cps("=?utf-8?B??="), 0);
        Assert.NotNull(emptyB);
        Assert.Equal(12, emptyB.End);
        Assert.NotNull(emptyB.Result);
        Assert.Empty(emptyB.Result);
        Assert.Empty(emptyB.Codes);
        Assert.Empty(emptyB.Details);
        Assert.Equal("utf-8", emptyB.Charset);
        Assert.Equal('B', emptyB.Enc);
        Assert.Null(emptyB.Lang);

        var first = Reader.Read(Text.Cps("=??="), 0);
        Assert.NotNull(first);
        var second = Reader.Read(Text.Cps("=?utf-8?Q?a?="), 0);
        Assert.NotNull(second);
        Assert.Equal(new[] { "EW001" }, first.Codes);
        Assert.Equal(new[] { "=??=" }, first.Details);
        Assert.Equal(new[] { 97 }, second.Result);
    }
}
