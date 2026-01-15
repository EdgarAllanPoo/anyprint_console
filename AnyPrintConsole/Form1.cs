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

        private string filePath;
        private int copiesToPrint = 1;

        private PictureBox logo;
        private Process onScreenKeyboardProc;

        // Update this path if Ghostscript version changes
        private readonly string ghostscriptPath = @"C:\Program Files\gs\gs10.06.0\bin\gswin64c.exe";

        public Form1()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            // Window style
            this.Text = "AnyPrint POS";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1100, 720);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = true;
            this.BackColor = Color.White;

            // Hide text title — logo replaces it
            titleLabel.Visible = false;

            // Create logo
            logo = new PictureBox();
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Anyprint.png");

            if (File.Exists(logoPath))
                logo.Image = Image.FromFile(logoPath);

            logo.SizeMode = PictureBoxSizeMode.Zoom;
            logo.Width = 500;
            logo.Height = 140;
            logo.Left = (this.ClientSize.Width - logo.Width) / 2;
            logo.Top = 20;

            this.Controls.Add(logo);

            // Layout controls
            LayoutControls();

            // Keyboard control
            textBoxCode.Enter += TextBoxCode_Enter;
            textBoxCode.Leave += TextBoxCode_Leave;

            // Colors
            codeLabel.ForeColor = Color.Black;
            fileLabel.ForeColor = Color.Black;
            statusLabel.ForeColor = Color.DimGray;

            // Button styling
            btnGetFile.BackColor = Color.FromArgb(0, 120, 215);
            btnGetFile.ForeColor = Color.White;
            btnGetFile.FlatStyle = FlatStyle.Flat;
            btnGetFile.FlatAppearance.BorderSize = 0;

            btnPrint.BackColor = Color.FromArgb(0, 180, 120);
            btnPrint.ForeColor = Color.White;
            btnPrint.FlatStyle = FlatStyle.Flat;
            btnPrint.FlatAppearance.BorderSize = 0;
        }

        private void LayoutControls()
        {
            int centerX = (this.ClientSize.Width - textBoxCode.Width) / 2;
            int baseTop = logo.Bottom + 30;

            codeLabel.Left = centerX;
            codeLabel.Top = baseTop;

            textBoxCode.Left = centerX;
            textBoxCode.Top = baseTop + 35;
            textBoxCode.Font = new Font("Segoe UI", 24, FontStyle.Bold);

            btnGetFile.Left = centerX;
            btnGetFile.Top = baseTop + 100;
            btnGetFile.Font = new Font("Segoe UI", 20, FontStyle.Bold);

            fileLabel.Left = centerX;
            fileLabel.Top = baseTop + 180;

            textBoxFile.Left = centerX;
            textBoxFile.Top = baseTop + 210;
            textBoxFile.Font = new Font("Segoe UI", 14);

            btnPrint.Left = centerX;
            btnPrint.Top = baseTop + 265;
            btnPrint.Font = new Font("Segoe UI", 20, FontStyle.Bold);

            statusLabel.Left = centerX;
            statusLabel.Top = baseTop + 330;
            statusLabel.Font = new Font("Segoe UI", 14);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            statusLabel.Text = "Status: Downloading...";
            textBoxFile.Text = "";
            filePath = null;

            string code = textBoxCode.Text.Trim();

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
                MessageBox.Show("Network error:\n\n" + ex.Message);
                statusLabel.Text = "Status: Network error";
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

        private void TextBoxCode_Enter(object sender, EventArgs e)
        {
            ShowKeyboard();
        }

        private void TextBoxCode_Leave(object sender, EventArgs e)
        {
            HideKeyboard();
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
                    RedirectStandardError = true
                };

                using (Process p = Process.Start(psi))
                {
                    p.WaitForExit();
                }
            }
        }

        private void ShowKeyboard()
        {
            try
            {
                // Kill existing keyboard if already open
                foreach (var proc in Process.GetProcessesByName("TabTip"))
                    proc.Kill();

                string tabTipPath = @"C:\Program Files\Common Files\microsoft shared\ink\TabTip.exe";

                if (File.Exists(tabTipPath))
                {
                    onScreenKeyboardProc = Process.Start(tabTipPath);
                }
            }
            catch { }
        }

        private void HideKeyboard()
        {
            try
            {
                foreach (var proc in Process.GetProcessesByName("TabTip"))
                    proc.Kill();
            }
            catch { }
        }
    }
}
