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
            //TestFamilyParameters(doc);
        }

        public void TestFamilyParameters (Document doc)
        {
            if (!doc.IsFamilyDocument)
            {
                MessageBox.Show("This is not a family document!");
            }

            FamilyManager mgr = doc.FamilyManager;

            int n = mgr.Parameters.Size;
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

        static string FamilyParamValueString(Autodesk.Revit.DB.FamilyType t, FamilyParameter fp, Document doc)
        {
            string value = t.AsValueString(fp);
            switch (fp.StorageType)
            {
                case StorageType.Double:
                    value = Util.RealString((double)t.AsDouble(fp)) + " (double)";
                    break;

                case StorageType.ElementId:
                    ElementId id = t.AsElementId(fp);
                    Element e = doc.GetElement(id);
                    value = id.IntegerValue.ToString() + " ("
                      + Util.ElementDescription(e) + ")";
                    break;

                case StorageType.Integer:
                    value = t.AsInteger(fp).ToString()
                      + " (int)";
                    break;

                case StorageType.String:
                    value = "'" + t.AsString(fp)
                      + "' (string)";
                    break;
            }
            return value;
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

                    // Print each key in the keys list
                    foreach (string key in keys)
                    {
                        Debug.Print(key);
                    }

                    // Print the number of types in the family and their names
                    n = familyManager.Types.Size;

                    Debug.Print("\nFamily {0} has {1} type{2}{3}", _doc.Title, n, Util.PluralSuffix(n), Util.DotOrColon(n));

                    // Iterate through each type in the family
                    foreach (Autodesk.Revit.DB.FamilyType t in familyManager.Types)
                    {
                        string name = t.Name;
                        Debug.Print(" {0}:", name);

                        // For each parameter key, check if the type has a value set and print it
                        foreach ( string key in keys )
                        {
                            FamilyParameter fp = fps[key];
                            if(t.HasValue(fp))
                            {
                                string value = FamilyParamValueString(t, fp, _doc);

                                Debug.Print("    {0} = {1}", key, value);
                            }
                        }
                    }



                    //    FamilyParameter depthParam = familyManager.get_Parameter("DD_Depth");

                    //    MessageBox.Show("Family Manager: " + familyManager.ToString() + "\nDepthParam: " + depthParam.ToString());


                    //    if (depthParam != null)
                    //    {
                    //        textBoxDD_Depth.Text = depthParam.ToString();
                    //        // Debugging: Show the parameter's storage type and value
                    //        MessageBox.Show($"Parameter storage type: {depthParam.StorageType}");
                    //    }
                    //    else
                    //    {
                    //        textBoxDD_Depth.Text = "Parameter not found";
                    //        // Debugging: Parameter not found
                    //        MessageBox.Show("DD_Depth parameter not found.");
                    //    }
                    //}
                    //else
                    //{
                    //    MessageBox.Show("No instances found for the selected family type.");
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
