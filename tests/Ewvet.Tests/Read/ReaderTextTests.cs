namespace Ewvet.Tests;

public sealed class ReaderTextTests
{
    [Theory]
    [InlineData("=?utf-8?B?SGk=?=", new int[] { 72, 105 }, 16, new string[] { }, new string[] { }, "utf-8", 'B', null)]
    [InlineData("=?utf-8?B?SGk?=", new int[] { 72, 105 }, 15, new string[] { "EW005" }, new string[] { "" }, "utf-8", 'B', null)]
    [InlineData("=?utf-8?B?SG?=", new int[] { 72 }, 14, new string[] { "EW005" }, new string[] { "" }, "utf-8", 'B', null)]
    [InlineData("=?utf-8?B?SG==?=", new int[] { 72 }, 16, new string[] { }, new string[] { }, "utf-8", 'B', null)]
    [InlineData("=?utf-8?B?QUJD?=", new int[] { 65, 66, 67 }, 16, new string[] { }, new string[] { }, "utf-8", 'B', null)]
    [InlineData("=?utf-8?B??=", new int[] { }, 12, new string[] { }, new string[] { }, "utf-8", 'B', null)]
    public void Base64PayloadsDecodeWithExactPaddingDiagnostics(
        string input,
        int[]? expectedResult,
        int expectedEnd,
        string[] expectedCodes,
        string[] expectedDetails,
        string? expectedCharset,
        char expectedEncoding,
        string? expectedLang)
    {
        var actual = Assert.IsType<Reading>(Reader.Read(Text.Cps(input), 0));

        Assert.Equal(expectedEnd, actual.End);
        Assert.Equal(expectedResult, actual.Result);
        Assert.Equal(expectedCodes, actual.Codes);
        Assert.Equal(expectedDetails, actual.Details);
        Assert.Equal(expectedCharset, actual.Charset);
        Assert.Equal(expectedEncoding, actual.Enc);
        Assert.Equal(expectedLang, actual.Lang);
    }

    [Theory]
    [InlineData("=?utf-8?B?S?=", 13)]
    [InlineData("=?utf-8?B?SGk==?=", 17)]
    [InlineData("=?utf-8?B?SG===?=", 17)]
    [InlineData("=?utf-8?B?SGk=x?=", 17)]
    [InlineData("=?utf-8?B?SGk-?=", 16)]
    [InlineData("=?utf-8?B?AA=A?=", 16)]
    public void InvalidBase64PayloadsAreKeptWithOneEmptyDetail(string input, int expectedEnd)
    {
        var actual = Assert.IsType<Reading>(Reader.Read(Text.Cps(input), 0));

        Assert.Equal(expectedEnd, actual.End);
        Assert.Null(actual.Result);
        Assert.Equal(new string[] { "EW004" }, actual.Codes);
        Assert.Equal(new string[] { "" }, actual.Details);
        Assert.Null(actual.Charset);
        Assert.Equal('\0', actual.Enc);
        Assert.Null(actual.Lang);
    }

    [Theory]
    [InlineData("=?utf-8?Q?a_b?=", new int[] { 97, 32, 98 }, 15)]
    [InlineData("=?utf-8?Q?caf=c3=A9?=", new int[] { 99, 97, 102, 233 }, 21)]
    [InlineData("=?utf-8?Q?=3D=3F=5F?=", new int[] { 61, 63, 95 }, 21)]
    [InlineData("=?utf-8?Q?(a)\"!?=", new int[] { 40, 97, 41, 34, 33 }, 17)]
    [InlineData("=?utf-8?Q??=", new int[] { }, 12)]
    [InlineData("=?utf-8?Q?A=4a?=", new int[] { 65, 74 }, 16)]
    public void QuotedPrintablePayloadsDecodeLiteralBytes(string input, int[] expectedResult, int expectedEnd)
    {
        var actual = Assert.IsType<Reading>(Reader.Read(Text.Cps(input), 0));

        Assert.Equal(expectedEnd, actual.End);
        Assert.Equal(expectedResult, actual.Result);
        Assert.Empty(actual.Codes);
        Assert.Empty(actual.Details);
        Assert.Equal("utf-8", actual.Charset);
        Assert.Equal('Q', actual.Enc);
        Assert.Null(actual.Lang);
    }

