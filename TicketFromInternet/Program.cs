﻿using HtmlAgilityPack;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace TicketFromInternet
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            List<string> TimeString = new List<string>();
            TimerCallback tc = new TimerCallback(Check);
            Timer tm = new Timer(tc, TimeString, 0, 300000);
            while (true);
        }

        private async static void Check(object state)
        {
            List<string> timeString = (List<string>)state;
            for (int i = 0; i < 3; i++)
            {
                using (HttpClient http = new HttpClient())
                {
                    string url = $"http://86.57.159.51:8028/ticket/Doctor/SelectDoctor?DoctorId=130&Offset={i}";
                    var response = await http.GetAsync(url);
                    var pageContent = await response.Content.ReadAsStringAsync();
                    HtmlDocument documentHome = new HtmlDocument();
                    documentHome.LoadHtml(pageContent);
                    var nodes = documentHome.DocumentNode.SelectNodes("//div[./div[./div[@class='ticket-ticketcount']//span] and ./div[./div[@class='ticket-daynumber']//label]]");
                    foreach (var node in nodes)
                    {
                        string talons = node.SelectSingleNode(".//div[@class='ticket-ticketcount']//span").InnerText;
                        string day = node.SelectSingleNode(".//div[@class='ticket-daynumber']//label").InnerText;
                        if (int.Parse(talons.Remove(0, talons.IndexOf('/') + 1).Trim()) != 0)
                        {
                            var times = node.ParentNode.SelectNodes(".//div[@class='ticketCont']");
                            string text = "\n";
                            foreach (var time in times)
                            {
                                string tempTime = time.InnerText;
                                if (timeString.FirstOrDefault(n => n == tempTime) == null)
                                {
                                    timeString.Add(tempTime);
                                    text += $" {tempTime}";
                                }
                            }
                            if (text != "\n")
                            {
                                var emailMessage = new MimeMessage();
                                emailMessage.From.Add(new MailboxAddress("Ололо", "drotik-timofeev@yandex.ru"));
                                emailMessage.To.Add(new MailboxAddress("", "droootik@gmail.com"));
                                emailMessage.Subject = "Талон";
                                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                                {
                                    Text = $"Появился талон в интернете! {day}.{DateTime.Now.Month + i} {text}"
                                };
                                var emailMessage1 = new MimeMessage();
                                emailMessage1.From.Add(new MailboxAddress("Ололо", "drotik-timofeev@yandex.ru"));
                                emailMessage1.To.Add(new MailboxAddress("", "alesya_mir@tut.by"));
                                emailMessage1.Subject = "Талон";
                                emailMessage1.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                                {
                                    Text = $"Появился талон в интернете! {day}.{DateTime.Now.Month + i}"
                                };

                                using (var client = new SmtpClient())
                                {
                                    client.Connect("smtp.yandex.ru", 465, true);
                                    client.Authenticate("drotik-timofeev@yandex.by", "superstar199416");
                                    client.Send(emailMessage);
                                    client.Send(emailMessage1);
                                    client.Disconnect(true);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
