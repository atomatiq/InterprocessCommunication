using System.Diagnostics;
using Backend.Server;

Process.Start("Frontend.exe");

var serverDispatcher = new ServerDispatcher();
await serverDispatcher.ListenAndDispatchConnections();