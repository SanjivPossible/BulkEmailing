
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;
using Attachment = System.Net.Mail.Attachment;

namespace beeEmailing
{
    public class Email
    {

        public bool SendEmailBySMTP(string emailTo, string emailCC, string emailBCC, FileInfo attachment, string msgSubject, string msgBody)
        {
            bool IsSend = false;
            string smtpserver = mEmailConfig.smtphost;
            int smptport = Convert.ToInt32(mEmailConfig.smtpport);

            string FromAddress = mEmailConfig.emailfrom;
            string FromAddressTitle = mEmailConfig.emailtitle;

            try
            {
                if (!string.IsNullOrEmpty(emailTo))
                {
                    //Create the mail message and supply it with from and to info
                    using (var mail = new MailMessage())
                    {
                        mail.From = new MailAddress(FromAddress, FromAddressTitle);

                        #region<<Mail to TO>>
                        if (emailTo.Trim().Contains(";"))
                        {
                            string[] ToId = emailTo.Split(';');
                            foreach (string ToEmail in ToId)
                            {
                                if (!string.IsNullOrEmpty(ToEmail) && !ToEmail.Equals("&nbsp;"))
                                {
                                    mail.To.Add(new MailAddress(ToEmail.Trim()));
                                }
                            }
                        }
                        else
                        {
                            mail.To.Add(new MailAddress(emailTo.Trim()));
                        }
                        #endregion

                        #region<<Mail to CC>>
                        if (!string.IsNullOrEmpty(emailCC))
                        {
                            if (emailCC.Contains(';'))
                            {
                                string[] CCId = emailCC.Split(';');
                                foreach (string CCEmail in CCId)
                                {
                                    if (!string.IsNullOrEmpty(CCEmail) && !CCEmail.Equals("&nbsp;"))
                                    {
                                        mail.CC.Add(new MailAddress(CCEmail.Trim()));
                                    }
                                }
                            }
                            else
                            {
                                mail.CC.Add(new MailAddress(emailCC.Trim()));
                            }
                        }
                        #endregion

                        #region<<Mail to BCC>>
                        if (!string.IsNullOrEmpty(emailBCC))
                        {
                            if (emailBCC.Contains(';'))
                            {
                                string[] BCCId = emailBCC.Split(';');

                                foreach (string BCCEmail in BCCId)
                                {
                                    if (!string.IsNullOrEmpty(BCCEmail) && !BCCEmail.Equals("&nbsp;"))
                                    {
                                        mail.Bcc.Add(new MailAddress(BCCEmail.Trim()));
                                    }
                                }
                            }
                            else
                            {
                                mail.Bcc.Add(new MailAddress(emailBCC.Trim()));
                            }
                        }
                        #endregion
                        if (attachment != null)
                        {
                            Attachment eAtt = new Attachment(attachment.FullName);
                            eAtt.Name = attachment.Name;
                            mail.Attachments.Add(new Attachment(attachment.FullName));
                        }

                        if (msgSubject.Length > 254) msgSubject = msgSubject.Substring(0, 254);
                        mail.Subject = msgSubject.Trim().Replace('\r', ' ').Replace('\n', ' ');
                        mail.Body = msgBody;
                        mail.BodyEncoding = System.Text.Encoding.ASCII;
                        mail.IsBodyHtml = true;
                        mail.Priority = MailPriority.Normal;

                        //Create the SMTP client object and send the message
                        using (var smtpClient = new SmtpClient())
                        {
                            smtpClient.Host = smtpserver;
                            smtpClient.Port = smptport;
                            if (mEmailConfig.smtpencryption.Equals("Ssl")) smtpClient.EnableSsl = true;
                            if (mEmailConfig.smtpauth.Equals("DefaultAuth", StringComparison.OrdinalIgnoreCase))
                            {
                                smtpClient.UseDefaultCredentials = true;
                            }
                            else
                            {
                                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                                smtpClient.Credentials = new System.Net.NetworkCredential(mEmailConfig.smtpusername, mEmailConfig.smtppassword);
                            }
                            smtpClient.Send(mail);
                        }
                        IsSend = true;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return IsSend;
        }

        public async Task<bool> SendEmailBySG(string emailTo, string emailCC, string emailBCC, FileInfo filename, string msgSubject, string msgBody)
        {
            bool IsSend = false;
            string sgemail = mEmailConfig.sendgridemailid;
            string sgkey = mEmailConfig.sendgridkey;

            string FromAddress = mEmailConfig.emailfrom;
            string FromAddressTitle = mEmailConfig.emailtitle;

            var tos = new List<EmailAddress>();
            try
            {
                if (!string.IsNullOrEmpty(emailTo))
                {

                    #region<<Mail to TO>>
                    if (emailTo.Contains(";"))
                    {
                        string[] ToId = emailTo.Split(';');

                        foreach (string ToEmail in ToId)
                        {
                            if (!string.IsNullOrEmpty(ToEmail) && !ToEmail.Equals("&nbsp;"))
                            {
                                tos.Add(new EmailAddress(ToEmail));
                            }
                        }
                    }
                    else
                    {
                        tos.Add(new EmailAddress(emailTo));
                    }
                    #endregion

                    #region<<Mail to CC>>
                    if (!string.IsNullOrEmpty(emailCC))
                    {
                        if (emailCC.Contains(';'))
                        {
                            string[] CCId = emailCC.Split(';');

                            foreach (string CCEmail in CCId)
                            {
                                if (!string.IsNullOrEmpty(CCEmail) && !CCEmail.Equals("&nbsp;"))
                                {
                                    tos.Add(new EmailAddress(CCEmail));
                                }
                            }
                        }
                        else
                        {
                            tos.Add(new EmailAddress(emailCC));
                        }
                    }
                    #endregion

                    #region<<Mail to BCC>>
                    if (!string.IsNullOrEmpty(emailBCC))
                    {
                        if (emailBCC.Contains(';'))
                        {
                            string[] BCCId = emailBCC.Split(';');

                            foreach (string BCCEmail in BCCId)
                            {
                                if (!string.IsNullOrEmpty(BCCEmail) && !BCCEmail.Equals("&nbsp;"))
                                {
                                    tos.Add(new EmailAddress(BCCEmail));
                                }
                            }
                        }
                        else
                        {
                            tos.Add(new EmailAddress(emailBCC));
                        }
                    }
                    #endregion


                    if (msgSubject.Length > 254) msgSubject = msgSubject.Substring(0, 254);
                    var Subject = msgSubject.Trim().Replace('\r', ' ').Replace('\n', ' ');

                    var client = new SendGridClient(sgkey);
                    var message = new SendGridMessage();

                    message.From = new EmailAddress(sgemail, FromAddressTitle);
                    message.Subject = Subject;
                    message.HtmlContent = msgBody;
                    message.AddTos(tos);

                    Response response;
                    using (var fileStream = File.OpenRead(filename.FullName))
                    {
                        await message.AddAttachmentAsync(filename.Name, fileStream);
                        response = await client.SendEmailAsync(message);
                    }
                    if (response.IsSuccessStatusCode)
                        IsSend = true;
                    else
                        IsSend = false;

                }

                return IsSend;
            }
            catch (Exception ex)
            {
                return IsSend;
            }

        }
    }
}
