using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Timers;
using System.Net.NetworkInformation;


namespace Service1
{
    public partial class Service1 : ServiceBase
    {
        private Timer timer;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(OnTimerElapsed);
            timer.Interval = TimeSpan.FromHours(12).TotalMilliseconds;
            timer.Start();
        }

        protected override void OnStop()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            string workloadData = CollectWorkloadData();
            string filePath = @"C:\Users\elmostafa\Desktop\L2S2\OS\Projects\Service1\Service1\WorkLoadData.txt";
            SaveToFile(filePath, workloadData);
            SendEmail(filePath);
        }

        private string CollectWorkloadData()
        {
            float cpuUsage = GetCPUUsage();
            float memoryUsage = GetMemoryUsage();
            float hddUsage = GetHDDUsage();
            float networkUsage = GetNetworkUsage();
            return $"CPU Usage: {cpuUsage}%\nMemory Usage: {memoryUsage} MB\nHDD Usage: {hddUsage} GB\nNetwork Usage: {networkUsage} KB/s";
        }

        private float GetCPUUsage()
        {
            using (PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
            {
                cpuCounter.NextValue(); 
                System.Threading.Thread.Sleep(1000); 
                return cpuCounter.NextValue();
            }
        }

        private float GetMemoryUsage()
        {
            using (PerformanceCounter memoryCounter = new PerformanceCounter("Memory", "Available MBytes"))
            {
                return memoryCounter.NextValue();
            }
        }

        private float GetHDDUsage()
        {
            DriveInfo driveInfo = new DriveInfo("C"); 
            return (float)driveInfo.TotalFreeSpace / (1024 * 1024 * 1024);
        }

        private float GetNetworkUsage()
        {
            // Use NetworkInterface to get the network usage
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            long totalBytesSent = 0;
            long totalBytesReceived = 0;

            foreach (var networkInterface in networkInterfaces)
            {
                totalBytesSent += networkInterface.GetIPv4Statistics().BytesSent;
                totalBytesReceived += networkInterface.GetIPv4Statistics().BytesReceived;
            }

            return (float)(totalBytesSent + totalBytesReceived) / 1024;
        }


        private void SaveToFile(string filePath, string data)
        {
            // Save the collected data
            File.WriteAllText(filePath, data);
        }


        private void SendEmail(string attachmentPath)
        {
            // Configure email settings
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587;
            string smtpUsername = "example@gmail.com";
            string smtpPassword = "App password";

            string recipientEmail = "recipient Email address";
            string subject = "PC Workload Data";
            string body = "Please find attached the PC workload data.";

            using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
            {
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = true;

                using (MailMessage mailMessage = new MailMessage(smtpUsername, recipientEmail, subject, body))
                {
                    // Attach the workload data file
                    Attachment attachment = new Attachment(attachmentPath);
                    mailMessage.Attachments.Add(attachment);

                    smtpClient.Send(mailMessage);
                }
            }
        }
    }
}
