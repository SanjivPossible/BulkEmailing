
using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace beeEmailing
{
    public class Email
    {


        public bool SendEmail(string emailTo, string emailCC, string emailBCC, FileInfo attachment, string msgSubject, string msgBody)
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

        //public bool SendEmailNew(string emailTo, string emailCC, string emailBCC, FileInfo attachment, string msgSubject, string msgBody)
        //{
        //    bool IsSend = false;
        //    string smtpserver = mEmailConfig.smtphost;
        //    int smptport = Convert.ToInt32(mEmailConfig.smtpport);

        //    string FromAddress = mEmailConfig.emailfrom;
        //    string FromAddressTitle = mEmailConfig.emailtitle;

        //    try
        //    {

        //        if (!string.IsNullOrEmpty(emailTo))
        //        {
        //            var mail = new MimeMessage();
        //            mail.From.Add(new MailboxAddress(FromAddressTitle, FromAddress));

        //            #region<<Mail to TO>>
        //            if (emailTo.Trim().Contains(";"))
        //            {
        //                string[] ToId = emailTo.Split(';');
        //                foreach (string ToEmail in ToId)
        //                {
        //                    if (!string.IsNullOrEmpty(ToEmail) && !ToEmail.Equals("&nbsp;"))
        //                    {
        //                        mail.To.Add(MailboxAddress.Parse(ToEmail.Trim()));
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                mail.To.Add(MailboxAddress.Parse(emailTo.Trim()));
        //            }
        //            #endregion

        //            #region<<Mail to CC>>
        //            if (!string.IsNullOrEmpty(emailCC))
        //            {
        //                if (emailCC.Contains(';'))
        //                {
        //                    string[] CCId = emailCC.Split(';');
        //                    foreach (string CCEmail in CCId)
        //                    {
        //                        if (!string.IsNullOrEmpty(CCEmail) && !CCEmail.Equals("&nbsp;"))
        //                        {
        //                            mail.Cc.Add(MailboxAddress.Parse(CCEmail.Trim()));
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    mail.Cc.Add(MailboxAddress.Parse(emailCC.Trim()));
        //                }
        //            }
        //            #endregion

        //            #region<<Mail to BCC>>
        //            if (!string.IsNullOrEmpty(emailBCC))
        //            {
        //                if (emailBCC.Contains(';'))
        //                {
        //                    string[] BCCId = emailBCC.Split(';');

        //                    foreach (string BCCEmail in BCCId)
        //                    {
        //                        if (!string.IsNullOrEmpty(BCCEmail) && !BCCEmail.Equals("&nbsp;"))
        //                        {
        //                            mail.Bcc.Add(MailboxAddress.Parse(BCCEmail.Trim()));
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    mail.Bcc.Add(MailboxAddress.Parse(emailBCC.Trim()));
        //                }
        //            }
        //            #endregion

        //            if (msgSubject.Length > 254) msgSubject = msgSubject.Substring(0, 254);
        //            mail.Subject = msgSubject.Trim().Replace('\r', ' ').Replace('\n', ' ');

        //            var builder = new BodyBuilder();
        //            if (attachment != null) builder.Attachments.Add(attachment.FullName);
        //            builder.HtmlBody = msgBody;

        //            mail.Body = builder.ToMessageBody();

        //            using (var client = new MailKit.Net.Smtp.SmtpClient())
        //            {                     

        //                if (mEmailConfig.smtpencryption.Equals("Ssl"))
        //                    client.Connect(smtpserver, smptport, SecureSocketOptions.Auto);
        //                else
        //                    client.Connect(smtpserver, 25, SecureSocketOptions.None);


        //                if (!mEmailConfig.smtpauth.Equals("DefaultAuth", StringComparison.OrdinalIgnoreCase))
        //                {
        //                    client.Authenticate(mEmailConfig.smtpusername, mEmailConfig.smtppassword);
        //                }


        //                client.Send(mail);
        //                client.Disconnect(true);

        //            }
        //            IsSend = true;
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return IsSend;
        //}

    }
}
