namespace Ewvet;

public static class QText
{
    public static byte[]? Decode(int[] text)
    {
        List<byte> bytes = [];

        for (int index = 0; index < text.Length; index++)
        {
            int codePoint = text[index];
            if (codePoint == '_')
            {
                bytes.Add(32);
            }
            else if (codePoint == '=')
            {
                if (index + 2 >= text.Length)
                {
                    return null;
                }

                int high = HexValue(text[index + 1]);
                int low = HexValue(text[index + 2]);
                if (high < 0 || low < 0)
                {
                    return null;
                }

                bytes.Add((byte)((high << 4) | low));
                index += 2;
            }
            else if (codePoint is >= '!' and <= '~')
            {
                bytes.Add((byte)codePoint);
            }
            else
            {
                return null;
            }
        }

        return bytes.ToArray();
    }

    public static string Encode(byte[] data)
    {
        const string HexDigits = "0123456789ABCDEF";
        System.Text.StringBuilder text = new();

        foreach (byte value in data)
        {
            if (value is >= (byte)'A' and <= (byte)'Z'
                or >= (byte)'a' and <= (byte)'z'
                or >= (byte)'0' and <= (byte)'9'
                or (byte)'!' or (byte)'*' or (byte)'+' or (byte)'-' or (byte)'/')
            {
                text.Append((char)value);
            }
            else if (value == 32)
            {
                text.Append('_');
            }
            else
            {
                text.Append('=');
                text.Append(HexDigits[value >> 4]);
                text.Append(HexDigits[value & 0x0F]);
            }
        }

        return text.ToString();
    }

    private static int HexValue(int codePoint) => codePoint switch
    {
        >= '0' and <= '9' => codePoint - '0',
        >= 'A' and <= 'F' => codePoint - 'A' + 10,
        >= 'a' and <= 'f' => codePoint - 'a' + 10,
        _ => -1
    };
}
