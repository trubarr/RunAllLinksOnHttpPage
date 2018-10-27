using HtmlAgilityPack;
using RunAllLinksOnHttpPage.Collections.Generic;
using RunAllLinksOnHttpPage.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace RunAllLinksOnHttpPage
{
    public partial class FrmRunAllLinksOnHttpPage : Form
    {
        private int MaxDepth;
        private int NoOfLinks;
        private const string BrowserType = @"firefox";
        private readonly List<string> listOfForbiddenExt = new List<string>
        {
            "NONE",
            ".VSIX",
            ".EXE",
            ".ZIP",
            ".BZ2",
            ".JPG",
            ".BMP",
            ".PNG",
            ".XLSX",
            ".PDF",
            ".DOC"
        };
        private readonly List<string> listOfForbiddenValues = new List<string>
            {
                "www.visitcostarica.com",
                "twitter.com/intent/tweet",
                "help.twitter.com",
                "plus.google.com/share",
                "accounts.google.com",
                "play.google.com/store",
                "docs.google.com",
                "support.google.com",
                "accounts.google.com",
                "www.facebook.com",
                "itunes.apple.com"
            };
        private readonly List<string> UrlList = new List<string>();
        private readonly BackgroundWorker bw = new BackgroundWorker();
        private readonly Random random = new Random();
        private static readonly ManualResetEvent Mre = new ManualResetEvent(false);

        public FrmRunAllLinksOnHttpPage()
        {
            InitializeComponent();

            FormBorderStyle = FormBorderStyle.FixedSingle;

            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(
                delegate { RetrievePagesBatch(); }
                );
            bw.ProgressChanged += new ProgressChangedEventHandler(
                delegate (object o, ProgressChangedEventArgs args) { Progress(args.ProgressPercentage); }
                );

            //var outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {MemberName} {SourceContext} {Message} {NewLine}{Exception}";
            var outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Caller} {Message} {NewLine}{Exception}";

            Log.Logger = new LoggerConfiguration()
                //.ReadFrom.AppSettings()
                //.Enrich.FromLogContext()
                .Enrich.WithCaller()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                //.WriteTo.File("log\\" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "_.log", rollingInterval: RollingInterval.Day)
                .WriteTo.File("log\\" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "_.log", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Verbose, outputTemplate: outputTemplate)
                //.WriteTo.File("log\\" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "_.log", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Verbose, outputTemplate: outputTemplate)
                //.WriteTo.RollingFile("log\\" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "_.log", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Verbose, outputTemplate: outputTemplate)
                .CreateLogger();
        }

        private void Progress(Int32 noOfProcessedUrls)
        {
            labProgress.Text = noOfProcessedUrls.ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //RetrievePagesBatch();

            //EndsWithForbiddenExtension(@"http://www.ametsoc.net/eee/2016/ch20.pdf");
        }

        private void RetrievePagesBatch()
        {
            try
            {
                Log.Information("Start");

                Mre.Set();

                for (int i = 0; i < textBoxUrls.Lines.Length; i++)
                {
                    RetrievePages(textBoxUrls.Lines[i], 0);
                    if (bw.CancellationPending)
                    {
                        KillBrowsers();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error: {ex}", ex);
            }
            finally
            {
                Log.Information("End");
            }
        }

        private bool EndsWithForbiddenExtension(string value)
        {
            foreach (var ext in listOfForbiddenExt)
            {
                if (value.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private bool ContainsForbiden(string value)
        {
            foreach (var forbiddenValue in listOfForbiddenValues)
            {
                //if (value.Contains(forbiddenValue))
                if (value.IndexOf(forbiddenValue, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        private void RetrievePages(string url, int deep)
        {
            try
            {
                HtmlWeb hw = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = hw.Load(url);
                //foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                HtmlNodeCollection htmlNodes = doc.DocumentNode.SelectNodes("//a[@href]");
                htmlNodes.Shuffle();
                for (int i = 0; i < htmlNodes.Count - 1; i++)
                {
                    if (bw.CancellationPending)
                    {
                        break;
                    }

                    Mre.WaitOne();

                    HtmlNode link = htmlNodes[i];
                    if (link != null)
                    {
                        HtmlAttribute att = link.Attributes["href"];
                        if ((att.Value.Length > 12) &&
                            (att.Value.Substring(0, 4).ToUpper() == "HTTP") &&
                            //(att.Value.Substring(att.Value.Length - 5, 5).ToUpper() != ".VSIX") &&
                            //(att.Value.Substring(att.Value.Length - 4, 4).ToUpper() != ".EXE") &&
                            //(att.Value.Substring(att.Value.Length - 4, 4).ToUpper() != ".ZIP") &&
                            //(att.Value.Substring(att.Value.Length - 4, 4).ToUpper() != ".BZ2") &&
                            !EndsWithForbiddenExtension(att.Value) &&
                            //!(att.Value.Contains("www.visitcostarica.com")) &&
                            //!(att.Value.Contains("twitter.com/intent/tweet")) &&
                            //!(att.Value.Contains("help.twitter.com")) &&
                            //!(att.Value.Contains("plus.google.com/share")) &&
                            //!(att.Value.Contains("docs.google.com")) && 
                            //!(att.Value.Contains("www.facebook.com") &&
                            !ContainsForbiden(att.Value)
                            )
                        {
                            //if ((!UrlList.Contains(att.Value)) &&
                            //    (!UrlList.EndWithPart(att.Value.Substring(att.Value.Length - Math.Min(att.Value.Length, 20)))))
                            if (!UrlList.EndWithPart(att.Value.Substring(att.Value.Length - Math.Min(att.Value.Length, 20))))
                            {
                                UrlList.Add(att.Value);
                                NoOfLinks++;
                                bw.ReportProgress(NoOfLinks);
                                Log.Information(NoOfLinks + ": " + att.Value);
                                //Console.WriteLine(NoOfLinks.ToString() + ": " + att.Value);

                                if (random.Next(1, 100) % 2 == 0)
                                {
                                    LoadPage(att.Value);
                                    RetrieveNextPages(att.Value, deep);
                                }
                                else
                                {
                                    RetrieveNextPages(att.Value, deep);
                                    LoadPage(att.Value);
                                }

                                if (MaxDepth != deep)
                                {
                                    RetrievePages(att.Value, deep + 1);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error: {ex}", ex);
            }
        }

        private void RetrieveNextPages(string url, int deep)
        {
            if (MaxDepth != deep)
            {
                RetrievePages(url, deep + 1);
            }
        }

        private void LoadPage(string url)
        {
            try
            {
                Process proc = new Process
                {
                    StartInfo =
                    {
                        FileName = BrowserType + ".exe",
                        Arguments = url,
                        //WindowStyle = ProcessWindowStyle.Hidden,
                        //CreateNoWindow = true,
                        //WorkingDirectory = @"C:\temp\browser",
                        //UseShellExecute = false
                    }
                };
                // att.Value;
                //proc.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                proc.Start();

                if (NoOfLinks % 50 == 0)
                {
                    KillBrowsers();
                }
                else
                {
                    Thread.Sleep(2000);
                    proc.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error: {ex}", ex);
            }
        }

        private static void KillBrowsers()
        {
            Thread.Sleep(5000);
            Process[] browsers = Process.GetProcessesByName(BrowserType);
            foreach (Process browser in browsers)
            {
                try
                {
                    browser.Kill();
                }
                catch (Exception ex)
                {
                    Log.Error($"Error: {ex}", ex);
                }
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                if (!bw.IsBusy)
                {
                    MaxDepth = int.Parse(numericUpDownMaxDepth.Text);
                    string[] lines = textBoxUrls.Lines;
                    lines.Shuffle();
                    textBoxUrls.Lines = lines;
                    //textBoxUrls.Lines.Shuffle();
                    bw.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error: {ex}", ex);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                if (bw.IsBusy)
                    bw.CancelAsync();
                labProgress.Text = @"0";
            }
            catch (Exception ex)
            {
                Log.Error($"Error: {ex}", ex);
            }
        }

        private void Form1_Leave(object sender, EventArgs e)
        {
            btnStop.PerformClick();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            try
            {
                if (bw.IsBusy)
                {
                    if (btnPause.Text == @"Pause")
                    {
                        btnPause.Text = @"Resume";
                        Mre.Reset();
                    }
                    else
                    {
                        btnPause.Text = @"Pause";
                        Mre.Set();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error: {ex}", ex);
            }
        }
    }
}
