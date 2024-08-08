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
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DualDeckEditorAddin
{
    public partial class MainFormEditor : System.Windows.Forms.Form
    {
        private Dictionary<string, string> changesTracker = new Dictionary<string, string>();
        private Document _doc;

        private FamilySymbol familySymbol = null;  // Add class-level family symbol
        private ElementId _initialSelectedId = null; // If a DualDeck is selected in the document when the program is launched, store it here
        private FamilyType activeFamilyType = null;

        private ExternalEvent exEvent;
        private ParameterUpdateHandler handler;

        private Dictionary<System.Windows.Forms.Control, EventHandler> textBoxDelegates = new Dictionary<System.Windows.Forms.Control, EventHandler>();
        private Dictionary<System.Windows.Forms.Control, EventHandler> checkBoxDelegates = new Dictionary<System.Windows.Forms.Control, EventHandler>();
        private Dictionary<string, string> parameterValues = new Dictionary<string, string>(); // Dictionary to store parameter values

        private bool isAdjust = false;
        private bool isMirror = false;
        private bool is1Skew = false;
        private bool is2Skew = false; 

        private bool areSkewedColumnsDisplayed = false;

        public MainFormEditor(Document doc, UIDocument uidoc)
        {
            InitializeComponent();
            _doc = doc;
            InitializeDualDeckSelection(uidoc);
            comboBoxFamilyType.SelectedIndexChanged += comboBoxFamilyType_SelectedIndexChanged; // Attach the event handler for updating the data based on DualDeck selection
            checkBoxTrussOffset.CheckedChanged += checkBoxTrussOffset_CheckedChanged;
            checkBoxAsymOME.CheckedChanged += checkBoxAsymOME_CheckChanged;
            AddOffsetCheckboxHandlers();
            AddStrandCheckboxHandlers();
            textBoxDD_Depth.Leave += textBoxDD_Leave;
            textBoxDD_Length.Leave += textBoxDD_Leave;
            textBoxDD_Width.Leave += textBoxDD_Leave;
            textBoxDD_BotJoint.Leave += textBoxDD_Leave;
            textBoxDD_LedgeJoint.Leave += textBoxDD_Leave;
            textBoxSkewOME.Leave += textBoxSkew_Leave;
            textBoxDD_Depth.KeyPress += textBoxDD_KeyPress;
            textBoxDD_Length.KeyPress += textBoxDD_KeyPress;
            textBoxDD_Width.KeyPress += textBoxDD_KeyPress;
            textBoxDD_BotJoint.KeyPress += textBoxDD_KeyPress;
            textBoxDD_LedgeJoint.KeyPress += textBoxDD_KeyPress;
            textBoxSkewOME.KeyPress += textBoxSkew_KeyPress;

            this.StartPosition = FormStartPosition.CenterScreen;

            handler = new ParameterUpdateHandler(); 
            exEvent = ExternalEvent.Create(handler);
        }

        private void AddStrandCheckboxHandlers()
        {
            // Checkboxes for top strands
            var topStrands = new List<CheckBox>
            {
                checkBoxTop_A, checkBoxTop_B, checkBoxTop_01, checkBoxTop_02,
                checkBoxTop_03, checkBoxTop_04, checkBoxTop_05, checkBoxTop_06,
                checkBoxTop_07, checkBoxTop_08, checkBoxTop_09, checkBoxTop_10,
                checkBoxTop_11, checkBoxTop_12, checkBoxTop_13, checkBoxTop_14,
                checkBoxTop_15, checkBoxTop_16, checkBoxTop_17, checkBoxTop_18,
                checkBoxTop_19, checkBoxTop_20
            };

            // Checkboxes for bottom strands
            var bottomStrands = new List<CheckBox>
            {
                checkBoxBot_A, checkBoxBot_B, checkBoxBot_01, checkBoxBot_02,
                checkBoxBot_03, checkBoxBot_04, checkBoxBot_05, checkBoxBot_06,
                checkBoxBot_07, checkBoxBot_08, checkBoxBot_09, checkBoxBot_10,
                checkBoxBot_11, checkBoxBot_12, checkBoxBot_13, checkBoxBot_14,
                checkBoxBot_15, checkBoxBot_16, checkBoxBot_17, checkBoxBot_18,
                checkBoxBot_19, checkBoxBot_20
            };

            foreach (var checkBox in topStrands)
            {
                checkBox.CheckedChanged += UpdateStrandCounts;
            }

            foreach (var checkBox in bottomStrands)
            {
                checkBox.CheckedChanged += UpdateStrandCounts;
            }
        }

        private void UpdateStrandCounts(object sender, EventArgs e)
        {
            UpdateStrandCounts();
        }

        private void UpdateStrandCounts ()
        {
            int totalTopStrands = 0;
            int totalBottomStrands = 0;

            // Checkboxes for top strands
            var topStrands = new List<string>
            {
                "checkBoxTop_A", "checkBoxTop_B", "checkBoxTop_01", "checkBoxTop_02",
                "checkBoxTop_03", "checkBoxTop_04", "checkBoxTop_05", "checkBoxTop_06",
                "checkBoxTop_07", "checkBoxTop_08", "checkBoxTop_09", "checkBoxTop_10",
                "checkBoxTop_11", "checkBoxTop_12", "checkBoxTop_13", "checkBoxTop_14",
                "checkBoxTop_15", "checkBoxTop_16", "checkBoxTop_17", "checkBoxTop_18",
                "checkBoxTop_19", "checkBoxTop_20"
            };

            // Checkboxes for bottom strands
            var bottomStrands = new List<string>
            {
                "checkBoxBot_A", "checkBoxBot_B", "checkBoxBot_01", "checkBoxBot_02",
                "checkBoxBot_03", "checkBoxBot_04", "checkBoxBot_05", "checkBoxBot_06",
                "checkBoxBot_07", "checkBoxBot_08", "checkBoxBot_09", "checkBoxBot_10",
                "checkBoxBot_11", "checkBoxBot_12", "checkBoxBot_13", "checkBoxBot_14",
                "checkBoxBot_15", "checkBoxBot_16", "checkBoxBot_17", "checkBoxBot_18",
                "checkBoxBot_19", "checkBoxBot_20"
            };

            // Count checked top strands
            foreach (var checkBoxName in topStrands)
            {
                var checkBox = this.Controls.Find(checkBoxName, true).FirstOrDefault() as CheckBox;
                if (checkBox != null && checkBox.Checked)
                {
                    totalTopStrands++;
                }
            }

            // Count checked bottom strands
            foreach (var checkBoxName in bottomStrands)
            {
                var checkBox = this.Controls.Find(checkBoxName, true).FirstOrDefault() as CheckBox;
                if (checkBox != null && checkBox.Checked)
                {
                    totalBottomStrands++;
                }
            }

            // Update the labels
            labelTotTop.Text = $"Total Top: {totalTopStrands*2}";
            labelTotBot.Text = $"Total Bot:  {totalBottomStrands*2}";
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
                    if (instance.Symbol.Family.Name == "VDC DualDeck_Mirror" || instance.Symbol.Family.Name == "VDC DualDeck_Adjust" || instance.Symbol.Family.Name == "VDC DualDeck_OME_Skewed" || instance.Symbol.Family.Name == "VDC DualDeck_Double_Skewed")
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
                             memberInstance.Symbol.Family.Name == "VDC DualDeck_Adjust" ||
                             memberInstance.Symbol.Family.Name == "VDC DualDeck_OME_Skewed" ||
                             memberInstance.Symbol.Family.Name == "VDC DualDeck_Double_Skewed"))
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
            isAdjust = false;
            isMirror = false;
            is1Skew = false;
            is2Skew = false;
            UpdateFamilyTypeData();
        }

        public void UpdateFamilyTypeData()
        {
            if (comboBoxFamilyType.SelectedItem is FamilyTypeItem selectedFamilyType)
            {

                isAdjust = selectedFamilyType.ToString().StartsWith("DD Adjust");
                isMirror = selectedFamilyType.ToString().StartsWith("DD Mirror");
                is1Skew = selectedFamilyType.ToString().StartsWith("DD Skew");
                is2Skew = selectedFamilyType.ToString().StartsWith("DD 2Skew");

                if (isAdjust)
                {
                    isAdjust=true;
                    checkBoxTrussOffset.Enabled = true;
                    checkBoxAsymOME.Visible = false;
                    labelSkewME.Visible = false;
                    labelSkewOME.Visible = false;
                    textBoxSkewME.Visible = false;
                    textBoxSkewOME.Visible = false;
                    // Switch to Adjust mappings
                    ControlMappings.TextBoxDimensionMappings = ControlMappings.TextBoxDimensionMappingsAdjust;
                    ControlMappings.TextBoxMappings = ControlMappings.TextBoxMappingsAdjust;
                    ControlMappings.CheckBoxMappings = ControlMappings.CheckBoxMappingsAdjust;
                    ResetTableLayoutPanelColumns();
                }
                else if (isMirror)
                {
                    isMirror = true;
                    checkBoxTrussOffset.Enabled = false;
                    checkBoxAsymOME.Visible = false;
                    labelSkewME.Visible = false;
                    labelSkewOME.Visible = false;
                    textBoxSkewME.Visible = false;
                    textBoxSkewOME.Visible = false;
                    // Since this checkbox does not exist in mirror, need to explicitly state it should be unchecked
                    checkBoxTrussOffset.Checked = false;
                    // Switch to Mirror mappings
                    ControlMappings.TextBoxDimensionMappings = ControlMappings.TextBoxDimensionMappingsMirror;
                    ControlMappings.TextBoxMappings = ControlMappings.TextBoxMappingsMirror;
                    ControlMappings.CheckBoxMappings = ControlMappings.CheckBoxMappingsMirror;
                    ResetTableLayoutPanelColumns();
                }
                else if (is1Skew)
                {
                    is1Skew = true;
                    checkBoxTrussOffset.Enabled = true;
                    checkBoxAsymOME.Visible = false;
                    checkBoxAsymOME.Checked = true;
                    labelSkewME.Visible = false;
                    labelSkewOME.Visible = true;
                    textBoxSkewME.Visible = false;
                    textBoxSkewOME.Visible = true;
                    // Switch to Skew mappings
                    ControlMappings.TextBoxDimensionMappings = ControlMappings.TextBoxDimensionMappingsSkew;
                    ControlMappings.TextBoxMappings = ControlMappings.TextBoxMappingsSkew;
                    ControlMappings.CheckBoxMappings = ControlMappings.CheckBoxMappingsSkew;
                    AddSkewedColumns();
                }
                else if (is2Skew)
                {
                    is2Skew = true;
                    checkBoxTrussOffset.Enabled = true;
                    checkBoxAsymOME.Visible = true;
                    labelSkewME.Visible = true;
                    labelSkewOME.Visible = true;
                    textBoxSkewME.Visible = true;
                    textBoxSkewOME.Visible = true;
                    // Switch to Skew mappings
                    ControlMappings.TextBoxDimensionMappings = ControlMappings.TextBoxDimensionMappings2Skew;
                    ControlMappings.TextBoxMappings = ControlMappings.TextBoxMappings2Skew;
                    ControlMappings.CheckBoxMappings = ControlMappings.CheckBoxMappings2Skew;
                    AddSkewedColumns();
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

                    PopulateParameterValuesFromSymbol(familySymbol);

                    // Temporarily disable change tracking, populate textboxes, and then re-enable
                    DisableChangeTracking();
                    populateCheckBoxes();
                    SetupChangeTracking();

                    UpdateMirrorTrussEnabledStatus();
                    UpdateTruss6EnabledStatus();
                    UpdateStrandCounts();
                }
                else
                {
                    MessageBox.Show("No instances of the selected family type were found.");
                }
            }
        }
       
        private void AddSkewedColumns()
        {
            if (areSkewedColumnsDisplayed)
                return;

            Font HeaderFont = new Font(DefaultFont, FontStyle.Bold);
            Size TextBoxSize = new Size(49, 20);

            tableLayoutPanelTruss.SuspendLayout();

            // Adjust the width of the tableLayoutPanel
            tableLayoutPanelTruss.Size = new Size(tableLayoutPanelTruss.Width + 140, tableLayoutPanelTruss.Height);

            // Adjust the width of columns 4 and 5
            tableLayoutPanelTruss.ColumnStyles[4].Width = 70;
            tableLayoutPanelTruss.ColumnStyles[5].Width = 70;


            // Move controls in the existing columns to the right
            for (int i = 5; i >= 4; i--)
            {
                for (int j = 0; j < tableLayoutPanelTruss.RowCount; j++)
                {
                    System.Windows.Forms.Control control = tableLayoutPanelTruss.GetControlFromPosition(i, j); // Get control at original position
                    if (control != null)
                    {
                        Debug.Print($"Moving control from ({i}, {j}) to ({i + 2}, {j})");
                        tableLayoutPanelTruss.SetColumn(control, i + 2); // Set new position
                    }
                }
            }

            // Add headers for the new columns
            tableLayoutPanelTruss.Controls.Add(new Label { Text = "OME Short", TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.None, Anchor = AnchorStyles.None , Font = HeaderFont }, 4, 0);
            tableLayoutPanelTruss.Controls.Add(new Label { Text = "OME Long", TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.None, Anchor = AnchorStyles.None , Font = HeaderFont }, 5, 0);

            // Define the order of names for text boxes
            string[] order = { "A", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "B" };

            // Add new TextBoxes for each row in the new columns
            for (int i = 0; i < order.Length; i++)
            {
                tableLayoutPanelTruss.Controls.Add(new System.Windows.Forms.TextBox { Name = "textBoxOME_Short_" + order[i], Dock = DockStyle.None, Anchor = AnchorStyles.None, Size = TextBoxSize }, 4, i + 1);
                tableLayoutPanelTruss.Controls.Add(new System.Windows.Forms.TextBox { Name = "textBoxOME_Long_" + order[i], Dock = DockStyle.None, Anchor = AnchorStyles.None, Size = TextBoxSize }, 5, i + 1);
            }

            areSkewedColumnsDisplayed = true; // Set the flag to true

            // Adjust the form size
            this.Size = new Size(600, 839);

            tableLayoutPanelTruss.ResumeLayout();
        }

        private void ResetTableLayoutPanelColumns()
        {
            if (!areSkewedColumnsDisplayed)
                return;

            tableLayoutPanelTruss.SuspendLayout();

            // Remove controls in the columns to be deleted
            for (int i = 4; i <= 5; i++)
            {
                for (int j = 0; j < tableLayoutPanelTruss.RowCount; j++)
                {
                    System.Windows.Forms.Control control = tableLayoutPanelTruss.GetControlFromPosition(i, j);
                    if (control != null)
                    {
                        tableLayoutPanelTruss.Controls.Remove(control);
                    }
                }
            }

            // Move controls in the existing columns to the left
            for (int i = 6; i < tableLayoutPanelTruss.ColumnCount; i++)
            {
                for (int j = 0; j < tableLayoutPanelTruss.RowCount; j++)
                {
                    System.Windows.Forms.Control control = tableLayoutPanelTruss.GetControlFromPosition(i, j);
                    if (control != null)
                    {
                        Debug.Print($"Moving control from ({i}, {j}) to ({i - 2}, {j})");
                        tableLayoutPanelTruss.SetColumn(control, i - 2);
                    }
                }
            }

            // Adjust the width of columns 4 and 5
            tableLayoutPanelTruss.ColumnStyles[4].Width = 50;
            tableLayoutPanelTruss.ColumnStyles[5].Width = 50;

            // Adjust the width of the tableLayoutPanel back
            tableLayoutPanelTruss.Size = new Size(tableLayoutPanelTruss.Width - 140, tableLayoutPanelTruss.Height);

            areSkewedColumnsDisplayed = false; // Reset the flag

            // Adjust the form size
            this.Size = new Size(528, 839);

            tableLayoutPanelTruss.ResumeLayout();
        }


        private void PopulateParameterValuesFromSymbol(FamilySymbol familySymbol)
        {
            // Set dimension text box values
            foreach (var entry in ControlMappings.TextBoxDimensionMappings)
            {
                if (entry.Key == "textBoxSkewME" || entry.Key == "textBoxSkewOME")
                {
                    Parameter param = familySymbol.LookupParameter(entry.Value);
                    if (param != null)
                    {
                        parameterValues[entry.Value] = UnitFormatUtils.Format(_doc.GetUnits(), SpecTypeId.Angle, param.AsDouble(), true);
                    }
                }
                else
                {
                    Parameter param = familySymbol.LookupParameter(entry.Value);
                    if (param != null)
                    {
                        parameterValues[entry.Value] = FormatParameterValue(param, _doc);
                    }
                }
            }

            // Set text box values
            foreach (var entry in ControlMappings.TextBoxMappings)
            {
                Parameter param = familySymbol.LookupParameter(entry.Value);
                if (param != null)
                {
                    parameterValues[entry.Value] = FormatParameterValue(param, _doc);
                }
            }

            // Set checkbox values
            foreach (var entry in ControlMappings.CheckBoxMappings)
            {
                Parameter param = familySymbol.LookupParameter(entry.Value);
                if (param != null)
                {
                    parameterValues[entry.Value] = param.AsInteger().ToString();
                }
            }
        }

        private string FormatParameterValue(Parameter param, Document doc)
        {
            switch (param.StorageType)
            {
                case StorageType.Double:
                    return UnitFormatUtils.Format(doc.GetUnits(), SpecTypeId.Length, param.AsDouble(), true);            
                case StorageType.Integer:
                    return param.AsInteger().ToString();
                case StorageType.String:
                    return param.AsString();
                default:
                    return "Unknown";
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

                foreach (Element element in collector)
                {
                    FamilySymbol symbol = element as FamilySymbol;
                    if (symbol != null)
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
                        else if (familyName == "VDC DualDeck_OME_Skewed")
                        {
                            display = $"DD Skew: {symbol.Name}";
                        }
                        else if (familyName == "VDC DualDeck_Double_Skewed")
                        {
                            display = $"DD 2Skew: {symbol.Name}";
                        }

                        if (!string.IsNullOrEmpty(display))
                        {
                            familyTypes.Add(new FamilyTypeItem(display, symbol.Id));
                        }
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
                (instance.Symbol.Family.Name == "VDC DualDeck_Mirror" || instance.Symbol.Family.Name == "VDC DualDeck_Adjust" || instance.Symbol.Family.Name == "VDC DualDeck_OME_Skewed"
                        || instance.Symbol.Family.Name == "VDC DualDeck_Double_Skewed"))
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
                         memberInstance.Symbol.Family.Name == "VDC DualDeck_Adjust" || 
                         memberInstance.Symbol.Family.Name == "VDC DualDeck_OME_Skewed" || 
                         memberInstance.Symbol.Family.Name == "VDC DualDeck_Double_Skewed"))
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
                       (fi.Symbol.Family.Name == "VDC DualDeck_Mirror" || fi.Symbol.Family.Name == "VDC DualDeck_Adjust" || fi.Symbol.Family.Name == "VDC DualDeck_OME_Skewed"
                        || fi.Symbol.Family.Name == "VDC DualDeck_Double_Skewed");
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false; // We only want to select elements, not references
            }
        }

        private void textBoxSkew_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true; // Prevent the beep sound
                FormatAsAngle(sender as System.Windows.Forms.TextBox);
            }
        }


        private void textBoxSkew_Leave(object sender, EventArgs e)
        {
            // Call the method to format the text in the TextBox to feet and inches format
            FormatAsAngle(sender as System.Windows.Forms.TextBox);
        }

        private void FormatAsAngle(System.Windows.Forms.TextBox textBox)
        {
            if (textBox == null) return;

            string input = textBox.Text.Trim();

            // Check if the input ends with the degree symbol and remove it for parsing
            if (input.EndsWith("°"))
            {
                input = input.TrimEnd('°').Trim();
            }

            if (decimal.TryParse(input, out decimal angle))
            {
                textBox.Text = $"{angle:F3}°"; // Format to 3 decimal places and add degree symbol
            }
            else
            {
                MessageBox.Show("Invalid angle format. Please enter a valid number.");
                textBox.Focus();
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
            if (changesTracker.Count > 0)
            {
                // Update the handler with current documents and parameters
                handler.Setup(_doc, familySymbol, activeFamilyType, changesTracker, this);
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
            if (is1Skew == true || is2Skew == true)
            {
                return;
            }
            else
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


        private readonly List<string> trussControlsToDisable = new List<string>
        {
            "textBoxShort_06", "textBoxLong_06",
            "checkBoxOnOff_06", "label12",
            "checkBox_2Out_06", "checkBox_2In_06"
        };

        private void checkBoxTrussOffset_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTruss6EnabledStatus();
        }

        private void UpdateTruss6EnabledStatus()
        {
            bool enable;

            if (is1Skew == true || is2Skew == true)
            {
                enable = true;
            }
            else
            {
                enable = !checkBoxTrussOffset.Checked;
            }

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

        private void checkBoxAsymOME_CheckChanged(object sender, EventArgs e)
        {
            UpdateOMETrussEnabledStatus();
        }

        private void UpdateOMETrussEnabledStatus()
        {
            // Positions of the entries you want to enable
            int[] textBoxMappingsPositions = Enumerable.Range(26, 26).ToArray(); // Entries 15-26 (0-based index)

            bool enable; // Initialize to a default value
            if (is1Skew == true || isAdjust == true || isMirror == true)
            {
                enable = true;
            }
            else
            {
                enable = checkBoxAsymOME.Checked;
            }

            List<string> keysToEnable = GetKeysToEnable(textBoxMappingsPositions, null, null, null);

            // Start the recursive process with the initial control
            UpdateMirrorTrussEnabledStatus(this, enable, keysToEnable);
        }

        private void UpdateMirrorTrussEnabledStatus()
        {
            // Positions of the entries you want to enable
            int[] textBoxMappingsPositions = null;
            int[] checkBoxMappingsPositionsPart1 = null;
            int[] checkBoxMappingsPositionsPart2 = null;                                                                            
            string[] specificLabels = { "label13", "label14", "label15", "label16", "label17", "label19" };

            bool enable = false; // Initialize to a default value
            if (is1Skew == true || is2Skew == true)
            {
                enable = true;
                // Positions of the entries you want to enable
                textBoxMappingsPositions = Enumerable.Range(14, 12).ToArray(); // Entries 15-26 (0-based index)
                checkBoxMappingsPositionsPart1 = Enumerable.Range(8, 6).ToArray(); // Entries 9-14 (0-based index)
                checkBoxMappingsPositionsPart2 = Enumerable.Range(28, 12).ToArray(); // Entries 29-40 (0-based index)                                                                            
            }
            else if (isMirror == true)
            {
                enable = false;
                // Positions of the entries you want to enable
                textBoxMappingsPositions = Enumerable.Range(14, 12).ToArray(); // Entries 15-26 (0-based index)
                checkBoxMappingsPositionsPart1 = Enumerable.Range(7, 6).ToArray(); // Entries 9-14 (0-based index)
                checkBoxMappingsPositionsPart2 = Enumerable.Range(27, 12).ToArray(); // Entries 29-40 (0-based index)                                                                            
            }
            else if (isAdjust == true)
            {
                enable = false;
                // Positions of the entries you want to enable
                textBoxMappingsPositions = Enumerable.Range(14, 12).ToArray(); // Entries 15-26 (0-based index)
                checkBoxMappingsPositionsPart1 = Enumerable.Range(8, 6).ToArray(); // Entries 9-14 (0-based index)
                checkBoxMappingsPositionsPart2 = Enumerable.Range(28, 12).ToArray(); // Entries 29-40 (0-based index)                                                                             
            }

            List<string> keysToEnable = GetKeysToEnable(textBoxMappingsPositions , checkBoxMappingsPositionsPart1 , checkBoxMappingsPositionsPart2 , specificLabels);

            // Start the recursive process with the initial control
            UpdateMirrorTrussEnabledStatus(this, enable, keysToEnable);
        }

        private void UpdateMirrorTrussEnabledStatus(System.Windows.Forms.Control control, bool enable, List<string> keysToEnable)
        {
            foreach (System.Windows.Forms.Control child in control.Controls)
            {
                if (child is System.Windows.Forms.TextBox textBox && keysToEnable.Contains(textBox.Name))
                {
                    textBox.Enabled = enable;
                }
                else if (child is System.Windows.Forms.CheckBox checkBox && keysToEnable.Contains(checkBox.Name))
                {
                    checkBox.Enabled = enable;
                }
                else if (child is System.Windows.Forms.Label label && keysToEnable.Contains(label.Name))
                {
                    label.Enabled = enable;
                }

                // Recursively handle child controls
                if (child.HasChildren)
                {
                    UpdateMirrorTrussEnabledStatus(child, enable, keysToEnable);
                }
            }
        }

        private List<string> GetKeysToEnable(int[] textBoxMappingsPositions, int[] checkBoxMappingsPositionsPart1, int[] checkBoxMappingsPositionsPart2, string[] specificLabels)
        {


            List<string> keysToEnable = new List<string>();

            if (textBoxMappingsPositions != null)
            {
                // Add keys from TextBoxMappings
                AddKeysFromDictionary(ControlMappings.TextBoxMappings, textBoxMappingsPositions, keysToEnable);
            }

            if (checkBoxMappingsPositionsPart1 != null && checkBoxMappingsPositionsPart2 != null)
            {
                // Add keys from CheckBoxMappings
                AddKeysFromDictionary(ControlMappings.CheckBoxMappings, checkBoxMappingsPositionsPart1, keysToEnable);
                AddKeysFromDictionary(ControlMappings.CheckBoxMappings, checkBoxMappingsPositionsPart2, keysToEnable);
            }

            if (specificLabels != null)
            {
                keysToEnable.AddRange(specificLabels); // Add keys from label list
            }

            // Debug print all keys to enable
            Debug.Print("Keys to enable:");
            foreach (var key in keysToEnable)
            {
                Debug.Print(key);
            }

            return keysToEnable;
        }

        // Function to get keys by positions from a dictionary
        private void AddKeysFromDictionary(Dictionary<string, string> dictionary, int[] positions, List<string> keys)
        {
            var keysArray = dictionary.Keys.ToArray();
            foreach (var pos in positions)
            {
                if (pos < keysArray.Length)
                {
                    keys.Add(keysArray[pos]); // Add the key, not the value
                }
            }
        }

        private void groupBoxStrand_Enter(object sender, EventArgs e)
        {

        }
    }
}
