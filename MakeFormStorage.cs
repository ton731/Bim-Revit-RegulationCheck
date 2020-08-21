using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

namespace RegulationCheck
{
    public class MakeFormStorage
    {
        private UIDocument UIdoc;

        public UIDocument RevitDoc
        {
            get
            {
                return UIdoc;
            }
        }

        public MakeFormStorage(UIApplication UIapp)
        {
            UIdoc = UIapp.ActiveUIDocument;
        }
    }
}
