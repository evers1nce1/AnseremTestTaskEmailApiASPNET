using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using WebApplication4.Models;

namespace WebApplication4.Services
{
    public class MessageProcessor : IMessageProcessor
    {
        private readonly HashSet<string> _domains = new HashSet<string>()
        {
            "tbank.ru", "alfa.com", "vtb.ru"
        };

        private readonly Dictionary<string, HashSet<string>> _exceptionEmails = new Dictionary<string, HashSet<string>>()
        {
            ["tbank.ru"] = new HashSet<string> { "i.ivanov@tbank.ru" },
            ["alfa.com"] = new HashSet<string> { "s.sergeev@alfa.com", "a.andreev@alfa.com" }
        };

        private readonly Dictionary<string, HashSet<string>> _addedEmails = new Dictionary<string, HashSet<string>>()
        {
            ["tbank.ru"] = new HashSet<string> { "t.tbankovich@tbank.ru", "v.veronickovna@tbank.ru" },
            ["alfa.com"] = new HashSet<string> { "v.vladislavovich@alfa.com" },
            ["vtb.ru"] = new HashSet<string> { "a.aleksandrov@vtb.ru" }
        };

        private List<string> GetCopyEmails(Message message)
        {
            var emails = new List<string>();

            if (!string.IsNullOrWhiteSpace(message.Copy))
            {
                emails.AddRange(message.Copy.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim().ToLower()).Where(x => !string.IsNullOrWhiteSpace(x)));
            }

            return emails;
        }

        private List<string> GetToEmails(Message message)
        {
            var emails = new List<string>();

            if (!string.IsNullOrWhiteSpace(message.To))
            {
                emails.AddRange(message.To.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim().ToLower()).Where(x => !string.IsNullOrWhiteSpace(x)));
            }

            return emails;
        }

        private List<string> GetAllEmails(Message message)
        {
            var emails = GetCopyEmails(message);
            emails.AddRange(GetToEmails(message));
            return emails;
        }
        private HashSet<string> GetExceptionDomains(Message message)
        {
            var result = new HashSet<string>();
            var emails = GetAllEmails(message);

            foreach (var email in emails)
            {
                string domain = ExtractDomain(email);
                if (domain == null)
                    continue;

                if (_exceptionEmails.TryGetValue(domain, out var exceptionEmails))
                {
                    if (exceptionEmails.Contains(email, StringComparer.OrdinalIgnoreCase))
                    {
                        result.Add(domain);
                    }
                }
            }

            return result;
        }
        private string ExtractDomain(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var ind = email.IndexOf('@');
            if (ind <= 0 || ind >= email.Length - 1)
                return null;

            return email.Substring(ind + 1).Trim();
        }
        private HashSet<string> GetWantedDomains(Message message)
        {
            var result = new HashSet<string>();
            foreach (var email in GetAllEmails(message))
            {
                var domain = ExtractDomain(email);
                if (_domains.Contains(domain))
                    result.Add(domain);
            }
            return result;
        }
        public Message ProcessMessage(Message message) 
        {
            var wantedDomains = GetWantedDomains(message);
            if (wantedDomains.Count < 1)
                return message;
            var exceptionDomains = GetExceptionDomains(message);
            var copyEmails = GetCopyEmails(message);
            var toMessages = GetToEmails(message);
            if (exceptionDomains.Count > 0)
            {
                foreach (string domain in exceptionDomains)
                {
                    copyEmails = copyEmails.Where(x => !_addedEmails.GetValueOrDefault(domain).Contains(x)).ToList();
                }
            }
            else
            {
                foreach (string domain in wantedDomains)
                {
                    foreach(string email in _addedEmails.GetValueOrDefault(domain))
                    {
                        if (!toMessages.Contains(email, StringComparer.OrdinalIgnoreCase))
                            copyEmails.Add(email);
                    }
                }
            }

            return new Message { BlindCopy = message.BlindCopy, Body = message.Body, From = message.From, Title = message.Title, To = message.To, Copy = string.Join("; ", copyEmails.Distinct(StringComparer.OrdinalIgnoreCase)) };
        }
    }
}
