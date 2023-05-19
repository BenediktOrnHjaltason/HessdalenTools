using static System.Net.Mime.MediaTypeNames;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;
using System.Diagnostics;

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

            for (int i = 1976; i < 2024; i++)
            {
                if (filePathsEN.Contains(i.ToString()) && filePathsNO.Contains(i.ToString()))
                {
                    StreamReader en = new StreamReader(filePathsEN.Where(x => x.Contains(i.ToString())).FirstOrDefault());

                    StreamReader no = new StreamReader(filePathsNO.Where(x => x.Contains(i.ToString())).FirstOrDefault());


                }
            }
        }
    }
}