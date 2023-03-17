using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Books_Management_System
{
    public partial class frmBooksManagement : Form
    {
        public frmBooksManagement()
        {
            InitializeComponent();
            
        }

        //宣告資料庫變數
        MySqlConnection mySqlConnection;
        MySqlCommand cmd;
        MySqlDataAdapter da;
        DataTable dtBooksManagement;
        CurrencyManager BooksManagementManager;
        DataTable dtauthors;
        ComboBox[] authorsCombo;
        DataTable dtISBNAuthors;
        DataTable dtPublisher;
        DataTable dtISBN;
        MySqlTransaction transaction;

        bool DatabaseError = false; //DB連線時Error Flag
        StringBuilder sb = new StringBuilder();
        string fnchange;
        int CurrentPostiion = 0;
        List<string> SqlCollection = new List<string>();
        List<Dictionary<string, object>> ListParams = new List<Dictionary<string, object>>();
        Dictionary<string, object> _params = new Dictionary<string, object>();

        private void frmBooksManagement_Load(object sender, EventArgs e)
        {
            isEdit(false);
            BooksManagementLoad();
        }

        private void BooksManagementLoad()
        {
            string cnnString = "server=127.0.0.1;uid=sa;pwd=P@ssword;database=mydatabase";
            //建立連線, 及相關參數
            try
            {
                mySqlConnection = new MySqlConnection();
                mySqlConnection.ConnectionString = cnnString;
                mySqlConnection.Open();
                dtBooksManagement = new DataTable();
                dtauthors = new DataTable();
                authorsCombo = new ComboBox[4];
                dtPublisher = new DataTable();

                try
                {
                    txtTitle.DataBindings.Clear();
                    txtYearPub.DataBindings.Clear();
                    msktxtISBN.DataBindings.Clear();
                    txtDescription.DataBindings.Clear();
                    txtNotes.DataBindings.Clear();
                    txtSubjects.DataBindings.Clear();
                    txtCmt.DataBindings.Clear();

                    //撈出資料
                    cmd = new MySqlCommand("SELECT * FROM Titles order by Title", mySqlConnection);
                    da = new MySqlDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(dtBooksManagement);

                    //將每筆資料綁定在textbox上
                    txtTitle.DataBindings.Add("Text", dtBooksManagement, "Title");
                    txtYearPub.DataBindings.Add("Text", dtBooksManagement, "Year_Published");
                    msktxtISBN.DataBindings.Add("Text", dtBooksManagement, "ISBN");
                    txtDescription.DataBindings.Add("Text", dtBooksManagement, "Description");
                    txtNotes.DataBindings.Add("Text", dtBooksManagement, "Notes");
                    txtSubjects.DataBindings.Add("Text", dtBooksManagement, "Subject");
                    txtCmt.DataBindings.Add("Text", dtBooksManagement, "Commets");

                    //使用CurrencyManager來控制綁定物件
                    //可以利用Position屬性, 來控制顯示上/下筆資料
                    BooksManagementManager = (CurrencyManager)BindingContext[dtBooksManagement];
                    BooksManagementManager.Position = 0;

                    GetAuthor_ISBN();
                    GetPublisher_PUBID();

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

        private void btnPre_Click(object sender, EventArgs e)
        {
            BooksManagementManager.Position--;
            GetAuthor_ISBN();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            BooksManagementManager.Position++;
            GetAuthor_ISBN();
        }

        private void btnfisrt_Click(object sender, EventArgs e)
        {
            BooksManagementManager.Position = 0;
            GetAuthor_ISBN();
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            BooksManagementManager.Position = BooksManagementManager.Count - 1;
            GetAuthor_ISBN();
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void frmClosing(object sender, FormClosingEventArgs e)
        {
            //非DB連線錯誤才需要去關連線跟相關參數
            if (!DatabaseError)
            {
                mySqlConnection.Close();
                mySqlConnection.Dispose();
                cmd.Dispose();
                da.Dispose();
                dtBooksManagement.Dispose();
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                sb = new StringBuilder();
                sb.Append("SELECT * FROM Titles where Title like @search or Year_Published like @search or ISBN like @search ");
                cmd = new MySqlCommand(Convert.ToString(sb), mySqlConnection);
                cmd.Parameters.AddWithValue("@search", "%" + txtSearch.Text + "%");

                MySqlDataReader drTemp = cmd.ExecuteReader();
                if (drTemp.Read())
                {
                    dtBooksManagement.DefaultView.Sort = "Title";
                    BooksManagementManager.Position = dtBooksManagement.DefaultView.Find(Convert.ToString(drTemp["Title"]));
                }
                else
                {
                    lblSearch.Visible = true;
                }

                drTemp.Close();
                GetAuthor_ISBN();
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

        private void isEdit(bool flag)
        {
            if (flag)
            {
                txtTitle.ReadOnly = false;
                txtYearPub.ReadOnly = false;
                if (fnchange == "Add")
                {
                    msktxtISBN.ReadOnly = false;
                }
                else
                {
                    msktxtISBN.ReadOnly = true;
                }
                txtDescription.ReadOnly = false;
                txtNotes.ReadOnly = false;
                txtSubjects.ReadOnly = false;
                txtCmt.ReadOnly = false;
                btnSave.Enabled = true;
                cbxAuth1.Enabled = true;
                cbxAuth2.Enabled = true;
                cbxAuth3.Enabled = true;
                cbxAuth4.Enabled = true;
                btnPre.Enabled = false;
                btnNext.Enabled = false;
                btnEdit.Enabled = false;
                btnDone.Enabled = false;
                btnCancel.Enabled = true;
                btnfisrt.Enabled = false;
                btnLast.Enabled = false;
                btnAdd.Enabled = false;
                btnDelete.Enabled = false;
                btnAu1.Enabled = true;
                btnAu2.Enabled = true;
                btnAu3.Enabled = true;
                btnAu4.Enabled = true;
                cbxPub.Enabled = true;
                btnSearch.Enabled = false;
                btnPublishers.Enabled = false;
                btnAuthors.Enabled = false;
            }
            else
            {
                txtTitle.ReadOnly = true;
                txtYearPub.ReadOnly = true;
                msktxtISBN.ReadOnly = true;
                txtDescription.ReadOnly = true;
                txtNotes.ReadOnly = true;
                txtSubjects.ReadOnly = true;
                txtCmt.ReadOnly = true;
                btnCancel.Enabled = false;
                btnSave.Enabled = false;
                btnPre.Enabled = true;
                btnNext.Enabled = true;
                btnEdit.Enabled = true;
                btnDone.Enabled = true;
                btnfisrt.Enabled = true;
                btnLast.Enabled = true;
                btnAdd.Enabled = true;
                btnDelete.Enabled = true;
                cbxAuth1.Enabled = false;
                cbxAuth2.Enabled = false;
                cbxAuth3.Enabled = false;
                cbxAuth4.Enabled = false;
                btnAu1.Enabled = false;
                btnAu2.Enabled = false;
                btnAu3.Enabled = false;
                btnAu4.Enabled = false;
                cbxPub.Enabled = false;
                btnSearch.Enabled = true;
                btnPublishers.Enabled = true;
                btnAuthors.Enabled = true;
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
                //BooksManagementManager.Refresh();
                isEdit(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Data Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            fnchange = "Edit";
            isEdit(true);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            isEdit(false);
            BooksManagementManager.CancelCurrentEdit();
            if (fnchange == "Add")
                BooksManagementManager.Position = CurrentPostiion;
        }

        private void btnAuthors_Click(object sender, EventArgs e)
        {
            frmAuthor frmAuthor = new frmAuthor();
            frmAuthor.ShowDialog();
            BooksManagementLoad();
        }

        private void btnPublishers_Click(object sender, EventArgs e)
        {
            frmPublishers frmPublishers = new frmPublishers();
            frmPublishers.ShowDialog();
            BooksManagementLoad();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            fnchange = "Add";
            cbxAuth1.Text = "";
            cbxAuth2.Text = "";
            cbxAuth3.Text = "";
            cbxAuth4.Text = "";
            CurrentPostiion = BooksManagementManager.Position;
            BooksManagementManager.AddNew();
            isEdit(true);
        }
        
        private void btnDelete_Click(object sender, EventArgs e)
        {
            //確認是否刪除提示
            DialogResult dialogDeleteResult;
            dialogDeleteResult = MessageBox.Show("Are you sure you want to delete the record?", "Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (dialogDeleteResult == DialogResult.No)
            {
                //如果否, 直接彈出
                return;
            }
            else
            {
                try
                {
                    // 需先刪除title_author
                    sb.Clear();
                    sb.Append("Delete from title_author where ISBN = @ISBN");
                    cmd = new MySqlCommand(Convert.ToString(sb), mySqlConnection);
                    cmd.Parameters.AddWithValue("@ISBN", msktxtISBN.Text);
                    cmd.ExecuteNonQuery();

                    //刪除titles資料
                    sb.Clear();
                    sb.Append("Delete from titles where ISBN = @ISBN");
                    cmd = new MySqlCommand(Convert.ToString(sb), mySqlConnection);
                    cmd.Parameters.AddWithValue("@ISBN", msktxtISBN.Text);
                    cmd.ExecuteNonQuery();
                    BooksManagementManager.RemoveAt(BooksManagementManager.Position);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Delete Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

        private void Edit()
        {
            try
            {
                cmd = new MySqlCommand();
                transaction = mySqlConnection.BeginTransaction();
                cmd.Connection = mySqlConnection;
                cmd.Transaction = transaction;
                sb = new StringBuilder();

                sb.Append("Update Titles set Title=@Title, Year_Published=@Year_Published, Description=@Description, ");
                sb.Append("Notes=@Notes, Subject=@Subject, Commets=@Comments, PubID=@PubID where ISBN=@ISBN");
                cmd.CommandText = sb.ToString();
                cmd.Parameters.AddWithValue("@Title", txtTitle.Text);
                cmd.Parameters.AddWithValue("@Year_Published", txtYearPub.Text);
                cmd.Parameters.AddWithValue("@Description", txtDescription.Text);
                cmd.Parameters.AddWithValue("@Notes", txtNotes.Text);
                cmd.Parameters.AddWithValue("@Subject", txtSubjects.Text);
                cmd.Parameters.AddWithValue("@Comments", txtCmt.Text);
                cmd.Parameters.AddWithValue("@ISBN", msktxtISBN.Text);
                cmd.Parameters.AddWithValue("@PubID", cbxPub.SelectedValue);

                cmd.ExecuteNonQuery();

                sb.Clear();
                sb.Append("DELETE FROM title_author where ISBN = @ISBN_DELETE");
                cmd.CommandText = sb.ToString();
                cmd.Parameters.AddWithValue("@ISBN_DELETE", msktxtISBN.Text);

                cmd.ExecuteNonQuery();

                transaction.Commit();
                MessageBox.Show("You have changed the record sucessfully.", "Data Edit", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                MessageBox.Show(ex.Message, "Data Edit Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Add()
        {
            try
            {
                transaction = mySqlConnection.BeginTransaction();
                cmd.Connection = mySqlConnection;
                cmd.Transaction = transaction;

                sb = new StringBuilder();
                sb.Append("Insert into Titles (Title, Year_Published, ISBN, Description, Notes, Subject, Commets ) ");
                sb.Append(" Values(@Title, @Year_Published, @ISBN, @Description, @Notes, @Subject, @Comments) ");
                cmd = new MySqlCommand(Convert.ToString(sb), mySqlConnection);
                cmd.Parameters.AddWithValue("@Title", txtTitle.Text);
                cmd.Parameters.AddWithValue("@Year_Published", txtYearPub.Text);
                cmd.Parameters.AddWithValue("@ISBN", msktxtISBN.Text);
                cmd.Parameters.AddWithValue("@Description", txtDescription.Text);
                cmd.Parameters.AddWithValue("@Notes", txtNotes.Text);
                cmd.Parameters.AddWithValue("@Subject", txtSubjects.Text);
                cmd.Parameters.AddWithValue("@Comments", txtCmt.Text);

                cmd.ExecuteNonQuery();

                Insert_title_author();

                transaction.Commit();
        
                MessageBox.Show("You have added the record successfully.", "Add", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                MessageBox.Show(ex.Message, "Data Add Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool Valid_Input()
        {
            string Msg = "";
            long YearBorn, CurrentYear;
            bool validValue = true;
            if (String.IsNullOrWhiteSpace(msktxtISBN.Text)
                || String.IsNullOrWhiteSpace(txtTitle.Text))
            {
                Msg = "ISBN and Title are required. \n";
                validValue = false;
            }

            if(msktxtISBN.Text.Length != 13)
            {
                Msg += "ISBN must be 13 charactors. \n";
            }

            if (!String.IsNullOrWhiteSpace(txtYearPub.Text))
            {
                YearBorn = Convert.ToInt64(txtYearPub.Text);
                CurrentYear = DateTime.Now.Year;
                if ((CurrentYear - YearBorn) <= 0)
                {
                    Msg += "The Year Born value is not valid";
                    validValue = false;
                }
            }

            if (!validValue)
                MessageBox.Show(Msg, "Input Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            return validValue;
        }

        private void TextValid(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= '0' && e.KeyChar <= '9') || e.KeyChar == 8 )
            {
                e.Handled = false;
            }
            else if (e.KeyChar == 13)
            {
                btnSave.Focus();
            }
            else
            {
                e.Handled = true;
            }
        }

        private void GetAuthor_ISBN()
        {
            dtauthors.Clear();
            authorsCombo[0] = cbxAuth1;
            authorsCombo[1] = cbxAuth2;
            authorsCombo[2] = cbxAuth3;
            authorsCombo[3] = cbxAuth4;

            dtISBNAuthors = AUIDforISBN();
            int rowcount = dtISBNAuthors.Rows.Count - 1;

            cmd = new MySqlCommand("SELECT Author, AU_ID FROM authors order by Author", mySqlConnection);
            da = new MySqlDataAdapter();
            da.SelectCommand = cmd;
            da.Fill(dtauthors);

            for (int i = 0; i < 4; i++)
            {
                DataTable dtTemp = new DataTable();
                dtTemp = dtauthors.Copy();
                authorsCombo[i].DataSource = dtTemp;
                authorsCombo[i].DisplayMember = "Author";
                authorsCombo[i].ValueMember = "AU_ID";

                if (rowcount >= 0)
                {
                    authorsCombo[i].SelectedValue = dtISBNAuthors.Rows[rowcount]["AU_ID"].ToString();
                    rowcount--;
                }
                else
                {
                    authorsCombo[i].SelectedIndex = -1;
                }
            }
        }

        private DataTable AUIDforISBN()
        {
            dtISBN = new DataTable();

            cmd = new MySqlCommand("SELECT * FROM title_author where ISBN = @ISBN", mySqlConnection);
            da = new MySqlDataAdapter();
            da.SelectCommand = cmd;
            cmd.Parameters.AddWithValue("@ISBN", msktxtISBN.Text);
            da.Fill(dtISBN);

            return dtISBN;
        }

        private void AuthorX_Click(object sender, EventArgs e)
        {
            Button btnX = (Button)sender;
            switch (btnX.Name)
            {
                case "btnAu1":
                    cbxAuth1.SelectedIndex = -1;
                    break;
                case "btnAu2":
                    cbxAuth2.SelectedIndex = -1;
                    break;
                case "btnAu3":
                    cbxAuth3.SelectedIndex = -1;
                    break;
                case "btnAu4":
                    cbxAuth4.SelectedIndex = -1;
                    break;

            }
        }

        private void GetPublisher_PUBID()
        {
            dtPublisher.Clear();
            cbxPub.DataBindings.Clear();
            cmd = new MySqlCommand("SELECT Name, PubID FROM publishers order by Name", mySqlConnection);
            da = new MySqlDataAdapter();
            da.SelectCommand = cmd;
            da.Fill(dtPublisher);
            cbxPub.DataSource = dtPublisher;
            cbxPub.DisplayMember = "Name";
            cbxPub.ValueMember = "PubID";
            cbxPub.DataBindings.Add("SelectedValue", dtBooksManagement, "PubID");
            
        }

        private DataTable PublisherFromPUID()
        {
            DataTable dtISBN = new DataTable();

            cmd = new MySqlCommand("SELECT PubID FROM titles where ISBN = @ISBN", mySqlConnection);
            da = new MySqlDataAdapter();
            da.SelectCommand = cmd;
            cmd.Parameters.AddWithValue("@ISBN", msktxtISBN.Text);
            da.Fill(dtISBN);

            return dtISBN;
        }

        private void Insert_title_author()
        {
            for (int i = 0; i < 4; i++)
            {
                if (authorsCombo[i].SelectedIndex != -1)
                {
                    sb.Clear();
                    sb.Append("Insert into title_author (ISBN, AU_ID) Values(@ISBN"+i+", @AU_ID"+i+")");
                    cmd.CommandText = Convert.ToString(sb);
                    cmd.Parameters.AddWithValue("@ISBN" + i, msktxtISBN.Text);
                    cmd.Parameters.AddWithValue("@AU_ID" + i, authorsCombo[i].SelectedValue);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //待Debug
        public void BatchExecuteNonQuery(List<string> sqlcollection, List<Dictionary<string, object>> _params)
        {
            for(int i = 0; i < sqlcollection.Count - 1; i++)
            {
                cmd.CommandText = sqlcollection[i];
                foreach(KeyValuePair<string, object> keyValuePair in _params[i])
                {
                    cmd.Parameters.AddWithValue(Convert.ToString(keyValuePair.Key), keyValuePair.Value);
                }
                cmd.ExecuteNonQuery();
            }
        }
    }
}
