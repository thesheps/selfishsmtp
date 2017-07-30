using System.Collections.Generic;
using MailKit.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MimeKit;

namespace SelfishSmtp.Tests
{
    [TestClass]
    public class GivenTestServer
    {
        [TestMethod]
        public void WhenISendAnEmail_ThenTheEmailIsSent()
        {
            const int port = 25;

            using (var server = new SmtpServer(port))
            {
                server.Start();

                var client = new MailKit.Net.Smtp.SmtpClient();
                client.Connect("localhost", port, SecureSocketOptions.None);
                client.Send(FormatOptions.Default, new MimeMessage
                {
                    To = { new MailboxAddress("test-recipient@test.com") },
                    Sender = new MailboxAddress("test-sender@test.com"),
                    Body = new TextPart("plain") { Text = "This is a test email" },
                    Subject = "This is a test subject",
                    From = { new MailboxAddress("test-from@test.com") }
                });

                Assert.AreEqual("test-recipient@test.com", server.Messages[0].To);
                Assert.AreEqual("test-from@test.com", server.Messages[0].From);
                Assert.AreEqual("test-sender@test.com", server.Messages[0].Sender);
                Assert.AreEqual("This is a test email", server.Messages[0].Body);
                Assert.AreEqual("This is a test subject", server.Messages[0].Subject);
            }
        }
    }
}