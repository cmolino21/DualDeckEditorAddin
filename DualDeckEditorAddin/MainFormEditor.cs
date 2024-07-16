using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DualDeckEditorAddin.MainFormEditor;
using BuildingCoder;

namespace DualDeckEditorAddin
{
    public partial class MainFormEditor : System.Windows.Forms.Form
    {
        private Document _doc;
        private ElementId _initialSelectedId = null; // If a DualDeck is selected in the document when the program is launched, store it here
        public MainFormEditor(Document doc, UIDocument uidoc)
        {
            InitializeComponent();
            _doc = doc;
            InitializeDualDeckSelection(uidoc);
            comboBoxFamilyType.SelectedIndexChanged += comboBoxFamilyType_SelectedIndexChanged; // Attach the event handler for updating the data based on DualDeck selection
        }

        
        // If a DualDeck is selected in the document when the program is launched, this will collect that ID
        private void InitializeDualDeckSelection(UIDocument uidoc)
        {
            var selectedIds = uidoc.Selection.GetElementIds();
            if (selectedIds.Count == 1) // Ensure only one element is selected
            {
                Element selectedElement = _doc.GetElement(selectedIds.First());
                if (selectedElement is FamilyInstance instance)
                {
                    if (instance.Symbol.Family.Name == "VDC DualDeck_Mirror" || instance.Symbol.Family.Name == "VDC DualDeck_Adjust")
                    {
                        _initialSelectedId = instance.Symbol.Id;
                    }
                }
                // Check if the selected element is an AssemblyInstance and search for DualDeck within it
                else if (selectedElement is AssemblyInstance assembly)
                {
                    // Iterate through the members of the assembly
                    var memberIds = assembly.GetMemberIds();
                    foreach (ElementId memberId in memberIds)
                    {
                        Element member = _doc.GetElement(memberId);
                        if (member is FamilyInstance memberInstance &&
                            (memberInstance.Symbol.Family.Name == "VDC DualDeck_Mirror" ||
                             memberInstance.Symbol.Family.Name == "VDC DualDeck_Adjust"))
                        {
                            _initialSelectedId = memberInstance.Symbol.Id;
                            return; // Exit after finding the first match
                        }
                    }
                }
            }
        }



