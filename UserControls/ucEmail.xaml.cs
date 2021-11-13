using HtmlAgilityPack;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Net.Mail;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;


namespace beeEmailing
{
    /// <summary>
    /// Interaction logic for ucEmail.xaml
    /// </summary>
    public partial class ucEmail : UserControl
    {

        const string COL_EMAILSTATUS = "EmailStatus";
        const string COL_ROWID = "Row_Id";
        const string SUCCESS = "Success";
        const string FAILED = "Failed";

        DateTime dtStartTimer = DateTime.Now;
        FileInfo EmailAttachment = null;

        Excel oExcel = new Excel();
        Email oEmail = new Email();
        DataTable dtEmaildata = new DataTable();
        EmailLogger emailLogger = new EmailLogger();
        DataSet dsConfig = new DataSet();
        BackgroundWorker bwSending = new BackgroundWorker();
        BackgroundWorker bwSmtpValidation = new BackgroundWorker();
        BackgroundWorker bwEmailValidation = new BackgroundWorker();
        DispatcherTimer tmCounter = new DispatcherTimer();
        OpenFileDialog openFileDialogAttachment = new OpenFileDialog();

        bool IsSendingEmail = false;
        bool IsSmtpOpen = true;

        string toColumn = string.Empty;
        string ccColumn = string.Empty;
        string bccColumn = string.Empty;
        string strSubect = string.Empty;
        string strBody = string.Empty;


        static int counter = 0;
        static int rowCount = 0;
        static int etaMin = 0;
        int tableRowCount = 0;

        public ucEmail()
        {
            InitializeComponent();
            tmCounter.Tick += TmCounter_Tick;
            tmCounter.Interval = new TimeSpan(0, 0, 1);
            bwSending.WorkerReportsProgress = true;
            bwSending.DoWork += BwSending_DoWork;
            bwSending.RunWorkerCompleted += BwSending_RunWorkerCompleted;
            bwSending.ProgressChanged += BwSending_ProgressChanged;
            bwSending.WorkerSupportsCancellation = true;

            bwSmtpValidation.DoWork += BwSmtpValidation_DoWork;
            bwSmtpValidation.RunWorkerCompleted += BwSmtpValidation_RunWorkerCompleted;

            bwEmailValidation.DoWork += BwEmailValidation_DoWork;
            bwEmailValidation.RunWorkerCompleted += BwEmailValidation_RunWorkerCompleted;

            ReadConfig();
        }

