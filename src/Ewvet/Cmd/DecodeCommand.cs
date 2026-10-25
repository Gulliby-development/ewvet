namespace Ewvet;

public static class DecodeCommand
{
    public static Output Run(List<string> args, ISet<string> flags, IDictionary<string, string> values, Inputs inputs)
    {
        if (args.Count == 0)
        {
            throw new Failure("missing FILE");
        }

        if (args.Count > 1)
        {
            throw new Failure("too many arguments");
        }

        var source = inputs.Read(args[0]);
        var codePoints = Text.Cps(source.Text);
        var output = Decoder.Decode(codePoints);

        var code = 0;
        if (flags.Contains("--strict"))
        {
            foreach (var occurrence in Scanner.Scan(codePoints))
            {
                if (occurrence.Reading.Codes.Count > 0)
                {
                    code = 1;
                    break;
                }
            }
        }

        return new Output(output, code);
    }
}
