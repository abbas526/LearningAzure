using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Mail;

namespace OrientalApplication
{
    public static class Utility
    {       
        
        private static string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/App_Data/OrientalDB.db") + ";Version=3;New=True;Compress=True;";

        public static List<string> GetProjectNames()
        {
            SQLiteConnection conn = new SQLiteConnection(connectionString);

            conn.Open();

            SQLiteCommand cmd = new SQLiteCommand(conn)
            {
                CommandText = "select Name from ProjectMaster where IsActive='yes'"
            };
            var reader = cmd.ExecuteReader();
            List<string> projectNames = new List<string>();
            while (reader.Read())
            {
                projectNames.Add(reader[0].ToString());
            }

            conn.Close();
            if (projectNames.Count() > 0)
            {
                projectNames = projectNames.OrderBy(x => x).ToList();
            }
            return projectNames;
        }
        public static List<string> GetOldProjectNames()
        {
            SQLiteConnection conn = new SQLiteConnection(connectionString);

            conn.Open();

            SQLiteCommand cmd = new SQLiteCommand(conn)
            {
                CommandText = "select Name from ProjectMaster where IsActive='no'"
            };
            var reader = cmd.ExecuteReader();
            List<string> projectNames = new List<string>();
            while (reader.Read())
            {
                projectNames.Add(reader[0].ToString());
            }

            conn.Close();
            if (projectNames.Count() > 0)
            {
                projectNames = projectNames.OrderBy(x => x).ToList();
            }
            return projectNames;

        }


        public static List<string> GetPaymentTerms() {

            SQLiteConnection conn = new SQLiteConnection(connectionString);

            conn.Open();

            SQLiteCommand cmd = new SQLiteCommand(conn)
            {
                CommandText = "select PaymentTerm from PaymentTermsMaster order by 1 asc"
            };
            var reader = cmd.ExecuteReader();
            List<string> paymentTerms = new List<string>();
            while (reader.Read())
            {
                paymentTerms.Add(reader[0].ToString());
            }

            conn.Close();
            if (paymentTerms.Count() > 0)
            {
                paymentTerms = paymentTerms.OrderBy(x => x).ToList();
            }
            return paymentTerms;
        }

        public static void SendEmail()
        {
            string smtpAddress = "smtp.gmail.com";
            int portNumber = 587;
            bool enableSSL = true;
            string emailFromAddress = "abbas.stovewala@gmail.com"; //Sender Email Address  
            string password = "Masjid1234#"; //Sender Password  
            string emailToAddress = "bstovewala@gmail.com"; //Receiver Email Address  
            string subject = "Hello";
            string body = "Hello, This is Email sending test using gmail.";

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(emailFromAddress);
                mail.To.Add(emailToAddress);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;
                //mail.Attachments.Add(new Attachment("D:\\TestFile.txt"));//--Uncomment this to send any attachment  
                using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                {
                    smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                    smtp.EnableSsl = enableSSL;
                    smtp.Send(mail);
                }
            }
        }
    }
}