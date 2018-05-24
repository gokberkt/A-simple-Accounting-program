using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DCIO
{
    public class Helpers
    {
        public static string PasswordToMD5(string password)
        {
            
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] dizi = Encoding.UTF8.GetBytes(password);
            dizi = md5.ComputeHash(dizi);
            StringBuilder sb = new StringBuilder();
            foreach (byte ba in dizi)
            {
                sb.Append(ba.ToString("x2").ToLower());
            }
            return sb.ToString();
        }

        
        public static string SendEmail()
        {
            string result = "";
            try
            {
                dcDBEntities db = new dcDBEntities();
                MailSettings ms = db.MailSettings.FirstOrDefault(x => x.ID == 1);

                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(ms.SmptServer);

                mail.From = new MailAddress(ms.MailAddress);
                mail.To.Add(ms.MailTo);
                mail.Subject = "Giriş denemesi bildirimi";
                mail.Body = "Tarih ve Saat : " + DateTime.Now.ToString();

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential(ms.MailAddress, ms.MailPassword);
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
                result = "Success";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
    }
}
