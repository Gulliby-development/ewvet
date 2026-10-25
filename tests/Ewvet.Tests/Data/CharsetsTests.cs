namespace Ewvet.Tests;

public sealed class CharsetsTests
{
    [Fact]
    public void AllReturnsFreshListsWithExactOrderedEntries()
    {
        List<Charsets.Charset> first = Charsets.All();
        List<Charsets.Charset> second = Charsets.All();

        Assert.NotSame(first, second);
        Assert.Equal(4, first.Count);
        Assert.Equal("iso-8859-1", first[0].Name);
        Assert.Equal(new[] { "iso8859-1", "l1", "latin1" }, first[0].Aliases);
        Assert.Equal("1", first[0].Bytes);
        Assert.Equal("iso-8859-15", first[1].Name);
        Assert.Equal(new[] { "iso8859-15", "l9", "latin9" }, first[1].Aliases);
        Assert.Equal("1", first[1].Bytes);
        Assert.Equal("us-ascii", first[2].Name);
        Assert.Equal(new[] { "ascii", "us" }, first[2].Aliases);
        Assert.Equal("1", first[2].Bytes);
        Assert.Equal("utf-8", first[3].Name);
        Assert.Equal(new[] { "utf8" }, first[3].Aliases);
        Assert.Equal("1 to 4", first[3].Bytes);

        first.RemoveAt(0);
        Assert.Equal(4, second.Count);
        Assert.Equal("iso-8859-1", second[0].Name);
    }

    [Theory]
    [InlineData("iso-8859-1", "iso-8859-1")]
    [InlineData("iso8859-1", "iso-8859-1")]
    [InlineData("l1", "iso-8859-1")]
    [InlineData("latin1", "iso-8859-1")]
    [InlineData("Latin1", "iso-8859-1")]
    [InlineData("ISO-8859-1", "iso-8859-1")]
    [InlineData("iso-8859-15", "iso-8859-15")]
    [InlineData("iso8859-15", "iso-8859-15")]
    [InlineData("l9", "iso-8859-15")]
    [InlineData("latin9", "iso-8859-15")]
    [InlineData("L9", "iso-8859-15")]
    [InlineData("ISO-8859-15", "iso-8859-15")]
    [InlineData("us-ascii", "us-ascii")]
    [InlineData("ascii", "us-ascii")]
    [InlineData("us", "us-ascii")]
    [InlineData("ASCII", "us-ascii")]
    [InlineData("US-ASCII", "us-ascii")]
    [InlineData("utf-8", "utf-8")]
    [InlineData("utf8", "utf-8")]
    [InlineData("UTF-8", "utf-8")]
    [InlineData("", null)]
    [InlineData("utf_8", null)]
    [InlineData("utf-16", null)]
    [InlineData("koi8-r", null)]
    [InlineData(" utf-8", null)]
    [InlineData("utf-8 ", null)]
    [InlineData("utf", null)]
    [InlineData("ＵＴＦ-8", null)]
    public void FindUsesWholeAsciiFoldedNamesAndAliases(string name, string? expectedName)
    {
        Assert.Equal(expectedName, Charsets.Find(name)?.Name);
    }

    [Theory]
    [InlineData(new byte[] { }, new int[] { })]
    [InlineData(new byte[] { 0x00, 0x0D, 0x0A, 0x41, 0x7F }, new int[] { 0x0000, 0x000D, 0x000A, 0x0041, 0x007F })]
    [InlineData(new byte[] { 0xC2, 0x80 }, new int[] { 0x0080 })]
    [InlineData(new byte[] { 0xDF, 0xBF }, new int[] { 0x07FF })]
    [InlineData(new byte[] { 0xE0, 0xA0, 0x80 }, new int[] { 0x0800 })]
    [InlineData(new byte[] { 0xED, 0x9F, 0xBF }, new int[] { 0xD7FF })]
    [InlineData(new byte[] { 0xEE, 0x80, 0x80 }, new int[] { 0xE000 })]
    [InlineData(new byte[] { 0xEF, 0xBF, 0xBF }, new int[] { 0xFFFF })]
    [InlineData(new byte[] { 0xF0, 0x90, 0x80, 0x80 }, new int[] { 0x10000 })]
    [InlineData(new byte[] { 0xF4, 0x8F, 0xBF, 0xBF }, new int[] { 0x10FFFF })]
    [InlineData(new byte[] { 0xC3, 0xA9, 0xE2, 0x82, 0xAC, 0xF0, 0x9F, 0x98, 0x80 }, new int[] { 0x00E9, 0x20AC, 0x1F600 })]
    [InlineData(new byte[] { 0xEF, 0xBB, 0xBF, 0x00, 0x0D, 0x0A }, new int[] { 0xFEFF, 0x0000, 0x000D, 0x000A })]
    public void DecodesValidUtf8AsLiteralScalars(byte[] data, int[] expected)
    {
        Assert.Equal(expected, Charmap.Decode("utf-8", data));
    }

