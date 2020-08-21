using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Windows.Forms;


//這個是要讓使用者可以選擇想要建立的門種類，然後建立在選擇的牆上
namespace RegulationCheck
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class MakeFormConnect : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication UIapp = commandData.Application;
            UIDocument UIdoc = UIapp.ActiveUIDocument;
            Document doc = UIdoc.Document;

            if (doc == null)
            {
                message = "Active document is null";
                return Result.Failed;
            }

            try
            {
                Main main = new Main();
                main.Execute(commandData, ref message, elements);
                MakeFormStorage creator = new MakeFormStorage(UIapp);
                MakeFormWindowsForm windowsForm = new MakeFormWindowsForm(creator);
                windowsForm.Show();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
