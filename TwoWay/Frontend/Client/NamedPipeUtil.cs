using System.IO;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;

namespace Frontend.Client;

public static class NamedPipeUtil
{
    /// <summary>
    /// Create a client for the current user only
    /// </summary>
    public static NamedPipeClientStream CreateClient(PipeDirection? pipeDirection = null)
    {
        const PipeOptions pipeOptions = PipeOptions.Asynchronous | PipeOptions.WriteThrough | PipeOptions.CurrentUserOnly;
        return new NamedPipeClientStream(".",
            GetPipeName(),
            pipeDirection ?? PipeDirection.InOut,
            pipeOptions);
    }

    private static string GetPipeName()
    {
        // Normalize away trailing slashes. File APIs include / exclude this with no 
        // discernable pattern. Easiest to normalize it here vs. auditing every caller
        // of this method.
        var clientDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);

        var pipeNameInput = $"{Environment.UserName}.{clientDirectory}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(pipeNameInput));

        return Convert.ToBase64String(bytes)
            .Replace("/", "_")
            .Replace("=", string.Empty);
    }
}