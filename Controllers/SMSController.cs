using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio.AspNet.Mvc;
using Twilio.TwiML;
using System.Configuration;
using Twilio.Rest.Api.V2010.Account;
using Twilio;
using Twilio.AspNet.Common;
using Twilio.TwiML.Messaging;

namespace CafateriaSystem.Controllers
{

    public class SMSController : TwilioController
    {

        public ActionResult SendSMS()
        {

            var accountSid = ConfigurationManager.AppSettings["TwilioAccountSid"];
            var authToken = ConfigurationManager.AppSettings["TwilioAuthToken"];
            TwilioClient.Init(accountSid, authToken);

            var message = MessageResource.Create(
            from: new Twilio.Types.PhoneNumber("+12055837128"),
            to: new Twilio.Types.PhoneNumber("+94768536328"),
            body: "Please confirm your deliver by typing YES and replying to this message...");

            return Content(message.Sid);
        }

        public ActionResult Index()
        {
            var response = new MessagingResponse();
            var message = new Message();
            message.Body("Hello World!");
            response.Append(message);
            response.Redirect(url: new Uri("https://demo.twilio.com/welcome/sms/"));

           return View(response);
        }





    }
}

