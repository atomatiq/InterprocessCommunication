using System.IO;
using System.Text;

namespace Frontend.Client;

/// <summary>
/// Represents a request from the client. A request is as follows.
/// 
///  Field Name         Type                Size (bytes)
/// ----------------------------------------------------
///  RequestType        enum RequestType    4
///  RequestBody        Request subclass    variable
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

    protected abstract void AddRequestBody(BinaryWriter writer);

    /// <summary>
    ///     Write a Request to the stream.
    /// </summary>
    public async Task WriteAsync(Stream outStream)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream, Encoding.Unicode);

        writer.Write((int) Type);
        AddRequestBody(writer);
        writer.Flush();

        // Write the length of the request
        var length = checked((int) memoryStream.Length);

        // There is no way to know the number of bytes written to
        // the pipe stream. We just have to assume all of them are written
        await outStream.WriteAsync(BitConverter.GetBytes(length), 0, 4);
        memoryStream.Position = 0;
        await memoryStream.CopyToAsync(outStream, length);
    }

    /// <summary>
    /// Write a string to the Writer where the string is encoded
    /// as a length prefix (signed 32-bit integer) follows by
    /// a sequence of characters.
    /// </summary>
    protected static void WriteLengthPrefixedString(BinaryWriter writer, string value)
    {
        writer.Write(value.Length);
        writer.Write(value.ToCharArray());
    }
}

/// <summary>
/// Represents a Request from the client. A Request is as follows.
/// 
///  Field Name         Type            Size (bytes)
/// --------------------------------------------------
///  RequestType        Integer         4
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

    protected override void AddRequestBody(BinaryWriter writer)
    {
        WriteLengthPrefixedString(writer, Message);
    }
}

/// <summary>
/// Represents a Request from the client. A Request is as follows.
/// 
///  Field Name         Type            Size (bytes)
/// --------------------------------------------------
///  RequestType        Integer         4
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

    protected override void AddRequestBody(BinaryWriter writer)
    {
        writer.Write(Iterations);
        writer.Write(ForceUpdate);
        WriteLengthPrefixedString(writer, ModelName);
    }
}
