using System.Globalization;
using System.Text;

namespace Ewvet;

public static class Text
{
    public static int[] Cps(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var codePoints = new List<int>();
        foreach (var rune in text.EnumerateRunes())
        {
            codePoints.Add(rune.Value);
        }

        return codePoints.ToArray();
    }

    public static string Str(int[] cps, int from, int to)
    {
        ArgumentNullException.ThrowIfNull(cps);

        if (from < 0 || from > cps.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(from));
        }

        if (to < from || to > cps.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(to));
        }

        var result = new StringBuilder();
        for (var index = from; index < to; index++)
        {
            result.Append(new Rune(cps[index]));
        }

        return result.ToString();
    }

    public static string Show(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return "\"" + Plain(text) + "\"";
    }

    public static string Plain(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var result = new StringBuilder();
        foreach (var rune in text.EnumerateRunes())
        {
            result.Append(rune.Value is >= 0x20 and <= 0x7E ? (char)rune.Value : '?');
        }

        return result.ToString();
    }

    public static string Clip(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var cps = Cps(text);
        return cps.Length <= 24 ? text : Str(cps, 0, 21) + "...";
    }

    public static string Plural(int n, string word)
    {
        ArgumentNullException.ThrowIfNull(word);
        return n.ToString(CultureInfo.InvariantCulture) + " " + word + (n == 1 ? string.Empty : "s");
    }

    public static string Hex(int cp)
    {
        return "U+" + cp.ToString("X4", CultureInfo.InvariantCulture);
    }

    public static bool IsBlank(int cp)
    {
        return cp is 0x20 or 0x09 or 0x0D or 0x0A;
    }

    public static bool IsDigit(int cp)
    {
        return cp is >= '0' and <= '9';
    }

    public static bool IsHex(int cp)
    {
        return IsDigit(cp) || cp is >= 'A' and <= 'F' || cp is >= 'a' and <= 'f';
    }

    public static bool IsAlnum(int cp)
    {
        return IsDigit(cp) || cp is >= 'A' and <= 'Z' || cp is >= 'a' and <= 'z';
    }

    public static string Fold(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var result = new StringBuilder();
        foreach (var rune in text.EnumerateRunes())
        {
            var cp = rune.Value;
            if (cp is >= 'A' and <= 'Z')
            {
                cp += 'a' - 'A';
            }

            result.Append(new Rune(cp));
        }

        return result.ToString();
    }
}
