using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace bEmailing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class frmHost : Window
    {        
        EmailLogger emailLogger = new EmailLogger();       
        ucEmail oucEmail = null;
        public frmHost()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string logFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Logs";
            if (!Directory.Exists(logFolder)) Directory.CreateDirectory(logFolder);

            oucEmail = new ucEmail();
            oucEmail.viewImportData.Height = this.pnlHost.ActualHeight;
            ContentArea.Content = oucEmail;
            this.Title = "Bulk Mailing : Schedule & Send Email";
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            frmHelp frmHelp = new frmHelp();
            frmHelp.ShowDialog();
            this.Title = "Bulk Mailing : How to send an Email";
        }

        private void SmtpConfig_Click(object sender, RoutedEventArgs e)
        {
            frmSettings frm = new frmSettings();
            frm.ShowDialog();
            this.Title = "Bulk Mailing : SMTP Configuration";

            //this.pnlHost.Children.Clear();
            //var uc = new ucSMTPConfig();
            //this.pnlHost.Children.Add(uc);
            //this.Title = "Bulk Mailing : SMTP Configuration";

            //bool find = false;
            //foreach (var tb in FindVisualChildren<ucSMTPConfig>(this.pnlHost))
            //{
            //    find = true;

            //    var ui = tb as UIElement;
            //    var maxZ = this.pnlHost.Children.OfType<UIElement>().Where(x => x != ui).Select(x => Panel.GetZIndex(x)).Max();
            //    Panel.SetZIndex(ui, maxZ + 1);

            //    break;
            //}

            //if (find == false)
            //{
            //    this.pnlHost.Children.Clear();

            //    var uc = new ucSMTPConfig();
            //    this.pnlHost.Children.Add(uc);
            //    this.Title = "Bulk Mailing : SMTP Configuration";
            //    //var ui = uc as UIElement;
            //    //var maxZ = this.pnlHost.Children.OfType<UIElement>().Where(x => x != ui).Select(x => Panel.GetZIndex(x)).Max();
            //    //Panel.SetZIndex(ui, 100);

            //}


        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                if (child != null && child is T)
                    yield return (T)child;

                foreach (T childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }

        private void btnImgText_Click(object sender, RoutedEventArgs e)
        {
            frmImageToText frm = new frmImageToText();
            frm.ShowDialog();
            this.Title = "Bulk Mailing : Convert Image to Text";
        }

        private void btnSendEmail_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new ucEmail();
            this.Title = "Bulk Mailing : Schedule & Send Email";
        }

        private void btnImportData_Click(object sender, RoutedEventArgs e)
        {

            foreach (var tb in FindVisualChildren<ucEmail>(this.pnlHost))
            {
                oucEmail = tb as ucEmail;
            }
            oucEmail.viewImportData.Visibility = Visibility.Visible;
            oucEmail.viewDraftEmail.Visibility = Visibility.Collapsed;
            oucEmail.viewImportData.Height = this.pnlHost.ActualHeight;

            //OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //openFileDialog1.Multiselect = false;
            //openFileDialog1.ValidateNames = true;
            //openFileDialog1.DereferenceLinks = false; // Will return .lnk in shortcuts.
            //openFileDialog1.Filter = "Excel Files|*.xls;*.xlsx";

            //Nullable<bool> result = openFileDialog1.ShowDialog();
            //if (result == true)
            //{
            //    try
            //    {
            //        string selectedFile = openFileDialog1.FileName;
            //        if (string.IsNullOrEmpty(selectedFile) || selectedFile.Contains(".lnk"))
            //        {
            //            MessageBox.Show("Please select a valid Excel File");
            //        }
            //        else
            //        {

            //            dtEmaildata.Clear();
            //            oucEmail.cmbTo.Items.Clear();
            //            oucEmail.cmbCc.Items.Clear();
            //            oucEmail.cmbBcc.Items.Clear();
            //            oucEmail.dgvEmailData.ItemsSource = null;

            //            DataTable dt = oExcel.GetDataTableFromExcel(selectedFile);
            //            using (DbDataReader dr = dt.CreateDataReader())
            //            {
            //                //Get Original Datatable structure
            //                dtEmaildata = dt.Clone();

            //                // Add Auto Increment Column called ID
            //                dtEmaildata.Columns.Add(new DataColumn("Row_Id", typeof(System.Int32))
            //                {
            //                    AutoIncrement = true,
            //                    AllowDBNull = false,
            //                    AutoIncrementSeed = 1,
            //                    AutoIncrementStep = 1,
            //                    Unique = true
            //                });

            //                // Change Auto Increment Column Ordinal Position to 0 (ie First Column)
            //                dtEmaildata.Columns["Row_Id"].SetOrdinal(0);

            //                // Re-load original Data
            //                dtEmaildata.Load(dr);
            //            }
            //            if (dtEmaildata.Columns.Count > 0) dtEmaildata.Columns.Add(COL_EMAILSTATUS).SetOrdinal(1);

            //            oucEmail.dgvEmailData.ItemsSource = dtEmaildata.DefaultView;
            //            oucEmail.dgvEmailData.Columns[0].Visibility = Visibility.Hidden;
            //            oucEmail.dgvEmailData.Visibility = Visibility.Visible;


            //            foreach (DataColumn cl in dtEmaildata.Columns)
            //            {
            //                if (cl.ColumnName.Equals(COL_ROWID) || cl.ColumnName.Equals(COL_EMAILSTATUS)) continue;
            //                oucEmail.cmbTo.Items.Add("{" + cl.ColumnName + "}");
            //            }

            //            oucEmail.cmbCc.Items.Add("");
            //            oucEmail.cmbBcc.Items.Add("");
            //            foreach (DataColumn cl in dtEmaildata.Columns)
            //            {
            //                if (cl.ColumnName.Equals(COL_ROWID) || cl.ColumnName.Equals(COL_EMAILSTATUS)) continue;
            //                oucEmail.cmbCc.Items.Add("{" + cl.ColumnName + "}");
            //                oucEmail.cmbBcc.Items.Add("{" + cl.ColumnName + "}");

            //            }
            //            oucEmail.lblCount.Text = "Status: " + dtEmaildata.Rows.Count.ToString() + "/" + dtEmaildata.Rows.Count.ToString();

            //        }

            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show(ex.Message);
            //    }
            //}
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

        private void btnDraftEmail_Click(object sender, RoutedEventArgs e)
        {
            foreach (var tb in FindVisualChildren<ucEmail>(this.pnlHost))
            {
                oucEmail = tb as ucEmail;
            }
            oucEmail.viewDraftEmail.Visibility = Visibility.Visible;
            oucEmail.viewImportData.Visibility = Visibility.Collapsed;
            oucEmail.viewDraftEmail.Height = this.pnlHost.ActualHeight;
        }

        private void btnReleaseNote_Click(object sender, RoutedEventArgs e)
        {
            var frm = new frmReleaseNote();
            frm.ShowDialog();
            this.Title = "Bulk Mailing : Release Note";
        }
    }
}
