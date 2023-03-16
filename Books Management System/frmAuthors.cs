using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Drawing.Printing;
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

        bool DatabaseError = false; //DB連線時Error Flag
        StringBuilder sb = new StringBuilder();
        string fnchange;


        //載入表單
        private void frmAuthor_Load(object sender, EventArgs e)
        {
            //連線字串
            string cnnString = "server=127.0.0.1;uid=sa;pwd=P@ssword;database=mydatabase";
            isEdit(false);

            //建立連線, 及相關參數
            try
            {
                mySqlConnection = new MySqlConnection();
                mySqlConnection.ConnectionString = cnnString;
                mySqlConnection.Open();
                cmd = new MySqlCommand();
                daAuthors = new MySqlDataAdapter();
                dtAuthors = new DataTable();

                try
                {
                    //撈出資料
                    cmd.CommandText = "SELECT * FROM Authors Order by Au_ID";
                    cmd.Connection = mySqlConnection;
                    daAuthors.SelectCommand = cmd;
                    daAuthors.Fill(dtAuthors);

                    //將每筆資料綁定在textbox上
                    txtAuthorID.DataBindings.Add("Text", dtAuthors, "Au_ID");
                    txtAutorName.DataBindings.Add("Text", dtAuthors, "Author");
                    txtYearBorn.DataBindings.Add("Text", dtAuthors, "Year_Born");
                    //使用CurrencyManager來控制綁定物件
                    //可以利用Position屬性, 來控制顯示上/下筆資料
                    authorsManager = (CurrencyManager)BindingContext[dtAuthors];
                    authorsManager.Position = 0;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            catch (MySqlException ex)
            {
                DatabaseError = true;
                MessageBox.Show(ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            //非DB連線錯誤才需要去關連線跟相關參數
            if (!DatabaseError)
            {
                mySqlConnection.Close();
                mySqlConnection.Dispose();
                cmd.Dispose();
                daAuthors.Dispose();
                dtAuthors.Dispose();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            bool isValid = Valid_Input();
            if (!isValid)
                return;
            try
            {
                switch (fnchange)
                {
                    case "Edit":
                        Edit();
                        break;
                    case "Add":
                        Add();
                        break;
                }
                authorsManager.Refresh();
                isEdit(false);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Data Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                try
                {
                    //刪除資料
                    authorsManager.RemoveAt(authorsManager.Position);
                    sb.Append("Delete from authors where Au_ID = @AuthorID");
                    cmd = new MySqlCommand(Convert.ToString(sb), mySqlConnection);
                    cmd.Parameters.AddWithValue("@AuthorID",txtAuthorID.Text);
                    cmd.ExecuteNonQuery();
                    sb.Clear();
                    cmd.Parameters.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Delete Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
                btnAdd.Enabled = false;
                btnCancel.Enabled = true;
                btnSave.Enabled = true;
                btnPrevious.Enabled = false;
                btnNext.Enabled = false;
                btnEdit.Enabled = false;
                btnDel.Enabled = false;
                btnDone.Enabled = false;
                txtAutorName.TabStop = true;
                txtYearBorn.TabStop = true;
            }
            else
            {
                txtAutorName.TabStop = false;
                txtYearBorn.TabStop = false;
                txtAuthorID.ReadOnly = true;
                txtAutorName.ReadOnly = true;
                txtYearBorn.ReadOnly = true;
                btnAdd.Enabled = true;
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
            fnchange = "Edit";
        }

        private void YearBorn_KeyPress(object sender, KeyPressEventArgs e)
        {
            if((e.KeyChar >= '0' && e.KeyChar <= '9') || e.KeyChar == 8)
            {
                e.Handled = false;
                lblYearBornValidation.Visible = false;
            }
            else if(e.KeyChar == 13)
            {
                btnSave.Focus();
            }
            else
            {
                e.Handled = true;
                lblYearBornValidation.Visible = true;
            }
        }

        private bool Valid_Input()
        {
            string Msg = "";
            long YearBorn, CurrentYear;
            bool validValue = true;
            if(String.IsNullOrWhiteSpace(txtAutorName.Text))
            {
                Msg = "Author Name is required. \n";
                validValue = false;
            }
            if (!String.IsNullOrWhiteSpace(txtYearBorn.Text))
            {
                YearBorn = Convert.ToInt64(txtYearBorn.Text);
                CurrentYear = DateTime.Now.Year;
                if((CurrentYear - YearBorn) <= 0)
                {
                    Msg += "The Year Born value is not valid";
                    validValue = false;
                }
            }

            if (!validValue)
                MessageBox.Show(Msg, "Input Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            return validValue;
        }

        private void AuthorName_keyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
                txtYearBorn.Focus();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            isEdit(true);
            authorsManager.AddNew();
            fnchange = "Add";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            isEdit(false);
            if (string.IsNullOrWhiteSpace(txtAutorName.Text))
                authorsManager.RemoveAt(authorsManager.Position);
            authorsManager.Refresh();            
        }

        private void Edit()
        {
            try
            {
                sb.Append("Update Authors set Author=@Author, Year_Born=@Yearborn Where Au_ID=@AuID");
                cmd = new MySqlCommand(Convert.ToString(sb), mySqlConnection);
                cmd.Parameters.AddWithValue("@Author", txtAutorName.Text);
                cmd.Parameters.AddWithValue("@Yearborn", txtYearBorn.Text);
                cmd.Parameters.AddWithValue("@AuID", txtAuthorID.Text);

                cmd.ExecuteNonQuery();

                MessageBox.Show("You have changed the record sucessfully.", "Data Edit", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Data Edit Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Add()
        {
            try
            {
                sb = new StringBuilder();
                sb.Append("Insert into Authors (Author, Year_Born) Values(@Author, @Year_Born) ");

                cmd = new MySqlCommand(Convert.ToString(sb), mySqlConnection);

                cmd.Parameters.AddWithValue("@Author", txtAutorName.Text);
                cmd.Parameters.AddWithValue("@Year_Born", txtYearBorn.Text);

                cmd.ExecuteNonQuery();
                sb.Clear();
                cmd.Parameters.Clear();

                MessageBox.Show("You have added the record successfully.", "Add", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Data Add Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            authorsManager.Position = 0;
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            authorsManager.Position = authorsManager.Count - 1;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                sb = new StringBuilder();
                sb.Append("SELECT * FROM Authors where Au_ID like @search or Year_Born like @search or Author like @search");
                cmd = new MySqlCommand(Convert.ToString(sb), mySqlConnection);
                cmd.Parameters.AddWithValue("@search", "%"+txtSearch.Text+"%");

                MySqlDataReader drTemp = cmd.ExecuteReader();
                if(drTemp.Read())
                {
                    dtAuthors.DefaultView.Sort = "Author";
                    authorsManager.Position = dtAuthors.DefaultView.Find(Convert.ToString(drTemp["Author"]));
                }
                else
                {
                    lblSearch.Visible = true;
                }

                drTemp.Close();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SearchBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            lblSearch.Visible = false;
        }
    }
}
