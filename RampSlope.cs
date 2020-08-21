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


//這個程式碼是要檢驗坡道的斜率，看有沒有符合規範
//建築物內規定應設置之樓梯可以坡道代替樓梯之坡度，不得超過 1:8 
//汽車坡道應設截水溝、坡度不得超過 1/6 
//機車坡道及汽機車併用車道坡度不得超過 1 比 8
namespace RegulationCheck
{
    [Transaction(TransactionMode.Manual)]
    public class RampSlope : IExternalCommand
    {

        public static List<string[]> form_list7 = new List<string[]>();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //我把坡道的樓板都用comment備註「坡道」了
            Document doc = commandData.Application.ActiveUIDocument.Document;
          


            FilteredElementCollector floorCollector = new FilteredElementCollector(doc);
            ElementCategoryFilter floorFilter = new ElementCategoryFilter(BuiltInCategory.OST_Floors);

            List<Element> floors = floorCollector.WherePasses(floorFilter).WhereElementIsNotElementType().ToElements().ToList();
            List<Floor> ramps = new List<Floor>();

            foreach(Element elem in floors)
            {
                if (elem.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString() == "坡道")
                    ramps.Add(elem as Floor);
            }

            //先找到高度變化了多少，找到裡面最大、最小的z
            double max_z = -100000;
            double min_z = 100000;

            foreach(Floor ramp in ramps)
            {
                BoundingBoxXYZ b = ramp.get_BoundingBox(null);
                if (b.Max.Z > max_z)
                    max_z = b.Max.Z;
                if (b.Min.Z < min_z)
                    min_z = b.Min.Z;
            }
            //原本是feet，轉成meter
            double ramp_height = (max_z - min_z) * 0.3048;
            //MessageBox.Show(min_z.ToString() + "," + max_z.ToString());


            //車道的長度 = 車道floor總面積 / 車道寬度，其中車道寬度 = 400cm
            double ramp_total_area = 0;
            foreach(Floor ramp in ramps)
            {
                string area_string = ramp.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED).AsValueString();
                int area_index = ramp.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED).AsValueString().IndexOf('m');
                ramp_total_area += Convert.ToDouble(area_string.Substring(0, area_index));
            }

            //這個是meter
            double ramp_length = ramp_total_area / 4;

            double ramp_slope = ramp_height / ramp_length;
            double regulation_slope = 0.166667;


            string newFileName = @"C:\Users\8014\Desktop\BIM模型檢核.csv";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("7.坡道坡度檢核");
            sb.AppendLine("規範：機車坡道及汽機車併用車道坡度不得超過 1 比 8");
            sb.AppendLine("規範車道最大坡度\t此模型車道坡度\t是否通過規範");

            form_list7.Add(new string[] { "7.坡道坡度檢核: " });
            form_list7.Add(new string[] { "規範：機車坡道及汽機車併用車道坡度不得超過 1 比 8" });


            sb.AppendLine(regulation_slope.ToString() + "\t" + ramp_slope.ToString() + "\t"
                + (ramp_slope <= regulation_slope).ToString());

            string[] row = new string[] { "無", "坡道", ramp_slope.ToString(), "無", regulation_slope.ToString(), (ramp_slope <= regulation_slope).ToString() };
            form_list7.Add(row);

            sb.AppendLine();
            sb.AppendLine();
            File.AppendAllText(newFileName, sb.ToString(), Encoding.Unicode);

            form_list7.Add(new string[] { });
            form_list7.Add(new string[] { });




            return Result.Succeeded;
        }
    }
}
