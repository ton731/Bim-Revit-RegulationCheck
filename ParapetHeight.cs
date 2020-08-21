using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Windows.Forms;
using System.IO;



//這個要檢測女兒牆高度，不得高過1.5m
//兩層以上>1m，三層以上>1.1m，十層以上>1.2m
namespace RegulationCheck
{
    [Transaction(TransactionMode.Manual)]
    public class ParapetHeight :IExternalCommand
    {
        int regulation_parapet_min_height;
        int regulation_parapet_max_height = 150;
        public static int floor_count;
        public static double highest_elev = 0;
        public static List<string[]> form_list2 = new List<string[]>();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            //先查出房子有幾樓，然後設定規範女兒牆高度
            FilteredElementCollector levelCollector = new FilteredElementCollector(doc);
            ElementCategoryFilter levelFilter = new ElementCategoryFilter(BuiltInCategory.OST_Levels);

            List<Element> levels = levelCollector.WherePasses(levelFilter).WhereElementIsNotElementType().ToElements().ToList();

            floor_count = 0;

            foreach(Element elem in levels)
            {
                //順便算一夏最高樓高
                if (elem.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == "樓層")
                {
                    //在地表的樓層才算，Elevation>=0，屋突好像也不算，名字P開頭的不行
                    Level level = elem as Level;
                    if(level.Name.ToList()[0] != 'P' & level.Name.ToList()[0] != 'G')
                    {
                        if (level.Elevation >= 0)
                        {
                            floor_count += 1;
                        }
                    }

                    double elev = Convert.ToDouble(level.get_Parameter(BuiltInParameter.LEVEL_ELEV).AsValueString());
                    if (elev > highest_elev)
                        highest_elev = elev;
                }
                    
            }
            //RF應該要扣掉
            floor_count -= 1;


            if (floor_count >= 10)
                regulation_parapet_min_height = 120;
            else if (floor_count >= 3)
                regulation_parapet_min_height = 120;
            else
                regulation_parapet_min_height = 100;





            string newFileName = @"C:\Users\8014\Desktop\BIM模型檢核.csv";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("2.女兒牆高度檢核：");
            sb.AppendLine("規範：女兒牆高度，兩層以上大於1m，三層以上大於1.1m，十層以上大於1.2m，不得超過1.5m");
            sb.AppendLine("此建築模型樓層數：" + "\t" + floor_count.ToString());
            sb.AppendLine("女兒牆最低高度：" + "\t" + regulation_parapet_min_height.ToString() +"cm");
            sb.AppendLine("女兒牆名稱\t樓層\t女兒牆高(cm)\t最低高度(cm)\t最高高度(cm)\t是否符合規範");

            form_list2.Add(new string[] { "2.女兒牆高度檢核：" });
            form_list2.Add(new string[] { "規範：女兒牆高度，兩層以上大於1m，三層以上大於1.1m，十層以上大於1.2m，不得超過1.5m" });


            //女兒牆上面都有一個欄杆，高度是20cm，它裡面沒有這個參數，所以我等下牆的高度要再加上20cm才是女兒牆的高度
            //先把牆都找到
            FilteredElementCollector wallCollector = new FilteredElementCollector(doc);
            ElementCategoryFilter wallFilter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);

            List<Element> walls = wallCollector.WherePasses(wallFilter).ToElements().ToList();

            int count = 0;

            foreach(Element elem in walls)
            {
                try
                {
                    if (!(elem.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString() == "女兒牆"))
                        continue;
                }
                catch(Exception e)
                {
                    continue;
                }

                Wall wall = elem as Wall;
                //MessageBox.Show(wall.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET).AsValueString());

                count += 1;
                //女兒牆都有top offset
                string wall_name = wall.Name;
                string wall_level = doc.GetElement(wall.LevelId).Name;

                double wall_Height = Convert.ToDouble(wall.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET).AsValueString()) + 20;
                string wall_height = wall_Height.ToString();

                string s = wall_name + "\t" + wall_level + "\t" + wall_height + "\t"
                    + regulation_parapet_min_height.ToString() + "\t" + regulation_parapet_max_height.ToString()
                    + "\t" + (wall_Height >= regulation_parapet_min_height & wall_Height <= regulation_parapet_max_height).ToString();

                sb.AppendLine(s);

                string[] row = new string[] { wall.Id.ToString(), wall_name, wall_height, regulation_parapet_min_height.ToString(),regulation_parapet_max_height.ToString(), (wall_Height >= regulation_parapet_min_height & wall_Height <= regulation_parapet_max_height).ToString() };
                form_list2.Add(row);
            }

            sb.AppendLine();
            sb.AppendLine();

            File.AppendAllText(newFileName, sb.ToString(), Encoding.Unicode);

            form_list2.Add(new string[] { });
            form_list2.Add(new string[] { });

            return Result.Succeeded;
        }
    }
}
