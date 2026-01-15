using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Printing;

namespace AnyPrintConsole
{
    public partial class Form1 : Form
    {
        private AnyPrintApiClient apiClient = new AnyPrintApiClient();

        private Process onScreenKeyboardproc;
        private string filePath;
        private int copiesToPrint = 1;

        // Update this path if Ghostscript version changes
        private readonly string ghostscriptPath = @"C:\Program Files\gs\gs10.06.0\bin\gswin64c.exe";

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
            filePath = null;

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

                filePath = apiClient.DownloadFile(job.fileUrl, folder);
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
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("No file loaded");
                return;
            }

            statusLabel.Text = $"Status: Printing {copiesToPrint} copies...";

            try
            {
                PrintWithGhostscript(filePath, copiesToPrint);

                statusLabel.Text = $"Status: Print sent ({copiesToPrint} copies)";

                if (File.Exists(filePath))
                    File.Delete(filePath);

                filePath = null;
                textBoxCode.Text = "";
                textBoxFile.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Print failed: " + ex.Message);
                statusLabel.Text = "Status: Print failed";
            }
        }

        private void PrintWithGhostscript(string pdfPath, int copies)
        {
            if (!File.Exists(ghostscriptPath))
                throw new Exception("Ghostscript not found. Please install Ghostscript.");

            PrinterSettings printerSettings = new PrinterSettings();
            string printerName = printerSettings.PrinterName;

            for (int i = 0; i < copies; i++)
            {
                string args =
                    $"-dPrinted -dBATCH -dNOPAUSE -sDEVICE=mswinpr2 " +
                    $"-sOutputFile=\"%printer%{printerName}\" \"{pdfPath}\"";

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = ghostscriptPath,
                    Arguments = args,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                using (Process p = Process.Start(psi))
                {
                    p.WaitForExit();

                    if (p.ExitCode != 0)
                    {
                        string err = p.StandardError.ReadToEnd();
                        throw new Exception("Ghostscript error: " + err);
                    }
                }
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
