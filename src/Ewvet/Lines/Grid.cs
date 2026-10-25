using System.Text;

namespace Ewvet;

public static class Grid
{
    public static string Render(List<List<string>> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        if (rows.Count == 0)
        {
            return string.Empty;
        }

        var columnCount = rows[0].Count;
        var widths = new int[columnCount];
        var cellWidths = new List<int[]>(rows.Count);

        foreach (var row in rows)
        {
            var rowWidths = new int[columnCount];
            for (var column = 0; column < columnCount; column++)
            {
                var width = Text.Cps(row[column]).Length;
                rowWidths[column] = width;
                widths[column] = Math.Max(widths[column], width);
            }

            cellWidths.Add(rowWidths);
        }

        var output = new StringBuilder();
        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var rowOutput = new StringBuilder();
            for (var column = 0; column < columnCount; column++)
            {
                if (column > 0)
                {
                    rowOutput.Append("  ");
                }

                rowOutput.Append(rows[rowIndex][column]);
                if (column < columnCount - 1)
                {
                    rowOutput.Append(' ', widths[column] - cellWidths[rowIndex][column]);
                }
            }

            while (rowOutput.Length > 0 && rowOutput[^1] == ' ')
            {
                rowOutput.Length--;
            }

            output.Append(rowOutput);
            output.Append('\n');
        }

        return output.ToString();
    }
}