    [Theory]
    [InlineData("=?utf-8?Q?=4?=", 14)]
    [InlineData("=?utf-8?Q?=G1?=", 15)]
    [InlineData("=?utf-8?Q?a=?=", 14)]
    [InlineData("=?utf-8?Q?café?=", 16)]
    [InlineData("=?utf-8?Q?\u0000?=", 13)]
    [InlineData("=?utf-8?Q?\u007F?=", 13)]
    [InlineData("=?utf-8?Q?\u00A0?=", 13)]
    public void InvalidQuotedPrintablePayloadsAreKeptWithOneEmptyDetail(string input, int expectedEnd)
    {
        var actual = Assert.IsType<Reading>(Reader.Read(Text.Cps(input), 0));

        Assert.Equal(expectedEnd, actual.End);
        Assert.Null(actual.Result);
        Assert.Equal(new string[] { "EW006" }, actual.Codes);
        Assert.Equal(new string[] { "" }, actual.Details);
        Assert.Null(actual.Charset);
        Assert.Equal('\0', actual.Enc);
        Assert.Null(actual.Lang);
    }

    [Theory]
    [InlineData("=?UTF-8?Q?a?=", new int[] { 97 }, 13, new string[] { }, new string[] { }, "utf-8", 'Q')]
    [InlineData("=?ASCII?Q?a?=", new int[] { 97 }, 13, new string[] { }, new string[] { }, "us-ascii", 'Q')]
    [InlineData("=?Latin1?Q?=E9?=", new int[] { 233 }, 16, new string[] { }, new string[] { }, "iso-8859-1", 'Q')]
    [InlineData("=?L9?Q?=A4?=", new int[] { 8364 }, 12, new string[] { }, new string[] { }, "iso-8859-15", 'Q')]
    [InlineData("=?utf-8?Q?=C3=A9?=", new int[] { 233 }, 18, new string[] { }, new string[] { }, "utf-8", 'Q')]
    [InlineData("=?utf-8?Q?=F0=9F=98=80?=", new int[] { 128512 }, 24, new string[] { }, new string[] { }, "utf-8", 'Q')]
    [InlineData("=?iso-8859-1?Q?=E9=80?=", new int[] { 233, 128 }, 23, new string[] { }, new string[] { }, "iso-8859-1", 'Q')]
    [InlineData("=?latin9?Q?=A4=BE?=", new int[] { 8364, 376 }, 19, new string[] { }, new string[] { }, "iso-8859-15", 'Q')]
    [InlineData("=?utf-8?Q?=C3?=", null, 15, new string[] { "EW007" }, new string[] { "utf-8" }, null, '\0')]
    [InlineData("=?utf-8?Q?=ED=A0=80?=", null, 21, new string[] { "EW007" }, new string[] { "utf-8" }, null, '\0')]
    [InlineData("=?utf-8?Q?=C0=AF?=", null, 18, new string[] { "EW007" }, new string[] { "utf-8" }, null, '\0')]
    [InlineData("=?utf-8?Q?=F4=90=80=80?=", null, 24, new string[] { "EW007" }, new string[] { "utf-8" }, null, '\0')]
    [InlineData("=?us-ascii?Q?=E9?=", null, 18, new string[] { "EW007" }, new string[] { "us-ascii" }, null, '\0')]
    public void CharsetDecodingIsStrictAndUsesCanonicalNames(
        string input,
        int[]? expectedResult,
        int expectedEnd,
        string[] expectedCodes,
        string[] expectedDetails,
        string? expectedCharset,
        char expectedEncoding)
    {
        var actual = Assert.IsType<Reading>(Reader.Read(Text.Cps(input), 0));

        Assert.Equal(expectedEnd, actual.End);
        Assert.Equal(expectedResult, actual.Result);
        Assert.Equal(expectedCodes, actual.Codes);
        Assert.Equal(expectedDetails, actual.Details);
        Assert.Equal(expectedCharset, actual.Charset);
        Assert.Equal(expectedEncoding, actual.Enc);
        Assert.Null(actual.Lang);
    }

    [Theory]
    [InlineData("=?utf-8?Q?aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa?=", new int[] { 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97 }, 75, new string[] { }, new string[] { }, 'Q', null)]
    [InlineData("=?utf-8?Q?aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa?=", new int[] { 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97 }, 76, new string[] { "EW008" }, new string[] { "76" }, 'Q', null)]
    [InlineData("=?utf-8*en?Q?aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa?=", new int[] { 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97 }, 79, new string[] { "EW008" }, new string[] { "79" }, 'Q', "en")]
    [InlineData("=?utf-8?B?QUJDQUJDQUJDQUJDQUJDQUJDQUJDQUJDQUJDQUJDQUJDQUJDQUJDQUJDQUJDQUJDQUJD?=", new int[] { 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67 }, 80, new string[] { "EW008" }, new string[] { "80" }, 'B', null)]
    public void WholeTokenLengthUsesCodePointBoundariesAndASeventyFiveCharacterLimit(
        string input,
        int[] expectedResult,
        int expectedEnd,
        string[] expectedCodes,
        string[] expectedDetails,
        char expectedEncoding,
        string? expectedLang)
    {
        var actual = Assert.IsType<Reading>(Reader.Read(Text.Cps(input), 0));

        Assert.Equal(expectedEnd, actual.End);
        Assert.Equal(expectedResult, actual.Result);
        Assert.Equal(expectedCodes, actual.Codes);
        Assert.Equal(expectedDetails, actual.Details);
        Assert.Equal("utf-8", actual.Charset);
        Assert.Equal(expectedEncoding, actual.Enc);
        Assert.Equal(expectedLang, actual.Lang);
    }

