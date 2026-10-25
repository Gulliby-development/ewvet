namespace Ewvet;

public static class Charsets
{
    public sealed record Charset(string Name, IReadOnlyList<string> Aliases, string Bytes);

    public static List<Charset> All() =>
    [
        new("iso-8859-1", Array.AsReadOnly(new[] { "iso8859-1", "l1", "latin1" }), "1"),
        new("iso-8859-15", Array.AsReadOnly(new[] { "iso8859-15", "l9", "latin9" }), "1"),
        new("us-ascii", Array.AsReadOnly(new[] { "ascii", "us" }), "1"),
        new("utf-8", Array.AsReadOnly(new[] { "utf8" }), "1 to 4")
    ];

    public static Charset? Find(string name)
    {
        string foldedName = Text.Fold(name);
        foreach (Charset charset in All())
        {
            if (string.Equals(charset.Name, foldedName, StringComparison.Ordinal))
            {
                return charset;
            }

            foreach (string alias in charset.Aliases)
            {
                if (string.Equals(alias, foldedName, StringComparison.Ordinal))
                {
                    return charset;
                }
            }
        }

        return null;
    }

}
