using System.IO.Pipes;

namespace Frontend.Client;

/// <summary>
///     This class manages the connections, timeout and general scheduling of requests to the server.
/// </summary>
public class ClientDispatcher
{
    private const int TimeOutNewProcess = 10000;

    private Task _connectionTask;
    private readonly NamedPipeClientStream _client = NamedPipeUtil.CreateClient(PipeDirection.Out);

    /// <summary>
    ///     Connects to server without awaiting
    /// </summary>
    public void ConnectToServer()
    {
        _connectionTask = _client.ConnectAsync(TimeOutNewProcess);
    }

    /// <summary>
    ///     Write a Request to the server.
    /// </summary>
    public async Task WriteRequestAsync(Request request)
    {
        await _connectionTask;
        await request.WriteAsync(_client);
    }
}