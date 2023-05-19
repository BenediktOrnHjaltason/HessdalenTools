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
            var extensions = new List<string>() { "htm", "html" };

            var filePathsEN = Directory
                .EnumerateFiles("C:\\Users\\Admin\\Desktop\\Observations\\EN", "*.*", SearchOption.AllDirectories)
                .Where(s => extensions.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));

            Debug.WriteLine($"Found {filePathsEN.Count()} english htm files");

            var filePathsNO = Directory
                .EnumerateFiles("C:\\Users\\Admin\\Desktop\\Observations\\NO", "*.*", SearchOption.AllDirectories)
                .Where(s => extensions.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));

            Debug.WriteLine($"Found {filePathsNO.Count()} norwegian htm files");
        }
    }
}