        private void TmCounter_Tick(object? sender, EventArgs e)
        {
            TimeSpan ts = (DateTime.Now - dtStartTimer);
            lblTimer.Content = ts.ToString(@"hh\:mm\:ss");
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Multiselect = false;
            openFileDialog1.ValidateNames = true;
            openFileDialog1.DereferenceLinks = false; // Will return .lnk in shortcuts.
            openFileDialog1.Filter = "Excel Files|*.xls;*.xlsx";

            Nullable<bool> result = openFileDialog1.ShowDialog();
            if (result == true)
            {
                try
                {
                    string selectedFile = openFileDialog1.FileName;
                    if (string.IsNullOrEmpty(selectedFile) || selectedFile.Contains(".lnk"))
                    {
                        MessageBox.Show("Please select a valid Excel File");
                    }
                    else
                    {

                        ResetColumn();

                        DataTable dt = oExcel.GetDataTableFromExcel(selectedFile);
                        using (DbDataReader dr = dt.CreateDataReader())
                        {
                            //Get Original Datatable structure
                            dtEmaildata = dt.Clone();

                            // Add Auto Increment Column called ID
                            dtEmaildata.Columns.Add(new DataColumn("Row_Id", typeof(System.Int32))
                            {
                                AutoIncrement = true,
                                AllowDBNull = false,
                                AutoIncrementSeed = 1,
                                AutoIncrementStep = 1,
                                Unique = true
                            });

                            // Change Auto Increment Column Ordinal Position to 0 (ie First Column)
                            dtEmaildata.Columns["Row_Id"].SetOrdinal(0);

                            // Re-load original Data
                            dtEmaildata.Load(dr);
                        }
                        if (dtEmaildata.Columns.Count > 0) dtEmaildata.Columns.Add(COL_EMAILSTATUS).SetOrdinal(1);
                        FillComboBox(dtEmaildata);

                        dgvEmailData.ItemsSource = dtEmaildata.DefaultView;
                        dgvEmailData.Columns[0].Visibility = Visibility.Hidden;
                        dgvEmailData.Visibility = Visibility.Visible;
                        pbStatus.Value = 0;
                        tableRowCount = dtEmaildata.Rows.Count;
                        txtSuccessStatus.Content = "0";
                        txtFailedStatus.Content = "0";
                        txtRowCount.Content = tableRowCount.ToString();
                        txtRowDecrement.Content = tableRowCount.ToString();
                        lblTimer.Content = "00 : 00 : 00";
                        lblPbStatus.Visibility = Visibility.Hidden;

                        if (bwSmtpValidation.IsBusy == false) bwSmtpValidation.RunWorkerAsync();

                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnSendmail_Click(object sender, RoutedEventArgs e)
        {
            if (IsSendingEmail == false)
            {
                if (bwSmtpValidation.IsBusy == false) bwSmtpValidation.RunWorkerAsync();
                lblPbStatus.Visibility = Visibility.Hidden;
                if (string.IsNullOrEmpty(mEmailConfig.emailfrom))
                {
                    MessageBox.Show("Fill the SMTP configuration details", "Validation Failed");
                    return;
                }
                if (tableRowCount <= 0)
                {
                    MessageBox.Show("Select email data for bulk emailing", "Validation Failed");
                    return;
                }
                bool isvalid = DataValidationCheck();
                if (isvalid == false) return;


                MessageBoxResult result = MessageBox.Show("Are you sure you want to send email to all recipient?" + Environment.NewLine + "You can stop sending email while clicking the same button.", "Confirmation", MessageBoxButton.OKCancel);
                if (MessageBoxResult.OK == result && bwSending.IsBusy == false)
                {
                    strSubect = txtSubject.Text;
                    strBody = new TextRange(txtBody.Document.ContentStart, txtBody.Document.ContentEnd).Text;

                    dtStartTimer = DateTime.Now;
                    tmCounter.Start();

                    btnImport.IsEnabled = false;
                    lblSendmail.Text = "Stop Sending";
                    IsSendingEmail = true;
                    pbStatus.Value = 0;
                    txtSuccessStatus.Content = "0";
                    txtFailedStatus.Content = "0";
                    txtRowCount.Content = tableRowCount.ToString();
                    txtRowDecrement.Content = tableRowCount.ToString();
                    lblPbStatus.Visibility = Visibility.Visible;

                    bwSending.RunWorkerAsync();
                }
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to stop sending email?", "Confirmation", MessageBoxButton.OKCancel);
                if (MessageBoxResult.OK == result && bwSending.IsBusy)
                {
                    bwSending.CancelAsync();
                }
            }
        }

        private void FillComboBox(DataTable dt)
        {

            foreach (DataColumn cl in dtEmaildata.Columns)
            {
                if (cl.ColumnName.Equals(COL_ROWID) || cl.ColumnName.Equals(COL_EMAILSTATUS)) continue;
                cmbTo.Items.Add("{" + cl.ColumnName + "}");
            }

            foreach (DataColumn cl in dtEmaildata.Columns)
            {
                if (cl.ColumnName.Equals(COL_ROWID) || cl.ColumnName.Equals(COL_EMAILSTATUS)) continue;
                cmbCc.Items.Add("{" + cl.ColumnName + "}");
                cmbBcc.Items.Add("{" + cl.ColumnName + "}");
            }
        }

        private void btnAttachment_Click(object sender, RoutedEventArgs e)
        {

            openFileDialogAttachment.Multiselect = false;
            openFileDialogAttachment.ValidateNames = true;
            openFileDialogAttachment.DereferenceLinks = false; // Will return .lnk in shortcuts.

            Nullable<bool> result = openFileDialogAttachment.ShowDialog();
            if (result == true)
            {
                try
                {
                    string selectedFile = openFileDialogAttachment.FileName;
                    if (string.IsNullOrEmpty(selectedFile) || selectedFile.Contains(".lnk"))
                    {
                        MessageBox.Show("Please select a valid File");
                    }
                    else
                    {
                        lblAttachment.Content = openFileDialogAttachment.SafeFileName;
                        EmailAttachment = new FileInfo(openFileDialogAttachment.FileName);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnResetAttachment_Click(object sender, RoutedEventArgs e)
        {
            EmailAttachment = null;
            lblAttachment.Content = string.Empty;
        }

        private bool DataValidationCheck()
        {

            string strBody = new TextRange(txtBody.Document.ContentStart, txtBody.Document.ContentEnd).Text;
            if (string.IsNullOrEmpty(cmbTo.Text) || string.IsNullOrEmpty(txtSubject.Text) || string.IsNullOrEmpty(strBody))
            {
                MessageBox.Show("Draft email content & fill all the mandatory fields", "Validation Failed");
                return false;
            }

            if (cmbTo.Text.IndexOf('{') >= 0)
            {
                toColumn = cmbTo.Text.Replace("{", "").Replace("}", "");
                if (dtEmaildata.Columns.Contains(toColumn) == false)
                {
                    MessageBox.Show("To field does not match with selected data columns");
                    return false;
                }
            }
            else
            {
                bool isValid = IsValidEmail(cmbTo.Text);
                if (isValid == false)
                {
                    MessageBox.Show("The email (To) does not match a correct email format");
                    return false;
                }

            }

            if (!string.IsNullOrEmpty(cmbCc.Text))
            {
                if (cmbCc.Text.IndexOf('{') >= 0)
                {
                    ccColumn = cmbCc.Text.Replace("{", "").Replace("}", "");
                    if (dtEmaildata.Columns.Contains(ccColumn) == false)
                    {
                        MessageBox.Show("Cc field does not match with selected data columns");
                    }
                }
                else
                {
                    bool isValid = IsValidEmail(cmbTo.Text);
                    if (isValid == false)
                    {
                        MessageBox.Show("The email (Cc) does not match a correct email format");
                        return false;
                    }
                }
            }

            if (!string.IsNullOrEmpty(cmbBcc.Text))
            {
                if (cmbBcc.Text.IndexOf('{') >= 0)
                {
                    bccColumn = cmbBcc.Text.Replace("{", "").Replace("}", "");
                    if (dtEmaildata.Columns.Contains(bccColumn) == false)
                    {
                        MessageBox.Show("Bcc field does not match with selected data columns");
                    }
                }
                else
                {
                    bool isValid = IsValidEmail(cmbTo.Text);
                    if (isValid == false)
                    {
                        MessageBox.Show("The email (Bcc) does not match a correct email format");
                        return false;
                    }
                }
            }


            return true;
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if (dtEmaildata == null || tableRowCount <= 0)
            {
                MessageBox.Show("No record found", "Validation");
                return;
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Excel Files| *.xls; *.xlsx";
            saveFileDialog1.Title = "Save an Excel File";
            saveFileDialog1.ShowDialog();


            if (saveFileDialog1.FileName != "")
            {
                oExcel.ExportDataTableToExcel(dtEmaildata, new FileInfo(saveFileDialog1.FileName));
            }
        }

        private void cmbTo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bwEmailValidation.IsBusy) bwEmailValidation.CancelAsync();

            ValidateValue(cmbTo, lblTo, out toColumn);

        }

        private void ValidateValue(ComboBox cmbText, Label lblText, out string strColVariable)
        {
            lblText.Content = string.Empty;
            strColVariable = cmbText.Text.Replace("{", "").Replace("}", "");
            if (!string.IsNullOrEmpty(strColVariable))
            {
                if (dtEmaildata.Columns.Contains(strColVariable))
                {
                    bool isvalid = IsValidEmail(dtEmaildata.Rows[0][strColVariable].ToString());
                    if (isvalid == false)
                    {
                        lblText.Content = "Not a valid email id";
                        lblText.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        lblText.Content = dtEmaildata.Rows[0][strColVariable].ToString();
                        lblText.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
                else
                {
                    bool isvalid = IsValidEmail(cmbText.Text.Trim());
                    if (isvalid == false)
                    {
                        lblText.Content = "Not a valid email id";
                        lblText.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        lblText.Content = cmbText.Text.Trim();
                        lblText.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
            }
        }

        private void cmbCc_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateValue(cmbCc, lblCc, out ccColumn);
        }

        private void cmbBcc_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateValue(cmbBcc, lblBcc, out bccColumn);
        }

        private void txtSubject_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblSubject.Content = string.Empty;
            string SubColumn = txtSubject.Text;
            foreach (DataColumn clm in dtEmaildata.Columns)
            {
                SubColumn = SubColumn.Replace("{" + clm.ColumnName + "}", dtEmaildata.Rows[0][clm.ColumnName].ToString());
            }
            lblSubject.Content = SubColumn;

        }

        private void wbPreview_Loaded(object sender, RoutedEventArgs e)
        {
            char[] x = { '"', '\'' };
            string strBody = new TextRange(txtBody.Document.ContentStart, txtBody.Document.ContentEnd).Text;
            if (!string.IsNullOrEmpty(strBody))
            {

                int wordCount = 0;
                int updateCount = 0;
                int stIndex = 0;
                int enIndex = 0;
                StringBuilder BodyColumn = new StringBuilder(strBody);
                foreach (DataColumn clm in dtEmaildata.Columns)
                {
                    BodyColumn.Replace("{" + clm.ColumnName + "}", dtEmaildata.Rows[0][clm.ColumnName].ToString());
                }
                foreach (Match m in Regex.Matches(BodyColumn.ToString(), "src", RegexOptions.IgnoreCase))
                {
                    wordCount++;
                }

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(BodyColumn.ToString());

                HtmlNodeCollection imgs = new HtmlNodeCollection(doc.DocumentNode.ParentNode);
                imgs = doc.DocumentNode.SelectNodes("//img");
                if (imgs != null)
                {
                    foreach (HtmlNode img in imgs)
                    {
                        HtmlAttribute src = img.Attributes[@"src"];
                        string filename = src.Value;
                        if (File.Exists(filename))
                        {
                            var imgText = ImageToText(filename);
                            src.Value = imgText;
                        }
                    }
                }

                wbPreview.NavigateToString(doc.DocumentNode.OuterHtml);
            }
        }

        public void ReadConfig()
        {
            try
            {
                dsConfig.Clear();
                string filename = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Configuration\\AppConfig.xml";
                dsConfig.ReadXml(filename);
                DataTable smptconfig = dsConfig.Tables["smtpconfig"];

                //txtFrom.Text = smptconfig.Rows[0]["emailtitle"].ToString() + " : " + smptconfig.Rows[0]["emailfrom"].ToString();

                mEmailConfig.emailfrom = smptconfig.Rows[0]["emailfrom"].ToString();
                mEmailConfig.emailtitle = smptconfig.Rows[0]["emailtitle"].ToString();
                mEmailConfig.smtphost = smptconfig.Rows[0]["smtphost"].ToString();
                mEmailConfig.smtpport = smptconfig.Rows[0]["smtpport"].ToString();
                mEmailConfig.smtpencryption = smptconfig.Rows[0]["smtpencryption"].ToString();
                mEmailConfig.smtpauth = smptconfig.Rows[0]["smtpauth"].ToString();
                mEmailConfig.smtpusername = smptconfig.Rows[0]["smtpusername"].ToString();
                mEmailConfig.smtppassword = smptconfig.Rows[0]["smtppassword"].ToString();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Validation Error");
            }

        }

        private void BwSending_DoWork(object? sender, DoWorkEventArgs e)
        {
            rowCount = dtEmaildata.Rows.Count;
            StringBuilder subValue = new StringBuilder();
            StringBuilder bodyValue = new StringBuilder();
            mEmailPreview mpreview = null;
            List<mEmailSchema> oEmailSchema = new List<mEmailSchema>();


            try
            {
                foreach (DataRow row in dtEmaildata.Rows)
                {

                    subValue.Clear();
                    bodyValue.Clear();
                    mpreview = null;

                    mpreview = new mEmailPreview();
                    mpreview.To = row[toColumn].ToString();
                    if (!string.IsNullOrEmpty(ccColumn)) mpreview.CC = row[ccColumn].ToString();
                    if (!string.IsNullOrEmpty(bccColumn)) mpreview.BCC = row[bccColumn].ToString();

                    subValue.Append(strSubect);
                    bodyValue.Append(strBody);

                    foreach (DataColumn clm in dtEmaildata.Columns)
                    {
                        subValue.Replace("{" + clm.ColumnName + "}", row[clm.ColumnName].ToString());
                        bodyValue.Replace("{" + clm.ColumnName + "}", row[clm.ColumnName].ToString());
                    }

                    mpreview.Subject = subValue.ToString();
                    mpreview.Body = bodyValue.ToString();

                    mEmailSchema emailSchema = new mEmailSchema();
                    emailSchema.Row = row;
                    emailSchema.EmailData = mpreview;

                    oEmailSchema.Add(emailSchema);
                }


                Parallel.For(0, oEmailSchema.Count, new ParallelOptions { MaxDegreeOfParallelism = 25 }, (i, state) =>
                {
                    if (bwSending.CancellationPending)
                    {
                        e.Cancel = true;
                        state.Break();
                    }
                    else
                    {
                        UpdateTable(oEmailSchema[i]);
                    }
                });

            }
            catch (Exception ex)
            {
                ErrorLogger errorLogger = new ErrorLogger();
                errorLogger.Log(ex);
            }
        }
        private void UpdateTable(object mObject)
        {
            var tModel = mObject as mEmailSchema;
            mEmailPreview mpreview = tModel.EmailData;
            bool isSend = oEmail.SendEmail(mpreview.To, mpreview.CC, mpreview.BCC, EmailAttachment, mpreview.Subject, mpreview.Body);
            Interlocked.Increment(ref counter);

            lock (dtEmaildata)
            {
                tModel.Row[COL_EMAILSTATUS] = isSend ? SUCCESS : FAILED;
                LogEmail(mpreview, isSend);
                TimeSpan ts = (DateTime.Now - dtStartTimer);
                etaMin = (int)((ts.TotalSeconds / counter) * (rowCount - counter));
            }

            bwSending.ReportProgress(counter);
        }
        private void BwSending_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            txtRowDecrement.Content = (tableRowCount - counter).ToString();
            decimal perc = ((decimal)counter / (decimal)tableRowCount) * 100;
            pbStatus.Value = Convert.ToInt32(perc);
            TimeSpan time = TimeSpan.FromSeconds(etaMin);
            string str = time.ToString(@"mm\:ss");
            lblETA.Text = "ETA (mm:ss): " + str;
        }
        private void BwSending_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            lblSendmail.Text = "Start Sending";
            btnImport.IsEnabled = true;
            tmCounter.Stop();
            counter = 0;
            IsSendingEmail = false;

            int failedCount = dtEmaildata.Select(COL_EMAILSTATUS + " = 'Failed'").Length;
            int successCount = dtEmaildata.Select(COL_EMAILSTATUS + " = 'Success'").Length;
            txtFailedStatus.Content = failedCount.ToString();
            txtSuccessStatus.Content = successCount.ToString();
        }

        private void LogEmail(mEmailPreview mpreview, bool isSend)
        {
            mEmailLog oEmailLog = new mEmailLog();
            oEmailLog.LoggedUser = Environment.UserName;
            oEmailLog.From = mEmailConfig.emailfrom;
            oEmailLog.To = mpreview.To;
            oEmailLog.CC = mpreview.CC;
            oEmailLog.Subject = mpreview.Subject;
            oEmailLog.SendTime = DateTime.Now;
            oEmailLog.IsSend = isSend;
            emailLogger.Log(oEmailLog);
        }

        private string ImageToText(string filename)
        {
            var ext = System.IO.Path.GetExtension(filename);
            ext = ext.Substring(1);
            string content = string.Format("data:image/{0};base64,{1}", ext, Convert.ToBase64String(File.ReadAllBytes(filename)));

            return content;
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            viewImportData.Visibility = Visibility.Collapsed;
            viewDraftEmail.Visibility = Visibility.Visible;
            viewDraftEmail.Height = this.ActualHeight - 20;
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            viewImportData.Visibility = Visibility.Visible;
            viewImportData.Height = this.ActualHeight - 20;
            viewDraftEmail.Visibility = Visibility.Collapsed;
        }

        private void TextBlock_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (bwSmtpValidation.IsBusy == false)
            {
                txtSMTP.Text = "Refreshing...";
                bwSmtpValidation.RunWorkerAsync();
            }
        }

        private void BwSmtpValidation_DoWork(object? sender, DoWorkEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(mEmailConfig.smtpport))
                {
                    IsSmtpOpen = CheckSMTPConnection(mEmailConfig.smtphost, Convert.ToInt32(mEmailConfig.smtpport));
                }
            }
            catch (Exception)
            { }

        }
        private void BwSmtpValidation_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (IsSmtpOpen)
            {
                gdSMTP.Height = 0;
            }
            else
            {
                gdSMTP.Height = 20;
                txtSMTP.Text = "Smtp server is not responding, click here to refresh";
            }
        }

        private void btnValidate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(cmbTo.Text.Trim()))
                MessageBox.Show("Please select column in To field", "Email Validation");

            if (bwEmailValidation.IsBusy == false)
            {
                btnValidate.Content = "Validating...";
                bwEmailValidation.RunWorkerAsync();
            }
        }
        int toEmailErrors = 0;
        private void BwEmailValidation_DoWork(object? sender, DoWorkEventArgs e)
        {
            toEmailErrors = 0;

            if (dtEmaildata.Rows.Count > 0 && !string.IsNullOrEmpty(toColumn))
            {
                if (dtEmaildata.Columns.Contains(toColumn))
                {
                    foreach (DataRow dr in dtEmaildata.Rows)
                    {
                        if (IsValidEmail(dr[toColumn].ToString()) == false) toEmailErrors++;
                    }
                }
            }
        }

        private void BwEmailValidation_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            btnValidate.Content = "Validate all emails";
            MessageBox.Show("Incorrect email address has identified in To field: " + toEmailErrors.ToString(), "Email Validation");
        }


        public bool IsValidEmail(string emailid)
        {
            if (string.IsNullOrEmpty(emailid))
            {
                return false;
            }

            try
            {
                MailAddress m = new MailAddress(emailid);
                return true;
            }
            catch (Exception)
            {
                return false;
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
            { }
            finally
            {
                if (tc != null)
                {
                    tc.Close();
                }
            }
            return isOpen;
        }
        private void ResetColumn()
        {
            dtEmaildata.Clear();
            dgvEmailData.ItemsSource = null;

            cmbTo.Items.Clear();
            cmbCc.Items.Clear();
            cmbBcc.Items.Clear();

            cmbTo.Text = string.Empty;
            cmbCc.Text = string.Empty;
            cmbBcc.Text = string.Empty;

            txtSubject.Text = string.Empty;
            txtBody.Document.Blocks.Clear();
        }


    }
}
