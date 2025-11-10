namespace RevitParametersCopier.Models;
public class ParamItem
{
    public ParamKeyType KeyType { get; set; }
    public string Key { get; set; } // for BuiltIn -> int string; for Shared -> GUID; for ByName -> Definition name
    public string Name { get; set; }
    public StorageType StorageType { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsElementId => StorageType == StorageType.ElementId;
    public string DisplayValue { get; set; } // AsValueString when available
    public string RawString { get; set; }    // fallback raw string representation
}

public enum ParamKeyType { BuiltIn, Shared, ByName }
