using System.Diagnostics;
using System.Windows;
using Frontend.Client;
using Frontend.ViewModels;
using Frontend.Views;

namespace Frontend;

/// <summary>
///     Programme entry point
/// </summary>
public sealed partial class App
{
    public static ClientDispatcher ClientDispatcher { get; }

    static App()
    {
        ClientDispatcher = new ClientDispatcher();
        ClientDispatcher.ConnectToServer();
    }

    protected override void OnStartup(StartupEventArgs args)
    {
        ParseCommandArguments(args.Args);

        var viewModel = new MainViewModel();
        var view = new MainView(viewModel);
        view.Show();
    }

    private void ParseCommandArguments(string[] args)
    {
        var ownerPid = args[0];
        var ownerProcess = Process.GetProcessById(int.Parse(ownerPid));
        ownerProcess.EnableRaisingEvents = true;
        ownerProcess.Exited += (_, _) => Shutdown();
    }
}