        private void comboBoxFamilyType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxFamilyType.SelectedItem is FamilyTypeItem selectedFamilyType)
            {
                // Use a filtered element collector to find all instances of the selected family type
                var collector = new FilteredElementCollector(_doc)
                    .OfClass(typeof(FamilyInstance))
                    .WhereElementIsNotElementType()
                    .Where(x => ((FamilyInstance)x).Symbol.Id == selectedFamilyType.Id);

                // Attempt to retrieve the first instance found; this will be used to access the family data
                var selectedInstance = collector.FirstOrDefault() as FamilyInstance;

                if (selectedInstance != null)
                {

                    Family family = selectedInstance.Symbol.Family; // Retrieve the family from the instance symbol

                    Document familyDoc = _doc.EditFamily( family ); // Open the family for editing which returns the family document

                    FamilyManager familyManager = familyDoc.FamilyManager; // Get the family manager which manages the parameters of the family

                    // Retrieve and print the number of parameters in the family
                    int n = familyManager.Parameters.Size; 

                    Debug.Print("\nFamily {0} has {1} parameter{2}", _doc.Title, n, Util.PluralSuffix( n ) );

                    // Create a dictionary to map parameter names to their corresponding FamilyParameter objects
                    Dictionary<string, FamilyParameter> fps = new Dictionary<string, FamilyParameter>(n);

                    foreach (FamilyParameter fp in familyManager.Parameters)
                    {
                        string name = fp.Definition.Name;
                        fps.Add (name, fp );
                    }
                    // Sort the keys (parameter names) for better manageability
                    List<string> keys = new List<string>( fps.Keys );
                    keys.Sort();

                    // Print the number of types in the family and their names
                    n = familyManager.Types.Size;

                    //Debug.Print("\nFamily {0} has {1} type{2}{3}", _doc.Title, n, Util.PluralSuffix(n), Util.DotOrColon(n));

                    string matchName = selectedFamilyType.ToString()
                            .Replace("DD Mirror: ", "")
                            .Replace("DD Adjust: ", "");

                    Dictionary<string, string> parameterValues = new Dictionary<string, string>(); // Dictionary to store parameter values

                    // Iterate through each type in the family
                    foreach (Autodesk.Revit.DB.FamilyType t in familyManager.Types)
                    {
                        // Get the match name and remove "DD Mirror: " or "DD Adjust: " from it
                        if (t.Name == matchName)
                        {
                            // For each parameter key, check if the type has a value set and print it
                            foreach (string key in keys)
                            {
                                FamilyParameter fp = fps[key];
                                if (t.HasValue(fp))
                                {
                                    string value = Util.FamilyParamValueString(t, fp, _doc);
                                    parameterValues[key] = value; // Store the parameter value in the dictionary
                                    //Debug.Print("    {0} = {1}", key, value);
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }


                    // Set dimension text box values
                    foreach (var entry in ControlMappings.TextBoxDimensionMappings)
                    {
                        var textBox = this.Controls.Find(entry.Key, true).FirstOrDefault() as System.Windows.Forms.TextBox;
                        if (textBox != null)
                        {
                            textBox.Text = parameterValues.ContainsKey(entry.Value) ? parameterValues[entry.Value] : "Error";
                        }
                    }

                    //// Set dimension text box values
                    //foreach (var entry in ControlMappings.TextBoxDimensionMappings)
                    //{
                    //    var textBox = this.Controls.Find(entry.Key, true).FirstOrDefault() as System.Windows.Forms.TextBox;
                    //    if (textBox != null)
                    //    {
                    //        if (parameterValues.ContainsKey(entry.Value))
                    //        {
                    //            double decimalFeet;
                    //            if (double.TryParse(parameterValues[entry.Value], out decimalFeet))
                    //            {
                    //                textBox.Text = parameterValues[entry.Value] + " => " + UnitConverter.ConvertFeetToFeetAndFractionalInches(decimalFeet);
                    //            }
                    //            else
                    //            {
                    //                textBox.Text = "Error";
                    //            }
                    //        }
                    //        else
                    //        {
                    //            textBox.Text = "Error";
                    //        }
                    //    }
                    //}

                    // Set text box values
                    foreach (var entry in ControlMappings.TextBoxMappings)
                    {
                        var textBox = this.Controls.Find(entry.Key, true).FirstOrDefault() as System.Windows.Forms.TextBox;
                        if (textBox != null)
                        {
                            textBox.Text = parameterValues.ContainsKey(entry.Value) ? parameterValues[entry.Value] : "Error";
                        }
                    }

                    // Set checkbox values
                    foreach (var entry in ControlMappings.CheckBoxMappings)
                    {
                        var checkBox = this.Controls.Find(entry.Key, true).FirstOrDefault() as System.Windows.Forms.CheckBox;
                        if (checkBox != null)
                        {
                            checkBox.Checked = parameterValues.ContainsKey(entry.Value) && parameterValues[entry.Value] == "1";
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No instances of the selected family type were found.");
                }
            }
        }

        public static class UnitConverter
        {
            public static string ConvertFeetToFeetAndFractionalInches(double decimalFeet)
            {
                double feet = Math.Floor(decimalFeet);
                double inches = (decimalFeet - feet) * 12;
                double fraction = Math.Round(inches - Math.Floor(inches), 3);
                int wholeInches = (int)Math.Floor(inches);
                int numerator = (int)(fraction * 8);
                int denominator = 8;

                // Simplify the fraction
                while (numerator % 2 == 0 && denominator % 2 == 0)
                {
                    numerator /= 2;
                    denominator /= 2;
                }

                string result = $"{feet}' {wholeInches}";
                if (numerator > 0)
                {
                    result += $" {numerator}/{denominator}";
                }
                result += "\"";

                return result;
            }
        }

        public class FamilyTypeItem
        {
            public string Name { get; set; }
            public ElementId Id { get; set; }

            public FamilyTypeItem(string name, ElementId id)
            {
                Name = name;
                Id = id;
            }

            public override string ToString()
            {
                return Name;  // This controls what gets displayed in the CheckedListBox
            }
        }

        public void LoadDualDeckTypes(Document doc)
        {  
            try
            {
                // Collector that retrieves all elements of category OST_Families, filtering by class FamilySymbol
                FilteredElementCollector collector = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_GenericModel);

                // Clear existing items
                comboBoxFamilyType.Items.Clear();

                // Create a temporary list for sorting before adding them to the combobox
                List<FamilyTypeItem> familyTypes = new List<FamilyTypeItem>();

                // This HashSet is used to ensure unique names are added (optional based on your exact needs)
                HashSet<string> addedNames = new HashSet<string>();

                foreach (FamilySymbol symbol in collector)
                {
                    string familyName = symbol.Family.Name;
                    string display = "";

                    // Apply shorthand
                    if (familyName == "VDC DualDeck_Mirror")
                    {
                        display = $"DD Mirror: {symbol.Name}";
                    }
                    else if (familyName == "VDC DualDeck_Adjust")
                    {
                        display = $"DD Adjust: {symbol.Name}";
                    }

                    if (!string.IsNullOrEmpty(display))
                    {
                        familyTypes.Add(new FamilyTypeItem(display, symbol.Id));
                    }
                }

                // Sort the list alphabetically by the display name
                familyTypes = familyTypes.OrderBy(ft => ft.Name).ToList();

                // Add sorted items to the comboBox
                foreach (var familyType in familyTypes)
                {
                    comboBoxFamilyType.Items.Add(familyType);
                }

                if (comboBoxFamilyType.Items.Count == 0)
                {
                    MessageBox.Show("No matching family types found.");
                }

                if (_initialSelectedId != null)
                {
                    foreach (FamilyTypeItem item in comboBoxFamilyType.Items)
                    {
                        if (item.Id == _initialSelectedId)
                        {
                            comboBoxFamilyType.SelectedItem = item;
                            break;
                        }
                    }
                    _initialSelectedId = null; // Reset after setting to avoid re-setting unnecessarily
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load family types: {ex.Message}");
            }
        }

        private void btnSelectDeck_Click(object sender, EventArgs e)
        {
            try
            {
                UIDocument uidoc = new UIDocument(_doc);
                var refElem = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, new DualDeckSelectionFilter(), "Select a DualDeck");
                Element elem = _doc.GetElement(refElem.ElementId);

                // Check if selected element is a DualDeck within an assembly or directly
                ElementId dualDeckId = CheckAndRetrieveDualDeckId(elem);

                if (dualDeckId != null)
                {
                    // Update combobox selection based on picked element
                    foreach (FamilyTypeItem item in comboBoxFamilyType.Items)
                    {
                        if (item.Id == dualDeckId)
                        {
                            comboBoxFamilyType.SelectedItem = item;
                            break;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Selected element is not a DualDeck.");
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // Handle the case where the user canceled the selection
                MessageBox.Show("Selection canceled.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during selection: {ex.Message}");
            }
        }

        private ElementId CheckAndRetrieveDualDeckId(Element selectedElement)
        {
            if (selectedElement is FamilyInstance instance &&
                (instance.Symbol.Family.Name == "VDC DualDeck_Mirror" || instance.Symbol.Family.Name == "VDC DualDeck_Adjust"))
            {
                return instance.Symbol.Id;
            }
            else if (selectedElement is AssemblyInstance assembly)
            {
                var memberIds = assembly.GetMemberIds();
                foreach (ElementId memberId in memberIds)
                {
                    Element member = _doc.GetElement(memberId);
                    if (member is FamilyInstance memberInstance &&
                        (memberInstance.Symbol.Family.Name == "VDC DualDeck_Mirror" ||
                         memberInstance.Symbol.Family.Name == "VDC DualDeck_Adjust"))
                    {
                        return memberInstance.Symbol.Id;
                    }
                }
            }
            return null;
        }

        // Filter to allow selection of DualDeck only
        public class DualDeckSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                return elem is FamilyInstance fi &&
                       (fi.Symbol.Family.Name == "VDC DualDeck_Mirror" || fi.Symbol.Family.Name == "VDC DualDeck_Adjust");
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false; // We only want to select elements, not references
            }
        }

    }
}