    [Theory]
    [InlineData(0x0000, new byte[] { 0x00 })]
    [InlineData(0x0041, new byte[] { 0x41 })]
    [InlineData(0x007F, new byte[] { 0x7F })]
    [InlineData(0x0080, new byte[] { 0xC2, 0x80 })]
    [InlineData(0x000D, new byte[] { 0x0D })]
    [InlineData(0x000A, new byte[] { 0x0A })]
    [InlineData(0xFEFF, new byte[] { 0xEF, 0xBB, 0xBF })]
    [InlineData(0x07FF, new byte[] { 0xDF, 0xBF })]
    [InlineData(0x0800, new byte[] { 0xE0, 0xA0, 0x80 })]
    [InlineData(0xD7FF, new byte[] { 0xED, 0x9F, 0xBF })]
    [InlineData(0xE000, new byte[] { 0xEE, 0x80, 0x80 })]
    [InlineData(0xFFFF, new byte[] { 0xEF, 0xBF, 0xBF })]
    [InlineData(0x10000, new byte[] { 0xF0, 0x90, 0x80, 0x80 })]
    [InlineData(0x10FFFF, new byte[] { 0xF4, 0x8F, 0xBF, 0xBF })]
    [InlineData(0x00E9, new byte[] { 0xC3, 0xA9 })]
    [InlineData(0x20AC, new byte[] { 0xE2, 0x82, 0xAC })]
    [InlineData(0x1F600, new byte[] { 0xF0, 0x9F, 0x98, 0x80 })]
    public void EncodesUtf8ScalarsAsLiteralBytes(int cp, byte[] expected)
    {
        Assert.Equal(expected, Charmap.Encode("utf-8", cp));
    }

    [Theory]
    [InlineData(new byte[] { 0x80 })]
    [InlineData(new byte[] { 0xC0, 0xAF })]
    [InlineData(new byte[] { 0xE0, 0x80, 0xAF })]
    [InlineData(new byte[] { 0xF0, 0x80, 0x80, 0xAF })]
    [InlineData(new byte[] { 0xED, 0xA0, 0x80 })]
    [InlineData(new byte[] { 0xF4, 0x90, 0x80, 0x80 })]
    [InlineData(new byte[] { 0xF5, 0x80, 0x80, 0x80 })]
    [InlineData(new byte[] { 0xC3 })]
    [InlineData(new byte[] { 0xE2, 0x82 })]
    [InlineData(new byte[] { 0xF0, 0x9F, 0x98 })]
    [InlineData(new byte[] { 0xC3, 0x28 })]
    [InlineData(new byte[] { 0xE2, 0x28, 0xA1 })]
    [InlineData(new byte[] { 0x61, 0xFF, 0x0A })]
    public void RejectsMalformedUtf8WithoutPartialResults(byte[] data)
    {
        Assert.Null(Charmap.Decode("utf-8", data));
    }

    [Theory]
    [InlineData("us-ascii", new byte[] { }, new int[] { })]
    [InlineData("us-ascii", new byte[] { 0x00, 0x41, 0x7F }, new int[] { 0x0000, 0x0041, 0x007F })]
    [InlineData("us-ascii", new byte[] { 0x80 }, null)]
    [InlineData("us-ascii", new byte[] { 0x41, 0xE9 }, null)]
    [InlineData("iso-8859-1", new byte[] { }, new int[] { })]
    [InlineData("iso-8859-1", new byte[] { 0x00, 0x7F, 0x80, 0xA4, 0xE9, 0xFF }, new int[] { 0x0000, 0x007F, 0x0080, 0x00A4, 0x00E9, 0x00FF })]
    public void DecodesAsciiAndLatin1BytesExactly(string charset, byte[] data, int[]? expected)
    {
        Assert.Equal(expected, Charmap.Decode(charset, data));
    }

    [Theory]
    [InlineData("us-ascii", 0x00, new byte[] { 0x00 })]
    [InlineData("us-ascii", 0x7F, new byte[] { 0x7F })]
    [InlineData("us-ascii", 0x80, null)]
    [InlineData("us-ascii", 0xE9, null)]
    [InlineData("iso-8859-1", 0x00, new byte[] { 0x00 })]
    [InlineData("iso-8859-1", 0xE9, new byte[] { 0xE9 })]
    [InlineData("iso-8859-1", 0xFF, new byte[] { 0xFF })]
    [InlineData("iso-8859-1", 0x100, null)]
    [InlineData("iso-8859-1", 0x20AC, null)]
    public void EncodesAsciiAndLatin1ScalarsExactly(string charset, int cp, byte[]? expected)
    {
        Assert.Equal(expected, Charmap.Encode(charset, cp));
    }

