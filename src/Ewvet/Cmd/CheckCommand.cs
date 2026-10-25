namespace Ewvet;

public static class CheckCommand
{
    public static Output Run(List<string> args, ISet<string> flags, IDictionary<string, string> values, Inputs inputs)
    {
        if (args.Count == 0)
        {
            throw new Failure("missing FILE");
        }

        var sources = new List<Inputs.Source>(args.Count);
        foreach (var arg in args)
        {
            sources.Add(inputs.Read(arg));
        }

        return Checker.Check(sources);
    }
}
