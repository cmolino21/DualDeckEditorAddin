using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualDeckEditorAddin
{
    public class ParameterUpdateHandler : IExternalEventHandler
    {
        private Document doc;
        private FamilyManager familyManager;
        private Dictionary<string, string> changesTracker;

        public void Setup(Document doc, FamilyManager manager, Dictionary<string, string> tracker)
        {
            this.doc = doc;
            this.familyManager = manager;
            this.changesTracker = new Dictionary<string, string>(tracker);
        }

        public void Execute(UIApplication app)
        {
            using (Transaction tx = new Transaction(doc, "Update Parameters"))
            {
                tx.Start();
                // Process text box changes
                foreach (var textBoxEntry in ControlMappings.TextBoxDimensionMappings.Concat(ControlMappings.TextBoxMappings))
                {
                    if (changesTracker.TryGetValue(textBoxEntry.Key, out string newValue))
                    {
                        var parameter = familyManager.get_Parameter(textBoxEntry.Value);
                        if (parameter != null)
                        {
                            UpdateFamilyParameter(parameter, newValue);
                        }
                    }
                }
                // Process checkbox changes
                foreach (var checkBoxEntry in ControlMappings.CheckBoxMappings)
                {
                    if (changesTracker.TryGetValue(checkBoxEntry.Key, out string newCheckValue))
                    {
                        var parameter = familyManager.get_Parameter(checkBoxEntry.Value);
                        if (parameter != null)
                        {
                            int checkboxValue = newCheckValue == "1" ? 1 : 0;
                            UpdateFamilyParameter(parameter, checkboxValue.ToString());
                        }
                    }
                }
                tx.Commit();
            }
        }

        private void UpdateFamilyParameter(FamilyParameter parameter, string value)
        {
            if (parameter.StorageType == StorageType.String)
            {
                familyManager.Set(parameter, value);
            }
            else if (parameter.StorageType == StorageType.Integer)
            {
                if (int.TryParse(value, out int intValue))
                {
                    familyManager.Set(parameter, intValue);
                }
            }
            else if (parameter.StorageType == StorageType.Double)
            {
                if (double.TryParse(value, out double doubleValue))
                {
                    familyManager.Set(parameter, doubleValue);
                }
            }
            // Add other storage types handling as needed
        }

        public string GetName()
        {
            return "Parameter Update Handler";
        }
    }
}
