using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;
using System.Windows.Forms;
using System.Net.Http.Headers;
using System.IO;


//居室及浴室房間高度不可小於2.1m ，樓板算到天花板
namespace RegulationCheck
{
    [Transaction(TransactionMode.Manual)]
    public class RoomHeight : IExternalCommand
    {
        double regulation_height = 210;
        public static int room_count = 0;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            FilteredElementCollector roomCollector = new FilteredElementCollector(doc);
            ElementCategoryFilter roomFilter = new ElementCategoryFilter(BuiltInCategory.OST_Rooms);

            List<Element> rooms = roomCollector.WherePasses(roomFilter).WhereElementIsNotElementType().ToElements().ToList();
            room_count = rooms.Count;

            string newFileName = @"C:\Users\8014\Desktop\BIM模型檢核.csv";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("1.房間種類高度檢核：");
            sb.AppendLine("規範：居室及浴室房間高度不可小於2.1m ");
            sb.AppendLine("房間樓層\t房間名稱(種類)\t目前房高(cm)\t規範最低房高(cm)\t是否通過規範");

            foreach (Element elem in rooms)
            {
                Room room = elem as Room;
                string level_name = room.get_Parameter(BuiltInParameter.LEVEL_NAME).AsString();
                string room_name = room.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
                double room_height = Convert.ToDouble(room.get_Parameter(BuiltInParameter.ROOM_HEIGHT).AsValueString()) / 10;
                sb.AppendLine(level_name + "\t" + room_name + "\t" + room_height.ToString() + "\t"
                    + regulation_height.ToString() + "\t"
                    + (room_height >= regulation_height).ToString());
            }

            sb.AppendLine();
            sb.AppendLine();
            File.AppendAllText(newFileName, sb.ToString(), Encoding.Unicode);

            return Result.Succeeded;
        }






        //    Document doc = commandData.Application.ActiveUIDocument.Document;

        //    //先把整棟樓層的Room找出來
        //    FilteredElementCollector roomCollector = new FilteredElementCollector(doc);
        //    ElementCategoryFilter roomFilter = new ElementCategoryFilter(BuiltInCategory.OST_Rooms);

        //    List<Element> roomList = roomCollector.WherePasses(roomFilter).WhereElementIsNotElementType().ToElements().ToList();


        //    //把不是1F的room刪掉
        //    List<int> not_1F_list = new List<int>();
        //    int count = 0;
        //    foreach (Element elem in roomList)
        //    {
        //        Room room = elem as Room;
        //        Level level = doc.GetElement(room.LevelId) as Level;
        //        if (level.Name != "1F")
        //        {
        //            not_1F_list.Add(count);
        //        }
        //        count = count + 1;
        //    }

        //    for (int i = not_1F_list.Count - 1; i >= 0; i--)
        //    {
        //        roomList.RemoveAt(not_1F_list[i]);
        //    }


        //    //找到一樓的以後，先區分客房和浴室，主要分法就是有沒有包含衛工裝置
        //    //先把所以1F的衛工裝置位置存到一個list
        //    FilteredElementCollector sanitaryCollector = new FilteredElementCollector(doc);
        //    ElementCategoryFilter sanitaryFilter = new ElementCategoryFilter(BuiltInCategory.OST_PlumbingFixtures);

        //    IList<Element> sanitaryList = sanitaryCollector.WherePasses(sanitaryFilter).WhereElementIsNotElementType().ToElements();
        //    List<XYZ> sanitary_1F = new List<XYZ>();

        //    foreach (Element sanitary in sanitaryList)
        //    {
        //        string levelString = sanitary.get_Parameter(BuiltInParameter.SCHEDULE_LEVEL_PARAM).AsValueString();
        //        if (levelString == "1F")
        //        {
        //            LocationPoint locPoint = sanitary.Location as LocationPoint;
        //            sanitary_1F.Add(locPoint.Point);
        //        }
        //    }

        //    //收集所有的天花板，然後只留下1F的
        //    FilteredElementCollector ceilingCollector = new FilteredElementCollector(doc);
        //    ElementCategoryFilter ceilingFilter = new ElementCategoryFilter(BuiltInCategory.OST_Ceilings);

