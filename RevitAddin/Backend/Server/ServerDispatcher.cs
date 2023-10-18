using System.IO;
using System.IO.Pipes;
using Backend.Core;

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
                if (request.Type == Request.RequestType.DeleteElements)
                {
                    await ProcessDeleteElementsAsync();
                }
            }
            catch (EndOfStreamException)
            {
                return; //Pipe disconnected
            }
        }
    }

    private async Task ProcessDeleteElementsAsync()
    {
        try
        {
            var deletedIds = await Application.AsyncEventHandler.RaiseAsync(_ => RevitApi.DeleteSelectedElements());
            await WriteResponseAsync(new DeletionCompletedResponse(deletedIds.Count));
        }
        catch (Exception exception)
        {
            await WriteResponseAsync(new RejectedResponse(exception.Message));
        }
    }

    /// <summary>
    ///     Write a Response to the client.
    /// </summary>
    public async Task WriteResponseAsync(Response response) => await response.WriteAsync(_server);
}