    [Theory]
    [InlineData("=?utf-8?Q?=00?=", new int[] { 0 }, 15, new string[] { "EW009" }, new string[] { "U+0000" })]
    [InlineData("=?utf-8?Q?=09?=", new int[] { 9 }, 15, new string[] { "EW009" }, new string[] { "U+0009" })]
    [InlineData("=?utf-8?Q?a=1Fb=00?=", new int[] { 97, 31, 98, 0 }, 20, new string[] { "EW009" }, new string[] { "U+001F" })]
    [InlineData("=?utf-8?Q?=7F?=", new int[] { 127 }, 15, new string[] { "EW009" }, new string[] { "U+007F" })]
    [InlineData("=?utf-8?Q?=C2=80?=", new int[] { 128 }, 18, new string[] { }, new string[] { })]
    [InlineData("=?utf-8?Q?=20?=", new int[] { 32 }, 15, new string[] { }, new string[] { })]
    public void DecodedControlsReportOnlyTheFirstDisallowedCodePoint(
        string input,
        int[] expectedResult,
        int expectedEnd,
        string[] expectedCodes,
        string[] expectedDetails)
    {
        var actual = Assert.IsType<Reading>(Reader.Read(Text.Cps(input), 0));

        Assert.Equal(expectedEnd, actual.End);
        Assert.Equal(expectedResult, actual.Result);
        Assert.Equal(expectedCodes, actual.Codes);
        Assert.Equal(expectedDetails, actual.Details);
        Assert.Equal("utf-8", actual.Charset);
        Assert.Equal('Q', actual.Enc);
        Assert.Null(actual.Lang);
    }

    [Theory]
    [InlineData("=?ut(f?X?=G1?=", null, 14, new string[] { "EW001" }, new string[] { "=?ut(f?X?=G1?=" }, null, '\0', null)]
    [InlineData("=?koi8-r*en?X?=G1?=", null, 19, new string[] { "EW002" }, new string[] { "koi8-r" }, null, '\0', null)]
    [InlineData("=?utf-8*en?X?=G1?=", null, 18, new string[] { "EW003" }, new string[] { "X" }, null, '\0', null)]
    [InlineData("=?utf-8?B?/w?=", null, 14, new string[] { "EW007" }, new string[] { "utf-8" }, null, '\0', null)]
    [InlineData("=?utf-8?B?AA?=", new int[] { 0 }, 14, new string[] { "EW005", "EW009" }, new string[] { "", "U+0000" }, "utf-8", 'B', null)]
    [InlineData("=?utf-8?B?QUJDQUJDQUJDQUJDQUJDQUJDQUJDQUJDQUJDQUJDQUJDQUJDQUJDQUJDQUJDQUJDAA?=", new int[] { 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 65, 66, 67, 0 }, 78, new string[] { "EW005", "EW008", "EW009" }, new string[] { "", "78", "U+0000" }, "utf-8", 'B', null)]
    [InlineData("=?utf-8?Q?aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa=00?=", new int[] { 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 97, 0 }, 79, new string[] { "EW008", "EW009" }, new string[] { "79", "U+0000" }, "utf-8", 'Q', null)]
    [InlineData("=?utf-8?Q?aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa=G1?=", null, 79, new string[] { "EW006" }, new string[] { "" }, null, '\0', null)]
    public void EarlierFailuresAndOrderedWarningsControlTheFinalReading(
        string input,
        int[]? expectedResult,
        int expectedEnd,
        string[] expectedCodes,
        string[] expectedDetails,
        string? expectedCharset,
        char expectedEncoding,
        string? expectedLang)
    {
        var actual = Assert.IsType<Reading>(Reader.Read(Text.Cps(input), 0));

        Assert.Equal(expectedEnd, actual.End);
        Assert.Equal(expectedResult, actual.Result);
        Assert.Equal(expectedCodes, actual.Codes);
        Assert.Equal(expectedDetails, actual.Details);
        Assert.Equal(expectedCharset, actual.Charset);
        Assert.Equal(expectedEncoding, actual.Enc);
        Assert.Equal(expectedLang, actual.Lang);
    }
}
