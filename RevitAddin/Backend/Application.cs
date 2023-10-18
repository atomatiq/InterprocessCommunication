using Autodesk.Revit.DB;
using Nice3point.Revit.Toolkit.External;
using Backend.Commands;
using Backend.Core;
using Nice3point.Revit.Toolkit.External.Handlers;

namespace Backend;

[UsedImplicitly]
public class Application : ExternalApplication
{
    public static AsyncEventHandler<ICollection<ElementId>> AsyncEventHandler { get; set; }

    public override void OnStartup()
    {
        RevitApi.UiApplication = UiApplication;
        AsyncEventHandler = new AsyncEventHandler<ICollection<ElementId>>();
        CreateRibbon();
    }

    private void CreateRibbon()
    {
        var panel = Application.CreatePanel("Commands", "Backend");

        var showButton = panel.AddPushButton<Command>("Execute");
        showButton.SetImage("/Backend;component/Resources/Icons/RibbonIcon16.png");
        showButton.SetLargeImage("/Backend;component/Resources/Icons/RibbonIcon32.png");
    }
}