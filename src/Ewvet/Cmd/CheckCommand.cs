namespace Ewvet;

public static class CheckCommand
{
    public static Output Run(List<string> args, ISet<string> flags, IDictionary<string, string> values, Inputs inputs)
    {
        if (args.Count == 0)
        {
            throw new Failure("missing FILE");
        }

        var ignored = new HashSet<string>(StringComparer.Ordinal);
        if (values.TryGetValue("--ignore", out var list))
        {
            if (list.Length == 0)
            {
                throw new Failure("--ignore needs a non-empty value");
            }

            foreach (var code in list.Split(','))
            {
                if (!IsProblemCode(code))
                {
                    throw new Failure("unknown problem code " + Text.Show(code));
                }

                ignored.Add(code);
            }
        }

        var sources = new List<Inputs.Source>(args.Count);
        foreach (var arg in args)
        {
            sources.Add(inputs.Read(arg));
        }

        return Checker.Check(sources, ignored, flags.Contains("--quiet"));
    }

    private static bool IsProblemCode(string code)
    {
        return code.Length == 5
            && code.StartsWith("EW00", StringComparison.Ordinal)
            && code[4] is >= '1' and <= '9';
    }
}
