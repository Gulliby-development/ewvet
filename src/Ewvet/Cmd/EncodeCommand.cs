namespace Ewvet;

public static class EncodeCommand
{
    public static Output Run(
        List<string> args,
        ISet<string> flags,
        IDictionary<string, string> values,
        Inputs inputs)
    {
        if (args.Count == 0)
        {
            throw new Failure("missing FILE");
        }

        if (args.Count > 1)
        {
            throw new Failure("too many arguments");
        }

        string charsetName = values.TryGetValue("--charset", out string? requestedCharset)
            ? requestedCharset
            : "utf-8";
        Charsets.Charset? charset = Charsets.Find(charsetName);
        if (charset is null)
        {
            throw new Failure("unknown charset " + Text.Show(charsetName));
        }

        string choice = values.TryGetValue("--encoding", out string? requestedChoice)
            ? requestedChoice
            : "auto";
        if (choice is not "auto" and not "b" and not "q")
        {
            throw new Failure("invalid encoding " + Text.Show(choice) + " (expected auto, b or q)");
        }

        Inputs.Source source = inputs.Read(args[0]);
        return new Output(Encoder.Encode(Text.Cps(source.Text), charset.Name, choice), 0);
    }
}

