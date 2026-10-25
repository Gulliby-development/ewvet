namespace Ewvet.Tests;

public sealed class TextTests
{
    [Fact]
    public void ContractsPreserveTheirSuppliedValues()
    {
        var failure = new Failure("sample");
        var output = new Output("x\n", 1);
        var source = new Inputs.Source("f", "x");

        Assert.Equal("sample", failure.Message);
        Assert.Equal("x\n", output.Out);
        Assert.Equal(1, output.Code);
        Assert.Equal("f", source.Display);
        Assert.Equal("x", source.Text);
    }

    [Fact]
    public void CpsEnumeratesUnicodeScalarsAndPreservesControlAndBomCodePoints()
    {
        Assert.Equal(Array.Empty<int>(), Text.Cps(""));
        Assert.Equal(new[] { 65, 233, 128512, 13, 10, 0, 65279 }, Text.Cps("Aé😀\r\n\0\uFEFF"));
        Assert.Equal(new[] { 233 }, Text.Cps("é"));
        Assert.Equal(new[] { 128512 }, Text.Cps("😀"));
    }

    [Fact]
    public void StrUsesHalfOpenCodePointRanges()
    {
        Assert.Equal("é😀", Text.Str(new[] { 65, 233, 128512, 66 }, 1, 3));
        Assert.Equal("", Text.Str(new[] { 65 }, 0, 0));
        Assert.Equal("\r\n\0\uFEFF", Text.Str(new[] { 13, 10, 0, 65279 }, 0, 4));
        Assert.Equal("😀", Text.Str(new[] { 65, 128512, 66 }, 1, 2));
    }

    [Fact]
    public void ClassificationUsesOnlyTheSpecifiedAsciiCodePoints()
    {
        Assert.True(Text.IsBlank(32));
        Assert.True(Text.IsBlank(9));
        Assert.True(Text.IsBlank(13));
        Assert.True(Text.IsBlank(10));
        Assert.False(Text.IsBlank(160));
        Assert.False(Text.IsBlank(133));
        Assert.False(Text.IsBlank(12288));
        Assert.False(Text.IsBlank(0));

        Assert.True(Text.IsDigit(48));
        Assert.True(Text.IsDigit(57));
        Assert.False(Text.IsDigit(47));
        Assert.False(Text.IsDigit(58));
        Assert.False(Text.IsDigit(1635));

        Assert.True(Text.IsHex(48));
        Assert.True(Text.IsHex(57));
        Assert.True(Text.IsHex(65));
        Assert.True(Text.IsHex(70));
        Assert.True(Text.IsHex(97));
        Assert.True(Text.IsHex(102));
        Assert.False(Text.IsHex(47));
        Assert.False(Text.IsHex(58));
        Assert.False(Text.IsHex(64));
        Assert.False(Text.IsHex(71));
        Assert.False(Text.IsHex(96));
        Assert.False(Text.IsHex(103));
        Assert.False(Text.IsHex(65313));

        Assert.True(Text.IsAlnum(65));
        Assert.True(Text.IsAlnum(90));
        Assert.True(Text.IsAlnum(97));
        Assert.True(Text.IsAlnum(122));
        Assert.True(Text.IsAlnum(48));
        Assert.True(Text.IsAlnum(57));
        Assert.False(Text.IsAlnum(233));
        Assert.False(Text.IsAlnum(1635));
        Assert.False(Text.IsAlnum(65313));
        Assert.False(Text.IsAlnum(95));
    }

    [Fact]
    public void FoldChangesOnlyAsciiCapitalLetters()
    {
        Assert.Equal("azaz09ÉＡ😀", Text.Fold("AZaz09ÉＡ😀"));
        Assert.Equal("", Text.Fold(""));
        Assert.Equal("é", Text.Fold("é"));
    }

    [Fact]
    public void PlainAndShowApplyTheirDistinctEchoRules()
    {
        Assert.Equal("??????", Text.Plain("é😀\n\0\u007F\uFEFF"));
        Assert.Equal(" ~\"", Text.Plain(" ~\""));
        Assert.Equal("\"a\"b\"", Text.Show("a\"b"));
        Assert.Equal("\"\"", Text.Show(""));
        Assert.Equal("\"??\"", Text.Show("é😀"));
    }

    [Fact]
    public void ClipUsesCodePointBoundariesAtTwentyFourAndTwentyFive()
    {
        Assert.Equal("abcdefghijklmnopqrstuvwx", Text.Clip("abcdefghijklmnopqrstuvwx"));
        Assert.Equal("abcdefghijklmnopqrstu...", Text.Clip("abcdefghijklmnopqrstuvwxy"));
        Assert.Equal("abcdefghijklmnopqrst😀...", Text.Clip("abcdefghijklmnopqrst😀uvwx"));
        Assert.Equal("\"abcdefghijklmnopqrst?...\"", Text.Show(Text.Clip("abcdefghijklmnopqrst😀uvwx")));
    }

