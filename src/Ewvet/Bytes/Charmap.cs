using System.Text;

namespace Ewvet;

public static class Charmap
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    public static int[]? Decode(string charset, byte[] data)
    {
        switch (charset)
        {
            case "utf-8":
                try
                {
                    var text = StrictUtf8.GetString(data);
                    var codePoints = new List<int>();
                    foreach (var rune in text.EnumerateRunes())
                    {
                        codePoints.Add(rune.Value);
                    }

                    return codePoints.ToArray();
                }
                catch (DecoderFallbackException)
                {
                    return null;
                }

            case "us-ascii":
                {
                    var codePoints = new int[data.Length];
                    for (var index = 0; index < data.Length; index++)
                    {
                        if (data[index] > 0x7F)
                        {
                            return null;
                        }

                        codePoints[index] = data[index];
                    }

                    return codePoints;
                }

            case "iso-8859-1":
                {
                    var codePoints = new int[data.Length];
                    for (var index = 0; index < data.Length; index++)
                    {
                        codePoints[index] = data[index];
                    }

                    return codePoints;
                }

            case "iso-8859-15":
                {
                    var codePoints = new int[data.Length];
                    for (var index = 0; index < data.Length; index++)
                    {
                        codePoints[index] = data[index] switch
                        {
                            0xA4 => 0x20AC,
                            0xA6 => 0x0160,
                            0xA8 => 0x0161,
                            0xB4 => 0x017D,
                            0xB8 => 0x017E,
                            0xBC => 0x0152,
                            0xBD => 0x0153,
                            0xBE => 0x0178,
                            _ => data[index]
                        };
                    }

                    return codePoints;
                }

            default:
                return null;
        }
    }

    public static byte[]? Encode(string charset, int cp)
    {
        if (!Rune.TryCreate(cp, out var rune))
        {
            return null;
        }

        switch (charset)
        {
            case "utf-8":
                {
                    Span<byte> bytes = stackalloc byte[4];
                    var length = rune.EncodeToUtf8(bytes);
                    return bytes[..length].ToArray();
                }

            case "us-ascii":
                return cp <= 0x7F ? [(byte)cp] : null;

            case "iso-8859-1":
                return cp <= 0xFF ? [(byte)cp] : null;

            case "iso-8859-15":
                return cp switch
                {
                    0x20AC => [0xA4],
                    0x0160 => [0xA6],
                    0x0161 => [0xA8],
                    0x017D => [0xB4],
                    0x017E => [0xB8],
                    0x0152 => [0xBC],
                    0x0153 => [0xBD],
                    0x0178 => [0xBE],
                    0x00A4 or 0x00A6 or 0x00A8 or 0x00B4 or 0x00B8 or 0x00BC or 0x00BD or 0x00BE => null,
                    <= 0xFF => [(byte)cp],
                    _ => null
                };

            default:
                return null;
        }
    }
}
