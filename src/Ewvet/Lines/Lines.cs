namespace Ewvet;

public class Lines
{
    private readonly int[] lineStarts;

    private readonly int length;

    public Lines(int[] text)
    {
        ArgumentNullException.ThrowIfNull(text);

        length = text.Length;
        var starts = new List<int> { 0 };
        for (var index = 0; index < text.Length; index++)
        {
            if (text[index] == '\n')
            {
                starts.Add(index + 1);
            }
        }

        lineStarts = starts.ToArray();
    }

    public int Line(int index)
    {
        return FindLineStart(index) + 1;
    }

    public int Col(int index)
    {
        var lineStart = lineStarts[FindLineStart(index)];
        return index - lineStart + 1;
    }

    private int FindLineStart(int index)
    {
        if (index < 0 || index > length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        var low = 0;
        var high = lineStarts.Length - 1;
        while (low <= high)
        {
            var middle = low + ((high - low) / 2);
            if (lineStarts[middle] <= index)
            {
                low = middle + 1;
            }
            else
            {
                high = middle - 1;
            }
        }

        return high;
    }
}
