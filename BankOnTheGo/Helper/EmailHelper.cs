using System.Net.Mail;

public static class EmailHelper
{
    public static void SendEmail(string to, string subject, string body)
    {
        using (var client = new SmtpClient("smtp.gmail.com", 587))
        {
            client.Credentials = new System.Net.NetworkCredential("tina.kakhniashvili1@gmail.com", "password");
            client.EnableSsl = true;

            var mailMessage = new MailMessage("tina.kakhniashvili1@gmail.com", to, subject, body);
            client.Send(mailMessage);
        }
    }
}