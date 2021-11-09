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

        MySqlConnection mySqlConnection;
        MySqlCommand cmd;
        MySqlDataAdapter daAuthors;
        DataTable dtAuthors;
        CurrencyManager authorsManager;
        
        private void frmAuthor_Load(object sender, EventArgs e)
        {
            string cnnString = "server=127.0.0.1;uid=sa;pwd=P@ssword;database=mydatabase";

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
                cmd = new MySqlCommand("SELECT * FROM Authors Order by Author", mySqlConnection);
                daAuthors = new MySqlDataAdapter();
                dtAuthors = new DataTable();
                daAuthors.SelectCommand = cmd;
                daAuthors.Fill(dtAuthors);

                txtAuthorID.DataBindings.Add("Text", dtAuthors, "Au_ID");
                txtAutorName.DataBindings.Add("Text", dtAuthors, "Author");
                txtYearBorn.DataBindings.Add("Text", dtAuthors, "Year_Born");
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
            authorsManager.Position--;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
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
    }
}
