using System;
using System.Net.Mail;
using System.Threading;

namespace MetX.Standard.IO
{
	/// <summary>
	/// Allows for the sending of a simple email asynchronously on another thread.
	/// </summary>
	public class Email
	{
		/// <summary>Allows for the quick and asynchronous sending of a simple email</summary>
        /// <param name="fromName">Display name of the person sending the email</param>
        /// <param name="fromEmail">From Email address</param>
        /// <param name="toName">Display name for the person receiving the email</param>
        /// <param name="toEmail">To Email Address</param>
        /// <param name="subject">Email Subject</param>
        /// <param name="body">Email Body (pure text)</param>
        public static void SendMail(string fromName, string fromEmail, string toName, string toEmail, string subject, string body)
		{
            // Join();
            var mm = new MailMessage(new MailAddress(fromEmail, fromName), new MailAddress(toEmail, toName))
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = body.IndexOf("<HTML>", StringComparison.Ordinal) > -1 
                             || body.IndexOf("<html>", StringComparison.Ordinal) > -1
            };
            Send(mm);
		}

        /// <summary>
        /// Asynchronously sends a MailMessage
        /// </summary>
        /// <param name="mm">The MailMessage to send</param>
        public static void Send(MailMessage mm)
        {
            ThreadPool.QueueUserWorkItem(Start, mm);
        }
        /// <summary>Private function for sending the asychronous email on a new thread</summary>
        private static void Start(object objMailMessage)
        {
            try
            {
                new SmtpClient(System.Configuration.ConfigurationManager.AppSettings["SmtpServer"]).Send(
                    (MailMessage) objMailMessage);
            }
            catch
            {
                // Ignored
            }
        }

    }
}
