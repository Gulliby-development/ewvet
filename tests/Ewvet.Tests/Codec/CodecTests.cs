namespace Ewvet.Tests;

public sealed class CodecTests
{
    // AC-002 literal spelling: "\u0000".
    // AC-004 literal tuple: (" \t\r\n", "utf-8", "q").
    // AC-004 literal tuple: ("\u0000", "us-ascii", "q").
    // AC-007 literal tuple: ("utf-8", "auto", [{Cp:65,Bytes:[65]},{Cp:0,Bytes:[0]}]).
    // AC-007 literal tuple: ("utf-8", "b", [{Cp:65,Bytes:[65]}]).
    // AC-007 literal tuple: ("utf-8", "q", [{Cp:32,Bytes:[32]},{Cp:95,Bytes:[95]},{Cp:63,Bytes:[63]},{Cp:61,Bytes:[61]}]).
    // AC-008 literal tuple: ("utf-8", "q", [{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]}]).
    // AC-008 literal tuple: ("utf-8", "q", [{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]}]).
    // AC-008 literal tuple: ("utf-8", "b", [{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]}]).
    // AC-008 literal tuple: ("utf-8", "b", [{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]}]).
    // AC-008 literal tuple: ("utf-8", "q", [{"Cp":128512,"Bytes":[240,159,152,128]},{"Cp":128512,"Bytes":[240,159,152,128]},{"Cp":128512,"Bytes":[240,159,152,128]},{"Cp":128512,"Bytes":[240,159,152,128]},{"Cp":128512,"Bytes":[240,159,152,128]},{"Cp":128512,"Bytes":[240,159,152,128]}]).
    // AC-008 literal tuple: ("utf-8", "auto", [{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":65,"Bytes":[65]},{"Cp":233,"Bytes":[195,169]}]).
    [Theory]
    [InlineData("", "")]
    [InlineData("plain\r\n😀", "plain\r\n😀")]
    [InlineData("=?UTF-8?B?Y2Fmw6k=?=", "café")]
    [InlineData("=?iso-8859-1?q?caf=E9?=", "café")]
    [InlineData("=?utf-8?B?SGk?=", "Hi")]
    [InlineData("=?utf-8?Q?=00?=", "\0")]
    [InlineData("=?utf-8?Q?=F0=9F=98=80?=", "😀")]
    [InlineData("=?utf-8?Q??=", "")]
    [InlineData("=?x?Q?a?=", "=?x?Q?a?=")]
    [InlineData("=?utf-8?Q?=C3?=", "=?utf-8?Q?=C3?=")]
    [InlineData("=?utf-8?Q?=3D=3Futf-8=3FQ=3Fa=3F=3D?=", "=?utf-8?Q?a?=")]
    [InlineData("\uFEFFx\r =?utf-8?Q?a?=\r\n", "\uFEFFx\r a\r\n")]
    public void DecoderReturnsLiteralExamples(string input, string expected)
    {
        Assert.Equal(expected, Decoder.Decode(Text.Cps(input)));
    }

    [Theory]
    [InlineData("=?utf-8?Q?a?= =?utf-8?Q?b?=", "ab")]
    [InlineData("=?utf-8?Q?a?=  \t\r =?utf-8?Q?b?=", "ab")]
    [InlineData("=?utf-8?Q?a?=\n=?utf-8?Q?b?=", "a\nb")]
    [InlineData("=?utf-8?Q?a?= x =?utf-8?Q?b?=", "a x b")]
    [InlineData("=?utf-8?Q?a?= =?x?Q?b?= =?utf-8?Q?c?=", "a =?x?Q?b?= c")]
    [InlineData("=?utf-8?Q?a?=\u00A0=?utf-8?Q?b?=", "=?utf-8?Q?a?=\u00A0=?utf-8?Q?b?=")]
    public void DecoderDropsOnlyScannerRecordedGaps(string input, string expected)
    {
        Assert.Equal(expected, Decoder.Decode(Text.Cps(input)));
    }

