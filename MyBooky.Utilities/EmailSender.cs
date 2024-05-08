using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Authenticators;
using System;



namespace MyBooky.Utilities
{
    public class EmailSender : IEmailSender
    {
        public string? MailgunApiKey { get; set; }
        public string? MailgunDomain { get; set; }

        public EmailSender(IConfiguration _config)
        {
            MailgunApiKey = _config.GetValue<string>("Mailgun:ApiKey");
            MailgunDomain = _config.GetValue<string>("Mailgun:Domain");

        }


        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (MailgunApiKey == null || MailgunDomain == null)
            {
                throw new InvalidOperationException("Mailgun API key or domain is not configured.");
            }
            RestClient client = new RestClient("https://api.mailgun.net/v3");
            RestRequest request = new RestRequest();

            // Set up authentication
            string apiKey = "api:" + MailgunApiKey;
            request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(apiKey)));

            // Set up request parameters
            request.AddParameter("domain", MailgunDomain, ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "Mybooky <postmaster@me.mrdee.org>");
            request.AddParameter("to", email);
            request.AddParameter("subject", subject);
            request.AddParameter("html", htmlMessage);
            request.Method = Method.Post;

            // Execute the request asynchronously and return the resulting task
            return client.ExecuteAsync(request);
        }

    }
}
