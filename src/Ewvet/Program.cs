using System.Text;

namespace Ewvet;

public static class Program
{
    private static readonly UTF8Encoding Utf8 = new(false);

    public static int Main(string[] args)
    {
        return Cli.Run(args, new RealIo());
    }

    private sealed class RealIo : IIo
    {
        public byte[] ReadStdin()
        {
            using var input = Console.OpenStandardInput();
            using var buffer = new MemoryStream();
            input.CopyTo(buffer);
            return buffer.ToArray();
        }

        public byte[] ReadFile(string path)
        {
            try
            {
                return File.ReadAllBytes(path);
            }
            catch (ArgumentException exception)
            {
                throw new IOException("Invalid file path.", exception);
            }
            catch (NotSupportedException exception)
            {
                throw new IOException("Invalid file path.", exception);
            }
            catch (UnauthorizedAccessException exception)
            {
                throw new IOException("Cannot access file.", exception);
            }
        }

        public void Out(string text)
        {
            Write(Console.OpenStandardOutput(), text);
        }

        public void Err(string text)
        {
            Write(Console.OpenStandardError(), text);
        }

        private static void Write(Stream stream, string text)
        {
            byte[] bytes = Utf8.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }
    }
}

