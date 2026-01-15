using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Spire.Pdf;

namespace AnyPrintConsole
{
    public partial class Form1 : Form
    {
        private AnyPrintApiClient apiClient = new AnyPrintApiClient();

        private Process onScreenKeyboardproc;
        private string ffile;
        private string filePath;
        private int copiesToPrint = 1;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            BackColor = Color.FromArgb(230, 226, 223);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            statusLabel.Text = "Status: Downloading...";
            textBoxFile.Text = "";
            ffile = null;

            string code = textBoxCode.Text.Trim();

            // Enforce 8-digit numeric code
            if (code.Length != 8 || !long.TryParse(code, out _))
            {
                MessageBox.Show("Invalid code. Code must be 8 digits.");
                statusLabel.Text = "Status: Invalid code";
                return;
            }

            try
            {
                var job = apiClient.GetJob(code);

                string folder = @"C:\AnyPrintFolder\FilesToPrint";
                Directory.CreateDirectory(folder);

                string localFilePath = apiClient.DownloadFile(job.fileUrl, folder);

                ffile = Path.GetFileName(localFilePath);
                filePath = localFilePath;
                copiesToPrint = job.copies;

                textBoxFile.Text = job.filename + $"  (Copies: {job.copies})";
                statusLabel.Text = "Status: Ready to print";
            }
            catch (Exception ex)
            {
                if (ex.Message == "JOB_NOT_PAID")
                {
                    MessageBox.Show("Job not found or not paid yet.");
                    statusLabel.Text = "Status: Job not paid";
                }
                else
                {
                    MessageBox.Show("Network error:\n\n" + ex.ToString());
                    statusLabel.Text = "Status: Network error";
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ffile))
            {
                MessageBox.Show("No file loaded");
                return;
            }

            statusLabel.Text = "Status: Printing...";

            try
            {
                PdfDocument pdf = new PdfDocument();
                pdf.LoadFromFile(@"C:\AnyPrintFolder\FilesToPrint\" + ffile);

                var settings = new Spire.Pdf.Print.PrintSettings();
                settings.Copies = (short)copiesToPrint;

                pdf.Print(settings);

                statusLabel.Text = $"Status: Printing {copiesToPrint} copies";

                if (File.Exists(filePath))
                    File.Delete(filePath);

                ffile = null;
                textBoxCode.Text = "";
                textBoxFile.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Print failed: " + ex.Message);
                statusLabel.Text = "Status: Print failed";
            }
        }

        private void CallKeyboard()
        {
            foreach (Process p in Process.GetProcessesByName("TabTip"))
            {
                p.Kill();
            }

            string progFiles = @"C:\Program Files\Common Files\microsoft shared\ink";
            string onScreenKeyboardPath = Path.Combine(progFiles, "TabTip.exe");
            onScreenKeyboardproc = Process.Start(onScreenKeyboardPath);
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            CallKeyboard();
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            foreach (Process p in Process.GetProcessesByName("TabTip"))
            {
                p.Kill();
            }
        }
    }
}
