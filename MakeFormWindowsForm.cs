using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Structure;

namespace RegulationCheck
{
    public partial class MakeFormWindowsForm : System.Windows.Forms.Form
    {
        private MakeFormStorage m_creator;
        private UIDocument UIdoc;
        private Document doc;

        public MakeFormWindowsForm(MakeFormStorage creator)
        {
            m_creator = creator;
            UIdoc = m_creator.RevitDoc;
            doc = UIdoc.Document;

            ExternalCommandData commandData = null;

            InitializeComponent();

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            //接著把form_list一個一個加進來datagridview
            foreach (string[] row in RoomHeight.form_list1)
            {
                dataGridView1.Rows.Add(row);
            }
            foreach (string[] row in ParapetHeight.form_list2)
            {
                dataGridView1.Rows.Add(row);
            }
            foreach (string[] row in WuToo.form_list3)
            {
                dataGridView1.Rows.Add(row);
            }
            foreach (string[] row in WuToo.form_list4)
            {
                dataGridView1.Rows.Add(row);
            }
            foreach (string[] row in StairsCheck.form_list5)
            {
                dataGridView1.Rows.Add(row);
            }
            foreach (string[] row in StairsCheck.form_list6)
            {
                dataGridView1.Rows.Add(row);
            }
            foreach (string[] row in RampSlope.form_list7)
            {
                dataGridView1.Rows.Add(row);
            }
            foreach (string[] row in FireProofDoor.form_list8)
            {
                dataGridView1.Rows.Add(row);
            }

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                //避免選到標題或是規範那個row
                if (row.Cells[5].Value != null)
                {
                    if (row.Cells[5].Value.ToString() == "False")
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.Red;
                }
            }

            










        }
        private void MakeFormWindowsForm_Load(object sender, EventArgs e)
        {

        }



        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //避免選到標題或是規範那個row
            if (dataGridView1.SelectedRows[0].Cells[5].Value != null)
                IdTextBox.Text = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FindButton_Click(object sender, EventArgs e)
        {
            if (IdTextBox.Text == "")
            {
                MessageBox.Show("請選擇ID");
                return;
            }
                

            //依據id，把那個東西highlight起來，然後看能不能切換到那個視圖
            ElementId selectedId = new ElementId(Convert.ToInt32(IdTextBox.Text));
            ICollection<ElementId> ids = new List<ElementId>();
            ids.Add(selectedId);
            UIdoc.Selection.SetElementIds(ids);
            UIdoc.ShowElements(ids);

 
        }
    }
}
