using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DualDeckEditorAddin
{
    public class ParameterUpdateHandler : IExternalEventHandler
    {
        private Document _doc;
        private FamilySymbol familySymbol;
        private FamilyManager familyManager;
        private FamilyType currentFamilyType;
        private Dictionary<string, string> changesTracker;
        private MainFormEditor _mainForm;

        public void Setup(Document _doc, FamilySymbol familySymbol, FamilyType familyType , Dictionary<string, string> tracker, MainFormEditor mainForm)
        {
            this._doc = _doc;
            this.familySymbol = familySymbol;
            this.changesTracker = new Dictionary<string, string>(tracker);
            this.currentFamilyType = familyType; // Setup with current family type
            _mainForm = mainForm;
        }

        public void Execute(UIApplication app)
        {
            Debug.Print("Executing update with changes count: " + changesTracker.Count); // Check if this shows and has count > 0
            using (Transaction tx = new Transaction(_doc, "Update Parameters"))
            {
                tx.Start();

                if (familySymbol != null)
                {
                    // Modify the family type parameters
                    foreach (var textBoxEntry in ControlMappings.TextBoxDimensionMappings.Concat(ControlMappings.TextBoxMappings))
                    {
                        if (changesTracker.TryGetValue(textBoxEntry.Key, out string newValue))
                        {
                            Parameter parameter = familySymbol.LookupParameter(textBoxEntry.Value);
                            if (parameter != null)
                            {
                                double decimalFeet = ConvertToDecimalFeet(newValue);
                                Debug.Print("Changing " + textBoxEntry.Value.ToString() + " to " + decimalFeet);
                                parameter.Set(decimalFeet);
                            }
                        }
                    }

                    foreach (var checkBoxEntry in ControlMappings.CheckBoxMappings)
                    {
                        if (changesTracker.TryGetValue(checkBoxEntry.Key, out string newCheckValue))
                        {
                            Parameter parameter = familySymbol.LookupParameter(checkBoxEntry.Value);
                            if (parameter != null)
                            {
                                int checkboxValue = newCheckValue == "1" ? 1 : 0;
                                Debug.Print("Changing " + checkBoxEntry.Value.ToString() + " to " + checkboxValue);
                                parameter.Set(checkboxValue);
                            }
                        }
                    }
                }

                try
                {
                    Debug.Print("Committing tx: Update parameters");
                    tx.Commit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to Commit. Rolling back.");
                    Debug.Print("Failed to commit transaction: " + ex.Message);
                    tx.RollBack();
                }
            }
            _mainForm.UpdateFamilyTypeData();
        }

        public double ConvertToDecimalFeet(string input)
        {
            double feet = 0;
            double inches = 0;

            // Split the input into feet and inches parts
            string[] parts = input.Split(new char[] { '\'', '"' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 0)
            {
                // Parse the feet part
                if (double.TryParse(parts[0], out double parsedFeet))
                {
                    feet = parsedFeet;
                }
            }

            if (parts.Length > 1)
            {
                // Parse the inches part, including fractional part if present
                string inchPart = parts[1].Trim();
                if (inchPart.Contains(" "))
                {
                    // Handle fractional inches
                    string[] inchParts = inchPart.Split(' ');
                    if (double.TryParse(inchParts[0], out double wholeInches))
                    {
                        inches = wholeInches;
                    }

                    if (inchParts.Length > 1)
                    {
                        string[] fractionParts = inchParts[1].Split('/');
                        if (fractionParts.Length == 2 &&
                            double.TryParse(fractionParts[0], out double numerator) &&
                            double.TryParse(fractionParts[1], out double denominator))
                        {
                            inches += numerator / denominator;
                        }
                    }
                }
                else
                {
                    // No fractional part
                    if (double.TryParse(inchPart, out double parsedInches))
                    {
                        inches = parsedInches;
                    }
                }
            }

            // Convert to decimal feet
            double decimalFeet = Math.Round(feet + (inches / 12.0) , 6);
            return decimalFeet;
        }

        public string GetName()
        {
            return "Parameter Update Handler";
        }
    }
}
