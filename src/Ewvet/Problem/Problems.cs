namespace Ewvet;

public static class Problems
{
    public static string Message(string code, string detail)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(detail);

        return code switch
        {
            "EW001" => "malformed encoded word " + Text.Show(Text.Clip(detail)),
            "EW002" => "unknown charset " + Text.Show(Text.Clip(detail)),
            "EW003" => "unknown encoding " + Text.Show(Text.Clip(detail)),
            "EW004" => "invalid base64 text",
            "EW005" => "base64 text is missing its padding",
            "EW006" => "invalid quoted-printable text",
            "EW007" => "bytes are not valid " + Text.Plain(detail),
            "EW008" => "encoded word has " + detail + " characters, the limit is 75",
            "EW009" => "decoded text has the control character " + detail,
            _ => throw new Failure("unknown problem code " + Text.Show(code)),
        };
    }
}
