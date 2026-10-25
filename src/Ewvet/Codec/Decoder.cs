using System.Text;

namespace Ewvet;

public static class Decoder
{
    public static string Decode(int[] text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var result = new StringBuilder();
        var cursor = 0;
        foreach (var occurrence in Scanner.Scan(text))
        {
            var copyThrough = occurrence.Start - occurrence.Gap;
            result.Append(Text.Str(text, cursor, copyThrough));

            var reading = occurrence.Reading;
            result.Append(reading.Result is null
                ? Text.Str(text, occurrence.Start, reading.End)
                : Text.Str(reading.Result, 0, reading.Result.Length));
            cursor = reading.End;
        }

        result.Append(Text.Str(text, cursor, text.Length));
        return result.ToString();
    }
}
