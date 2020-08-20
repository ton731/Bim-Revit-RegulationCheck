using Autodesk.Revit.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using System.IO;


//防火門填入防火等級與防火時效後，尺寸寬應 在七十五公分以上，高應在一百八十公分
namespace RegulationCheck
{
    [Transaction(TransactionMode.Manual)]
    public class FireProofDoor : IExternalCommand
    {
        double regulation_door_width = 75;
        double regulation_door_height = 180;


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            FilteredElementCollector familyInstanceCollector = new FilteredElementCollector(doc);
            ElementClassFilter familyInstanceFilter = new ElementClassFilter(typeof(FamilyInstance));

            List<Element> familyInstances = familyInstanceCollector.WherePasses(familyInstanceFilter).WhereElementIsNotElementType().ToElements().ToList();

            string newFileName = @"C:\Users\8014\Desktop\BIM模型檢核.csv";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("8.防火門尺寸檢核：");
            sb.AppendLine("規範：防火門填入防火等級與防火時效後，尺寸寬應 在七十五公分以上，高應在一百八十公分");
            sb.AppendLine("防火門樓層\t防火門名稱\t寬度(cm)\t高度(cm)\t規範最小寬度(cm)\t規範最小高度(cm)\t是否通過規範");





            foreach (Element elem in familyInstances)
            {
                if (elem.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString().Contains("防火門"))
                {
                    FamilyInstance familyInstance = elem as FamilyInstance;
                    string doorLevel = familyInstance.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM).AsValueString();
                    string doorName = familyInstance.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString();
                    double width = Convert.ToDouble(familyInstance.Symbol.get_Parameter(BuiltInParameter.DOOR_WIDTH).AsValueString());
                    double height = Convert.ToDouble(familyInstance.Symbol.get_Parameter(BuiltInParameter.DOOR_HEIGHT).AsValueString());
                    sb.AppendLine(doorLevel + "\t" + doorName + "\t" + width.ToString() + "\t" + height.ToString()
                       + "\t" + regulation_door_width.ToString() + "\t" + regulation_door_height.ToString() + "\t"
                       + (width >= regulation_door_width & height >= regulation_door_height).ToString());
                }
            }

            sb.AppendLine();
            sb.AppendLine();
            File.AppendAllText(newFileName, sb.ToString(), Encoding.Unicode);






            return Result.Succeeded;
        }
    }
}
