using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace bEmailing
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void HostApp_Startup(object sender, StartupEventArgs e)
        {
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ShowExceptionDetails(e.Exception);
        }

        static void ShowExceptionDetails(Exception Ex)
        {
            // Do logging of exception details
            MessageBox.Show(Ex.Message, "Unhandled Exception");

            try
            {
                ErrorLogger oLog = new ErrorLogger();
                oLog.Log(Ex);
            }
            catch
            { }

        }

    }
}