    [Theory]
    [InlineData(new byte[] { 0xA4, 0xA6, 0xA8, 0xB4, 0xB8, 0xBC, 0xBD, 0xBE }, new int[] { 0x20AC, 0x0160, 0x0161, 0x017D, 0x017E, 0x0152, 0x0153, 0x0178 })]
    [InlineData(new byte[] { 0x00, 0x80, 0xA5, 0xE9, 0xFF }, new int[] { 0x0000, 0x0080, 0x00A5, 0x00E9, 0x00FF })]
    public void DecodesLatin9ReplacementsAndUnaffectedBytes(byte[] data, int[] expected)
    {
        Assert.Equal(expected, Charmap.Decode("iso-8859-15", data));
    }

    [Theory]
    [InlineData(0x20AC, new byte[] { 0xA4 })]
    [InlineData(0x0160, new byte[] { 0xA6 })]
    [InlineData(0x0161, new byte[] { 0xA8 })]
    [InlineData(0x017D, new byte[] { 0xB4 })]
    [InlineData(0x017E, new byte[] { 0xB8 })]
    [InlineData(0x0152, new byte[] { 0xBC })]
    [InlineData(0x0153, new byte[] { 0xBD })]
    [InlineData(0x0178, new byte[] { 0xBE })]
    [InlineData(0x00A4, null)]
    [InlineData(0x00A6, null)]
    [InlineData(0x00A8, null)]
    [InlineData(0x00B4, null)]
    [InlineData(0x00B8, null)]
    [InlineData(0x00BC, null)]
    [InlineData(0x00BD, null)]
    [InlineData(0x00BE, null)]
    [InlineData(0x0000, new byte[] { 0x00 })]
    [InlineData(0x0080, new byte[] { 0x80 })]
    [InlineData(0x00A5, new byte[] { 0xA5 })]
    [InlineData(0x00E9, new byte[] { 0xE9 })]
    [InlineData(0x00FF, new byte[] { 0xFF })]
    [InlineData(0x0100, null)]
    [InlineData(0x1F600, null)]
    public void EncodesLatin9ReplacementsExclusionsAndUnaffectedScalars(int cp, byte[]? expected)
    {
        Assert.Equal(expected, Charmap.Encode("iso-8859-15", cp));
    }

    [Theory]
    [InlineData("utf-8", -1)]
    [InlineData("utf-8", 0xD800)]
    [InlineData("utf-8", 0xDFFF)]
    [InlineData("utf-8", 0x110000)]
    [InlineData("us-ascii", -1)]
    [InlineData("us-ascii", 0xD800)]
    [InlineData("us-ascii", 0xDFFF)]
    [InlineData("us-ascii", 0x110000)]
    [InlineData("iso-8859-1", -1)]
    [InlineData("iso-8859-1", 0xD800)]
    [InlineData("iso-8859-1", 0xDFFF)]
    [InlineData("iso-8859-1", 0x110000)]
    [InlineData("iso-8859-15", -1)]
    [InlineData("iso-8859-15", 0xD800)]
    [InlineData("iso-8859-15", 0xDFFF)]
    [InlineData("iso-8859-15", 0x110000)]
    public void RejectsInvalidScalarsForEveryCharset(string charset, int cp)
    {
        Assert.Null(Charmap.Encode(charset, cp));
    }

    [Theory]
    [InlineData("utf-8")]
    [InlineData("us-ascii")]
    [InlineData("iso-8859-1")]
    [InlineData("iso-8859-15")]
    public void DecodesEmptyDataForEveryCharset(string charset)
    {
        Assert.Equal(Array.Empty<int>(), Charmap.Decode(charset, Array.Empty<byte>()));
    }

    [Fact]
    public void RejectsUnsupportedCharmapCharsetArguments()
    {
        // Decode("koi8-r", [41]) and Encode("koi8-r", 65) both return null.
        Assert.Null(Charmap.Decode("koi8-r", [41]));
        Assert.Null(Charmap.Encode("koi8-r", 65));
        // Decode("utf8", [41]) and Encode("utf8", 65) both return null.
        Assert.Null(Charmap.Decode("utf8", [41]));
        Assert.Null(Charmap.Encode("utf8", 65));
    }
}
