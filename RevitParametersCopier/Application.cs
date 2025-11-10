using Nice3point.Revit.Toolkit.External;
using RevitParametersCopier.Commands;

namespace RevitParametersCopier;
/// <summary>
///     Application entry point
/// </summary>
[UsedImplicitly]
public class Application : ExternalApplication
{
    public override void OnStartup()
    {
        CreateRibbon();
    }

    private void CreateRibbon()
    {
        var panel = Application.CreatePanel("Parameters copier", "SHSS Tools");

        panel.AddPushButton<CmdRevitParametersCopy>("Parameters\ncopy")
            .SetImage("/RevitParametersCopier;component/Resources/Icons/copy16.png")
            .SetLargeImage("/RevitParametersCopier;component/Resources/Icons/copy32.png");

        panel.AddPushButton<CmdRevitParametersPaste>("Parameters\npaste")
            .SetImage("/RevitParametersCopier;component/Resources/Icons/paste16.png")
            .SetLargeImage("/RevitParametersCopier;component/Resources/Icons/paste32.png");

        //panel.AddPushButton<CmdRevitParametersPaste>("Parameters\npaste")
        //    .SetImage("/RevitParametersCopier;component/Resources/Icons/paste16.png")
        //    .SetLargeImage("/RevitParametersCopier;component/Resources/Icons/paste32.png");
    }
}