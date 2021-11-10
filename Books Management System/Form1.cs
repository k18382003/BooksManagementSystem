using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Books_Management_System
{
    public partial class frmAuthor : Form
    {
        public frmAuthor()
        {
            InitializeComponent();
        }

        //宣告資料庫變數
        MySqlConnection mySqlConnection;
        MySqlCommand cmd;
        MySqlDataAdapter daAuthors;
        DataTable dtAuthors;
        CurrencyManager authorsManager;

        
        //載入表單
        private void frmAuthor_Load(object sender, EventArgs e)
        {
            //連線字串
            string cnnString = "server=127.0.0.1;uid=sa;pwd=P@ssword;database=mydatabase";
            isEdit(false);

            try
            {
                mySqlConnection = new MySqlConnection();
                mySqlConnection.ConnectionString = cnnString;
                mySqlConnection.Open();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }

            try
            {
                //撈出資料
                cmd = new MySqlCommand("SELECT * FROM Authors Order by Author", mySqlConnection);
                daAuthors = new MySqlDataAdapter();
                dtAuthors = new DataTable();
                daAuthors.SelectCommand = cmd;
                daAuthors.Fill(dtAuthors);

                //將每筆資料綁定在textbox上
                txtAuthorID.DataBindings.Add("Text", dtAuthors, "Au_ID");
                txtAutorName.DataBindings.Add("Text", dtAuthors, "Author");
                txtYearBorn.DataBindings.Add("Text", dtAuthors, "Year_Born");
                //使用CurrencyManager來控制綁定物件
                //可以利用Position屬性, 來控制顯示上/下筆資料
                authorsManager =(CurrencyManager)BindingContext[dtAuthors];
                authorsManager.Position = 0;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            //顯示上一筆
            authorsManager.Position--;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            //顯示下一筆
            authorsManager.Position++;
        }

        private void frmClosing(object sender, FormClosingEventArgs e)
        {
            mySqlConnection.Close();
            mySqlConnection.Dispose();
            cmd.Dispose();
            daAuthors.Dispose();
            dtAuthors.Dispose();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Record saved", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            //確認是否刪除提示
            DialogResult dialogDeleteResult;
            dialogDeleteResult =  MessageBox.Show("Are you sure you want to delete the record?", "Delete", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,MessageBoxDefaultButton.Button2);

            if(dialogDeleteResult == DialogResult.No)
            {
                //如果否, 直接彈出
                return;
            }
            else
            {
                //刪除資料
                return;
            }

        }

        ///<summary>
        ///改變Controler的狀態, 可編輯與不可編輯轉換
        ///</summary>
        private void isEdit(bool flag)
        {
            if (flag)
            {
                txtAuthorID.ReadOnly = true;
                txtAutorName.ReadOnly = false;
                txtYearBorn.ReadOnly = false;
                btnAdd.Enabled = true;
                btnCancel.Enabled = true;
                btnSave.Enabled = true;
                btnPrevious.Enabled = false;
                btnNext.Enabled = false;
                btnEdit.Enabled = false;
                btnDel.Enabled = false;
                btnDone.Enabled = false;
            }
            else
            {
                txtAuthorID.ReadOnly = true;
                txtAutorName.ReadOnly = true;
                txtYearBorn.ReadOnly = true;
                btnAdd.Enabled = false;
                btnCancel.Enabled = false;
                btnSave.Enabled = false;
                btnPrevious.Enabled = true;
                btnNext.Enabled = true;
                btnEdit.Enabled = true;
                btnDel.Enabled = true;
                btnDone.Enabled = true;

            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            isEdit(true);
        }
    }
}
