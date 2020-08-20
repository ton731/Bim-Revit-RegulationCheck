using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Windows.Forms;
using Autodesk.Revit.DB.Architecture;
using System.IO;


//樓梯的檢測
//樓梯之垂直淨空距離不得小於 190 公分
//設置於露臺、陽臺、室外走廊、室外樓梯、平屋頂及室內天井部 分等之欄桿扶手高度欄桿扶手高度，不得小於 1.1 公尺，十層以上者，不得小 於 1.2 公尺
//也就是說，樓梯的高度要大於190cm?

namespace RegulationCheck
{
    [Transaction(TransactionMode.Manual)]
    public class StairsCheck : IExternalCommand
    {
        double regulation_stairs_height = 190;
        double regulation_railing_height;


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;


            //因為樓層數會影響一些標準的高度，所以先把樓層高度找出來
            FilteredElementCollector levelCollector = new FilteredElementCollector(doc);
            ElementCategoryFilter levelFilter = new ElementCategoryFilter(BuiltInCategory.OST_Levels);

            List<Element> levels = levelCollector.WherePasses(levelFilter).ToElements().ToList();

            int floor_count = 0;

            foreach (Element elem in levels)
            {
                if (elem.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == "樓層")
                {
                    //在地表的樓層才算，Elevation>=0，屋突好像也不算，名字P開頭的不行
                    Level level = elem as Level;
                    if (level.Name.ToList()[0] != 'P' & level.Name.ToList()[0] != 'G')
                    {
                        if (level.Elevation >= 0)
                        {
                            floor_count += 1;
                        }
                    }

                }

            }
            if (floor_count >= 10)
                regulation_railing_height = 120;
            else
                regulation_railing_height = 110;




            string newFileName = @"C:\Users\8014\Desktop\BIM模型檢核.csv";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("5.樓梯垂直淨空高度檢核");
            sb.AppendLine("規範：樓梯之垂直淨空距離不得小於 190 公分");
            sb.AppendLine("樓梯名稱\t樓梯高度(cm)\t規範高度(cm)\t是否符合規範");


            //先把全部的樓梯抓出來，有分室外梯以及室內梯，不過好像不用特別分室外跟室內耶

            FilteredElementCollector stairsCollector = new FilteredElementCollector(doc);
            ElementCategoryFilter stairsFilter = new ElementCategoryFilter(BuiltInCategory.OST_Stairs);

            List<Element> all_stairs = stairsCollector.WherePasses(stairsFilter).WhereElementIsNotElementType().ToElements().ToList();
            //List<Element> indoor_stairs = new List<Element>();

            //foreach(Element elem in all_stairs)
            //{
            //    if (elem.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsValueString().Contains("室內RC"))
            //        indoor_stairs.Add(elem);
            //}

            foreach(Element elem in all_stairs)
            {
                Stairs stairs = elem as Stairs;
                double stairs_height = Convert.ToDouble(stairs.get_Parameter(BuiltInParameter.STAIRS_STAIRS_HEIGHT).AsValueString());
                string s = stairs.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString() + "\t" + stairs_height.ToString() + "\t"
                    + regulation_stairs_height.ToString() + "\t"
                    + (stairs_height >= regulation_stairs_height).ToString();
                sb.AppendLine(s);
            }

            sb.AppendLine();
            sb.AppendLine();
            File.AppendAllText(newFileName, sb.ToString(), Encoding.Unicode);






            StringBuilder sb2 = new StringBuilder();
            sb2.AppendLine("6.欄杆扶手高度檢核：");
            sb2.AppendLine("規範：設置於露臺、陽臺、室外走廊、室外樓梯、平屋頂及室內天井部 分等之欄桿扶手高度欄桿扶手高度，不得小於1.1公尺，十層以上者，不得小於1.2公尺");
            sb2.AppendLine("欄杆名稱\t欄杆高度(cm)\t規範高度(cm)\t是否符合規範");

            //接下來要取欄杆高度
            //樓梯用的扶手、欄杆是900mm的，跟女兒牆的布一樣
            //要扣除掉女兒牆的欄杆的
            FilteredElementCollector railingCollector = new FilteredElementCollector(doc);
            ElementCategoryFilter railingFilter = new ElementCategoryFilter(BuiltInCategory.OST_StairsRailing);

            List<Element> railings = railingCollector.WherePasses(railingFilter).WhereElementIsNotElementType().ToElements().ToList();


            foreach(Element elem in railings)
            {
                if (!(elem.Name.Contains("900mm")))
                    continue;
                Railing railing = elem as Railing;
                ElementType elementType = doc.GetElement(railing.GetTypeId()) as ElementType;
                double railing_height = Convert.ToDouble(elementType.get_Parameter(BuiltInParameter.STAIRS_RAILING_HEIGHT).AsValueString());
                string s = railing.Name + "\t" + railing_height.ToString() + "\t" + regulation_railing_height.ToString()
                    + "\t" + (railing_height >= regulation_railing_height).ToString();
                sb2.AppendLine(s);
            }

            sb2.AppendLine();
            sb2.AppendLine();

            File.AppendAllText(newFileName, sb2.ToString(), Encoding.Unicode);



            return Result.Succeeded;
        }

    }
}
