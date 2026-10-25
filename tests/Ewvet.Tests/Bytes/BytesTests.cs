namespace Ewvet.Tests;

public sealed class BytesTests
{
    [Theory]
    [InlineData(new int[] { }, new byte[] { }, false)]
    [InlineData(new int[] { 83, 71, 107, 61 }, new byte[] { 72, 105 }, false)]
    [InlineData(new int[] { 83, 71, 107 }, new byte[] { 72, 105 }, true)]
    [InlineData(new int[] { 83, 71, 61, 61 }, new byte[] { 72 }, false)]
    [InlineData(new int[] { 83, 71 }, new byte[] { 72 }, true)]
    [InlineData(new int[] { 83, 71, 107, 65 }, new byte[] { 72, 105, 0 }, false)]
    [InlineData(new int[] { 81, 81, 61, 61 }, new byte[] { 65 }, false)]
    [InlineData(new int[] { 81, 82, 61, 61 }, new byte[] { 65 }, false)]
    [InlineData(new int[] { 83, 71, 110, 61 }, new byte[] { 72, 105 }, false)]
    [InlineData(new int[] { 43, 47, 56, 61 }, new byte[] { 251, 255 }, false)]
    [InlineData(new int[] { 47, 47, 47, 47 }, new byte[] { 255, 255, 255 }, false)]
    [InlineData(new int[] { 65, 65 }, new byte[] { 0 }, true)]
    [InlineData(new int[] { 65, 65, 65 }, new byte[] { 0, 0 }, true)]
    public void Base64DecodeAcceptsLiteralExamples(int[] input, byte[] expectedBytes, bool expectedMissingPadding)
    {
        Base64.Result result = Base64.Decode(input);

        Assert.Equal(expectedBytes, result.Bytes);
        Assert.Equal(expectedMissingPadding, result.MissingPadding);
    }

    [Theory]
    [InlineData(new int[] { 83 })]
    [InlineData(new int[] { 65, 65, 65, 65, 65 })]
    [InlineData(new int[] { 83, 71, 107, 61, 61 })]
    [InlineData(new int[] { 83, 71, 61, 61, 61 })]
    [InlineData(new int[] { 83, 71, 107, 61, 120 })]
    [InlineData(new int[] { 83, 71, 107, 45 })]
    [InlineData(new int[] { 83, 71, 107, 95 })]
    [InlineData(new int[] { 61 })]
    [InlineData(new int[] { 61, 61 })]
    [InlineData(new int[] { 65, 65, 65, 65, 61 })]
    [InlineData(new int[] { 83, 71, 32, 107 })]
    [InlineData(new int[] { 83, 71, 107, 10 })]
    [InlineData(new int[] { 233 })]
    public void Base64DecodeRejectsLiteralExamples(int[] input)
    {
        Base64.Result result = Base64.Decode(input);

        Assert.Null(result.Bytes);
        Assert.False(result.MissingPadding);
    }

    [Theory]
    [InlineData(new byte[] { }, "")]
    [InlineData(new byte[] { 65 }, "QQ==")]
    [InlineData(new byte[] { 72, 105 }, "SGk=")]
    [InlineData(new byte[] { 65, 66, 67 }, "QUJD")]
    [InlineData(new byte[] { 251, 255 }, "+/8=")]
    [InlineData(new byte[] { 0, 0, 0, 0 }, "AAAAAA==")]
    public void Base64EncodeReturnsCanonicalLiteralExamples(byte[] input, string expected)
    {
        Assert.Equal(expected, Base64.Encode(input));
    }

    [Theory]
    [InlineData(new int[] { }, new byte[] { })]
    [InlineData(new int[] { 97, 95, 98 }, new byte[] { 97, 32, 98 })]
    [InlineData(new int[] { 99, 97, 102, 61, 99, 51, 61, 65, 57 }, new byte[] { 99, 97, 102, 195, 169 })]
    [InlineData(new int[] { 61, 51, 68, 61, 51, 70, 61, 53, 70 }, new byte[] { 61, 63, 95 })]
    [InlineData(new int[] { 61, 48, 48, 61, 55, 102, 61, 70, 70 }, new byte[] { 0, 127, 255 })]
    [InlineData(new int[] { 33, 126, 63 }, new byte[] { 33, 126, 63 })]
    [InlineData(new int[] { 40, 97, 41, 34, 33 }, new byte[] { 40, 97, 41, 34, 33 })]
    [InlineData(new int[] { 61, 52, 49, 52 }, new byte[] { 65, 52 })]
    public void QTextDecodeAcceptsLiteralExamples(int[] input, byte[] expectedBytes)
    {
        Assert.Equal(expectedBytes, QText.Decode(input));
    }

    [Theory]
    [InlineData(new int[] { 61, 52 })]
    [InlineData(new int[] { 97, 61 })]
    [InlineData(new int[] { 61, 71, 49 })]
    [InlineData(new int[] { 61, 49, 71 })]
    [InlineData(new int[] { 32 })]
    [InlineData(new int[] { 9 })]
    [InlineData(new int[] { 127 })]
    [InlineData(new int[] { 160 })]
    [InlineData(new int[] { 233 })]
    [InlineData(new int[] { 128512 })]
    public void QTextDecodeRejectsLiteralExamples(int[] input)
    {
        Assert.Null(QText.Decode(input));
    }

    [Theory]
    [InlineData(new byte[] { }, "")]
    [InlineData(new byte[] { 65, 90, 97, 122, 48, 57, 33, 42, 43, 45, 47 }, "AZaz09!*+-/")]
    [InlineData(new byte[] { 32 }, "_")]
    [InlineData(new byte[] { 61, 63, 95 }, "=3D=3F=5F")]
    [InlineData(new byte[] { 0, 9, 10, 13, 31, 127, 128, 255 }, "=00=09=0A=0D=1F=7F=80=FF")]
    [InlineData(new byte[] { 34, 40, 41, 46, 126 }, "=22=28=29=2E=7E")]
    [InlineData(new byte[] { 99, 97, 102, 195, 169 }, "caf=C3=A9")]
    public void QTextEncodeReturnsCanonicalLiteralExamples(byte[] input, string expected)
    {
        Assert.Equal(expected, QText.Encode(input));
    }
}
