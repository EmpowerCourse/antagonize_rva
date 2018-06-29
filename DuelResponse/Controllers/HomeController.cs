using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Configuration;
using DuelServices;

namespace DuelResponse.Controllers
{
    public class HomeController : Controller
    {
        private const string MIME_PDF = "application/pdf";
        private const string CURRENT_USER_COOKIE_KEY = "CURRENT_USER";

        private bool hasValidSessionCookie()
        {
            return Request.Cookies.AllKeys.Contains(CURRENT_USER_COOKIE_KEY);
        }


        public ActionResult LogOn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogOn(string username)
        {
            DatabaseServices dbs = new DatabaseServices();
            if (dbs.IsAuthorizedUser(username))
            {
                HttpCookie cookie = new HttpCookie(CURRENT_USER_COOKIE_KEY);
                cookie.Value = username;
                this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                return RedirectToAction("Index");
            }
            return RedirectToAction("Forbidden");
        }

        public ActionResult Forbidden()
        {
            return View();
        }

        public ActionResult Index()
        {
            if (!hasValidSessionCookie()) return RedirectToAction("LogOn");
            return View();
        }

        public ActionResult SendMail()
        {
            if (!hasValidSessionCookie()) return RedirectToAction("LogOn");
            new Antagonist().SendMail();
            return new JsonResult()
            {
                Data = new
                {
                    success = true
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetRandomPair()
        {
            if (!hasValidSessionCookie()) return RedirectToAction("LogOn");
            var randomPair = new Antagonist().GetMaleFemaleRandomPair();
            return new JsonResult()
            {
                Data = new
                {
                    success = true,
                    Message = String.Format("Pssst - what's up with {0}? ...and {1} is being a real jerk today too.",
                        randomPair.Item1.FullName, randomPair.Item2.FullName)
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult CallHarper()
        {
            if (!hasValidSessionCookie()) return RedirectToAction("LogOn");
            new Antagonist().CallProbationReminder();
            return new JsonResult()
            {
                Data = new { success = true },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpPost]
        public ActionResult MailHarper(string subject, string body)
        {
            if (!hasValidSessionCookie()) return RedirectToAction("LogOn");
            new Antagonist().SendHarperMail(subject, body);
            return new JsonResult()
            {
                Data = new { success = true },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult CallThem()
        {
            if (!hasValidSessionCookie()) return RedirectToAction("LogOn");
            new Antagonist().Call();
            return new JsonResult() {
                Data = new { success = true },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult Appointment()
        {
            if (!hasValidSessionCookie()) return RedirectToAction("LogOn");
            var dbs = new DatabaseServices();
            var randomMaleParticipant = dbs.GetParticipants(TypeOfGender.Male).OrderBy(x => Guid.NewGuid()).First();
            var ant = new Antagonist();
            byte[] pdfBytes = ant.GetAppointmentPDF(randomMaleParticipant);
            string filename = String.Format("\"AppointmentRequest{0}.{1}\"",
                DateTime.Now.ToString("dd_MMM_yyyy_HHmm"), "pdf");
            Response.AppendHeader("Content-Disposition", String.Format("attachment; filename={0}", filename));
            return new FileContentResult(pdfBytes, MIME_PDF);
        }
    }
}