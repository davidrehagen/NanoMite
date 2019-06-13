using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.String;

namespace NanoMite
{
    class Program
    {
        private static void Main(string[] args)
        {
            var configuration = JObject.Parse(File.ReadAllText("NanoMite.json"));            
            var mites = MiteLoader.LoadMites(configuration["mite-plugin-path"].Value<string>());
            
            foreach (var arg in args)
            {
                switch (arg)
                {
                    case "-heartbeat":
                        SendHeartbeat(mites, configuration);
                        break;
                    case "-checkup":
                        Checkup(mites, configuration);
                        break;
                    case "-help":
                        GetHelp(mites);
                        break;
                    default:
                        ExecuteCustomAction(arg, mites);
                        break;
                }
            }
            Console.WriteLine("Finished.");
        }
        
        private static void SendHeartbeat(IEnumerable<Mite> mites, JObject configuration)
        {
            var emailSubject = configuration["heartbeat-email-subject"].Value<string>()
                .Replace("[COMPUTER-NAME]", configuration["computer-name"].Value<string>());

            var emailBody = Empty;

            foreach (var mite in mites)
            {
                foreach (var item in mite.Heartbeat())
                {
                    emailBody += item + "<br>";
                }
            }

            var task = SendEmail(configuration["aws-ses-settings"]["key"].Value<string>(), configuration["aws-ses-settings"]["secret"].Value<string>(),
                configuration["email-settings"]["from"].Value<string>(), configuration["email-settings"]["to"].Value<string>(), emailSubject, emailBody);

            task.Wait();
        }
       
        private static void ExecuteCustomAction(string arg, IEnumerable<Mite> mites)
        {
            var usedCustomAction = false;

            foreach (var mite in mites)
            {
                if (mite.CustomAction(arg))
                    usedCustomAction = true;
            }

            if (!usedCustomAction)
            {
                Console.WriteLine(
                    $"Argument '{arg}' was not recognized, try using -help to see all supported arguments.");
            }
        }

        private static void GetHelp(IEnumerable<Mite> mites)
        {
            Console.WriteLine(
                "-heartbeat: E-mails a general status to ensure the system is running");
            Console.WriteLine(
                "-checkup: E-mails when one of the check is outside of the norm.");
            Console.WriteLine(
                "-help: Lists supported arguments.");

            foreach (var mite in mites)
            {
                foreach (var item in mite.GetHelpParameters())
                {
                    Console.WriteLine(item);
                }
            }
        }

        private static void Checkup(IEnumerable<Mite> mites, JObject configuration)
        {
            var emailSubject = configuration["checkup-email-subject"].Value<string>()
                .Replace("[COMPUTER-NAME]", configuration["computer-name"].Value<string>());

            var emailBody = Empty;

            var checkupItems = 0;
            foreach (var mite in mites)
            {
                foreach (var item in mite.Checkup())
                {
                    emailBody += item + "<br>";
                    checkupItems++;
                }
            }

            if (checkupItems > 0)
            {
                var task = SendEmail(configuration["aws-ses-settings"]["key"].Value<string>(), configuration["aws-ses-settings"]["secret"].Value<string>(),
                    configuration["email-settings"]["from"].Value<string>(), configuration["email-settings"]["to"].Value<string>(), emailSubject, emailBody);

                task.Wait();
            }
        }

        private static async Task<bool> SendEmail(string awsKey, string awsSecret, string from, string to,
            string subject, string html)
        {
            using (var ses = new AmazonSimpleEmailServiceClient(awsKey, awsSecret, RegionEndpoint.USEast1))
            {
                var request = new SendEmailRequest(from, new Destination(new List<string> {to}),
                    new Message
                    {
                        Subject = new Content(subject),
                        Body = new Body
                        {
                            Html = new Content(html)
                        }
                    });

                var sendResult = await ses.SendEmailAsync(request);

                return sendResult.HttpStatusCode == HttpStatusCode.OK;
            }
        }
    }
}