using AccountGen.Utils;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AccountGen.Services
{
    public sealed class IMAPService : IDisposable
    {
        private static readonly string fileName = MethodBase.GetCurrentMethod()!.DeclaringType!.ToString();
        private static IMAPService? _instance;
        private static readonly object _lock = new object();

        public static IMAPService GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance = new IMAPService();
                }
            }

            return _instance;
        }

        public readonly ImapClient Client = new ImapClient();

        public IMailFolder? MailFolder { get; private set; }

        private UniqueId LastId { get; set; } = new UniqueId();

        public bool Connect()
        {
            string host = SettingsHelper.GetImapHost();
            int port = int.Parse(SettingsHelper.GetImapPort());
            string username = SettingsHelper.GetImapUsername();
            string password = SettingsHelper.GetImapPassword();

            if (!Client.IsConnected)
            {
                Connect(host, port, username, password);
            }

            if (!Client.IsConnected)
            {
                LoggingHelper.Log(fileName + " | Could not connect to IMAP", LoggingHelper.LogType.Error);
                return false;
            }

            MailFolder = GetMainInboxAndOpenIt();

            if (MailFolder == null)
            {
                LoggingHelper.Log(fileName + " | Failed to open inbox", LoggingHelper.LogType.Error);
                return false;
            }

            return true;
        }

        private void Connect(string host, int port, string username, string password)
        {
            if (String.IsNullOrWhiteSpace(host) || port == 0 || String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine(fileName + " | IMAP Host, Port, Username or Password is empty. Skipping client connection");
                return;
            }

            if (Client.IsConnected)
            {
                Disconnect();
            }

            try
            {
                Client.Connect(host, port, true);
                Client.Authenticate(username, password);
            }
            catch (Exception e)
            {
                Console.WriteLine(fileName + " | " + e.Message);

               Client.Dispose();
            }
        }

        public string GetVerificationIdFromEmailDawn(string requiredRecipient)
        {
            LoggingHelper.Log("Searching E-Mail for " + requiredRecipient);

            var query = SearchQuery.FromContains("hello@dawninternet.com").And(SearchQuery.NotSeen);

            if (MailFolder == null || !Client.IsConnected)
            {
                Connect();
            }

            var uids = MailFolder.Search(query);

            // Check each email for the code and return it if found

            for (int i = 0; i < uids.Count; i++)
            {
                MimeMessage message = MailFolder.GetMessage(uids[i]);
                InternetAddressList recipients = message.To;
                recipients.AddRange(message.Cc);
                recipients.AddRange(message.Bcc);

                bool containsRequiredRecipient = recipients.OfType<MailboxAddress>().Any(mailbox => string.Equals(mailbox.Address, requiredRecipient, StringComparison.OrdinalIgnoreCase));
                if (String.IsNullOrEmpty(requiredRecipient) || containsRequiredRecipient)
                { 
                    string body = message.HtmlBody;

                    if (body.Contains("verify.dawninternet.com"))
                    {
                        // Use Regex to get the code
                        string pattern = @"verifyconfirm\?key=(?<guid>[A-Fa-f0-9]{8}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{12})";
                        var match = Regex.Match(body, pattern);
                        if (match.Success)
                        {
                            Console.WriteLine("E-Mail found for " + requiredRecipient);
                            LastId = uids[i];
                            return match.Groups["guid"].Value;
                        }
                    }
                }
            }
            return "";
        }

        public string GetVerificationLinkFromEmailGrass(string requiredRecipient)
        {
            LoggingHelper.Log("Searching E-Mail for " + requiredRecipient);

            var query = SearchQuery.FromContains("no-reply@grassfoundation.io").And(SearchQuery.NotSeen);

            if (MailFolder == null || !Client.IsConnected)
            {
                Connect();
            }

            var uids = MailFolder.Search(query);

            // Check each email for the code and return it if found

            for (int i = 0; i < uids.Count; i++)
            {
                MimeMessage message = MailFolder.GetMessage(uids[i]);
                InternetAddressList recipients = message.To;
                recipients.AddRange(message.Cc);
                recipients.AddRange(message.Bcc);

                bool containsRequiredRecipient = recipients.OfType<MailboxAddress>().Any(mailbox => string.Equals(mailbox.Address, requiredRecipient, StringComparison.OrdinalIgnoreCase));
                if (String.IsNullOrEmpty(requiredRecipient) || containsRequiredRecipient)
                {
                    string body = message.HtmlBody;

                    if (body.Contains("confirm-email"))
                    {
                        // Use Regex to get the code
                        string pattern = @"""([^""]*?confirm-email[^""]*?)"""; ;
                        var match = Regex.Match(body, pattern);
                        if (match.Success)
                        {
                            Console.WriteLine("E-Mail found for " + requiredRecipient);
                            LastId = uids[i];
                            return match.Groups[1].Value;
                        }
                    }
                }
            }
            return "";
        }

        public void MarkLastEmailAsRead()
        {
            MarkAsRead(MailFolder, LastId);
        }

        public IMailFolder GetMainInboxAndOpenIt()
        {
            var inbox = Client.Inbox;
            inbox.Open(FolderAccess.ReadWrite);
            return inbox;
        }

        public static void MarkAsRead(IMailFolder folder, UniqueId uid)
        {
            folder.Store(uid, new StoreFlagsRequest(StoreAction.Add, MessageFlags.Seen) { Silent = true });
        }

        public void Disconnect()
        {
            Client.Disconnect(true);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
