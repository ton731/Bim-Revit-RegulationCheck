using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.IO;


//這個程式要檢查屋突高度、面積檢討
//屋突高度需在六公尺內，或昇降機設備通 達之屋突高度需在九公尺之內。
namespace RegulationCheck
{
    [Transaction(TransactionMode.Manual)]
    public class WuToo : IExternalCommand
    {
        bool has_elevator = true;
        double max_wutoo_height = 0;
        public static List<string[]> form_list3 = new List<string[]>();
        public static List<string[]> form_list4 = new List<string[]>();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //屋突高度為RF與PR之間的高度差

            if (has_elevator)
                max_wutoo_height = 900;
            else
                max_wutoo_height = 600;




            Document doc = commandData.Application.ActiveUIDocument.Document;

            FilteredElementCollector levelCollector = new FilteredElementCollector(doc);
            ElementCategoryFilter levelFilter = new ElementCategoryFilter(BuiltInCategory.OST_Levels);

            List<Element> levels = levelCollector.WherePasses(levelFilter).ToElements().ToList();

            Level rf = null;
            Level pr = null;
            

            foreach(Element elem in levels)
            {
                if (elem.Name == "RF")
                    rf = elem as Level;
                if (elem.Name == "PR")
                    pr = elem as Level;
            }

            double wutoo_height = Convert.ToDouble(pr.get_Parameter(BuiltInParameter.LEVEL_ELEV).AsValueString())
                                        - Convert.ToDouble(rf.get_Parameter(BuiltInParameter.LEVEL_ELEV).AsValueString());

            //MessageBox.Show(wutoo_height.ToString());

            string newFileName = @"C:\Users\8014\Desktop\BIM模型檢核.csv";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("3.屋突高度檢核：");
            sb.AppendLine("規範：屋突高度需在六公尺內，或昇降機設備通 達之屋突高度需在九公尺之內。");
            sb.AppendLine("此建築模型是否有升降機設備通達屋突高度：" + "\t" + has_elevator.ToString());
            sb.AppendLine("屋突最大高度(cm):" + "\t" + max_wutoo_height.ToString());
            sb.AppendLine("此棟建築屋突是否有升降梯\t屋突高度(cm)\t最大屋突高度(cm)\t是否有符合規範");

            form_list3.Add(new string[] { "3.屋突高度檢核：" });
            form_list3.Add(new string[] { "規範：屋突高度需在六公尺內，或昇降機設備通 達之屋突高度需在九公尺之內。" });



            sb.AppendLine(has_elevator.ToString()+"\t"+ wutoo_height.ToString()+"\t"
                + max_wutoo_height.ToString()+"\t"+ (wutoo_height < max_wutoo_height).ToString());

            string[] row = new string[] { "無", "無", wutoo_height.ToString(), "無", max_wutoo_height.ToString(), (wutoo_height < max_wutoo_height).ToString() };
            form_list3.Add(row);

            sb.AppendLine();
            sb.AppendLine();

            form_list3.Add(new string[] { });
            form_list3.Add(new string[] { });

            File.AppendAllText(newFileName, sb.ToString(), Encoding.Unicode);



            //接下來要算建築物面積，有點不太確定怎麼算，先算最簡單的樓板好了
            //因為有些是裝修版，所以去找名字叫樓板RC版的就好(名字有RC的)
            //但是我必須先扣掉RF那層，不然會算到RF整塊的樓板，但是我要的只有屋突那塊
            //所以我先算上面那三層，再乘以4/3
            FilteredElementCollector floorCollector = new FilteredElementCollector(doc);
            ElementCategoryFilter floorFilter = new ElementCategoryFilter(BuiltInCategory.OST_Floors);

            List<Element> floors = floorCollector.WherePasses(floorFilter).WhereElementIsNotElementType().ToElements().ToList();

            double total_area = 0;
            double wutoo_area = 0;
            double rf_elevation = Convert.ToDouble(rf.get_Parameter(BuiltInParameter.LEVEL_ELEV).AsValueString()) + 50;

            foreach (Element elem in floors)
            {
                Floor floor = elem as Floor;
                if (!(floor.Name.Contains("RC")))
                    continue;

                string area_string = floor.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED).AsValueString();
                int area_index = floor.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED).AsValueString().IndexOf('m');
                total_area += Convert.ToDouble(area_string.Substring(0, area_index));

                double floor_elev = Convert.ToDouble(floor.get_Parameter(BuiltInParameter.STRUCTURAL_ELEVATION_AT_TOP).AsValueString());
                if(floor_elev >= rf_elevation)
                    wutoo_area += Convert.ToDouble(area_string.Substring(0, area_index));

            }

            wutoo_area = wutoo_area * 4 / 3;

            StringBuilder sb2 = new StringBuilder();
            sb2.AppendLine("4.屋突面積檢核：");
            sb2.AppendLine("規範：屋突面積需小於建築面積之 1/8 ");
            sb2.AppendLine("建築物總面積(m2)\t屋突總面積(m2)\t是否小於規範要求的面積");

            form_list4.Add(new string[] { "4.屋突面積檢核：" });
            form_list4.Add(new string[] { "規範：屋突面積需小於建築面積之 1/8" });

            sb2.AppendLine(total_area.ToString() + "\t" + wutoo_area.ToString() + "\t"
                + (wutoo_area < total_area / 8).ToString());

            string[] row2 = new string[] { "無", "無", wutoo_area.ToString(), "無", (total_area/8).ToString(), (wutoo_area < total_area / 8).ToString() };
            form_list4.Add(row2);

            sb2.AppendLine();
            sb2.AppendLine();

            form_list4.Add(new string[] { });
            form_list4.Add(new string[] { });

            File.AppendAllText(newFileName, sb2.ToString(), Encoding.Unicode);


            return Result.Succeeded;
        }
    }
}
