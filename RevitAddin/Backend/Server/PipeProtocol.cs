using System.IO;
using System.Text;

namespace Backend.Server;

/// <summary>
/// Represents a request from the client. A request is as follows.
/// 
///  Field Name         Type                Size (bytes)
/// ----------------------------------------------------
///  RequestType       enum RequestType   4
///  RequestBody       Request subclass   variable
/// 
/// </summary>
public abstract class Request
{
    public enum RequestType
    {
        DeleteElements
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

        var requestBuffer = new byte[length];
        await ReadAllAsync(stream, requestBuffer, requestBuffer.Length);

        using var reader = new BinaryReader(new MemoryStream(requestBuffer), Encoding.Unicode);

        var requestType = (RequestType) reader.ReadInt32();
        return requestType switch
        {
            RequestType.DeleteElements => DeleteElementsRequest.Create(reader),
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
///  RequestType        Integer         4
///  Iterations         Integer         4
///  ForceUpdate        Boolean         1
///  ModelName          String          Variable
/// 
/// Strings are encoded via a character count prefix as a 
/// 32-bit integer, followed by an array of characters.
/// 
/// </summary>
public class DeleteElementsRequest : Request
{
    public override RequestType Type => RequestType.DeleteElements;

    public static DeleteElementsRequest Create(BinaryReader reader) => new();
}

/// <summary>
/// Base class for all possible responses to a request.
/// The ResponseType enum should list all possible response types
/// and ReadResponse creates the appropriate response subclass based
/// on the response type sent by the client.
/// The format of a response is:
///
/// Field Name       Field Type          Size (bytes)
/// -------------------------------------------------
/// ResponseType     enum ResponseType   4
/// ResponseBody     Response subclass   variable
/// 
/// </summary>
public abstract class Response
{
    public enum ResponseType
    {
        // The request completed on the server and the results are contained in the message. 
        Success,

        // The request was rejected by the server.
        Rejected
    }

    public abstract ResponseType Type { get; }

    protected abstract void AddResponseBody(BinaryWriter writer);

    /// <summary>
    ///     Write a Response to the stream.
    /// </summary>
    public async Task WriteAsync(Stream outStream)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream, Encoding.Unicode);

        writer.Write((int) Type);
        AddResponseBody(writer);
        writer.Flush();

        // Write the length of the response
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
/// Represents a Response from the server. A Request is as follows.
/// 
///  Field Name         Type            Size (bytes)
/// --------------------------------------------------
///  RequestType        Integer         4
///  Changes            Integer         4
///  Version            String          Variable
/// 
/// Strings are encoded via a character count prefix as a 
/// 32-bit integer, followed by an array of characters.
/// 
/// </summary>
public class DeletionCompletedResponse : Response
{
    public int Changes { get; }

    public override ResponseType Type => ResponseType.Success;

    public DeletionCompletedResponse(int changes)
    {
        Changes = changes;
    }

    protected override void AddResponseBody(BinaryWriter writer)
    {
        writer.Write(Changes);
    }
}

/// <summary>
/// Represents a Response from the server. A Request is as follows.
/// 
///  Field Name         Type            Size (bytes)
/// --------------------------------------------------
///  RequestType        Integer         4
///  Reason             String          Variable
/// 
/// Strings are encoded via a character count prefix as a 
/// 32-bit integer, followed by an array of characters.
/// 
/// </summary>
public class RejectedResponse : Response
{
    public string Reason { get; }

    public override ResponseType Type => ResponseType.Rejected;

    public RejectedResponse(string reason)
    {
        Reason = reason;
    }

    protected override void AddResponseBody(BinaryWriter writer)
    {
        WriteLengthPrefixedString(writer, Reason);
    }
}