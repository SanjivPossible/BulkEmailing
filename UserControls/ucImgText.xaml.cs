using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace beeEmailing
{
    /// <summary>
    /// Interaction logic for ucImgText.xaml
    /// </summary>
    public partial class ucImgText : UserControl
    {
        public ucImgText()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialogAttachment = new OpenFileDialog();
            openFileDialogAttachment.Multiselect = false;
            openFileDialogAttachment.ValidateNames = true;
            openFileDialogAttachment.DereferenceLinks = false; // Will return .lnk in shortcuts.
            openFileDialogAttachment.Filter = "Image Files|*.jpg;*.jpeg;*.gif";

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

                        ImageToText(openFileDialogAttachment.FileName);
                        //var ext = System.IO.Path.GetExtension(openFileDialogAttachment.FileName);
                        //ext = ext.Substring(1);

                        //BitmapImage bitmap = new BitmapImage();
                        //bitmap.BeginInit();
                        //bitmap.UriSource = new Uri(openFileDialogAttachment.FileName);
                        //bitmap.EndInit();
                        //imgText.Source = bitmap;

                        //string content = string.Format("src='data:image/{0};base64,{1}'", ext, Convert.ToBase64String(File.ReadAllBytes(openFileDialogAttachment.FileName)));
                        //txtImage.Document.Blocks.Clear();
                        //txtImage.Document.Blocks.Add(new Paragraph(new Run(content)));

                        //lblImagesize.Content = "Image size: Width(" + string.Format("{0:0.##}", bitmap.Width) + ")*" + "Height(" + string.Format("{0:0.##}", bitmap.Height) + ")";

                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void ImgTextCopy_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string richText = new TextRange(txtImage.Document.ContentStart, txtImage.Document.ContentEnd).Text;
            System.Windows.Clipboard.SetText(richText);
        }

        private void ImageToText(string filename)
        {
            var ext = System.IO.Path.GetExtension(filename);
            ext = ext.Substring(1);

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(filename);
            bitmap.EndInit();
            imgText.Source = bitmap;

            string content = string.Format("src='data:image/{0};base64,{1}'", ext, Convert.ToBase64String(File.ReadAllBytes(filename)));
            txtImage.Document.Blocks.Clear();
            txtImage.Document.Blocks.Add(new Paragraph(new Run(content)));

            lblImagesize.Content = "Image size: Width(" + string.Format("{0:0.##}", bitmap.Width) + ")*" + "Height(" + string.Format("{0:0.##}", bitmap.Height) + ")";

        }

    }
}
