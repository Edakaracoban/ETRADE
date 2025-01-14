//System.Net: Ağ ile ilgili işlemler için gerekli sınıfları içerir. Bu, örneğin SMTP (Simple Mail Transfer Protocol) ile e-posta göndermek için kullanılır.
//System.Net.Mail: E - posta ile ilgili sınıfları içerir. MailMessage ve SmtpClient gibi sınıflar burada tanımlıdır.
using System.Net;
using System.Net.Mail;

namespace ETRADE.WebUI.EmailService
{
    public static class MailHelper //e-posta göndermek için kullanılan bir yardımcı sınıf olan MailHelper'ı içeriyor. 
    {
        public static bool SendEmail(string body, string to, string subject, bool isHtml = true)
        {
            return SendEmail(body, new List<string> { to }, subject, isHtml);
        }

        private static bool SendEmail(string body, List<string> to, string subject, bool isHtml)
        {
            bool result = false;

            try
            {
                var message = new MailMessage();
                message.From = new MailAddress("ucuncubinyilakademimailservice@gmail.com");

                to.ForEach(x => //birden çok mail adresine mail gönderme
                {
                    message.To.Add(new MailAddress(x));
                });

                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;

                //mail server ayarları
                using (var smtp = new SmtpClient("smtp.gmail.com", 587)) //smtp : secure mail transfer protocol
                {
                    smtp.EnableSsl = true;
                    smtp.Credentials = new NetworkCredential("ucuncubinyilakademimailservice@gmail.com", "wdpy prpp pekv nfll");
                    smtp.UseDefaultCredentials = false;
                    smtp.Send(message);
                    result = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                result = false;
            }

            return result;
        }
    }
}