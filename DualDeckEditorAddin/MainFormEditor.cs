using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DualDeckEditorAddin.MainFormEditor;

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
            if (comboBoxFamilyType.SelectedItem is FamilyType selectedFamilyType)
            {
                // Use a filtered element collector to find all instances of the selected family type
                var collector = new FilteredElementCollector(_doc)
                    .OfClass(typeof(FamilyInstance))
                    .WhereElementIsNotElementType()
                    .Where(x => ((FamilyInstance)x).Symbol.Id == selectedFamilyType.Id);

                // Debugging: Output the number of instances found
                MessageBox.Show($"Number of instances found: {collector.Count()}");

                // Try to get the first instance of this family type
                var selectedInstance = collector.FirstOrDefault() as FamilyInstance;

                if (selectedInstance != null)
                {
                    // Debugging: Confirm the instance ID
                    MessageBox.Show($"Instance ID: {selectedInstance.Id.IntegerValue}");

                    Family family = selectedInstance.Symbol.Family;

                    Document familyDoc = _doc.EditFamily( family );
                    
                    FamilyManager familyManager = familyDoc.FamilyManager;

                    FamilyParameter depthParam = familyManager.get_Parameter("DD_Depth");

                    if (depthParam != null)
                    {
                        textBoxDD_Depth.Text = depthParam.ToString();
                        // Debugging: Show the parameter's storage type and value
                        MessageBox.Show($"Parameter storage type: {depthParam.StorageType}");
                    }
                    else
                    {
                        textBoxDD_Depth.Text = "Parameter not found";
                        // Debugging: Parameter not found
                        MessageBox.Show("DD_Depth parameter not found.");
                    }
                }
                else
                {
                    MessageBox.Show("No instances found for the selected family type.");
                }
            }
        }


        //private string FormatLength(double val)
        //{
        //    // Assuming 'val' is in feet and we need to convert it to a string in feet and inches format.
        //    // Use FormatUtils for formatting based on built-in types
        //    return UnitFormatUtils.Format(
        //        _doc.GetUnits(),                      // Use the document's unit settings
        //        UnitType.UT_Length,                   // Specify the unit type as length
        //        val,                                  // The value in feet
        //        false,                                // Specify if the units are abbreviated
        //        false,                                // Specify if the value is in a different unit system
        //        false);                               // Display zero in fields if necessary
        //}

        public class FamilyType
        {
            public string Name { get; set; }
            public ElementId Id { get; set; }

            public FamilyType(string name, ElementId id)
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
                List<FamilyType> familyTypes = new List<FamilyType>();

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
                        familyTypes.Add(new FamilyType(display, symbol.Id));
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
                    foreach (FamilyType item in comboBoxFamilyType.Items)
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
                    foreach (FamilyType item in comboBoxFamilyType.Items)
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
