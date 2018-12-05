using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace SmtpWithTcpClient
{
    class Program
    {
        public static string GetFQDN()
        {
            string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            string hostName = Dns.GetHostName();

            domainName = "." + domainName;
            if (!hostName.EndsWith(domainName))  // if hostname does not already include domain name
            {
                hostName += domainName;          // add the domain name part
            }

            return hostName;                     // return the fully qualified name
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        static void CheckResponse(string response)
        {
            if (!(response.StartsWith("2") || response.StartsWith("3")))
            {
                throw new Exception(response);
            }
        }

        static bool SendMail(string host, int port,
            string username, string password,
            string from, string to,
            string subject, string textMessage)
        {
            var fqdn = GetFQDN();
            using (var client = new TcpClient(host, port))
            using (var networkStream = client.GetStream())
            using (StreamReader reader = new StreamReader(networkStream, Encoding.ASCII))
            using (var writer = new StreamWriter(networkStream, Encoding.ASCII) { AutoFlush = true })
            {
                CheckResponse(reader.ReadLine());
                writer.WriteLine($"HELO {fqdn}");
                CheckResponse(reader.ReadLine());
                writer.WriteLine("AUTH LOGIN");
                CheckResponse(reader.ReadLine());
                writer.WriteLine(Base64Encode(username));
                CheckResponse(reader.ReadLine());
                writer.WriteLine(Base64Encode(password));
                CheckResponse(reader.ReadLine());
                writer.WriteLine($"MAIL FROM:<{from}>");
                CheckResponse(reader.ReadLine());
                writer.WriteLine($"RCPT TO:<{to}>");
                CheckResponse(reader.ReadLine());
                writer.WriteLine($"DATA");
                CheckResponse(reader.ReadLine());
                writer.WriteLine($"From: <{from}>");
                writer.WriteLine($"To: <{to}>");
                writer.WriteLine($"Subject: {subject}");
                writer.WriteLine();
                writer.WriteLine(textMessage);
                writer.WriteLine(".");
                writer.WriteLine();
                CheckResponse(reader.ReadLine());
                writer.WriteLine($"QUIT");
                return true;


            }
        }

        static void Main(string[] args)
        {
            SendMail("smtp.sendgrid.net", 2525,
                "apikey",
                "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",
                "fredrik.haglund@nackademin.se",
                "fredrik.haglund@nackademin.se",
                "Hello World "+DateTime.Now.ToString("HH:mm:ss"),
                "This is a simple message! /Fredrik"
                );
        }
    }
}