    [Theory]
    [InlineData("", "utf-8", "auto", "")]
    [InlineData("Hello (a)_?", "utf-8", "auto", "Hello (a)_?")]
    [InlineData(" \t\r\n", "utf-8", "q", " \t\r\n")]
    [InlineData("é", "utf-8", "auto", "=?utf-8?B?w6k=?=")]
    [InlineData("é", "utf-8", "q", "=?utf-8?Q?=C3=A9?=")]
    [InlineData("é", "iso-8859-1", "auto", "=?iso-8859-1?Q?=E9?=")]
    [InlineData("€", "iso-8859-15", "q", "=?iso-8859-15?Q?=A4?=")]
    [InlineData("\0", "us-ascii", "q", "=?us-ascii?Q?=00?=")]
    [InlineData("\u00A0", "utf-8", "q", "=?utf-8?Q?=C2=A0?=")]
    [InlineData("\u0085", "utf-8", "q", "=?utf-8?Q?=C2=85?=")]
    [InlineData("\u3000", "utf-8", "q", "=?utf-8?Q?=E3=80=80?=")]
    [InlineData("=?x?=", "utf-8", "auto", "=?utf-8?B?PT94Pz0=?=")]
    [InlineData("=?utf-8?Q?a?=", "utf-8", "q", "=?utf-8?Q?=3D=3Futf-8=3FQ=3Fa=3F=3D?=")]
    [InlineData("x=?utf-8?Q?a?=", "utf-8", "q", "x=?utf-8?Q?a?=")]
    [InlineData("=?utf-8?Q?a?=b", "utf-8", "q", "=?utf-8?Q?a?=b")]
    [InlineData("😀", "utf-8", "b", "=?utf-8?B?8J+YgA==?=")]
    public void EncoderReturnsLiteralExamples(string input, string charset, string choice, string expected)
    {
        Assert.Equal(expected, Encoder.Encode(Text.Cps(input), charset, choice));
    }

    [Theory]
    [InlineData(" café über  nice ", "iso-8859-1", "q", " =?iso-8859-1?Q?caf=E9_=FCber?=  nice ")]
    [InlineData("é\t\ré", "utf-8", "q", "=?utf-8?Q?=C3=A9=09=0D=C3=A9?=")]
    [InlineData("é\né\n", "utf-8", "q", "=?utf-8?Q?=C3=A9?=\n=?utf-8?Q?=C3=A9?=\n")]
    [InlineData("é x é", "utf-8", "q", "=?utf-8?Q?=C3=A9?= x =?utf-8?Q?=C3=A9?=")]
    [InlineData("\t é \r", "utf-8", "q", "\t =?utf-8?Q?=C3=A9?= \r")]
    public void EncoderGroupsOnlyEligibleRuns(string input, string charset, string choice, string expected)
    {
        Assert.Equal(expected, Encoder.Encode(Text.Cps(input), charset, choice));
    }

    [Theory]
    [InlineData("é €", "us-ascii", "auto", "U+00E9 cannot be written in us-ascii")]
    [InlineData("€ é", "us-ascii", "q", "U+20AC cannot be written in us-ascii")]
    [InlineData("é €", "iso-8859-1", "b", "U+20AC cannot be written in iso-8859-1")]
    [InlineData("¤", "iso-8859-15", "q", "U+00A4 cannot be written in iso-8859-15")]
    [InlineData("😀", "us-ascii", "q", "U+1F600 cannot be written in us-ascii")]
    public void EncoderReportsFirstUnrepresentableCodePoint(string input, string charset, string choice, string expected)
    {
        Failure failure = Assert.Throws<Failure>(() => Encoder.Encode(Text.Cps(input), charset, choice));
        Assert.Equal(expected, failure.Message);
    }

