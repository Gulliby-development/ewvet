using System.Globalization;

namespace Ewvet;

public static class Reader
{
    private static readonly IReadOnlyList<string> NoItems = Array.AsReadOnly(Array.Empty<string>());

    public static Reading? Read(int[] text, int at)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (at < 0 || at >= text.Length || text[at] != '=' || at + 1 >= text.Length || text[at + 1] != '?')
        {
            return null;
        }

        if (at != 0 && !Text.IsBlank(text[at - 1]))
        {
            return null;
        }

        var end = at + 2;
        while (end < text.Length && !Text.IsBlank(text[end]))
        {
            end++;
        }

        var tokenLength = end - at;
        if (tokenLength < 4 || text[end - 2] != '?' || text[end - 1] != '=')
        {
            return null;
        }

        var token = Text.Str(text, at, end);
        var middleStart = at + 2;
        var middleEnd = end - 2;
        var firstSeparator = -1;
        var secondSeparator = -1;
        for (var index = middleStart; index < middleEnd; index++)
        {
            if (text[index] == '?')
            {
                if (firstSeparator < 0)
                {
                    firstSeparator = index;
                }
                else if (secondSeparator < 0)
                {
                    secondSeparator = index;
                }
                else
                {
                    return Kept(end, "EW001", token);
                }
            }
        }

        if (firstSeparator < 0 || secondSeparator < 0)
        {
            return Kept(end, "EW001", token);
        }

        var nameStart = middleStart;
        var nameEnd = firstSeparator;
        var encodingStart = firstSeparator + 1;
        var encodingEnd = secondSeparator;
        var payloadStart = secondSeparator + 1;
        var payloadEnd = middleEnd;

        var star = -1;
        for (var index = nameStart; index < nameEnd; index++)
        {
            if (text[index] == '*')
            {
                star = index;
                break;
            }
        }

        var charsetNameEnd = star < 0 ? nameEnd : star;
        if (charsetNameEnd == nameStart || !HasCharsetName(text, nameStart, charsetNameEnd))
        {
            return Kept(end, "EW001", token);
        }

        string? lang = null;
        if (star >= 0)
        {
            var tagStart = star + 1;
            if (tagStart == nameEnd || !HasLanguageTag(text, tagStart, nameEnd))
            {
                return Kept(end, "EW001", token);
            }

            lang = Text.Str(text, tagStart, nameEnd);
        }

        var originalName = Text.Str(text, nameStart, charsetNameEnd);
        var charset = Charsets.Find(originalName);
        if (charset is null)
        {
            return Kept(end, "EW002", originalName);
        }

        if (encodingEnd - encodingStart != 1)
        {
            return Kept(end, "EW003", Text.Str(text, encodingStart, encodingEnd));
        }

        var encoding = text[encodingStart];
        var enc = encoding switch
        {
            'B' or 'b' => 'B',
            'Q' or 'q' => 'Q',
            _ => '\0'
        };
        if (enc == '\0')
        {
            return Kept(end, "EW003", Text.Str(text, encodingStart, encodingEnd));
        }

        var payload = text[payloadStart..payloadEnd];
        byte[]? bytes;
        var missingPadding = false;
        if (enc == 'B')
        {
            var decoded = Base64.Decode(payload);
            bytes = decoded.Bytes;
            missingPadding = decoded.MissingPadding;
            if (bytes is null)
            {
                return Kept(end, "EW004", string.Empty);
            }
        }
        else
        {
            bytes = QText.Decode(payload);
            if (bytes is null)
            {
                return Kept(end, "EW006", string.Empty);
            }
        }

        var result = Charmap.Decode(charset.Name, bytes);
        if (result is null)
        {
            return Kept(end, "EW007", charset.Name);
        }

        var codes = new List<string>();
        var details = new List<string>();
        if (missingPadding)
        {
            codes.Add("EW005");
            details.Add(string.Empty);
        }

        if (tokenLength > 75)
        {
            codes.Add("EW008");
            details.Add(tokenLength.ToString(CultureInfo.InvariantCulture));
        }

        foreach (var codePoint in result)
        {
            if (codePoint < 0x20 || codePoint == 0x7F)
            {
                codes.Add("EW009");
                details.Add(Text.Hex(codePoint));
                break;
            }
        }

        return new Reading(end, result, Snapshot(codes), Snapshot(details), charset.Name, enc, lang);
    }

    private static Reading Kept(int end, string code, string detail)
    {
        return new Reading(end, null, Array.AsReadOnly(new[] { code }), Array.AsReadOnly(new[] { detail }), null, '\0', null);
    }

    private static IReadOnlyList<string> Snapshot(List<string> values)
    {
        return values.Count == 0 ? NoItems : Array.AsReadOnly(values.ToArray());
    }

    private static bool HasCharsetName(int[] text, int start, int end)
    {
        for (var index = start; index < end; index++)
        {
            var codePoint = text[index];
            if (!Text.IsAlnum(codePoint) && codePoint is not ('-' or '_' or '.' or ':' or '+'))
            {
                return false;
            }
        }

        return true;
    }

    private static bool HasLanguageTag(int[] text, int start, int end)
    {
        for (var index = start; index < end; index++)
        {
            var codePoint = text[index];
            if (!Text.IsAlnum(codePoint) && codePoint != '-')
            {
                return false;
            }
        }

        return true;
    }
}
