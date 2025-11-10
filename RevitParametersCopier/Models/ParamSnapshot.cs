namespace RevitParametersCopier.Models;
public class ParamSnapshot
{
    public string SourceElementDesc { get; set; }
    public IList<ParamItem> Items { get; } = new List<ParamItem>();
}

