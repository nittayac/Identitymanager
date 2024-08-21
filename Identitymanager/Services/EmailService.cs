using FluentEmail.Core;
using FluentEmail.Smtp;
using System.Net.Mail;
using System.Text;

namespace Identitymanager.Services;

public class EmailService
{
    public void SendEmailAsync(string mailto,string name,string msg,string mailfrom="admin@admin.com")
    {
        var sender = new SmtpSender(() => new System.Net.Mail.SmtpClient("localhost")
        { 
            EnableSsl = false,
            Port =25
            //DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.SpecifiedPickupDirectory,
            //PickupDirectoryLocation =@"E:\Email"
        });

        StringBuilder sb = new StringBuilder();
        sb.Clear();
        sb.Append("Confirm reset a password");

        Email.DefaultSender = sender;

        var email = Email
            .From(mailfrom)
            .To(mailto)
            .Subject("Reset Password")
            .UsingTemplate(sb.ToString(), new
            {
                FirstName = "Small Farm",
                ProductName = "E-commerce MVC"
            }).Body(msg).SendAsync();
    }
}
