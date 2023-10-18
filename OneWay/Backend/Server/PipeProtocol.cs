using System.Text;

namespace Backend.Server;

/// <summary>
/// Represents a request from the client. A request is as follows.
/// 
///  Field Name         Type                Size (bytes)
/// ----------------------------------------------------
///  ResponseType       enum ResponseType   4
///  ResponseBody       Response subclass   variable
/// 
/// </summary>
public abstract class Request
{
    public enum RequestType
    {
        PrintMessage,
        UpdateModel
    }

    public abstract RequestType Type { get; }

    /// <summary>
    ///     Read a Request from the given stream.
    /// </summary>
    public static async Task<Request> ReadAsync(Stream stream)
    {
        var lengthBuffer = new byte[4];
        await ReadAllAsync(stream, lengthBuffer, 4).ConfigureAwait(false);
        var length = BitConverter.ToUInt32(lengthBuffer, 0);

        var responseBuffer = new byte[length];
        await ReadAllAsync(stream, responseBuffer, responseBuffer.Length);

        using var reader = new BinaryReader(new MemoryStream(responseBuffer), Encoding.Unicode);

        var responseType = (RequestType) reader.ReadInt32();
        return responseType switch
        {
            RequestType.PrintMessage => PrintMessageRequest.Create(reader),
            RequestType.UpdateModel => UpdateModelRequest.Create(reader),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    /// <summary>
    /// This task does not complete until we are completely done reading.
    /// </summary>
    private static async Task ReadAllAsync(Stream stream, byte[] buffer, int count)
    {
        var totalBytesRead = 0;
        do
        {
            var bytesRead = await stream.ReadAsync(buffer, totalBytesRead, count - totalBytesRead);
            if (bytesRead == 0) throw new EndOfStreamException("Reached end of stream before end of read.");
            totalBytesRead += bytesRead;
        } while (totalBytesRead < count);
    }

    /// <summary>
    /// Read a string from the Reader where the string is encoded
    /// as a length prefix (signed 32-bit integer) followed by
    /// a sequence of characters.
    /// </summary>
    protected static string ReadLengthPrefixedString(BinaryReader reader)
    {
        var length = reader.ReadInt32();
        return length < 0 ? null : new string(reader.ReadChars(length));
    }
}

/// <summary>
/// Represents a Request from the client. A Request is as follows.
/// 
///  Field Name         Type            Size (bytes)
/// --------------------------------------------------
///  ResponseType       Integer         4
///  Message            String          Variable
/// 
/// Strings are encoded via a character count prefix as a 
/// 32-bit integer, followed by an array of characters.
/// 
/// </summary>
public class PrintMessageRequest : Request
{
    public string Message { get; }

    public override RequestType Type => RequestType.PrintMessage;

    public PrintMessageRequest(string message)
    {
        Message = message;
    }

    public static PrintMessageRequest Create(BinaryReader reader)
    {
        var message = ReadLengthPrefixedString(reader);
        return new PrintMessageRequest(message);
    }
}

/// <summary>
/// Represents a Request from the client. A Request is as follows.
/// 
///  Field Name         Type            Size (bytes)
/// --------------------------------------------------
///  ResponseType       Integer         4
///  Iterations         Integer         4
///  ForceUpdate        Boolean         1
///  ModelName          String          Variable
/// 
/// Strings are encoded via a character count prefix as a 
/// 32-bit integer, followed by an array of characters.
/// 
/// </summary>
public class UpdateModelRequest : Request
{
    public int Iterations { get; }
    public bool ForceUpdate { get; }
    public string ModelName { get; }

    public override RequestType Type => RequestType.UpdateModel;

    public UpdateModelRequest(string modelName, int iterations, bool forceUpdate)
    {
        Iterations = iterations;
        ForceUpdate = forceUpdate;
        ModelName = modelName;
    }

    public static UpdateModelRequest Create(BinaryReader reader)
    {
        var iterations = reader.ReadInt32();
        var forceUpdate = reader.ReadBoolean();
        var modelName = ReadLengthPrefixedString(reader);
        return new UpdateModelRequest(modelName, iterations, forceUpdate);
    }
}