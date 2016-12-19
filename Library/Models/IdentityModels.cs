using System;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using Library.Repositories;
using System.Net.Mail;
using System.Net;
using System.Text;

namespace Library.Models
{
    public class User : IUser
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }

        public string Id
        {
            get
            {
                return UserId.ToString();
            }
        }

        public void sendMail()
        {
            List<UserHistory> userHistoryList = TransactionRepository.getAllUserTransactions(Id);

            if (userHistoryList == null)
                return;

            StringBuilder mailText = new StringBuilder("<h2>You took the folowing books in our library:</h2>");
            mailText.Append("<span style='line-height: 1.5;'><table><tr><th>Book title</th><th></th><th>Date taken</th><th></th><th>Date returned</th></tr>");

            foreach (UserHistory userHistory in userHistoryList)
            {
                mailText.Append("<tr><td>" + userHistory.Title + "</td><td>|</td>");
                mailText.Append("<td>" + userHistory.DateTaken.ToString("dd MMM yyyy, hh:mm") + "</td><td>|</td>");

                if (!userHistory.DateReturned.Equals(new DateTime(2000, 1, 1)))
                    mailText.Append(userHistory.DateReturned.ToString("dd MMM yyyy, hh:mm"));
                //mailText.Append("; <br />");
                mailText.Append("</td></tr>");
            }
            mailText.Append("</table></span>");

            using (MailMessage mm = new MailMessage("librarytesttask@gmail.com", UserName))
            {
                mm.Subject = "Your book history report";
                mm.Body = mailText.ToString();
                mm.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                NetworkCredential NetworkCred = new NetworkCredential("librarytesttask@gmail.com", "kf135M25");
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;
                smtp.Port = 587;
                smtp.Send(mm);
            }
        }
    }
}