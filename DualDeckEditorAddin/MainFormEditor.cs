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
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DualDeckEditorAddin
{
    public partial class MainFormEditor : System.Windows.Forms.Form
    {
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
        private Dictionary<string, string> parameterValues = new Dictionary<string, string>(); // Dictionary to store parameter values

        public MainFormEditor(Document doc, UIDocument uidoc)
        {
            InitializeComponent();
            _doc = doc;
            InitializeDualDeckSelection(uidoc);
            comboBoxFamilyType.SelectedIndexChanged += comboBoxFamilyType_SelectedIndexChanged; // Attach the event handler for updating the data based on DualDeck selection
            checkBoxTrussOffset.CheckedChanged += checkBoxTrussOffset_CheckedChanged;
            AddOffsetCheckboxHandlers();
            textBoxDD_Depth.Leave += textBoxDD_Leave;
            textBoxDD_Length.Leave += textBoxDD_Leave;
            textBoxDD_Width.Leave += textBoxDD_Leave;
            textBoxDD_BotJoint.Leave += textBoxDD_Leave;
            textBoxDD_LedgeJoint.Leave += textBoxDD_Leave;
            textBoxDD_Depth.KeyPress += textBoxDD_KeyPress;
            textBoxDD_Length.KeyPress += textBoxDD_KeyPress;
            textBoxDD_Width.KeyPress += textBoxDD_KeyPress;
            textBoxDD_BotJoint.KeyPress += textBoxDD_KeyPress;
            textBoxDD_LedgeJoint.KeyPress += textBoxDD_KeyPress;
            this.FormClosing += MainFormEditor_FormClosing;

            this.StartPosition = FormStartPosition.CenterScreen;

            handler = new ParameterUpdateHandler(); 
            exEvent = ExternalEvent.Create(handler);
        }

        private void MainFormEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Close the family document if it's open
            if (familyDoc != null)
            {
                familyDoc.Close(false);
            }
        }

        private void checkBoxTrussOffset_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTruss6EnabledStatus();
        }

        private void AddOffsetCheckboxHandlers()
        {
            var offsetCheckboxes = new List<CheckBox>
            {
                checkBox_2Out_A, checkBox_2In_A, checkBox_2Out_01, checkBox_2In_01,
                checkBox_2Out_02, checkBox_2In_02, checkBox_2Out_03, checkBox_2In_03,
                checkBox_2Out_04, checkBox_2In_04, checkBox_2Out_05, checkBox_2In_05,
                checkBox_2Out_06, checkBox_2In_06, checkBox_2Out_07, checkBox_2In_07,
                checkBox_2Out_08, checkBox_2In_08, checkBox_2Out_09, checkBox_2In_09,
                checkBox_2Out_10, checkBox_2In_10, checkBox_2Out_11, checkBox_2In_11,
                checkBox_2Out_B, checkBox_2In_B
            };

            foreach (var checkBox in offsetCheckboxes)
            {
                checkBox.CheckedChanged += OffsetCheckbox_CheckedChanged;
            }
        }

        private void OffsetCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is CheckBox changedCheckBox && changedCheckBox.Checked)
            {
                string checkBoxName = changedCheckBox.Name;
                string oppositeCheckBoxName = null;

                if (checkBoxName.Contains("_2Out_"))
                {
                    oppositeCheckBoxName = checkBoxName.Replace("_2Out_", "_2In_");
                }
                else if (checkBoxName.Contains("_2In_"))
                {
                    oppositeCheckBoxName = checkBoxName.Replace("_2In_", "_2Out_");
                }

                if (!string.IsNullOrEmpty(oppositeCheckBoxName))
                {
                    var oppositeCheckBox = this.Controls.Find(oppositeCheckBoxName, true).FirstOrDefault() as CheckBox;
                    if (oppositeCheckBox != null && oppositeCheckBox.Checked)
                    {
                        oppositeCheckBox.Checked = false;
                    }
                }
            }
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
            UpdateFamilyTypeData();
        }

        public void UpdateFamilyTypeData()
        {
            if (comboBoxFamilyType.SelectedItem is FamilyTypeItem selectedFamilyType)
            {

                bool isAdjust = selectedFamilyType.ToString().StartsWith("DD Adjust");
                bool isMirror = selectedFamilyType.ToString().StartsWith("DD Mirror");

                if (isAdjust)
                {
                    checkBoxTrussOffset.Enabled = true;
                    // Switch to Adjust mappings
                    ControlMappings.TextBoxDimensionMappings = ControlMappings.TextBoxDimensionMappingsAdjust;
                    ControlMappings.TextBoxMappings = ControlMappings.TextBoxMappingsAdjust;
                    ControlMappings.CheckBoxMappings = ControlMappings.CheckBoxMappingsAdjust;
                }
                else if (isMirror)
                {
                    checkBoxTrussOffset.Enabled = false;
                    // Since this checkbox does not exist in mirror, need to explicitly state it should be unchecked
                    checkBoxTrussOffset.Checked = false;
                    // Switch to Mirror mappings
                    ControlMappings.TextBoxDimensionMappings = ControlMappings.TextBoxDimensionMappingsMirror;
                    ControlMappings.TextBoxMappings = ControlMappings.TextBoxMappingsMirror;
                    ControlMappings.CheckBoxMappings = ControlMappings.CheckBoxMappingsMirror;
                }

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
                    parameterValues.Clear();

                    familySymbol = selectedInstance.Symbol;

                    Family family = selectedInstance.Symbol.Family; // Retrieve the family from the instance symbol

                    familyDoc = _doc.EditFamily(family); // Open the family for editing which returns the family document

                    familyManager = familyDoc.FamilyManager; // Get the family manager which manages the parameters of the family

                    // Retrieve and print the number of parameters in the family
                    int n = familyManager.Parameters.Size;

                    //Debug.Print("\nFamily {0} has {1} parameter{2}", _doc.Title, n, Util.PluralSuffix( n ) );

                    // Create a dictionary to map parameter names to their corresponding FamilyParameter objects
                    Dictionary<string, FamilyParameter> fps = new Dictionary<string, FamilyParameter>(n);

                    foreach (FamilyParameter fp in familyManager.Parameters)
                    {
                        string name = fp.Definition.Name;
                        fps.Add(name, fp);
                    }
                    // Sort the keys (parameter names) for better manageability
                    List<string> parameters = new List<string>(fps.Keys);
                    parameters.Sort();

                    // Print the number of types in the family and their names
                    n = familyManager.Types.Size;

                    //Debug.Print("\nFamily {0} has {1} type{2}{3}", _doc.Title, n, Util.PluralSuffix(n), Util.DotOrColon(n));

                    string matchName = selectedFamilyType.ToString()
                            .Replace("DD Mirror: ", "")
                            .Replace("DD Adjust: ", "");

                    parameterValues = new Dictionary<string, string>(); // Dictionary to store parameter values

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
                    // Temporarily disable change tracking, populate textboxes, and then re-enable
                    DisableChangeTracking();
                    populateCheckBoxes();
                    SetupChangeTracking();

                    UpdateTruss6EnabledStatus();
                }
                else
                {
                    MessageBox.Show("No instances of the selected family type were found.");
                }
            }
        }

        public void populateCheckBoxes()
        {
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
            Debug.Print("Types loaded successfully");
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

        private void textBoxDD_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true; // Prevent the beep sound
                FormatAsFeetAndInches(sender as System.Windows.Forms.TextBox);
            }
        }


        private void textBoxDD_Leave(object sender, EventArgs e)
        {
            // Call the method to format the text in the TextBox to feet and inches format
            FormatAsFeetAndInches(sender as System.Windows.Forms.TextBox);
        }

        // Convert text to proper format in text boxes
        private void FormatAsFeetAndInches(System.Windows.Forms.TextBox textBox)
        {
            if (textBox == null) return;

            // Get the trimmed text from the TextBox
            string input = textBox.Text.Trim();
            // Format the input text and set it back to the TextBox
            textBox.Text = FormatAsFeetAndInches(input);
        }

        // Method to format a given string as feet and inches
        internal static string FormatAsFeetAndInches(string input)
        {
            // Regular expression patterns to match various input formats
            string feetInchesFraction = @"^(?<feet>\d+)\s*(?<inches>\d*)\s*(?<fraction>\d+/\d+)?$";
            string feetInchesDecimal = @"^(?<feet>\d+)\.(?<decimal>\d+)$";
            string feetWholeInchesDecimal = @"^(?<feet>\d+)\s+(?<inches>\d+)\.(?<decimal>\d+)$";
            string inchesOnly = @"^(?<inches>\d+)\""$";
            string feetOnlyWithApostrophe = @"^(?<feet>\d+)\'$";
            string feetAndInchesWithSymbols = @"^(?<feet>\d+)\s*'\s*(?<inches>\d+)\s*\""$";
            string inchesDecimal = @"^(?<inches>\d+)\.(?<decimal>\d+)\""$";
            string feetInchesFractionWithSymbols = @"^(?<feet>\d+)\s*'\s*(?<inches>\d+)\s+(?<fraction>\d+/\d+)\""$";

            // Match the input with the feet and inches with optional fraction pattern i.e 2 (feet only) 2 6 (feet and inches) 2 6 1/2(feet, inches, and fraction)
            Match match = Regex.Match(input, feetInchesFraction);
            if (match.Success)
            {
                Debug.WriteLine("feetInchesFraction detected");
                // Parse the feet, inches, and fraction parts from the matched groups
                int feet = int.Parse(match.Groups["feet"].Value);
                int inches = string.IsNullOrEmpty(match.Groups["inches"].Value) ? 0 : int.Parse(match.Groups["inches"].Value);
                string fractionPart = match.Groups["fraction"].Value;

                // Calculate total inches if fraction is present
                double fractionInches = 0.0;
                if (!string.IsNullOrEmpty(fractionPart))
                {
                    string[] fractionSplit = fractionPart.Split('/');
                    double numerator = double.Parse(fractionSplit[0]);
                    double denominator = double.Parse(fractionSplit[1]);
                    fractionInches = numerator / denominator;
                }

                // Calculate total inches including the fraction part
                double totalInches = feet * 12 + inches + fractionInches;
                feet = (int)totalInches / 12;
                inches = (int)totalInches % 12;
                fractionInches = totalInches - feet * 12 - inches;

                // Calculate the new fraction part in 1/16ths
                int numeratorInSixteenths = (int)Math.Round(fractionInches * 16);
                int denominatorInSixteenths = 16;

                // Reduce the fraction to its simplest form
                int gcd = GCD(numeratorInSixteenths, denominatorInSixteenths);
                numeratorInSixteenths /= gcd;
                denominatorInSixteenths /= gcd;

                // Construct the fraction string
                string fraction = numeratorInSixteenths == 0 ? "" : $" {numeratorInSixteenths}/{denominatorInSixteenths}";

                return $"{feet}'  {inches}{fraction}\"";
            }

            // Match the input with the feet and decimal inches pattern i.e. 2.5 (feet and decimal inches)
            match = Regex.Match(input, feetInchesDecimal);
            if (match.Success)
            {
                // Parse the feet and decimal part from the matched groups
                int feet = int.Parse(match.Groups["feet"].Value);
                double decimalPart = double.Parse("0." + match.Groups["decimal"].Value, CultureInfo.InvariantCulture);
                double totalInches = decimalPart * 12; // Convert the decimal part to inches
                int wholeInches = (int)Math.Floor(totalInches);
                double fractionalInches = totalInches - wholeInches;
                int numerator = (int)Math.Round(fractionalInches * 16); // Convert fractional part to 1/16
                int denominator = 16;

                // Reduce the fraction to its simplest form
                int gcd = GCD(numerator, denominator);
                numerator /= gcd;
                denominator /= gcd;

                Debug.WriteLine("feetInchesDecimal detected\nfeet: " + feet + "\ndecimalPart: " + decimalPart + "\ntotalInches: " + totalInches + "\nwholeInches: " + wholeInches + "\nfractionalInches: " + fractionalInches 
                    + "\nnumerator: " + numerator + "\ndenominator: " + denominator);

                string fraction = numerator == 0 ? "" : $" {numerator}/{denominator}";
                return $"{feet}'  {wholeInches}{fraction}\"";
            }

            // Match the input with the feet, whole inches, and decimal inches pattern i.e  2 6.5 (feet, whole inches, and decimal inches - fraction part not working)
            match = Regex.Match(input, feetWholeInchesDecimal);
            if (match.Success)
            {
                // Parse the feet, whole inches, and decimal part from the matched groups
                int feet = int.Parse(match.Groups["feet"].Value);
                int inches = int.Parse(match.Groups["inches"].Value);
                double decimalPart = double.Parse("0." + match.Groups["decimal"].Value, CultureInfo.InvariantCulture);

                // Calculate the fractional part in inches
                double fractionalInches = decimalPart;
                int numerator = (int)Math.Round(fractionalInches * 16); // Convert fractional part to 1/16
                int denominator = 16;

                // Reduce the fraction to its simplest form
                int gcd = GCD(numerator, denominator);
                numerator /= gcd;
                denominator /= gcd;

                Debug.WriteLine("feetInchesDecimal detected\nfeet: " + feet + "\ninches: " + inches + "\ndecimalPart: " + decimalPart + "\nfractionalInches: " + fractionalInches + "\nnumerator: " + numerator
                    + "\ndenominator: " + denominator);

                // Construct the fraction string
                string fraction = numerator == 0 ? "" : $" {numerator}/{denominator}";

                return $"{feet}'  {inches}{fraction}\"";
            }

            // Function to calculate the Greatest Common Divisor (GCD) using Euclidean algorithm
            int GCD(int a, int b)
            {
                while (b != 0)
                {
                    int temp = b;
                    b = a % b;
                    a = temp;
                }
                return a;
            }

            // Match the input with the inches only pattern i.e.  6" (inches only - working but maybe convert to feet if more than 12")
            match = Regex.Match(input, inchesOnly);
            if (match.Success)
            {
                Debug.WriteLine("inchesOnly detected");
                // Parse the inches part from the matched group
                int totalInches = int.Parse(match.Groups["inches"].Value);

                // Convert the total inches to feet and remaining inches
                int feet = totalInches / 12;
                int inches = totalInches % 12;
                return $"{feet}'  {inches}\"";
            }

            // Match the input with the feet only pattern i.e. 5'
            match = Regex.Match(input, feetOnlyWithApostrophe);
            if (match.Success)
            {
                Debug.WriteLine("feetOnlyWithApostrophe detected");
                // Parse the feet part from the matched group
                int feet = int.Parse(match.Groups["feet"].Value);
                return $"{feet}'  0\"";
            }

            // Match the input with the feet and inches pattern i.e. X' Y" (new case)
            match = Regex.Match(input, feetAndInchesWithSymbols);
            if (match.Success)
            {
                Debug.WriteLine("feetAndInchesWithSymbols detected");
                // Parse the feet and inches parts from the matched groups
                int feet = int.Parse(match.Groups["feet"].Value);
                int inches = int.Parse(match.Groups["inches"].Value);
                return $"{feet}'  {inches}\"";
            }

            // Match the input with the feet and inches pattern with fraction i.e. X' Y a/b"
            match = Regex.Match(input, feetInchesFractionWithSymbols);
            if (match.Success)
            {
                Debug.WriteLine("feetInchesFractionWithSymbols detected");
                int feet = int.Parse(match.Groups["feet"].Value);
                int inches = int.Parse(match.Groups["inches"].Value);
                string fractionPart = match.Groups["fraction"].Value;

                string[] fractionSplit = fractionPart.Split('/');
                int numerator = int.Parse(fractionSplit[0]);
                int denominator = int.Parse(fractionSplit[1]);

                // Reduce the fraction to its simplest form
                int gcd = GCD(numerator, denominator);
                numerator /= gcd;
                denominator /= gcd;

                string fraction = numerator == 0 ? "" : $"{numerator}/{denominator}";
                return $"{feet}'  {inches} {fraction}\"";
            }

            // Match the input with the decimal inches pattern i.e. Y.Z"
            match = Regex.Match(input, inchesDecimal);
            if (match.Success)
            {
                Debug.WriteLine("inchesDecimal detected");
                int inches = int.Parse(match.Groups["inches"].Value);
                double decimalPart = double.Parse("0." + match.Groups["decimal"].Value, CultureInfo.InvariantCulture);
                double fractionalInches = decimalPart;
                int numerator = (int)Math.Round(fractionalInches * 16); // Convert fractional part to 1/16
                int denominator = 16;

                // Reduce the fraction to its simplest form
                int gcd = GCD(numerator, denominator);
                numerator /= gcd;
                denominator /= gcd;

                string fraction = numerator == 0 ? "" : $"{numerator}/{denominator}";
                return $"0'  {inches} {fraction}\"";
            }

            // Return the input unchanged if it doesn't match any pattern and warn the user
            MessageBox.Show("Dimension formatting not recognized. Ensure you entered it in a valid format");
            return input;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Debug.Print("Changes to save: " + changesTracker.Count);
            if (familyDoc != null && familyManager != null && changesTracker.Count > 0)
            {
                // Update the handler with current documents and parameters
                handler.Setup(_doc, familyDoc, familySymbol, familyManager, activeFamilyType, changesTracker, this);
                Debug.Print("Handing off to exEvent: Edit parameters");
                exEvent.Raise();
            }
            else
            {
                MessageBox.Show("No changes detected");
            }

            if (changesTracker.Count > 0)
            {
                changesTracker.Clear();
            }
        }



        private void btnRevert_Click(object sender, EventArgs e)
        {
            populateCheckBoxes();
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
                            MirrorControlChanges(textBox);
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
                            MirrorControlChanges(checkBox);
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

        private void MirrorControlChanges(System.Windows.Forms.Control control)
        {
            bool isOffsetChecked = checkBoxTrussOffset.Checked;
            var mirroringPairs = isOffsetChecked ? ControlMappings.mirroringPairsWithOffset : ControlMappings.mirroringPairsWithoutOffset;

            if (mirroringPairs.TryGetValue(control.Name, out string mirrorControlName))
            {
                var mirrorControl = this.Controls.Find(mirrorControlName, true).FirstOrDefault();
                if (mirrorControl != null)
                {
                    if (control is System.Windows.Forms.TextBox textBox && mirrorControl is System.Windows.Forms.TextBox mirrorTextBox)
                    {
                        mirrorTextBox.Text = textBox.Text;
                    }
                    else if (control is CheckBox checkBox && mirrorControl is CheckBox mirrorCheckBox)
                    {
                        mirrorCheckBox.Checked = checkBox.Checked;
                    }
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


        private List<string> trussControlsToDisable = new List<string>
        {
            "textBoxShort_06", "textBoxLong_06",
            "checkBoxOnOff_06", "label12",
            "checkBox_2Out_06", "checkBox_2In_06"
        };

        private void UpdateTruss6EnabledStatus()
        {
            bool enable = !checkBoxTrussOffset.Checked;
            foreach (System.Windows.Forms.Control child in this.Controls)
            {
                if (child is System.Windows.Forms.TextBox textBox && trussControlsToDisable.Contains(textBox.Name))
                {
                    textBox.Enabled = enable;
                }
                else if (child is System.Windows.Forms.CheckBox checkBox && trussControlsToDisable.Contains(checkBox.Name))
                {
                    checkBox.Enabled = enable;
                }
                else if (child is System.Windows.Forms.Label label && trussControlsToDisable.Contains(label.Name))
                {
                    label.Enabled = enable;
                }

                // Recursively handle child controls
                if (child.HasChildren)
                {
                    UpdateTruss6EnabledStatus(child, enable);
                }
            }
        }

        private void UpdateTruss6EnabledStatus(System.Windows.Forms.Control control, bool enable)
        {
            foreach (System.Windows.Forms.Control child in control.Controls)
            {
                if (child is System.Windows.Forms.TextBox textBox && trussControlsToDisable.Contains(textBox.Name))
                {
                    textBox.Enabled = enable;
                }
                else if (child is System.Windows.Forms.CheckBox checkBox && trussControlsToDisable.Contains(checkBox.Name))
                {
                    checkBox.Enabled = enable;
                }
                else if (child is System.Windows.Forms.Label label && trussControlsToDisable.Contains(label.Name))
                {
                    label.Enabled = enable;
                }

                // Recursively handle child controls
                if (child.HasChildren)
                {
                    UpdateTruss6EnabledStatus(child, enable);
                }
            }
        }
    }
}
