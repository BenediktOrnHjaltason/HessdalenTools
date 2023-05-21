using static System.Net.Mime.MediaTypeNames;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using System.IO;

namespace HessdalenTools
{
    public partial class Form1 : Form
    {
        private readonly Dictionary<string, string> accessEntityToLetter =
                new Dictionary<string, string>() { { "&AElig;", "Æ" },
                                                    {"&aelig;", "æ" },
                                                    {"&Oslash;", "Ø" },
                                                    {"&oslash;", "ø" },
                                                    {"&Aring;", "Å" },
                                                    {"&aring;", "å" }};

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            

            var extensions = new List<string>() { "htm", "html" };

            var filePathsEN = Directory
                .EnumerateFiles("C:\\Users\\Admin\\Desktop\\Observations\\EN", "*.*", SearchOption.AllDirectories)
                .Where(s => extensions.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()) && !s.Contains("Filer_for"));

            foreach(var path in filePathsEN) { Debug.WriteLine(path); }


            var filePathsNO = Directory
                .EnumerateFiles("C:\\Users\\Admin\\Desktop\\Observations\\NO", "*.*", SearchOption.AllDirectories)
                .Where(s => extensions.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()) && s.Contains("index"));

            foreach (var path in filePathsNO) { Debug.WriteLine(path); }

            List<Report> norwegianReports = new List<Report>();
            List<Report> englishReports = new List<Report>();

            for (int i = 1976; i < 2024; i++)
            {
                StreamReader srEN = null;
                StreamReader srNO = null;

                foreach(var path in filePathsEN)
                {
                    if (path.Contains(i.ToString()))
                    {
                        srEN = new StreamReader(path);
                    }
                }

                foreach (var path in filePathsNO) 
                {  
                    if (path.Contains(i.ToString())) 
                    { 
                        srNO = new StreamReader(path); 
                    } 
                }

                if (srEN != null && srNO != null)
                {
                    StreamReader en = new StreamReader(filePathsEN.Where(x => x.Contains(i.ToString())).FirstOrDefault());

                    StreamReader no = new StreamReader(filePathsNO.Where(x => x.Contains(i.ToString())).FirstOrDefault());

                    //English


                    ScrapeReports(en, englishReports, "Date");
                    ScrapeReports(no, norwegianReports, "Dato");
                }
            }

            List<Report> mergedReports = MergeReports(englishReports, norwegianReports);

            foreach (var sampledReport in englishReports)
            {
                Debug.WriteLine($"Date: {sampledReport.Date}");
                Debug.WriteLine($"Time: {sampledReport.Time}");
                Debug.WriteLine($"Place name: {sampledReport.ObserverPlaceName}");
                Debug.WriteLine($"Observer name: {sampledReport.ObserverName}");
                Debug.WriteLine($"Description EN: {sampledReport.Description_EN}");
                Debug.WriteLine($"Description NO: {sampledReport.Description_NO}");
            }
        }

        private void ScrapeReports(StreamReader sr, List<Report> reports, string reportStartIdentifier)
        {
            string lineSampler = "";

            while (!sr.EndOfStream)
            {
                lineSampler = sr.ReadLine();

                if (lineSampler.Contains(reportStartIdentifier))
                {
                    Report report = new Report();

                    int openTags = 0;
                    int closedTags = 0;

                    for (int j = 0; j < lineSampler.Length; j++)
                    {
                        if (lineSampler[j] == '<')
                        {
                            if (lineSampler[j + 1] != 'i')
                                openTags++;
                        }
                        else if (lineSampler[j] == '>')
                        {
                            if (lineSampler[j - 1] != 'i')
                                closedTags++;
                        }

                        else if (openTags == 2 && closedTags == 2)
                        {
                            report.Date += lineSampler[j];
                        }

                        else if (openTags == 4 && closedTags == 4)
                        {
                            report.Time += lineSampler[j];
                        }

                        else if (openTags == 6 && closedTags == 6)
                        {
                            report.ObserverPlaceName += lineSampler[j];
                        }
                    }

                    lineSampler = sr.ReadLine();
                    openTags = 0;
                    closedTags = 0;

                    for (int j = 0; j < lineSampler.Length; j++)
                    {
                        if (lineSampler[j] == '<') { openTags++; }
                        else if (lineSampler[j] == '>') { closedTags++; }

                        if (openTags == 2 && closedTags == 2)
                        {
                            report.ObserverName += lineSampler[j];
                        }
                    }

                    //Scrape description:

                    bool doneScrapingDescription = false;

                    while (!doneScrapingDescription)
                    {
                        lineSampler = sr.ReadLine();

                        if (lineSampler == null || lineSampler.Contains("</p>") || lineSampler.Contains("<p>"))
                        {
                            doneScrapingDescription = true;
                        }
                        else
                        {
                            if (reportStartIdentifier == "Date")
                                report.Description_EN += lineSampler;

                            else if (reportStartIdentifier == "Dato")
                                report.Description_NO += lineSampler;
                        }
                    }

                    reports.Add(report);
                }
            }

            CleanReports(reports);
        }

        private void CleanReports(List<Report> reports)
        {
            foreach (Report report in reports) 
            {
                if (report.Date != null)
                {
                    report.Date.TrimStart();
                    report.Date.TrimEnd();
                }

                if (report.Time != null)
                {
                    report.Time.TrimStart();
                    report.Time.TrimEnd();
                }

                if (report.ObserverName != null)
                {
                    report.ObserverName.TrimStart();
                    report.ObserverName.TrimEnd();
                }

                if (report.ObserverPlaceName != null)
                {
                    report.ObserverPlaceName.TrimStart();
                    report.ObserverPlaceName.TrimEnd();
                }
                

                if (report.Description_NO != null)
                {
                    report.Description_NO.TrimStart();
                    report.Description_NO.TrimEnd();
                }

                if (report.Description_EN != null)
                {
                    report.Description_EN.TrimStart();
                    report.Description_EN.TrimEnd();
                }
            }
        }

        private List<Report> MergeReports(List<Report> englishReports, List<Report> norwegianReports)
        {
            foreach(var report in englishReports) 
            {
                Debug.WriteLine($"EN Dates: {report.Date}");
            }
            foreach (var report in norwegianReports)
            {
                Debug.WriteLine($"NO Observer: {report.Date}");
            }



            List<Report> result = new List<Report>();

            Debug.WriteLine($"Norwegian reports: {norwegianReports.Count}. English reports: {englishReports.Count}");

            int mergedReportsCount = 0;
            for(int i = 0; i < norwegianReports.Count; i++) 
            {
                var englishMatches = englishReports.Where(x => x.Date == norwegianReports[i].Date).ToList();

                if (englishMatches.Count() == 1)
                {
                    mergedReportsCount++;
                    result.Add(new Report()
                    {
                        Date = norwegianReports[i].Date,
                        Time = norwegianReports[i].Time,
                        ObserverName = norwegianReports[i].ObserverName,
                        ObserverPlaceName = norwegianReports[i].ObserverPlaceName,
                        Description_EN = englishMatches[0].Description_EN,
                        Description_NO = norwegianReports[i].Description_NO,
                    });
                }
            }

            var serialized = JsonConvert.SerializeObject(result, Formatting.Indented);

            File.WriteAllText("C:\\Users\\Admin\\Desktop\\ObservationMatching\\Matches.JSON", serialized);

            Debug.WriteLine($"Merged unequivocal results: {mergedReportsCount}");

            return result;
        }
    }

    public class Report
    {
        public string Date;
        public string Time;
        public string Description_NO;
        public string Description_EN;
        public string ObserverName;
        public string ObserverPlaceName;
    }
}