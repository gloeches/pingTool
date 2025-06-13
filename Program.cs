// See https://aka.ms/new-console-template for more information
using System.Net.NetworkInformation;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;
using System.Timers;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Transactions;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Fluent;
string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\agilent\\logs\\PingTool\\";
string dateTime = DateTime.Now.ToString();
string createddate = Convert.ToDateTime(dateTime).ToString("yyyy-MM-dd-h-mm");
string fileFailed = createddate+"pingfailed.txt";
string filePass = createddate + @"pingPass.txt";
int processId = Process.GetCurrentProcess().Id;

//spàce to add logging
Logger logger = LogManager.GetCurrentClassLogger();
logger.Info("Test log");




//start program
System.Timers.Timer aTimer;
Console.WriteLine($"Current process ID: {processId}");
logger.Info($"Current process ID: {processId}");
Console.WriteLine("PingAsync running.....");
logger.Info("PingAsync running.....");
string target = "";
AutoResetEvent waiter = new AutoResetEvent(false);
Ping pingSender = new Ping();
pingSender.PingCompleted += new PingCompletedEventHandler(PingCompleteCallback);
string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
byte[] buffer = Encoding.ASCII.GetBytes(data);
int timeout = 2000;
PingOptions options = new PingOptions(64, true);
Console.WriteLine("The application started at {0:HH:mm:ss.fff}", DateTime.Now);
logger.Info("The application started at {0:HH:mm:ss.fff}",DateTime.Now);
Console.WriteLine("Time to live: {0}", options.Ttl);
logger.Info("Time to live: {0}", options.Ttl);
//Console.WriteLine("Don't fragment: {0}", options.DontFragment);
Console.WriteLine("The log files are saved : {0}", path);
logger.Info("The log files are saved : {0}", path);
var ipOption = new Option<string>("--ip")
{
    Description = "Enter the ip or Hostname to ping. ",
    IsRequired = true
};
ipOption.AddAlias("-ip");
   
   
var rootCommand = new RootCommand("application to send pings ");
rootCommand.AddOption(ipOption);
rootCommand.SetHandler((ip) =>
{
    Console.WriteLine("ip: {0}",ip);
    target= ip;
    
    SetTimer();
    pingSender.SendAsync(target, timeout, buffer, options, waiter);
    waiter.WaitOne();
    Console.WriteLine("\nPress the Enter key to exit the application...\n");
    Console.ReadLine();
    aTimer.Stop();
    aTimer.Dispose();

    Console.WriteLine("Ping completed ...");
},
ipOption);
return rootCommand.InvokeAsync(args).Result;


/*for (int i = 0; i < 50; i++)
{
    pingSender.SendAsync(target, timeout, buffer, options, waiter);
    waiter.WaitOne();
}
*/


void PingCompleteCallback(object sender, PingCompletedEventArgs e)
{

    if (e.Cancelled)
    {
        Console.WriteLine("Ping canceled ");
        ((AutoResetEvent)e.UserState).Set();
        return;
    }
    if (e.Error != null)
    {
        string line = string.Format("Ping Failed .....");
        Console.WriteLine(line);
        //        string path = @"c:\temp\PingFailed.txt";
        WriteFailPing(path, fileFailed, line);


        ((AutoResetEvent)e.UserState).Set();
        return;
    }

    PingReply reply = e.Reply;
    if (reply.Status == IPStatus.TimedOut)
    {
        WriteFailPing(path, fileFailed, "Ping timeout ");
    }
    else
    {
        DisplayReply(reply);
    }

    ((AutoResetEvent)e.UserState).Set();
}

void DisplayReply(PingReply reply)
{
    if (reply == null)
        return;
    {
        string line = string.Format("The IP {0} Routing time {1} status {2}", reply.Address, reply.RoundtripTime, reply.Status);
    //    Console.WriteLine(line);
        WriteFailPing(path, filePass, line);
    }

}
void WriteFailPing(string path, string file, string linea)
{
    //   string path = @"c:\temp\MyTest.txt";
    // This text is added only once to the file.
    if (!Directory.Exists(path))
    {
        Directory.CreateDirectory(path);
    }
    if (!File.Exists(path + file))
    {
        // Create a file to write to.
        using (StreamWriter sw = File.CreateText(path + file))
        {
            sw.WriteLine("{0:yyyy-MMM-dd HH:mm:ss.fff}  message {1}", DateTime.Now, linea);

        }
    }

    // This text is always added, making the file longer over time
    // if it is not deleted.
    using (StreamWriter sw = File.AppendText(path + file))
    {
        sw.WriteLine("{0:yyyy-MMM-dd HH:mm:ss.fff}  message {1}", DateTime.Now, linea);

    }

    // Open the file to read from.


}

void OnTimedEvent(Object source, ElapsedEventArgs e)
{
/*    Console.WriteLine("The Elapsed event was raised at {0:yyyy-MMM-dd HH:mm:ss.fff}",
                      e.SignalTime);*/
    pingSender.SendAsync(target, timeout, buffer, options, waiter);
    waiter.WaitOne();
}
void SetTimer()
{
    // Create a timer with a two second interval.
    aTimer = new System.Timers.Timer(10000);
    // Hook up the Elapsed event for the timer. 
    aTimer.Elapsed += OnTimedEvent;
    aTimer.AutoReset = true;
    aTimer.Enabled = true;
}

