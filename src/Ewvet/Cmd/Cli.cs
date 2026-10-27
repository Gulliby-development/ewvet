namespace Ewvet;

public static class Cli
{
    private const string HelpText =
        "usage: ewvet decode [--strict] FILE\n" +
        "       ewvet explain FILE\n" +
        "       ewvet check [--quiet] [--ignore CODES] FILE...\n" +
        "       ewvet encode [--charset NAME] [--encoding auto|b|q] FILE\n" +
        "       ewvet charsets [PREFIX]\n";

    public static int Run(IReadOnlyList<string> argv, IIo io)
    {
        try
        {
            if (argv.Count == 0)
            {
                throw new Failure("missing command (decode, explain, check, encode or charsets)");
            }

            if (argv.Count == 1 && argv[0] == "--help")
            {
                io.Out(HelpText);
                return 0;
            }

            string command = argv[0];
            if (command is not "decode" and not "explain" and not "check" and not "encode" and not "charsets")
            {
                throw new Failure("unknown command " + Text.Show(command));
            }

            ISet<string> flags = command switch
            {
                "decode" => new HashSet<string>(StringComparer.Ordinal) { "--strict" },
                "check" => new HashSet<string>(StringComparer.Ordinal) { "--quiet" },
                _ => new HashSet<string>(StringComparer.Ordinal),
            };
            ISet<string> valued = command switch
            {
                "encode" => new HashSet<string>(StringComparer.Ordinal) { "--charset", "--encoding" },
                "check" => new HashSet<string>(StringComparer.Ordinal) { "--ignore" },
                _ => new HashSet<string>(StringComparer.Ordinal),
            };

            var parsed = Options.Parse(argv.Skip(1).ToArray(), flags, valued);
            var inputs = new Inputs(io);
            Output output = command switch
            {
                "decode" => DecodeCommand.Run(parsed.Args, parsed.Flags, parsed.Values, inputs),
                "explain" => ExplainCommand.Run(parsed.Args, parsed.Flags, parsed.Values, inputs),
                "check" => CheckCommand.Run(parsed.Args, parsed.Flags, parsed.Values, inputs),
                "encode" => EncodeCommand.Run(parsed.Args, parsed.Flags, parsed.Values, inputs),
                "charsets" => CharsetsCommand.Run(parsed.Args, parsed.Flags, parsed.Values, inputs),
                _ => throw new Failure("unknown command " + Text.Show(command))
            };

            if (output.Out.Length > 0)
            {
                io.Out(output.Out);
            }

            return output.Code;
        }
        catch (Failure failure)
        {
            io.Err("ewvet: " + failure.Message + "\n");
            return 2;
        }
    }
}
