using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Backend.Server;

public static class NamedPipeUtil
{
    public static NamedPipeServerStream CreateServer(PipeDirection? pipeDirection = null)
    {
        const PipeOptions pipeOptions = PipeOptions.Asynchronous | PipeOptions.WriteThrough;
        return new NamedPipeServerStream(
            GetPipeName(),
            pipeDirection ?? PipeDirection.InOut,
            NamedPipeServerStream.MaxAllowedServerInstances,
            PipeTransmissionMode.Byte,
            pipeOptions);
    }

    private static string GetPipeName()
    {
        // Normalize away trailing slashes. File APIs include / exclude this with no 
        // discernable pattern. Easiest to normalize it here vs. auditing every caller
        // of this method.
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var serverDirectory = Path.GetDirectoryName(assemblyLocation)!.TrimEnd(Path.DirectorySeparatorChar);

        var pipeNameInput = $"{Environment.UserName}.{serverDirectory}";
        var hash = new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(pipeNameInput));

        return Convert.ToBase64String(hash)
            .Replace("/", "_")
            .Replace("=", string.Empty);
    }
}