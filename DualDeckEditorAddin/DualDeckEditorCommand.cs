using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DualDeckEditorAddin
{
    [Transaction(TransactionMode.Manual)]
    public class DualDeckEditorCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Create and load the form
            MainFormEditor form = new MainFormEditor(doc, uiDoc);
            form.LoadDualDeckTypes(doc);

            // Set the Revit main window as the owner of the form
            IntPtr revitHandle = uiApp.MainWindowHandle;
            System.Windows.Forms.NativeWindow revitWindow = new System.Windows.Forms.NativeWindow();
            revitWindow.AssignHandle(revitHandle);

            form.Show(revitWindow);  // Show the form with Revit as its owner

            return Result.Succeeded;
        }
    }
}
