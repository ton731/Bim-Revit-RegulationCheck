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

namespace RegulationCheck
{
    [Transaction(TransactionMode.Manual)]
    public class BasicInfo : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //這裡裝一些建築物該有的基本資訊
            //檔案名稱、樓層數、樓高、總共房間數
            Document doc = commandData.Application.ActiveUIDocument.Document;
            string name = doc.Title;
            int floor_count = 0;
            double highest_elev = 0;
            int room_count = 0;






            //因為基礎資訊要放最上面，而醉意開始其他檢核的程式碼還沒執行到
            //所以我在一開始就先把要求的在這邊球一求好了
            FilteredElementCollector levelCollector = new FilteredElementCollector(doc);
            ElementCategoryFilter levelFilter = new ElementCategoryFilter(BuiltInCategory.OST_Levels);
            List<Element> levels = levelCollector.WherePasses(levelFilter).WhereElementIsNotElementType().ToElements().ToList();
            foreach (Element elem in levels)
            {
                //順便算一夏最高樓高
                if (elem.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == "樓層")
                {
                    //在地表的樓層才算，Elevation>=0，屋突好像也不算，名字P開頭的不行
                    Level level = elem as Level;
                    if (level.Name.ToList()[0] != 'P' & level.Name.ToList()[0] != 'G')
                    {
                        if (level.Elevation >= 0)
                            floor_count += 1;
                        
                    }

                    double elev = Convert.ToDouble(level.get_Parameter(BuiltInParameter.LEVEL_ELEV).AsValueString());
                    if (elev > highest_elev)
                        highest_elev = elev;
                }
            }
            //屋頂感覺要扣掉
            floor_count -= 1;



            FilteredElementCollector roomCollector = new FilteredElementCollector(doc);
            ElementCategoryFilter roomFilter = new ElementCategoryFilter(BuiltInCategory.OST_Rooms);
            List<Element> rooms = roomCollector.WherePasses(roomFilter).WhereElementIsNotElementType().ToElements().ToList();
            room_count = rooms.Count;





            //寫入csv
            string newFileName = @"C:\Users\8014\Desktop\BIM模型檢核.csv";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("建築模型基本資訊：");
            sb.AppendLine();
            sb.AppendLine("模型名稱：" + "\t" + name);
            sb.AppendLine("總共樓層數：" + "\t" + floor_count.ToString());
            sb.AppendLine("樓高：" + "\t" + (highest_elev/100).ToString() + "m");
            sb.AppendLine("總共房間種類：" + "\t" + room_count.ToString());

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            File.AppendAllText(newFileName, sb.ToString(), Encoding.Unicode);



            return Result.Succeeded;
        }
    }
}
