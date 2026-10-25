namespace Ewvet;

public static class Options
{
    public sealed record Parsed(HashSet<string> Flags, Dictionary<string, string> Values, List<string> Args);

    public static Parsed Parse(IReadOnlyList<string> argv, ISet<string> flags, ISet<string> valued)
    {
        ArgumentNullException.ThrowIfNull(argv);
        ArgumentNullException.ThrowIfNull(flags);
        ArgumentNullException.ThrowIfNull(valued);

        var recognizedFlags = new HashSet<string>(flags, StringComparer.Ordinal);
        var recognizedValues = new HashSet<string>(valued, StringComparer.Ordinal);
        var parsedFlags = new HashSet<string>(StringComparer.Ordinal);
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        var args = new List<string>();

        for (var index = 0; index < argv.Count; index++)
        {
            var argument = argv[index];
            if (!argument.StartsWith("--", StringComparison.Ordinal))
            {
                args.Add(argument);
                continue;
            }

            var equalsIndex = argument.IndexOf('=');
            var optionName = equalsIndex < 0 ? argument : argument[..equalsIndex];
            if (recognizedValues.Contains(optionName))
            {
                if (equalsIndex >= 0)
                {
                    values[optionName] = argument[(equalsIndex + 1)..];
                }
                else if (index + 1 < argv.Count)
                {
                    values[optionName] = argv[++index];
                }
                else
                {
                    throw new Failure(Text.Plain(optionName) + " needs a value");
                }

                continue;
            }

            if (recognizedFlags.Contains(argument))
            {
                parsedFlags.Add(argument);
                continue;
            }

            throw new Failure("unknown option " + Text.Show(Text.Plain(argument)));
        }

        return new Parsed(parsedFlags, values, args);
    }
}
