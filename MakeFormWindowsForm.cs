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

            AllDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            //接著把form_list一個一個加進來datagridview
            foreach (string[] row in RoomHeight.form_list1)
            {
                AllDataGridView.Rows.Add(row);
            }
            foreach (string[] row in ParapetHeight.form_list2)
            {
                AllDataGridView.Rows.Add(row);
            }
            foreach (string[] row in WuToo.form_list3)
            {
                AllDataGridView.Rows.Add(row);
            }
            foreach (string[] row in WuToo.form_list4)
            {
                AllDataGridView.Rows.Add(row);
            }
            foreach (string[] row in StairsCheck.form_list5)
            {
                AllDataGridView.Rows.Add(row);
            }
            foreach (string[] row in StairsCheck.form_list6)
            {
                AllDataGridView.Rows.Add(row);
            }
            foreach (string[] row in RampSlope.form_list7)
            {
                AllDataGridView.Rows.Add(row);
            }
            foreach (string[] row in FireProofDoor.form_list8)
            {
                AllDataGridView.Rows.Add(row);
            }

            foreach (DataGridViewRow row in AllDataGridView.Rows)
            {
                //避免選到標題或是規範那個row
                if (row.Cells[5].Value != null)
                {
                    if (row.Cells[5].Value.ToString() == "False")
                    {
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.Red;

                        //也把他加進去第二個頁籤(error)
                        string[] s = new string[]{row.Cells[0].Value.ToString(), row.Cells[1].Value.ToString(), row.Cells[2].Value.ToString(), row.Cells[3].Value.ToString(), row.Cells[4].Value.ToString(), row.Cells[5].Value.ToString()};
                        ErrorDataGridView.Rows.Add(s);
                    } 
                }
            }

        }
        private void MakeFormWindowsForm_Load(object sender, EventArgs e)
        {

        }



        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //避免選到標題或是規範那個row
            if (AllDataGridView.SelectedRows[0].Cells[5].Value != null)
                IdTextBox.Text = AllDataGridView.SelectedRows[0].Cells[0].Value.ToString();
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //避免選到標題或是規範那個row
            if (ErrorDataGridView.SelectedRows[0].Cells[5].Value != null)
                IdTextBox.Text = ErrorDataGridView.SelectedRows[0].Cells[0].Value.ToString();
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
