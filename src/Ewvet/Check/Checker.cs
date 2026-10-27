using System.Globalization;
using System.Text;

namespace Ewvet;

public static class Checker
{
    public static Output Check(List<Inputs.Source> sources, ISet<string>? ignored = null, bool quiet = false)
    {
        ArgumentNullException.ThrowIfNull(sources);

        var report = new StringBuilder();
        var problemCount = 0;
        var problematicSourceCount = 0;
        var occurrenceCount = 0;

        foreach (var source in sources)
        {
            var codePoints = Text.Cps(source.Text);
            var occurrences = Scanner.Scan(codePoints);
            var lines = new Lines(codePoints);
            var sourceHasProblems = false;

            occurrenceCount += occurrences.Count;
            foreach (var occurrence in occurrences)
            {
                for (var codeIndex = 0; codeIndex < occurrence.Reading.Codes.Count; codeIndex++)
                {
                    var code = occurrence.Reading.Codes[codeIndex];
                    if (ignored is not null && ignored.Contains(code))
                    {
                        continue;
                    }

                    var detail = occurrence.Reading.Details[codeIndex];
                    var message = Problems.Message(code, detail);
                    if (!quiet)
                    {
                        report.Append(FormattableString.Invariant(
                            $"{source.Display}:{lines.Line(occurrence.Start)}:{lines.Col(occurrence.Start)}: {code} {message}\n"));
                    }

                    problemCount++;
                    sourceHasProblems = true;
                }
            }

            if (sourceHasProblems)
            {
                problematicSourceCount++;
            }
        }

        if (problemCount > 0)
        {
            report.Append(Text.Plural(problemCount, "problem"));
            report.Append(" in ");
            report.Append(Text.Plural(problematicSourceCount, "file"));
            report.Append('\n');
            return new Output(report.ToString(), 1);
        }

        report.Append("ok, ");
        report.Append(Text.Plural(occurrenceCount, "encoded word"));
        report.Append(" in ");
        report.Append(Text.Plural(sources.Count, "file"));
        report.Append('\n');
        return new Output(report.ToString(), 0);
    }
}