    [Fact]
    public void PluralUsesInvariantCountsAndSingularOnlyForOne()
    {
        Assert.Equal("0 files", Text.Plural(0, "file"));
        Assert.Equal("1 file", Text.Plural(1, "file"));
        Assert.Equal("2 files", Text.Plural(2, "file"));
        Assert.Equal("-1 files", Text.Plural(-1, "file"));
    }

    [Fact]
    public void HexUsesInvariantUppercaseWithAtLeastFourDigits()
    {
        Assert.Equal("U+0026", Text.Hex(38));
        Assert.Equal("U+1F600", Text.Hex(128512));
        Assert.Equal("U+10FFFF", Text.Hex(1114111));
        Assert.Equal("U+0000", Text.Hex(0));
    }

    [Fact]
    public void ParseCollectsOperandsAndDeduplicatesFlags()
    {
        var empty = Options.Parse(Array.Empty<string>(), new HashSet<string> { "--strict" }, new HashSet<string> { "--charset", "--encoding" });
        Assert.Empty(empty.Flags);
        Assert.Empty(empty.Values);
        Assert.Empty(empty.Args);

        var parsed = Options.Parse(
            new[] { "f", "--strict", "-", "-5", "--strict" },
            new HashSet<string> { "--strict" },
            new HashSet<string> { "--charset", "--encoding" });

        Assert.Equal(new[] { "--strict" }, parsed.Flags);
        Assert.Empty(parsed.Values);
        Assert.Equal(new[] { "f", "-", "-5" }, parsed.Args);
    }

    [Fact]
    public void ParseRecognizesValuedOptionsBeforeFlagsAndKeepsTheLastValue()
    {
        var emptyValue = Options.Parse(
            new[] { "--charset=" },
            new HashSet<string> { "--strict" },
            new HashSet<string> { "--charset", "--encoding" });
        Assert.Empty(emptyValue.Flags);
        Assert.Equal(new Dictionary<string, string> { ["--charset"] = "" }, emptyValue.Values);
        Assert.Empty(emptyValue.Args);

        var firstEquals = Options.Parse(
            new[] { "--charset=a=b" },
            new HashSet<string> { "--strict" },
            new HashSet<string> { "--charset", "--encoding" });
        Assert.Equal("a=b", firstEquals.Values["--charset"]);

        var consumesOptionLookingArgument = Options.Parse(
            new[] { "--charset", "--strict" },
            new HashSet<string> { "--strict" },
            new HashSet<string> { "--charset", "--encoding" });
        Assert.Empty(consumesOptionLookingArgument.Flags);
        Assert.Equal("--strict", consumesOptionLookingArgument.Values["--charset"]);

        var repeated = Options.Parse(
            new[] { "--charset=a", "--charset", "b" },
            new HashSet<string> { "--strict" },
            new HashSet<string> { "--charset", "--encoding" });
        Assert.Equal(new Dictionary<string, string> { ["--charset"] = "b" }, repeated.Values);

        var consumesNegativeOperand = Options.Parse(
            new[] { "--encoding", "-5" },
            new HashSet<string> { "--strict" },
            new HashSet<string> { "--charset", "--encoding" });
        Assert.Equal("-5", consumesNegativeOperand.Values["--encoding"]);
        Assert.Empty(consumesNegativeOperand.Args);
    }

    [Theory]
    [InlineData(new string[] { "--charset" }, "--charset needs a value")]
    [InlineData(new string[] { "--" }, "unknown option \"--\"")]
    [InlineData(new string[] { "--help" }, "unknown option \"--help\"")]
    [InlineData(new string[] { "--strict=1" }, "unknown option \"--strict=1\"")]
    [InlineData(new string[] { "--é😀" }, "unknown option \"--??\"")]
    [InlineData(new string[] { "--bad", "--charset" }, "unknown option \"--bad\"")]
    [InlineData(new string[] { "--STRICT" }, "unknown option \"--STRICT\"")]
    public void ParseRejectsUnknownOptionsAndReportsTheFirstFailure(string[] argv, string expected)
    {
        var error = Assert.Throws<Failure>(() => Options.Parse(
            argv,
            new HashSet<string> { "--strict" },
            new HashSet<string> { "--charset", "--encoding" }));

        Assert.Equal(expected, error.Message);
    }
}
