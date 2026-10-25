using System.Text;

namespace Ewvet;

public static class Chunker
{
    public sealed record Piece(int Cp, byte[] Bytes);

    public static string Words(string charset, string choice, List<Piece> pieces)
    {
        if (pieces.Count == 0)
        {
            return string.Empty;
        }

        decimal totalBytes = 0;
        decimal quotedLength = 0;

        foreach (Piece piece in pieces)
        {
            totalBytes += piece.Bytes.Length;
            if (choice == "auto")
            {
                quotedLength += QText.Encode(piece.Bytes).Length;
            }
        }

        string encoding = choice switch
        {
            "b" => "B",
            "q" => "Q",
            "auto" => quotedLength <= 4m * decimal.Ceiling(totalBytes / 3m) ? "Q" : "B",
            _ => throw new Failure("invalid encoding")
        };

        int room = 75 - charset.Length - 7;
        int byteCapacity = room / 4 * 3;
        StringBuilder output = new();
        List<byte> currentBytes = [];
        int currentQuotedLength = 0;
        int currentByteCount = 0;

        foreach (Piece piece in pieces)
        {
            int pieceQuotedLength = QText.Encode(piece.Bytes).Length;
            bool doesNotFit = encoding == "Q"
                ? (long)currentQuotedLength + pieceQuotedLength > room
                : (long)currentByteCount + piece.Bytes.Length > byteCapacity;

            if (currentBytes.Count > 0 && doesNotFit)
            {
                AppendWord(output, charset, encoding, currentBytes);
                currentBytes.Clear();
                currentQuotedLength = 0;
                currentByteCount = 0;
            }

            currentBytes.AddRange(piece.Bytes);
            currentQuotedLength += pieceQuotedLength;
            currentByteCount += piece.Bytes.Length;
        }

        if (currentBytes.Count > 0)
        {
            AppendWord(output, charset, encoding, currentBytes);
        }

        return output.ToString();
    }

    private static void AppendWord(StringBuilder output, string charset, string encoding, List<byte> bytes)
    {
        if (output.Length > 0)
        {
            output.Append(' ');
        }

        output.Append("=?");
        output.Append(charset);
        output.Append('?');
        output.Append(encoding);
        output.Append('?');
        output.Append(encoding == "B" ? Base64.Encode(bytes.ToArray()) : QText.Encode(bytes.ToArray()));
        output.Append("?=");
    }
}
