using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Mail;

namespace UniEvents.WebApp.Pages {
   public class LoginModel : WebAppPageModel {

        public  int VerNumber = 1234;
        public  void SendEmail() 
        {
            int randomNumber = 1234;
            VerNumber = randomNumber;
            SmtpClient smtp = new SmtpClient("smtp.gmail.com");
            smtp.EnableSsl = true;
            smtp.Port = 587;
            smtp.Credentials = new NetworkCredential("UniEventsEmail@gmail.com",
               "gbocwdvvqkvpwwiy");
            smtp.Send("Nathan.j.ullman@gmail.com", "ullmanator12@gmail.com",
               "ya boi", VerNumber.ToString());
        }
      public void OnGet() {
        }

      public LoginModel(IHttpContextAccessor accessor): base(accessor) { }
   }
}