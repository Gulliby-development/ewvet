using System.Globalization;

namespace Ewvet;

public static class ExplainCommand
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
        var text = Text.Cps(source.Text);
        var lines = new Lines(text);
        var rows = new List<List<string>>
        {
            new() { "AT", "GAP", "TOKEN", "HOW", "RESULT", "CODES" }
        };

        foreach (var occurrence in Scanner.Scan(text))
        {
            var reading = occurrence.Reading;
            var token = Text.Str(text, occurrence.Start, reading.End);
            var how = reading.Result is null
                ? "kept"
                : reading.Enc + " " + reading.Charset + (reading.Lang is null ? string.Empty : "*" + reading.Lang);
            var result = reading.Result is null
                ? "-"
                : Text.Show(Text.Clip(Text.Str(reading.Result, 0, reading.Result.Length)));
            var codes = reading.Codes.Count == 0 ? "-" : string.Join(",", reading.Codes);

            rows.Add(new List<string>
            {
                lines.Line(occurrence.Start).ToString(CultureInfo.InvariantCulture) + ":" +
                    lines.Col(occurrence.Start).ToString(CultureInfo.InvariantCulture),
                occurrence.Gap.ToString(CultureInfo.InvariantCulture),
                Text.Show(Text.Clip(token)),
                how,
                result,
                codes
            });
        }

        return new Output(Grid.Render(rows), 0);
    }
}
