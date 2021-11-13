using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace beeEmailing
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
            oucEmail.viewImportData.Height = this.pnlHost.ActualHeight - 20;
            ContentArea.Content = oucEmail;
            this.Title = "beeEmailing : Schedule & Send Email";
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            frmHelp frmHelp = new frmHelp();
            frmHelp.ShowDialog();
            this.Title = "beeEmailing : How to send an Email";
        }

        private void SmtpConfig_Click(object sender, RoutedEventArgs e)
        {
            frmSettings frm = new frmSettings();
            frm.ShowDialog();
            this.Title = "beeEmailing : SMTP Configuration";
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
            this.Title = "beeEmailing : Convert Image to Text";
        }

        private void btnSendEmail_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new ucEmail();
            this.Title = "beeEmailing : Schedule & Send Email";
        }

        private void btnImportData_Click(object sender, RoutedEventArgs e)
        {
            foreach (var tb in FindVisualChildren<ucEmail>(this.pnlHost))
            {
                oucEmail = tb as ucEmail;
            }
            oucEmail.viewImportData.Visibility = Visibility.Visible;
            oucEmail.viewDraftEmail.Visibility = Visibility.Collapsed;
            oucEmail.viewImportData.Height = this.pnlHost.ActualHeight - 20;

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
            oucEmail.viewDraftEmail.Height = this.pnlHost.ActualHeight - 20;

            oucEmail.viewImportData.Visibility = Visibility.Collapsed;

        }

        private void btnReleaseNote_Click(object sender, RoutedEventArgs e)
        {
            var frm = new frmReleaseNote();
            frm.ShowDialog();
            this.Title = "beeEmailing : Release Note";
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private static ImageSource CreateGlyph(string text,
        FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight,
        FontStretch fontStretch, Brush foreBrush)
        {
            if (fontFamily != null && !String.IsNullOrEmpty(text))
            {
                Typeface typeface = new Typeface(fontFamily, fontStyle, fontWeight, fontStretch);
                GlyphTypeface glyphTypeface;
                if (!typeface.TryGetGlyphTypeface(out glyphTypeface))
                    throw new InvalidOperationException("No glyphtypeface found");

                ushort[] glyphIndexes = new ushort[text.Length];
                double[] advanceWidths = new double[text.Length];
                for (int n = 0; n < text.Length; n++)
                {
                    ushort glyphIndex = glyphTypeface.CharacterToGlyphMap[text[n]];
                    glyphIndexes[n] = glyphIndex;
                    double width = glyphTypeface.AdvanceWidths[glyphIndex] * 1.0;
                    advanceWidths[n] = width;
                }

                GlyphRun gr = new GlyphRun(glyphTypeface, 0, false, 1.0, glyphIndexes,new Point(0, 0), advanceWidths,null, null, null, null, null, null);
                GlyphRunDrawing glyphRunDrawing = new GlyphRunDrawing(foreBrush, gr);
                return new DrawingImage(glyphRunDrawing);

            }
            return null;
        }
    }
}
