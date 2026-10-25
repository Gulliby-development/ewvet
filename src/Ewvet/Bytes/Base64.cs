namespace Ewvet;

public static class Base64
{
    public sealed record Result(byte[]? Bytes, bool MissingPadding);

    public static Result Decode(int[] text)
    {
        var padding = 0;
        for (var index = text.Length - 1; index >= 0 && text[index] == '='; index--)
        {
            padding++;
        }

        if (padding > 2)
        {
            return new Result(null, false);
        }

        var bodyLength = text.Length - padding;
        var remainder = bodyLength % 4;
        if (remainder == 1 || (padding > 0 && (text.Length % 4 != 0 || (padding == 1 && remainder != 3) || (padding == 2 && remainder != 2))))
        {
            return new Result(null, false);
        }

        for (var index = 0; index < bodyLength; index++)
        {
            if (Sextet(text[index]) < 0)
            {
                return new Result(null, false);
            }
        }

        var outputLength = bodyLength / 4 * 3 + (remainder == 2 ? 1 : remainder == 3 ? 2 : 0);
        var bytes = new byte[outputLength];
        var outputIndex = 0;
        uint accumulator = 0;
        var bitCount = 0;

        for (var index = 0; index < bodyLength; index++)
        {
            accumulator = (accumulator << 6) | (uint)Sextet(text[index]);
            bitCount += 6;
            if (bitCount >= 8)
            {
                bitCount -= 8;
                bytes[outputIndex++] = (byte)(accumulator >> bitCount);
                accumulator &= bitCount == 0 ? 0u : (1u << bitCount) - 1;
            }
        }

        return new Result(bytes, padding == 0 && remainder is 2 or 3);
    }

    public static string Encode(byte[] data)
    {
        return Convert.ToBase64String(data);
    }

    private static int Sextet(int codePoint)
    {
        return codePoint switch
        {
            >= 'A' and <= 'Z' => codePoint - 'A',
            >= 'a' and <= 'z' => codePoint - 'a' + 26,
            >= '0' and <= '9' => codePoint - '0' + 52,
            '+' => 62,
            '/' => 63,
            _ => -1
        };
    }
}
