using System.Diagnostics;
using System.IO;
using System.Reflection;
using Autodesk.Revit.Attributes;
using Backend.Server;
using Nice3point.Revit.Toolkit.External;

namespace Backend.Commands;

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class Command : ExternalCommand
{
    public override void Execute()
    {
        RunClient("Frontend.exe");

        var serverDispatcher = new ServerDispatcher();
        _ = serverDispatcher.ListenAndDispatchConnections();
    }

    private static void RunClient(string clientName)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!.AppendPath(clientName),
            Arguments = Process.GetCurrentProcess().Id.ToString()
        };

        Process.Start(startInfo);
    }
}