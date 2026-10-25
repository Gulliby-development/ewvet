namespace Ewvet;

public static class Scanner
{
    public static List<Occurrence> Scan(int[] text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var occurrences = new List<Occurrence>();
        var at = 0;
        while (at < text.Length)
        {
            var reading = Reader.Read(text, at);
            if (reading is null)
            {
                at++;
                continue;
            }

            var gap = 0;
            if (occurrences.Count > 0 && occurrences[^1].Reading.Result is not null && reading.Result is not null)
            {
                var previousEnd = occurrences[^1].Reading.End;
                if (at > previousEnd)
                {
                    var onlyBlanks = true;
                    for (var index = previousEnd; index < at; index++)
                    {
                        if (!Text.IsBlank(text[index]) || text[index] == '\n')
                        {
                            onlyBlanks = false;
                            break;
                        }
                    }

                    if (onlyBlanks)
                    {
                        gap = at - previousEnd;
                    }
                }
            }

            occurrences.Add(new Occurrence(at, gap, reading));
            at = reading.End;
        }

        return occurrences;
    }
}
