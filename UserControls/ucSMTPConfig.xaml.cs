using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;


namespace bEmailing
{
    /// <summary>
    /// Interaction logic for ucSMTPConfig.xaml
    /// </summary>
    public partial class ucSMTPConfig : UserControl
    {

        RadioButton selectedEncryptrb;
        RadioButton selectedAuthrb;
        public ucSMTPConfig()
        {
            InitializeComponent();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DataSet dsConfig = new DataSet();
            string filename = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "AppConfig.xml";
            dsConfig.ReadXml(filename);

            DataTable smptconfig = dsConfig.Tables["smtpconfig"];

            txtFromEmail.Text = smptconfig.Rows[0]["emailfrom"].ToString();
            txtFromTitle.Text = smptconfig.Rows[0]["emailtitle"].ToString();
            txtSmtpHost.Text = smptconfig.Rows[0]["smtphost"].ToString();
            txtSmtpPort.Text = smptconfig.Rows[0]["smtpport"].ToString();
            txtUserName.Text = smptconfig.Rows[0]["smtpusername"].ToString();
            txtPassword.Text = smptconfig.Rows[0]["smtppassword"].ToString();
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
                string filename = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "AppConfig.xml";
                dsConfig.ReadXml(filename);


                if (string.IsNullOrEmpty(txtFromEmail.Text) && string.IsNullOrEmpty(txtFromTitle.Text) && string.IsNullOrEmpty(txtSmtpHost.Text) && string.IsNullOrEmpty(txtSmtpPort.Text))
                {
                    MessageBox.Show("All field is mandatory to fill", "Validation");
                    return;
                }
                if (UsernameAuth.IsChecked == true)
                {
                    if (string.IsNullOrEmpty(txtUserName.Text) && string.IsNullOrEmpty(txtPassword.Text))
                    {
                        MessageBox.Show("Please enter user name and password", "Validation");
                        return;
                    }
                }

                int port = 0;
                if (Int32.TryParse(txtSmtpPort.Text, out port) == false)
                {
                    MessageBox.Show("Please enter SMTP Port in numeric","Validation");
                    return;
                }

                dsConfig.Tables["smtpconfig"].Rows[0]["emailfrom"] = txtFromEmail.Text;
                dsConfig.Tables["smtpconfig"].Rows[0]["emailtitle"] = txtFromTitle.Text;
                dsConfig.Tables["smtpconfig"].Rows[0]["smtphost"] = txtSmtpHost.Text;
                dsConfig.Tables["smtpconfig"].Rows[0]["smtpport"] = port.ToString();
                dsConfig.Tables["smtpconfig"].Rows[0]["smtpencryption"] = selectedEncryptrb.Content;
                dsConfig.Tables["smtpconfig"].Rows[0]["smtpauth"] = selectedAuthrb.Name;
                dsConfig.Tables["smtpconfig"].Rows[0]["smtpusername"] = txtUserName.Text;
                dsConfig.Tables["smtpconfig"].Rows[0]["smtppassword"] = txtPassword.Text;

                dsConfig.WriteXml(filename);

                mEmailConfig.emailfrom = txtFromEmail.Text;
                mEmailConfig.emailtitle = txtFromTitle.Text;
                mEmailConfig.smtphost = txtSmtpHost.Text;
                mEmailConfig.smtpport = txtSmtpPort.Text;
                mEmailConfig.smtpencryption = selectedEncryptrb.Content.ToString();
                mEmailConfig.smtpauth = selectedAuthrb.Name;
                mEmailConfig.smtpusername = txtUserName.Text;
                mEmailConfig.smtppassword = txtPassword.Text;

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
                    txtPassword.Text = "";
                    txtUserName.Text = "";
                    txtUserName.IsEnabled = false;
                    txtPassword.IsEnabled = false;
                }
            }
        }

    }
}
