using System.Text;

namespace Ewvet;

public interface IIo
{
    byte[] ReadStdin();

    byte[] ReadFile(string path);

    void Out(string text);

    void Err(string text);
}

public class Inputs
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    private readonly IIo io;

    private bool stdinRead;

    public Inputs(IIo io)
    {
        this.io = io ?? throw new ArgumentNullException(nameof(io));
    }

    public record Source(string Display, string Text);

    public Source Read(string arg)
    {
        var isStdin = arg == "-";
        var display = isStdin ? "<stdin>" : Text.Plain(arg);

        if (isStdin && stdinRead)
        {
            throw new Failure("standard input is named twice");
        }

        byte[] bytes;
        try
        {
            if (isStdin)
            {
                stdinRead = true;
                bytes = io.ReadStdin();
            }
            else
            {
                bytes = io.ReadFile(arg);
            }
        }
        catch (IOException)
        {
            throw new Failure(display + ": cannot read the file");
        }

        try
        {
            return new Source(display, StrictUtf8.GetString(bytes));
        }
        catch (DecoderFallbackException)
        {
            throw new Failure(display + ": input is not valid UTF-8");
        }
    }
}
