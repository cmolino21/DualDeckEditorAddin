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
        //private Dictionary<string, string> originalParameterValues; // Dictionary to store parameter values
        private Dictionary<string, string> changesTracker = new Dictionary<string, string>();
        private Document _doc;

        private Document familyDoc;  // Add class-level family document
        private FamilySymbol familySymbol = null;  // Add class-level family symbol
        private FamilyManager familyManager;  // Add class-level family manager
        private ElementId _initialSelectedId = null; // If a DualDeck is selected in the document when the program is launched, store it here
        private FamilyType activeFamilyType = null;

        private ExternalEvent exEvent;
        private ParameterUpdateHandler handler;

        private Dictionary<System.Windows.Forms.Control, EventHandler> textBoxDelegates = new Dictionary<System.Windows.Forms.Control, EventHandler>();
        private Dictionary<System.Windows.Forms.Control, EventHandler> checkBoxDelegates = new Dictionary<System.Windows.Forms.Control, EventHandler>();
        public MainFormEditor(Document doc, UIDocument uidoc)
        {
            InitializeComponent();
            _doc = doc;
            InitializeDualDeckSelection(uidoc);
            comboBoxFamilyType.SelectedIndexChanged += comboBoxFamilyType_SelectedIndexChanged; // Attach the event handler for updating the data based on DualDeck selection
            textBoxDD_Depth.Leave += textBoxDD_Depth_Leave;
            //SetupChangeTracking();

            handler = new ParameterUpdateHandler(); // Initially empty, setup later
            exEvent = ExternalEvent.Create(handler);
            //btnSave.Click += btnSave_Click;
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
                    changesTracker.Clear();

                    familySymbol = selectedInstance.Symbol;

                    Family family = selectedInstance.Symbol.Family; // Retrieve the family from the instance symbol

                    familyDoc = _doc.EditFamily( family ); // Open the family for editing which returns the family document

                    familyManager = familyDoc.FamilyManager; // Get the family manager which manages the parameters of the family

                    // Retrieve and print the number of parameters in the family
                    int n = familyManager.Parameters.Size; 

                    //Debug.Print("\nFamily {0} has {1} parameter{2}", _doc.Title, n, Util.PluralSuffix( n ) );

                    // Create a dictionary to map parameter names to their corresponding FamilyParameter objects
                    Dictionary<string, FamilyParameter> fps = new Dictionary<string, FamilyParameter>(n);

                    foreach (FamilyParameter fp in familyManager.Parameters)
                    {
                        string name = fp.Definition.Name;
                        fps.Add (name, fp );
                    }
                    // Sort the keys (parameter names) for better manageability
                    List<string> parameters = new List<string>( fps.Keys );
                    parameters.Sort();

                    // Print the number of types in the family and their names
                    n = familyManager.Types.Size;

                    //Debug.Print("\nFamily {0} has {1} type{2}{3}", _doc.Title, n, Util.PluralSuffix(n), Util.DotOrColon(n));

                    string matchName = selectedFamilyType.ToString()
                            .Replace("DD Mirror: ", "")
                            .Replace("DD Adjust: ", "");

                    Dictionary<string, string> parameterValues = new Dictionary<string, string>(); // Dictionary to store parameter values

                    // Iterate through each type in the family
                    foreach (Autodesk.Revit.DB.FamilyType type in familyManager.Types)
                    {
                        // Get the match name and remove "DD Mirror: " or "DD Adjust: " from it
                        if (type.Name == matchName)
                        {
                            activeFamilyType = type;
                            // For each parameter key, check if the type has a value set and print it
                            foreach (string key in parameters)
                            {
                                FamilyParameter fp = fps[key];
                                if (type.HasValue(fp))
                                {
                                    string value = Util.FamilyParamValueString(type, fp, _doc);
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

                    // Temporarily disable change tracking
                    DisableChangeTracking();


                    // Set dimension text box values
                    foreach (var entry in ControlMappings.TextBoxDimensionMappings)
                    {
                        var textBox = this.Controls.Find(entry.Key, true).FirstOrDefault() as System.Windows.Forms.TextBox;
                        if (textBox != null)
                        {
                            textBox.Text = parameterValues.ContainsKey(entry.Value) ? parameterValues[entry.Value] : "Error";
                        }
                    }

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

                    SetupChangeTracking();

                    Parameter parameterTest = familySymbol.LookupParameter("DD_Width");
                    if (parameterTest != null)
                    {
                        MessageBox.Show("DD_Width: " + parameterTest.AsDouble());
                    }
                    else
                    {
                        MessageBox.Show("DD_Width not found");
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

        private void textBoxDD_Depth_Leave(object sender, EventArgs e)
        {
            FormatAsFeetAndInches(sender as System.Windows.Forms.TextBox);
        }

        // Convert text to proper format in text boxes
        private void FormatAsFeetAndInches(System.Windows.Forms.TextBox textBox)
        {
            if (textBox == null) return;

            string input = textBox.Text.Trim();
            double feet = 0, inches = 0;
            string fractionPart = "";
            bool isValid = false;

            // Split the input into parts to differentiate feet, inches, and fractions
            string[] parts = input.Split(new char[] { ' ', '\'' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
            {
                // Handle single value which could be feet or inches
                isValid = TryParseValue(parts[0], out feet);
                if (!isValid)
                {
                    isValid = TryParseFraction(parts[0], out inches);
                    if (isValid) feet = 0;
                }
                else
                {
                    inches = 0;
                }
            }
            else if (parts.Length == 2)
            {
                // Handle feet and inches or feet and fractional inches
                isValid = TryParseValue(parts[0], out feet) && (TryParseValue(parts[1], out inches) || TryParseFraction(parts[1], out inches));
            }
            else if (parts.Length == 3)
            {
                // Handle feet, inches, and fractions
                isValid = TryParseValue(parts[0], out feet) && TryParseValue(parts[1], out inches);
                if (isValid)
                {
                    isValid = TryParseFraction(parts[2], out double fraction);
                    if (isValid)
                    {
                        fractionPart = " " + parts[2]; // Keep the fraction part as string
                        inches += fraction;
                    }
                }
            }

            if (isValid)
            {
                textBox.Text = FormatFeetInchesString(feet, inches, fractionPart);
            }
            else
            {
                textBox.Text = input;
            }
        }

        private bool TryParseValue(string input, out double result)
        {
            result = 0;
            if (double.TryParse(input, out result)) // Direct decimal or integer
                return true;
            return TryParseFraction(input, out result); // Fractional value
        }


        private bool TryParseFraction(string input, out double fraction)
        {
            fraction = 0;
            if (input.Contains("/"))
            {
                var parts = input.Split('/');
                if (parts.Length == 2 && double.TryParse(parts[0], out double numerator) && double.TryParse(parts[1], out double denominator))
                {
                    if (denominator != 0)
                    {
                        fraction = numerator / denominator;
                        return true;
                    }
                }
                return false;
            }
            return double.TryParse(input, out fraction);
        }

        private string FormatFeetInchesString(double feet, double inches, string fractionPart)
        {
            int intFeet = (int)feet;
            int intInches = (int)inches;
            double remainingInches = inches % 1;

            // Format remaining inches fractionally if there is a fraction part
            string formattedInches = (remainingInches > 0 && string.IsNullOrEmpty(fractionPart)) ?
                                     $"{intInches + remainingInches:0.#}\"" : $"{intInches}{fractionPart}\"";

            return $"{intFeet}'  {formattedInches}";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Debug.Print("Changes to save: " + changesTracker.Count);
            if (familyDoc != null && familyManager != null)
            {
                // Update the handler with current documents and parameters
                handler.Setup( _doc, familyDoc, familySymbol, familyManager, activeFamilyType, changesTracker);
            }
            exEvent.Raise();
        }



        private void btnRevert_Click(object sender, EventArgs e)
        {
            //RevertChanges();
        }

        private void SetupChangeTracking()
        {
            // Initiate recursive setup from the top-level form controls
            SetupControlTracking(this);
        }

        private void SetupControlTracking(System.Windows.Forms.Control control)
        {
            foreach (System.Windows.Forms.Control child in control.Controls)
            {
                if (child is System.Windows.Forms.TextBox textBox)
                {
                    string originalText = textBox.Text; // Store initial text
                    textBox.Tag = originalText;  // Use Tag to store the original value
                    EventHandler textBoxHandler = (s, e) =>
                    {
                        if (textBox.Text != originalText)
                        {
                            changesTracker[textBox.Name] = textBox.Text;
                            originalText = textBox.Text; // Update the original text after the change
                        }
                    };
                    textBox.Leave += textBoxHandler;
                    textBoxDelegates[textBox] = textBoxHandler;
                }
                else if (child is CheckBox checkBox)
                {
                    bool originalState = checkBox.Checked; // Store initial state
                    checkBox.Tag = originalState;  // Use Tag to store the original value
                    EventHandler checkBoxHandler = (s, e) =>
                    {
                        if (checkBox.Checked != originalState)
                        {
                            changesTracker[checkBox.Name] = checkBox.Checked ? "1" : "0";
                            originalState = checkBox.Checked; // Update the original state after the change
                        }
                    };
                    checkBox.CheckedChanged += checkBoxHandler;
                    checkBoxDelegates[checkBox] = checkBoxHandler;
                }

                // Recursively handle child controls
                if (child.HasChildren)
                {
                    SetupControlTracking(child);
                }
            }
        }

        private void DisableChangeTracking()
        {
            DisableEventHandler(this);
        }

        private void DisableEventHandler(System.Windows.Forms.Control control)
        {
            foreach (System.Windows.Forms.Control child in control.Controls)
            {
                if (child is System.Windows.Forms.TextBox textBox && textBoxDelegates.ContainsKey(textBox))
                {
                    textBox.Leave -= textBoxDelegates[textBox];
                }
                else if (child is CheckBox checkBox && checkBoxDelegates.ContainsKey(checkBox))
                {
                    checkBox.CheckedChanged -= checkBoxDelegates[checkBox];
                }
                if (child.HasChildren)
                {
                    DisableEventHandler(child);
                }
            }
        }


        //// Revert functionality
        //private void RevertChanges()
        //{
        //    foreach (var entry in originalParameterValues)
        //    {
        //        var control = this.Controls.Find(entry.Key, true).FirstOrDefault();
        //        if (control is System.Windows.Forms.TextBox textBox)
        //        {
        //            textBox.Text = entry.Value;
        //        }
        //        else if (control is CheckBox checkBox)
        //        {
        //            checkBox.Checked = entry.Value == "1";
        //        }
        //    }
        //}

        // Save functionality

    }
}
