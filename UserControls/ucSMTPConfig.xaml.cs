using System;
using System.Data;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;


namespace beeEmailing
{
    /// <summary>
    /// Interaction logic for ucSMTPConfig.xaml
    /// </summary>
    public partial class ucSMTPConfig : UserControl
    {

        RadioButton selectedEncryptrb;
        RadioButton selectedAuthrb;
        DataTable dtSmtpServers = new DataTable();

        public ucSMTPConfig()
        {
            InitializeComponent();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dtSmtpServers = ReadSMTPTemplate();

            DataSet dsConfig = new DataSet();
            string filename = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Configuration\\AppConfig.xml";
            dsConfig.ReadXml(filename);

            DataTable smptconfig = dsConfig.Tables["smtpconfig"];

            txtFromEmail.Text = smptconfig.Rows[0]["emailfrom"].ToString();
            txtFromTitle.Text = smptconfig.Rows[0]["emailtitle"].ToString();
            txtSmtpHost.Text = smptconfig.Rows[0]["smtphost"].ToString();
            txtSmtpPort.Text = smptconfig.Rows[0]["smtpport"].ToString();
            txtUserName.Text = smptconfig.Rows[0]["smtpusername"].ToString();
            txtPassword.Password = smptconfig.Rows[0]["smtppassword"].ToString();
            string encryption = smptconfig.Rows[0]["smtpencryption"].ToString();


            if (encryption.Equals("Ssl", StringComparison.OrdinalIgnoreCase))
            {
                rbSsl.IsChecked = true;
                selectedEncryptrb = rbSsl;
            }
            else
            {
                rbNone.IsChecked = true;
                selectedEncryptrb = rbNone;
            }
            if (smptconfig.Rows[0]["smtpauth"].ToString().Equals("DefaultAuth", StringComparison.OrdinalIgnoreCase))
            {
                DefaultAuth.IsChecked = true;
                selectedAuthrb = DefaultAuth;
            }
            else
            {
                UsernameAuth.IsChecked = true;
                selectedAuthrb = UsernameAuth;
                txtUserName.IsEnabled = true;
                txtPassword.IsEnabled = true;
            }

            rbSsl.Checked += rbEncrypt_CheckedChanged;
            rbNone.Checked += rbEncrypt_CheckedChanged;

            DefaultAuth.Checked += rbAuth_CheckedChanged;
            UsernameAuth.Checked += rbAuth_CheckedChanged;
        }
        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataSet dsConfig = new DataSet();
                string filename = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Configuration\\AppConfig.xml";
                dsConfig.ReadXml(filename);


                if (string.IsNullOrEmpty(txtFromEmail.Text) && string.IsNullOrEmpty(txtFromTitle.Text) && string.IsNullOrEmpty(txtSmtpHost.Text) && string.IsNullOrEmpty(txtSmtpPort.Text))
                {
                    MessageBox.Show("All field is mandatory to fill", "Validation");
                    return;
                }
                if (UsernameAuth.IsChecked == true)
                {
                    if (string.IsNullOrEmpty(txtUserName.Text) && string.IsNullOrEmpty(txtPassword.Password))
                    {
                        MessageBox.Show("Please enter user name and password", "Validation");
                        return;
                    }
                }

                int port = 0;
                if (Int32.TryParse(txtSmtpPort.Text, out port) == false)
                {
                    MessageBox.Show("Please enter SMTP Port in numeric", "Validation");
                    return;
                }

                dsConfig.Tables["smtpconfig"].Rows[0]["emailfrom"] = txtFromEmail.Text;
                dsConfig.Tables["smtpconfig"].Rows[0]["emailtitle"] = txtFromTitle.Text;
                dsConfig.Tables["smtpconfig"].Rows[0]["smtphost"] = txtSmtpHost.Text;
                dsConfig.Tables["smtpconfig"].Rows[0]["smtpport"] = port.ToString();
                dsConfig.Tables["smtpconfig"].Rows[0]["smtpencryption"] = selectedEncryptrb.Content;
                dsConfig.Tables["smtpconfig"].Rows[0]["smtpauth"] = selectedAuthrb.Name;
                dsConfig.Tables["smtpconfig"].Rows[0]["smtpusername"] = txtUserName.Text;
                dsConfig.Tables["smtpconfig"].Rows[0]["smtppassword"] = txtPassword.Password;

                dsConfig.WriteXml(filename);

                mEmailConfig.emailfrom = txtFromEmail.Text;
                mEmailConfig.emailtitle = txtFromTitle.Text;
                mEmailConfig.smtphost = txtSmtpHost.Text;
                mEmailConfig.smtpport = txtSmtpPort.Text;
                mEmailConfig.smtpencryption = selectedEncryptrb.Content.ToString();
                mEmailConfig.smtpauth = selectedAuthrb.Name;
                mEmailConfig.smtpusername = txtUserName.Text;
                mEmailConfig.smtppassword = txtPassword.Password;

                var isOpen = CheckSMTPConnection(txtSmtpHost.Text, port);
                if (isOpen == false)
                {
                    MessageBox.Show("The selected SMTP Server is not responding on the port from this machine, might not able to send the email, please revalidate SMTP Server and Port.", "Validation Warning!");
                }

                MessageBox.Show("Success: Data has been updated", "Validation");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed: Failed to update: " + ex.Message, "Validation");
            }
        }
        private void rbEncrypt_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb == null)
            {
                MessageBox.Show("Sender is not a RadioButton");
                return;
            }
            if (rb.IsChecked == true)
            {
                selectedEncryptrb = rb;
            }
        }
        private void rbAuth_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb == null)
            {
                MessageBox.Show("Sender is not a RadioButton");
                return;
            }
            if (rb.IsChecked == true)
            {
                selectedAuthrb = rb;
                if (rb.Name.Equals("UsernameAuth", StringComparison.OrdinalIgnoreCase))
                {
                    txtUserName.IsEnabled = true;
                    txtPassword.IsEnabled = true;
                }
                else
                {
                    txtPassword.Password = "";
                    txtUserName.Text = "";
                    txtUserName.IsEnabled = false;
                    txtPassword.IsEnabled = false;
                }
            }
        }

        private void txtSmtpHost_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dtSmtpServers != null && dtSmtpServers.Rows.Count > 0)
            {
                var find = dtSmtpServers.Select("smtphost='" + txtSmtpHost.Text + "'");
                if (find != null && find.Length > 0)
                {
                    txtSmtpPort.Text = find[0]["smtpport"].ToString();
                    rbSsl.IsChecked = true;
                    UsernameAuth.IsChecked = true;
                }
            }

        }

        private DataTable ReadSMTPTemplate()
        {
            DataSet dsSmtp = new DataSet();
            string filename = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Configuration\\Smtplist.xml";
            dsSmtp.ReadXml(filename);

            if (dsSmtp.Tables.Count > 0)
            {
                foreach (DataRow dr in dsSmtp.Tables[0].Rows)
                {
                    txtSmtpHost.Items.Add(dr["smtphost"].ToString());
                }
            }

            if (dsSmtp.Tables.Count > 0)
            {
                return dsSmtp.Tables[0];
            }
            else
            {
                return null;
            }
        }
        private bool CheckSMTPConnection(string smtpserver, int port)
        {
            bool isOpen = false;
            TcpClient tc = null;
            try
            {
                tc = new TcpClient(smtpserver, port);
                isOpen = true;
            }
            catch (Exception se)
            {

            }
            finally
            {
                if (tc != null)
                {
                    tc.Close();
                }
            }

            return isOpen;
        }
    }
}
