using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace beeEmailing
{
    /// <summary>
    /// Interaction logic for frmReleaseNote.xaml
    /// </summary>
    public partial class frmReleaseNote : Window
    {
        public frmReleaseNote()
        {
            InitializeComponent();
        }

        private void wbReleaseNote_Loaded(object sender, RoutedEventArgs e)
        {
            string filename = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\ReleaseNote\\ReleaseNote.html";
            string html = File.ReadAllText(filename);
            wbReleaseNote.NavigateToString(html);
        }
    }
}
