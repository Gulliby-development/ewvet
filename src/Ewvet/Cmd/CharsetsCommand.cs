namespace Ewvet;

public static class CharsetsCommand
{
    public static Output Run(List<string> args, ISet<string> flags, IDictionary<string, string> values, Inputs inputs)
    {
        if (args.Count > 1)
        {
            throw new Failure("too many arguments");
        }

        string prefix = args.Count == 0 ? string.Empty : args[0];
        string foldedPrefix = Text.Fold(prefix);
        var rows = new List<List<string>>
        {
            new() { "NAME", "ALIASES", "BYTES" }
        };

        foreach (Charsets.Charset charset in Charsets.All())
        {
            bool matchesName = charset.Name.StartsWith(foldedPrefix, StringComparison.Ordinal);
            bool matchesAlias = charset.Aliases.Any(alias => alias.StartsWith(foldedPrefix, StringComparison.Ordinal));
            if (matchesName || matchesAlias)
            {
                rows.Add(new List<string> { charset.Name, string.Join(',', charset.Aliases), charset.Bytes });
            }
        }

        if (rows.Count == 1)
        {
            return new Output("no charset starts with " + Text.Show(prefix) + "\n", 1);
        }

        return new Output(Grid.Render(rows), 0);
    }
}
