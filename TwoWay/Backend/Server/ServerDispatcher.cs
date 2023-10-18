using System.IO.Pipes;

namespace Backend.Server;

/// <summary>
///     This class manages the connections, timeout and general scheduling of the client requests.
/// </summary>
public class ServerDispatcher
{
    private readonly NamedPipeServerStream _server = NamedPipeUtil.CreateServer(PipeDirection.InOut);

    /// <summary>
    ///     This function will accept and process new requests until the client disconnects from the server
    /// </summary>
    public async Task ListenAndDispatchConnections()
    {
        try
        {
            await _server.WaitForConnectionAsync();
            await ListenAndDispatchConnectionsCoreAsync();
        }
        finally
        {
            _server.Close();
        }
    }

    private async Task ListenAndDispatchConnectionsCoreAsync()
    {
        while (_server.IsConnected)
        {
            try
            {
                var request = await Request.ReadAsync(_server);
                if (request.Type == Request.RequestType.PrintMessage)
                {
                    var printRequest = (PrintMessageRequest) request;
                    Console.WriteLine($"Message from client: {printRequest.Message}");
                }
                else if (request.Type == Request.RequestType.UpdateModel)
                {
                    var printRequest = (UpdateModelRequest) request;

                    await Task.Delay(TimeSpan.FromSeconds(2));
                    Console.WriteLine($"The {printRequest.ModelName} model has been {(printRequest.ForceUpdate ? "forcibly" : string.Empty)} updated {printRequest.Iterations} times");

                    await WriteResponseAsync(new UpdateCompletedResponse(69, "2.1.7"));
                }
            }
            catch (EndOfStreamException)
            {
                return; //Pipe disconnected
            }
        }
    }
    
    /// <summary>
    ///     Write a Response to the client.
    /// </summary>
    public async Task WriteResponseAsync(Response response) => await response.WriteAsync(_server);
}