        //    IList<Element> ceilingList = ceilingCollector.WherePasses(ceilingFilter).WhereElementIsNotElementType().ToElements();
        //    List<Element> ceiling_1F = new List<Element>();

        //    foreach (Ceiling ceiling in ceilingList)
        //    {
        //        string levelString = ceiling.get_Parameter(BuiltInParameter.LEVEL_PARAM).AsValueString();
        //        if (levelString == "1F")
        //        {
        //            ceiling_1F.Add(ceiling);
        //        }
        //    }



        //    //有了1F所有衛工裝置位置後，跑每一個room，如果有任何一個衛工裝置在裡面的話，他就是浴室
        //    //然後順便配對天花板，天花板中心點距離最近的房間中點，就把那個天花版配對給他
        //    //然後紀錄每個房間是合格還是不合格.
        //    List<Room> BathRoom = new List<Room>();
        //    List<Room> OtherRoom = new List<Room>();
            


        //    string newFileName = @"C:\Users\8014\Desktop\BIM模型檢核.csv";
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine("房間高度檢核：");
        //    sb.AppendLine("規範：居室及浴室房間高度不可小於2.1m ");
        //    sb.AppendLine("房間名稱\t目前房高(cm)\t規範最低房高(cm)\t是否通過規範");

        //    foreach (Room room in roomList)
        //    {
        //        //先刪除掉錯誤的房間
        //        if (room.Area == 0)
        //        {
        //            Transaction trans = new Transaction(doc, "delete empty room");
        //            trans.Start();
        //            doc.Delete(room.Id);
        //            trans.Commit();
        //            continue;
        //        }

        //        //分成浴室和客房
        //        bool isBathRoom = false;
        //        foreach (XYZ sanitary_point in sanitary_1F)
        //        {
        //            if (room.IsPointInRoom(sanitary_point))
        //            {
        //                BathRoom.Add(room);
        //                isBathRoom = true;
        //                break;
        //            }
        //        }

        //        if (isBathRoom == false)
        //        {
        //            OtherRoom.Add(room);
        //        }

        //        //指派天花板
        //        Ceiling room_ceiling = ceiling_1F[0] as Ceiling;
        //        XYZ room_center = GetElementCenter(doc, room);
        //        foreach (Ceiling ceiling in ceiling_1F)
        //        {
        //            double current_distance = Distance(room_center, GetElementCenter(doc, room_ceiling));
        //            double new_distance = Distance(room_center, GetElementCenter(doc, ceiling));
        //            if (new_distance < current_distance)
        //                room_ceiling = ceiling;
        //        }

        //        string roomtype = "";
        //        if (isBathRoom)
        //            roomtype = "浴室";
        //        else
        //            roomtype = "客房";

        //        double current_height = Convert.ToDouble(room_ceiling.get_Parameter(BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM).AsValueString());


        //        string s = room.Name + "\t" + current_height.ToString()
        //            + "\t" + standard_height.ToString() + "\t" + (current_height > standard_height).ToString();

        //            sb.AppendLine(s);
        //    }
        //    sb.AppendLine();
        //    sb.AppendLine();

        //    File.AppendAllText(newFileName, sb.ToString(), Encoding.Unicode);
            

        //    return Result.Succeeded;
        //}
        

        //public XYZ GetElementCenter(Document doc, Element elem)
        //{
        //    BoundingBoxXYZ box = elem.get_BoundingBox(doc.ActiveView);
        //    double x = (box.Min.X + box.Max.X) / 2;
        //    double y = (box.Min.Y + box.Max.Y) / 2;
        //    double z = (box.Min.Y + box.Max.Z) / 2;
        //    return new XYZ(x, y, z);
        //}

        //public double Distance(XYZ pt1, XYZ pt2)
        //{
        //    return Math.Pow(pt1.X - pt2.X, 2) + Math.Pow(pt1.Y - pt2.Y, 2);
        //}
    }
}
