using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for ucEmailLogs.xaml
    /// </summary>
    public partial class ucEmailLogs : UserControl
    {
        public ucEmailLogs()
        {
            InitializeComponent();
        }

        private void EmailLogs_Loaded(object sender, RoutedEventArgs e)
        {
            dtpLogDate.SelectedDate = DateTime.Now;
            dtpLogDate.DisplayDate = DateTime.Now;

            EmailLogger emailLogger = new EmailLogger();
            DataTable dt = emailLogger.ReadLog(null);
            dgvEmailLog.ItemsSource = dt.DefaultView;
        }

        private void btnGet_Click(object sender, RoutedEventArgs e)
        {
            EmailLogger emailLogger = new EmailLogger();
            DataTable dt = emailLogger.ReadLog(dtpLogDate.SelectedDate);
            dgvEmailLog.ItemsSource = dt.DefaultView;
        }
    }
}
