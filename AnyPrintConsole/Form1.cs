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
            textBox4.Text = "";
            ffile = null;

            string code = textBox2.Text.Trim();

            if (code.Length < 6)
            {
                MessageBox.Show("Invalid code");
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

                textBox4.Text = job.filename;
                textBox4.Select();
            }
            catch (WebException)
            {
                MessageBox.Show("Job not found or not paid yet.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to download file: " + ex.Message);
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
            if (textBox4.Text != "")
            {
                PdfDocument pdf = new PdfDocument();
                pdf.LoadFromFile(@"C:\AnyPrintFolder\FilesToPrint\" + ffile);
                pdf.Print();

                textBox2.Text = "";
                textBox4.Text = "";

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                ffile = null;
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
