using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using RevitParametersCopier.Models;

namespace RevitParametersCopier.Commands;
[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class CmdRevitParametersPaste : IExternalCommand
{
    public Result Execute(ExternalCommandData data, ref string message, ElementSet elements)
    {
        UIDocument UIDoc = data.Application.ActiveUIDocument;
        Document document = UIDoc.Document;

        var elementIds = UIDoc.Selection.GetElementIds();
        if (elementIds.Count == 0)
        {
            TaskDialog.Show("Paste parameters", "Select one or more target elements.");
            return Result.Cancelled;
        }

        if (!ParameterClipboard.HasData)
        {
            TaskDialog.Show("Paste parameters", "Clipboard is empty. Use 'Copy parameters' first.");
            return Result.Cancelled;
        }

        var snap = ParameterClipboard.Snapshot;
        int setCount = 0;
        int skipped = 0;

        using (Transaction transaction = new(document, "Paste parameters"))
        {
            transaction.Start();
            foreach (var elementId in elementIds)
            {
                var element = document.GetElement(elementId);
                if (element == null)
                {
                    skipped++;
                    continue;
                }

                foreach (var item in snap.Items)
                {
                    if (item.IsReadOnly || item.IsElementId)
                        continue;

                    Parameter target = FindMatchingParameter(element, item);
                    if (target == null || target.IsReadOnly)
                    {
                        skipped++;
                        continue;
                    }

                    bool ok = TrySetParameterValue(target, item);
                    if (ok) setCount++; else skipped++;
                }
            }
            transaction.Commit();
        }

        TaskDialog.Show("Paste parameters", $"Parameters set: {setCount}\nSkipped: {skipped}");
        return Result.Succeeded;
    }

    private static Parameter FindMatchingParameter(Element element, ParamItem item)
    {
        switch (item.KeyType)
        {
            case ParamKeyType.Shared:
                return element.GetParameters(item.Name)
                         ?.FirstOrDefault(p => p.IsShared && p.GUID.ToString().Equals(item.Key, StringComparison.OrdinalIgnoreCase));
            case ParamKeyType.BuiltIn:
                if (int.TryParse(item.Key, out int bipInt))
                {
                    var bip = (BuiltInParameter)bipInt;
                    return element.get_Parameter(bip);
                }
                break;
            case ParamKeyType.ByName:
                return element.GetParameters(item.Name)?.FirstOrDefault();
        }
        return null;
    }

    private static bool TrySetParameterValue(Parameter p, ParamItem item)
    {
        try
        {
            if (!string.IsNullOrEmpty(item.DisplayValue))
            {
                try
                {
                    if (p.SetValueString(item.DisplayValue)) return true;
                }
                catch { }
            }

            switch (p.StorageType)
            {
                case StorageType.String:
                    return p.Set(item.RawString ?? item.DisplayValue ?? string.Empty);
                case StorageType.Integer:
                    if (int.TryParse(item.RawString, out int ival)) return p.Set(ival);
                    if (int.TryParse(item.DisplayValue, out ival)) return p.Set(ival);
                    break;
                case StorageType.Double:
                    if (double.TryParse(item.RawString, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double dval))
                        return p.Set(dval);
                    if (double.TryParse(item.DisplayValue, out dval)) return p.Set(dval);
                    break;
                case StorageType.ElementId:
                    if (int.TryParse(item.RawString ?? item.DisplayValue, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int idVal))
                    {
                        if (idVal == ElementId.InvalidElementId.Value)
                        {
                            return p.Set(ElementId.InvalidElementId);
                        }
                        var eid = new ElementId(idVal);
                        var ownerDoc = p.Element?.Document;
                        if (ownerDoc != null && ownerDoc.GetElement(eid) != null)
                        {
                            return p.Set(eid);
                        }
                    }
                    break;
            }
        }
        catch { }
        return false;
    }
}