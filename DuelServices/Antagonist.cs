using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Twilio;
using Twilio.Http;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Net.Mail;

namespace DuelServices
{
    public class Antagonist
    {
        private const string DATE_FORMAT = "MMMM d, yyyy";
        private string _twilioAccountSid;
        private string _twilioToken;
        private PhoneNumber _twilioFrom;
        private string _smtpServer;
        private int _smtpPort;
        private string _smtpUsername;
        private string _smtpPassword;
        private string _mailFrom;

        public Antagonist()
        {
            _twilioAccountSid = ConfigurationManager.AppSettings["TwilioSID"];
            _twilioToken = ConfigurationManager.AppSettings["TwilioToken"];
            _twilioFrom = new PhoneNumber(ConfigurationManager.AppSettings["CallFrom"]);
            _smtpServer = ConfigurationManager.AppSettings["SMTPServer"];
            _smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]);
            _smtpUsername = ConfigurationManager.AppSettings["SMTPUsername"];
            _smtpPassword = ConfigurationManager.AppSettings["SMTPPassword"];
            _mailFrom = ConfigurationManager.AppSettings["MailFrom"];
        }

        public void Call()
        {
            var instructionsXml = ConfigurationManager.AppSettings["TwilioInstructionsXML"];
            TwilioClient.Init(_twilioAccountSid, _twilioToken);
            List<PhoneNumber> numberList = new DatabaseServices().GetParticipants()
                .Select(x => new PhoneNumber(x.Phone))
                .ToList();
            Parallel.ForEach(numberList, (p) =>
            {
                var call = CallResource.Create(p, _twilioFrom, url: new Uri(instructionsXml), method: HttpMethod.Get);
            });
        }

        public void Text(Participant recipient, string message)
        {
            TwilioClient.Init(_twilioAccountSid, _twilioToken);
            List<PhoneNumber> numberList = new DatabaseServices().GetParticipants()
                .Select(x => new PhoneNumber(x.Phone))
                .ToList();
            Parallel.ForEach(numberList, (p) =>
            {
                var sms = MessageResource.Create(p, from: _twilioFrom, body: message);
            });
        }

        public void SendMail()
        {
            var participantList = new DatabaseServices().GetParticipants();
            SmtpClient postman = new SmtpClient(_smtpServer, _smtpPort);
            postman.EnableSsl = true;
            postman.Credentials = new System.Net.NetworkCredential(_smtpUsername, _smtpPassword);
            foreach (var p in participantList)
            {
                MailMessage email = new MailMessage(new MailAddress(_mailFrom, "Empower Course"), new MailAddress(p.Email))
                {
                    Subject = "Hi there!",
                    Body = "This is just a friendly reminder that Winston-Salem can code circles around you (just kidding).  Have a great evening!",
                    IsBodyHtml = true
                };
                postman.Send(email);
            }
        }

        public void SendHarperMail(string subject, string message)
        {
            MailMessage email = new MailMessage(new MailAddress(_mailFrom, "Empower Course"), new MailAddress("harper@sightsource.net"))
            {
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };
        }

        public void CallProbationReminder()
        {
            var instructionsXml2 = ConfigurationManager.AppSettings["TwilioInstructionsXML2"];
            TwilioClient.Init(_twilioAccountSid, _twilioToken);
            // this is Harper
            var call = CallResource.Create(new PhoneNumber("8045064006"), _twilioFrom, url: new Uri(instructionsXml2), method: HttpMethod.Get);
        }

        // here's an example of a tuple, and why I personally don't like them...
        public Tuple<Participant, Participant> GetMaleFemaleRandomPair()
        {
            List<Participant> allParticipants = new DatabaseServices().GetParticipants();
            var randomFemale = allParticipants
                .Where(x => x.Gender == TypeOfGender.Female)
                .OrderBy(x => Guid.NewGuid())
                .First();
            var randomMale = allParticipants
                .Where(x => x.Gender == TypeOfGender.Male)
                .OrderBy(x => Guid.NewGuid())
                .First();
            return new Tuple<Participant, Participant>(randomMale, randomFemale);
        }

        public byte[] GetAppointmentPDF(Participant maleParticipant)
        {
            var templatePath = ConfigurationManager.AppSettings["DoctorNoteTemplatePath"];
            using (MemoryStream pageStream = new MemoryStream())
            {
                PdfReader pdfReader = new PdfReader(templatePath);
                PdfStamper pdfStamper = new PdfStamper(pdfReader, pageStream);
                AcroFields pdfFormFields = pdfStamper.AcroFields;
                pdfFormFields.SetField("current_date", DateTime.Today.ToString(DATE_FORMAT));
                pdfFormFields.SetField("recipient_full_name_and_comma", maleParticipant.FirstName + " " + maleParticipant.LastName + ",");
                pdfStamper.FormFlattening = true;
                pdfStamper.Close();
                return pageStream.ToArray();
            }
        }
    }
}