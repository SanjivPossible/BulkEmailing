using System;
using System.Collections.Generic;
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
    /// Interaction logic for ucHelp.xaml
    /// </summary>
    public partial class ucHelp : UserControl
    {

        const string helpemail = "<body scroll='no' style='font-family:Arial;font-size:16px'><p class=MsoNormal style='margin-left:.75in;text-indent:-.25in'>How to send Bulk Email:<o:p></o:p></p><p class=MsoListParagraph style='margin-left:.75in'><o:p>&nbsp;</o:p></p><ol style='margin-top:0in' start=1 type=1> <li class=MsoListParagraph style='margin-left:.25in;mso-list:l0 level1 lfo3'>Add/Update SMTP configuration details<o:p></o:p></li> <li class=MsoListParagraph style='margin-left:.25in;mso-list:l0 level1 lfo3'>Load     an email data in excel format with columns that can be used as a     placeholder, for example(excel file): Email Id, Name, <span class=SpellE>EmployeeId</span>,     Voucher Code, etc...<o:p></o:p></li> <li class=MsoListParagraph style='margin-left:.25in;mso-list:l0 level1 lfo3'>Select/Add     column name in To/CC/BCC/Subject/Body field in the curly bracket as a     placeholder, like {Email Id}. Each column in the excel file should be     unique<o:p></o:p></li> <li class=MsoListParagraph style='margin-left:.25in;mso-list:l0 level1 lfo3'>Write/copy     HTML in Body textbox. Any Image need to convert into text format and     replace in <span class=SpellE>src</span> value<o:p></o:p></li> <li class=MsoListParagraph style='margin-left:.25in;mso-list:l0 level1 lfo3'>Use     the &quot;<span style='color:#4472C4;mso-themecolor:accent1'>Convert image     to Text</span>&quot; link to convert image to text<o:p></o:p></li></ol></body>";

        public ucHelp()
        {
            InitializeComponent();
        }
        private void wbHelp_Loaded(object sender, RoutedEventArgs e)
        {
            wbHelp.NavigateToString("<html>" + helpemail + "</html>");
        }

    }
}
