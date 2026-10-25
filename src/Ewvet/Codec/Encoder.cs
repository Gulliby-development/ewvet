using System.Text;

namespace Ewvet;

public static class Encoder
{
    public static string Encode(int[] text, string charset, string choice)
    {
        ArgumentNullException.ThrowIfNull(text);

        bool[] protectedPoints = new bool[text.Length];
        foreach (Occurrence occurrence in Scanner.Scan(text))
        {
            for (int index = occurrence.Start; index < occurrence.Reading.End; index++)
            {
                protectedPoints[index] = true;
            }
        }

        StringBuilder output = new();
        int at = 0;
        while (at < text.Length)
        {
            if (Text.IsBlank(text[at]))
            {
                int blankEnd = at + 1;
                while (blankEnd < text.Length && Text.IsBlank(text[blankEnd]))
                {
                    blankEnd++;
                }

                output.Append(Text.Str(text, at, blankEnd));
                at = blankEnd;
                continue;
            }

            int tokenEnd = FindTokenEnd(text, at);
            if (!NeedsEncoding(text, at, tokenEnd, protectedPoints))
            {
                output.Append(Text.Str(text, at, tokenEnd));
                at = tokenEnd;
                continue;
            }

            int runStart = at;
            int runEnd = tokenEnd;
            while (true)
            {
                int nextTokenStart = runEnd;
                bool containsLineFeed = false;
                while (nextTokenStart < text.Length && Text.IsBlank(text[nextTokenStart]))
                {
                    containsLineFeed |= text[nextTokenStart] == '\n';
                    nextTokenStart++;
                }

                if (containsLineFeed || nextTokenStart == text.Length)
                {
                    break;
                }

                int nextTokenEnd = FindTokenEnd(text, nextTokenStart);
                if (!NeedsEncoding(text, nextTokenStart, nextTokenEnd, protectedPoints))
                {
                    break;
                }

                runEnd = nextTokenEnd;
            }

            List<Chunker.Piece> pieces = [];
            for (int index = runStart; index < runEnd; index++)
            {
                byte[]? bytes = Charmap.Encode(charset, text[index]);
                if (bytes is null)
                {
                    throw new Failure(Text.Hex(text[index]) + " cannot be written in " + charset);
                }

                pieces.Add(new Chunker.Piece(text[index], bytes));
            }

            output.Append(Chunker.Words(charset, choice, pieces));
            at = runEnd;
        }

        return output.ToString();
    }

    private static int FindTokenEnd(int[] text, int start)
    {
        int end = start;
        while (end < text.Length && !Text.IsBlank(text[end]))
        {
            end++;
        }

        return end;
    }

    private static bool NeedsEncoding(int[] text, int start, int end, bool[] protectedPoints)
    {
        for (int index = start; index < end; index++)
        {
            if (protectedPoints[index] || text[index] is < 0x21 or > 0x7E)
            {
                return true;
            }
        }

        return false;
    }
}
