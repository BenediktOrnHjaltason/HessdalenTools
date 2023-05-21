using static System.Net.Mime.MediaTypeNames;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;
using System.Diagnostics;
using System.Linq;

namespace HessdalenTools
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Dictionary<string,string> accessEntityToLetter = 
                new Dictionary<string, string>() { { "&AElig;", "Æ" },
                                                    {"&aelig;", "æ" },
                                                    {"&Oslash;", "Ø" },
                                                    {"&oslash;", "ø" },
                                                    {"&Aring;", "Å" },
                                                    {"&aring;", "å" }};

            var extensions = new List<string>() { "htm", "html" };

            var filePathsEN = Directory
                .EnumerateFiles("C:\\Users\\Admin\\Desktop\\Observations\\EN", "*.*", SearchOption.AllDirectories)
                .Where(s => extensions.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()) && !s.Contains("Filer_for"));

            foreach(var path in filePathsEN) { Debug.WriteLine(path); }


            var filePathsNO = Directory
                .EnumerateFiles("C:\\Users\\Admin\\Desktop\\Observations\\NO", "*.*", SearchOption.AllDirectories)
                .Where(s => extensions.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()) && s.Contains("index"));

            foreach (var path in filePathsNO) { Debug.WriteLine(path); }

            List<Report> reports = new List<Report>();

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
                    Debug.WriteLine($"Found both paths for {i.ToString()}");

                    StreamReader en = new StreamReader(filePathsEN.Where(x => x.Contains(i.ToString())).FirstOrDefault());

                    StreamReader no = new StreamReader(filePathsNO.Where(x => x.Contains(i.ToString())).FirstOrDefault());

                    //English
                    string lineSampler = "";

                    while(!en.EndOfStream)
                    {
                        lineSampler = en.ReadLine();

                        if (lineSampler.Contains("Date"))
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

                                if (openTags == 2 && closedTags == 2)
                                {
                                    report.Date += lineSampler[j];
                                }

                                if (openTags == 4 && closedTags == 4)
                                {
                                    report.Time += lineSampler[j];
                                }

                                if (openTags == 6 && closedTags == 6)
                                {
                                    report.ObserverPlaceName += lineSampler[j];
                                }
                            }

                            lineSampler = en.ReadLine();
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
                                lineSampler = en.ReadLine();

                                if (lineSampler == null || lineSampler.Contains("</p>") || lineSampler.Contains("<p>"))
                                {
                                    doneScrapingDescription = true;
                                }
                                else
                                {
                                    report.Description_EN += lineSampler;
                                }
                            }

                            reports.Add(report);
                        }
                        
                    }
                }
            }

            foreach (var sampledReport in reports)
            {
                Debug.WriteLine($"Date: {sampledReport.Date}");
                Debug.WriteLine($"Time: {sampledReport.Time}");
                Debug.WriteLine($"Place name: {sampledReport.ObserverPlaceName}");
                Debug.WriteLine($"Observer name: {sampledReport.ObserverName}");
                Debug.WriteLine($"Description: {sampledReport.Description_EN}");
            }
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