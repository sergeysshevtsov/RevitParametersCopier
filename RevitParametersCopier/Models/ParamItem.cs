namespace RevitParametersCopier.Models;
public class ParamItem
{
    // Identity to find matching parameter on target elements
    public ParamKeyType KeyType { get; set; }
    public string Key { get; set; } // for BuiltIn -> int string; for Shared -> GUID; for ByName -> Definition name

    // For UI
    public string Name { get; set; }
    public StorageType StorageType { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsElementId => StorageType == StorageType.ElementId;

    // Values stored both as raw and display strings when possible
    public string DisplayValue { get; set; } // AsValueString when available; editable in UI (except ElementId)
    public string RawString { get; set; }    // fallback raw string representation
}

public enum ParamKeyType { BuiltIn, Shared, ByName }
