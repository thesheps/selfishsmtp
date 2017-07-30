using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SelfishSmtp
{
    public interface ISmtpServer : IDisposable
    {
        void Start();
        IList<MailMessage> Messages { get; }
    }

    public class SmtpServer : ISmtpServer
    {
        public IList<MailMessage> Messages { get; }

        public SmtpServer(int port)
        {
            Messages = new List<MailMessage>();
            _port = port;
        }

        public void Start()
        {
            ThreadPool.QueueUserWorkItem(StartListening);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~SmtpServer()
        {
            ReleaseUnmanagedResources();
        }

        private void StartListening(object state)
        {
            try
            {
                _listener = new TcpListener(IPAddress.Loopback, _port);
                _listener.Start();

                var client = _listener.AcceptTcpClientAsync().Result;

                Write(client, "220 localhost -- Selfish Smtp Server");

                while (true)
                {
                    try
                    {
                        var data = Read(client);

                        if (data.Length <= 0)
                            continue;

                        if (data.StartsWith("QUIT"))
                        {
                            client.Client.Dispose();
                            break;
                        }

                        if (data.StartsWith("EHLO"))
                        {
                            Write(client, "250 OK");
                        }

                        if (data.StartsWith("RCPT TO"))
                        {
                            Write(client, "250 OK");
                        }

                        if (data.StartsWith("MAIL FROM"))
                        {

                            Write(client, "250 OK");
                        }

                        if (!data.StartsWith("DATA"))
                            continue;

                        Write(client, "354 Start mail input; end with");
                        data = Read(client);

                        var message = data.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
                        Write(client, "250 OK");

                        data = Read(client);

                        Messages.Add(new MailMessage
                        {
                            From = ParseField(message, "From"),
                            Date = ParseField(message, "Date"),
                            Subject = ParseField(message, "Subject"),
                            To = ParseField(message, "To"),
                            Sender = ParseField(message, "Sender"),
                            Body = data.Replace(Environment.NewLine, string.Empty)
                        });
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static string ParseField(IEnumerable<string> data, string field)
        {
            return data.FirstOrDefault(d => d.Contains($"{field}:"))?.Replace($"{field}:", string.Empty).Trim();
        }

        private static void Write(TcpClient client, string strMessage)
        {
            var clientStream = client.GetStream();
            var encoder = new ASCIIEncoding();
            var buffer = encoder.GetBytes(strMessage + "\r\n");

            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();
        }

        private static string Read(TcpClient client)
        {
            var messageBytes = new byte[8192];
            var bytesRead = 0;
            var clientStream = client.GetStream();
            var encoder = new ASCIIEncoding();
            bytesRead = clientStream.Read(messageBytes, 0, 8192);

            var strMessage = encoder.GetString(messageBytes, 0, bytesRead);

            return strMessage;
        }

        private void ReleaseUnmanagedResources()
        {
            _listener.Stop();
        }

        private TcpListener _listener;
        private readonly int _port;
    }
}