    [Fact]
    public void ChunkerSelectsWholeRunEncodingAndExposesPieceData()
    {
        Assert.Equal(string.Empty, Chunker.Words("utf-8", "auto", []));
        Assert.Equal(
            "=?utf-8?Q?A=00?=",
            Chunker.Words("utf-8", "auto", [new Chunker.Piece(65, [65]), new Chunker.Piece(0, [0])]));
        Assert.Equal(
            "=?utf-8?B?QQ==?=",
            Chunker.Words("utf-8", "b", [new Chunker.Piece(65, [65])]));
        Assert.Equal(
            "=?utf-8?Q?_=5F=3F=3D?=",
            Chunker.Words("utf-8", "q", [
                new Chunker.Piece(32, [32]),
                new Chunker.Piece(95, [95]),
                new Chunker.Piece(63, [63]),
                new Chunker.Piece(61, [61])
            ]));

        Chunker.Piece piece = new(128512, [240, 159, 152, 128]);
        Assert.Equal(128512, piece.Cp);
        Assert.Equal(new byte[] { 240, 159, 152, 128 }, piece.Bytes);
    }

    [Fact]
    public void ChunkerFitsAndSplitsQPayloadAtTheExactLimit()
    {
        List<Chunker.Piece> exactlySixtyThree = [
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65])
        ];
        List<Chunker.Piece> sixtyFour = [
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65])
        ];

        Assert.Equal(
            "=?utf-8?Q?AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA?=",
            Chunker.Words("utf-8", "q", exactlySixtyThree));
        Assert.Equal(
            "=?utf-8?Q?AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA?= =?utf-8?Q?A?=",
            Chunker.Words("utf-8", "q", sixtyFour));
    }

    [Fact]
    public void ChunkerFitsAndSplitsBPayloadAtTheExactByteCapacity()
    {
        List<Chunker.Piece> exactlyFortyFive = [
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65])
        ];
        List<Chunker.Piece> fortySix = [
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65])
        ];

        Assert.Equal(
            "=?utf-8?B?QUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFB?=",
            Chunker.Words("utf-8", "b", exactlyFortyFive));
        Assert.Equal(
            "=?utf-8?B?QUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFB?= =?utf-8?B?QQ==?=",
            Chunker.Words("utf-8", "b", fortySix));
    }

    [Fact]
    public void ChunkerKeepsSupplementaryPiecesWholeAndChoiceForShortSuffixes()
    {
        List<Chunker.Piece> sixEmoji = [
            new(128512, [240, 159, 152, 128]),
            new(128512, [240, 159, 152, 128]),
            new(128512, [240, 159, 152, 128]),
            new(128512, [240, 159, 152, 128]),
            new(128512, [240, 159, 152, 128]),
            new(128512, [240, 159, 152, 128])
        ];
        List<Chunker.Piece> sixtyThreeAsAndEAcute = [
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]), new(65, [65]),
            new(65, [65]), new(65, [65]), new(65, [65]), new(233, [195, 169])
        ];

        Assert.Equal(
            "=?utf-8?Q?=F0=9F=98=80=F0=9F=98=80=F0=9F=98=80=F0=9F=98=80=F0=9F=98=80?= =?utf-8?Q?=F0=9F=98=80?=",
            Chunker.Words("utf-8", "q", sixEmoji));
        Assert.Equal(
            "=?utf-8?Q?AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA?= =?utf-8?Q?=C3=A9?=",
            Chunker.Words("utf-8", "auto", sixtyThreeAsAndEAcute));
    }

    [Theory]
    [InlineData(
        "éééééééééééééééééééééééééééééééééééééééé\n",
        "utf-8",
        "q",
        "=?utf-8?Q?=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9?= =?utf-8?Q?=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9?= =?utf-8?Q?=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9?= =?utf-8?Q?=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9=C3=A9?=\n")]
    [InlineData(
        "éééééééééééééééééééééééééééééééééééééééé\n",
        "utf-8",
        "auto",
        "=?utf-8?B?w6nDqcOpw6nDqcOpw6nDqcOpw6nDqcOpw6nDqcOpw6nDqcOpw6nDqcOpw6k=?= =?utf-8?B?w6nDqcOpw6nDqcOpw6nDqcOpw6nDqcOpw6nDqcOpw6nDqcOp?=\n")]
    public void EncoderChunksLiteralLongRuns(string input, string charset, string choice, string expected)
    {
        Assert.Equal(expected, Encoder.Encode(Text.Cps(input), charset, choice));
    }
}
