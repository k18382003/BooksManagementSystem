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
    public partial class frmPublishers : Form
    {
        public frmPublishers()
        {
            InitializeComponent();
        }

        //宣告資料庫變數
        MySqlConnection mySqlConnection;
        MySqlCommand cmd;
        MySqlDataAdapter daPublisher;
        DataTable dtPublisher;
        CurrencyManager PublisherManager;

        bool DatabaseError = false; //DB連線時Error Flag
        StringBuilder sb;
        string fnchange;


        private void frmPublishers_Load(object sender, EventArgs e)
        {
            string sql = "server =127.0.0.1; uid = sa; pwd = P@ssword; database = mydatabase";
            isEdit(false);
            try
            {

                mySqlConnection = new MySqlConnection();
                mySqlConnection.ConnectionString = sql;
                mySqlConnection.Open();
                cmd = new MySqlCommand();
                daPublisher = new MySqlDataAdapter();
                dtPublisher = new DataTable();

                try
                {
                    cmd.CommandText = "SELECT * FROM publishers order by PubID";
                    cmd.Connection = mySqlConnection;
                    daPublisher.SelectCommand = cmd;
                    daPublisher.Fill(dtPublisher);

                    txtPubID.DataBindings.Add("Text", dtPublisher, "PubID");
                    txtName.DataBindings.Add("Text", dtPublisher, "Name");
                    txtComName.DataBindings.Add("Text", dtPublisher, "Company_Name");
                    txtAddress.DataBindings.Add("Text", dtPublisher, "Address");
                    txtCity.DataBindings.Add("Text", dtPublisher, "City");
                    txtState.DataBindings.Add("Text", dtPublisher, "State");
                    txtZip.DataBindings.Add("Text", dtPublisher, "Zip");
                    txtTel.DataBindings.Add("Text", dtPublisher, "Telephone");
                    txtFax.DataBindings.Add("Text", dtPublisher, "Fax");
                    txtCmt.DataBindings.Add("Text", dtPublisher, "Comments");

                    PublisherManager = (CurrencyManager)BindingContext[dtPublisher];
                    PublisherManager.Position = 0;
                }
                catch(Exception ex)
                {

                    MessageBox.Show(ex.Message, "Load Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }


            }
            catch(Exception ex)
            {
                DatabaseError = true;
                MessageBox.Show(ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormClosingEvent(object sender, FormClosingEventArgs e)
        {
            mySqlConnection.Close();
            mySqlConnection.Dispose();
            cmd.Dispose();
            daPublisher.Dispose();
            dtPublisher.Dispose();
        }

        private void btnPre_Click(object sender, EventArgs e)
        {
            PublisherManager.Position--;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            PublisherManager.Position++;
        }

        private void isEdit(bool flag)
        {
            if (flag)
            {
                txtPubID.ReadOnly = true;
                txtName.ReadOnly = false;
                txtComName.ReadOnly = false;
                txtAddress.ReadOnly = false;
                txtCity.ReadOnly = false;
                txtState.ReadOnly = false;
                txtZip.ReadOnly = false;
                txtTel.ReadOnly = false;
                txtFax.ReadOnly = false;
                txtCmt.ReadOnly = false;
                btnAdd.Enabled = false;
                btnCancel.Enabled = true;
                btnSave.Enabled = true;
                btnPre.Enabled = false;
                btnNext.Enabled = false;
                btnEdit.Enabled = false;
                btnDel.Enabled = false;
                btnDone.Enabled = false;
            }
            else
            {
                txtPubID.ReadOnly = true;
                txtComName.ReadOnly = true;
                txtName.ReadOnly = true;
                txtAddress.ReadOnly = true;
                txtCity.ReadOnly = true;
                txtState.ReadOnly = true;
                txtZip.ReadOnly = true;
                txtTel.ReadOnly = true;
                txtFax.ReadOnly = true;
                txtCmt.ReadOnly = true;
                btnAdd.Enabled = true;
                btnCancel.Enabled = false;
                btnSave.Enabled = false;
                btnPre.Enabled = true;
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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            isEdit(true);
            PublisherManager.AddNew();
            txtPubID.Text = "Auto";
            fnchange = "Add";

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
                        PublisherManager.EndCurrentEdit();
                        break;
                    case "Add":
                        Add();
                        break;
                }
                PublisherManager.Refresh();
                isEdit(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Data Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void Edit()
        {
            try
            {
                sb = new StringBuilder();
                sb.Append("Update publishers set Name=@Name, Company_Name=@ComName, Address=@Address, ");
                sb.Append("City=@City, State=@State, Zip=@Zip, Telephone=@Tel, Fax=@Fax, Comments=@cmts ");
                sb.Append("Where PubID=@pubid");

                cmd = new MySqlCommand(Convert.ToString(sb), mySqlConnection);

                cmd.Parameters.AddWithValue("@Name", txtName.Text);
                cmd.Parameters.AddWithValue("@ComName", txtComName.Text);
                cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                cmd.Parameters.AddWithValue("@City", txtCity.Text);
                cmd.Parameters.AddWithValue("@State", txtState.Text);
                cmd.Parameters.AddWithValue("@Zip", txtZip.Text);
                cmd.Parameters.AddWithValue("@Tel", txtTel.Text);
                cmd.Parameters.AddWithValue("@Fax", txtFax.Text);
                cmd.Parameters.AddWithValue("@cmts", txtCmt.Text);
                cmd.Parameters.AddWithValue("@PubID", Convert.ToInt32(txtPubID.Text));

                cmd.ExecuteNonQuery();
                sb.Clear();
                cmd.Parameters.Clear();

                MessageBox.Show("You have changed the record successfully.", "Edit", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                sb.Append("Insert into publishers (Name, Company_Name ,Address, City, State, Zip, Telephone, Fax, Comments) ");
                sb.Append("Values(@Name, @ComName , @Address, @City, @State, @Zip, @Tel, @Fax, @cmts) ");

                cmd = new MySqlCommand(Convert.ToString(sb), mySqlConnection);

                cmd.Parameters.AddWithValue("@Name", txtName.Text);
                cmd.Parameters.AddWithValue("@ComName", txtComName.Text);
                cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                cmd.Parameters.AddWithValue("@City", txtCity.Text);
                cmd.Parameters.AddWithValue("@State", txtState.Text);
                cmd.Parameters.AddWithValue("@Zip", txtZip.Text);
                cmd.Parameters.AddWithValue("@Tel", txtTel.Text);
                cmd.Parameters.AddWithValue("@Fax", txtFax.Text);
                cmd.Parameters.AddWithValue("@cmts", txtCmt.Text);

                string a = cmd.CommandText;

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


        private void btnCancel_Click(object sender, EventArgs e)
        {
            isEdit(false);
            if (string.IsNullOrWhiteSpace(txtName.Text))
                PublisherManager.RemoveAt(PublisherManager.Position);
            PublisherManager.Refresh();
        }

        private bool Valid_Input()
        {
            bool validinput = true;

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Publisher Name is required", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                txtName.Focus();
                validinput = false;
            }

            return validinput;

        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult;
            dialogResult = MessageBox.Show("Are you sure you want to delete the record?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if(dialogResult == DialogResult.No)
            {
                return;
            }
            else
            {
                try
                {
                    PublisherManager.RemoveAt(PublisherManager.Position);
                    sb = new StringBuilder();
                    sb.Append("Delete from publishers where PubID = @PubID ");
                    cmd = new MySqlCommand(Convert.ToString(sb), mySqlConnection);
                    cmd.Parameters.AddWithValue("@PubID", Convert.ToInt32(txtPubID.Text));
                    cmd.ExecuteNonQuery();
                    sb.Clear();
                    cmd.Parameters.Clear();

                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Data Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                                 
            }
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                sb = new StringBuilder();
                sb.Append("SELECT * FROM publishers where Company_Name like @search or PubID like @search ");
                cmd = new MySqlCommand(Convert.ToString(sb), mySqlConnection);
                cmd.Parameters.AddWithValue("@search", "%" + txtsearch.Text + "%");

                MySqlDataReader drTemp = cmd.ExecuteReader();
                if (drTemp.Read())
                {
                    dtPublisher.DefaultView.Sort = "PubID";
                    PublisherManager.Position = dtPublisher.DefaultView.Find(Convert.ToString(drTemp["PubID"]));
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
