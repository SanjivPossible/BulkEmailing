using HtmlAgilityPack;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;


namespace bEmailing
{
    /// <summary>
    /// Interaction logic for ucEmail.xaml
    /// </summary>
    public partial class ucEmail : UserControl
    {

        const string COL_EMAILSTATUS = "EmailStatus";
        const string COL_ROWID = "Row_Id";
        DateTime dtStartTimer = DateTime.Now;

        FileInfo EmailAttachment = null;

        Excel oExcel = new Excel();
        Email oEmail = new Email();
        DataTable dtEmaildata = new DataTable();
        EmailLogger emailLogger = new EmailLogger();
        DataSet dsConfig = new DataSet();
        BackgroundWorker bwSending = new BackgroundWorker();
        DispatcherTimer tmCounter = new DispatcherTimer();
        OpenFileDialog openFileDialogAttachment = new OpenFileDialog();

        bool IsSendingEmail = false;

        string toColumn = string.Empty;
        string ccColumn = string.Empty;
        string bccColumn = string.Empty;
        string strSubect = string.Empty;
        string strBody = string.Empty;


        static int counter = 0;
        static int rowCount = 0;
        static int etaMin = 0;
        static DateTime startTime = DateTime.Now;

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

        }

        private void TmCounter_Tick(object? sender, EventArgs e)
        {
            TimeSpan ts = (DateTime.Now - dtStartTimer);
            lblTimer.Text = "Timer: " + ts.ToString(@"hh\:mm\:ss");
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

                        dtEmaildata.Clear();

                        dgvEmailData.ItemsSource = null;

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

                        lblCount.Text = "Status: " + dtEmaildata.Rows.Count.ToString() + "/" + dtEmaildata.Rows.Count.ToString();

                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void FillComboBox(DataTable dt)
        {
            cmbTo.Items.Clear();
            cmbCc.Items.Clear();
            cmbBcc.Items.Clear();
            foreach (DataColumn cl in dtEmaildata.Columns)
            {
                if (cl.ColumnName.Equals(COL_ROWID) || cl.ColumnName.Equals(COL_EMAILSTATUS)) continue;
                cmbTo.Items.Add("{" + cl.ColumnName + "}");
            }

            cmbCc.Items.Add("");
            cmbBcc.Items.Add("");
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

        private void btnSendmail_Click(object sender, RoutedEventArgs e)
        {
            if (IsSendingEmail == false)
            {
                ReadConfig();
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

        private bool DataValidationCheck()
        {
            if (string.IsNullOrEmpty(mEmailConfig.emailfrom))
            {
                MessageBox.Show("Fill the SMTP configuration details", "Validation Failed");
                return false;
            }
            if (dtEmaildata.Rows.Count <= 0)
            {
                MessageBox.Show("Select email data for bulk emailing", "Validation Failed");
                return false;
            }

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
                }
            }
            else
            {
                MessageBox.Show("To field does not contains correct format, column should enclosed with curly braket");
                return false;
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
                    MessageBox.Show("Cc field does not contains correct format, column should enclosed with curly braket");
                    return false;
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
                    MessageBox.Show("Bcc field does not contains correct format, column should enclosed with curly braket");
                    return false;
                }
            }


            return true;
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if (dtEmaildata == null || dtEmaildata.Rows.Count <= 0)
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
            lblTo.Content = string.Empty;
            string colToName = cmbTo.Text.Replace("{", "").Replace("}", "");
            if (!string.IsNullOrEmpty(colToName))
            {
                if (dtEmaildata.Columns.Contains(colToName))
                    lblTo.Content = dtEmaildata.Rows[0][colToName].ToString();

            }
        }

        private void cmbCc_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblCc.Content = string.Empty;
            string colCcName = cmbCc.Text.Replace("{", "").Replace("}", "");
            if (!string.IsNullOrEmpty(colCcName))
            {
                if (dtEmaildata.Columns.Contains(colCcName))
                    lblCc.Content = dtEmaildata.Rows[0][colCcName].ToString();

            }

        }

        private void cmbBcc_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblBcc.Content = string.Empty;
            string colName = cmbBcc.Text.Replace("{", "").Replace("}", "");
            if (!string.IsNullOrEmpty(colName))
            {
                if (dtEmaildata.Columns.Contains(colName))
                    lblBcc.Content = dtEmaildata.Rows[0][colName].ToString();

            }
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
                string filename = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "AppConfig.xml";
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
                tModel.Row[COL_EMAILSTATUS] = isSend ? "Success" : "Failed";
                LogEmail(mpreview, isSend);
                TimeSpan ts = (DateTime.Now - dtStartTimer);
                etaMin = (int)((ts.TotalSeconds / counter) * (rowCount - counter));
            }

            bwSending.ReportProgress(counter);
        }
        private void BwSending_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            lblCount.Text = "Status: " + (dtEmaildata.Rows.Count - counter).ToString() + "/" + dtEmaildata.Rows.Count.ToString();
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


    }
}
