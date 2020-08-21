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
using System.Runtime.Remoting.Messaging;

//這個是呼叫所有的class，然後做成csv
namespace RegulationCheck
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        //public static BasicInfo bi = new BasicInfo();
        //public static RoomHeight rh = new RoomHeight();
        //public static ParapetHeight ph = new ParapetHeight();
        //public static WuToo wt = new WuToo();
        //public static StairsCheck sc = new StairsCheck();
        //public static RampSlope rs = new RampSlope();
        //public static FireProofDoor fpd = new FireProofDoor();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            BasicInfo bi = new BasicInfo();
            RoomHeight rh = new RoomHeight();
            ParapetHeight ph = new ParapetHeight();
            WuToo wt = new WuToo();
            StairsCheck sc = new StairsCheck();
            RampSlope rs = new RampSlope();
            FireProofDoor fpd = new FireProofDoor();



            bi.Execute(commandData, ref message, elements);
            rh.Execute(commandData, ref message, elements);
            ph.Execute(commandData, ref message, elements);
            wt.Execute(commandData, ref message, elements);
            sc.Execute(commandData, ref message, elements);
            rs.Execute(commandData, ref message, elements);
            fpd.Execute(commandData, ref message, elements);
            



            return Result.Succeeded;
        }
    }
}
