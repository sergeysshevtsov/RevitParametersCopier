namespace RevitParametersCopier.Models;
public static class ParameterClipboard
{
    public static ParamSnapshot Snapshot { get; private set; }

    public static bool HasData => Snapshot != null && Snapshot.Items.Count > 0;

    public static void Set(ParamSnapshot snap)
    {
        Snapshot = snap;
    }
}
