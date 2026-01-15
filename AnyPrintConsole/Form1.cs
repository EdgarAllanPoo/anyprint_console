using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Spire.Pdf;

namespace AnyPrintConsole
{
    public partial class Form1 : Form
    {
        private AnyPrintApiClient apiClient = new AnyPrintApiClient();

        private Process onScreenKeyboardproc;
        string ffile;
        int codelenght;
        string filePath;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            statusLabel.Text = "Status: Downloading...";
            textBoxFile.Text = "";
            ffile = null;

            string code = textBoxCode.Text.Trim();

            if (code.Length < 6)
            {
                MessageBox.Show("Invalid code");
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

                textBoxFile.Text = job.filename;
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
                    MessageBox.Show("Network error. Please check internet connection.");
                    statusLabel.Text = "Status: Network error";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to download file: " + ex.Message);
                statusLabel.Text = "Status: Download failed";
            }
        }


        private void CallKeyboard()
        {
            Process[] oslProcessesArry = Process.GetProcessesByName("TabTip");
            foreach (Process onScreenProcess in oslProcessesArry)
            {
                onScreenProcess.Kill();
            }

            string progFiles = @"C:\Program Files\Common Files\microsoft shared\ink";
            string onScreenKeyboardPath = Path.Combine(progFiles, "TabTip.exe");
            onScreenKeyboardproc = Process.Start(onScreenKeyboardPath);
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            CallKeyboard();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            BackColor = Color.FromArgb(230, 226, 223);
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
                pdf.Print();

                statusLabel.Text = "Status: Print sent";

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


        private void textBox2_Leave(object sender, EventArgs e)
        {
            Process[] oslProcessesArry = Process.GetProcessesByName("TabTip");
            foreach (Process onScreenProcess in oslProcessesArry)
            {
                onScreenProcess.Kill();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (Process onProcess in Process.GetProcessesByName("TabTip"))
            {
                onProcess.Kill();
            }
        }
    }
}
