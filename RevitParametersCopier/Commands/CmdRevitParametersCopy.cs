using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using RevitParametersCopier.Models;

namespace RevitParametersCopier.Commands;

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class CmdRevitParametersCopy : IExternalCommand
{
    private const bool IncludeElementIdParameters = false;
    private const bool TrimStrings = true;

    public Result Execute(ExternalCommandData data, ref string message, ElementSet elements)
    {
        UIDocument UIDoc = data.Application.ActiveUIDocument;
        Document document = UIDoc.Document;

        ICollection<ElementId> selectedIds = UIDoc.Selection.GetElementIds();
        if (selectedIds.Count != 1)
        {
            TaskDialog.Show("Copy parameters", "Please select exactly one element.");
            return Result.Cancelled;
        }

        Element element = document.GetElement(selectedIds.First());
        if (element == null)
        {
            TaskDialog.Show("Copy parameters", "No element found.");
            return Result.Failed;
        }

        ParamSnapshot snap = new()
        {
            SourceElementDesc = $"{element.Category?.Name} : {element.Name} (Id {element.Id.Value})"
        };

        foreach (Parameter p in element.GetOrderedParameters())
        {
            if (p == null || p.Definition == null) continue;

            ParamItem item = new()
            {
                Name = p.Definition.Name,
                StorageType = p.StorageType,
                IsReadOnly = p.IsReadOnly,
                DisplayValue = SafeAsValueString(p),
                RawString = SafeAsRawString(p)
            };

            if (p.IsShared && p.GUID != Guid.Empty)
            {
                item.KeyType = ParamKeyType.Shared;
                item.Key = p.GUID.ToString();
            }
            else 
                if (Enum.IsDefined(typeof(BuiltInParameter), p.Id.Value))
                {
                    item.KeyType = ParamKeyType.BuiltIn;
                    item.Key = p.Id.Value.ToString();
                }
                else
                {
                    item.KeyType = ParamKeyType.ByName;
                    item.Key = p.Definition.Name;
                }

            snap.Items.Add(item);
        }

        ParameterClipboard.Set(snap);
        TaskDialog.Show("Copy parameters", $"Copied {snap.Items.Count} parameters from\n{snap.SourceElementDesc}.");
        return Result.Succeeded;
    }

    private static string SafeAsValueString(Parameter p)
    {
        try
        {
            string v = p.AsValueString();
            if (!string.IsNullOrEmpty(v)) return v;
        }
        catch { }
        return SafeAsRawString(p);
    }

    private static string SafeAsRawString(Parameter p)
    {
        try
        {
            switch (p.StorageType)
            {
                case StorageType.String:
                    return p.AsString();
                case StorageType.Integer:
                    return p.AsInteger().ToString();
                case StorageType.Double:
                    return p.AsDouble().ToString(System.Globalization.CultureInfo.InvariantCulture);
                case StorageType.ElementId:
                    return p.AsElementId()?.Value.ToString();
                case StorageType.None:
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }
        catch { }
        return string.Empty;
    }
}