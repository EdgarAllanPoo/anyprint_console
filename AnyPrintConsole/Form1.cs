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

        // Update this path if Ghostscript version changes
        private readonly string ghostscriptPath = @"C:\Program Files\gs\gs10.06.0\bin\gswin64c.exe";

        public Form1()
        {
            this.AutoScaleMode = AutoScaleMode.None; 
            InitializeComponent();
            SetupUI();
            this.Shown += Form1_Shown;   // wait until fullscreen exists
        }

        private void SetupUI()
        {
            // Window style (keep X button)
            this.Text = "AnyPrint POS";
            this.WindowState = FormWindowState.Normal;
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
            logo.Top = 10;
            logo.Anchor = AnchorStyles.Top;

            this.Controls.Add(logo);

            // POS colors
            codeLabel.ForeColor = Color.Black;
            fileLabel.ForeColor = Color.Black;
            statusLabel.ForeColor = Color.DimGray;

            // Button styling
            btnGetFile.BackColor = Color.FromArgb(0, 120, 215); // Windows blue
            btnGetFile.ForeColor = Color.White;
            btnGetFile.FlatStyle = FlatStyle.Flat;
            btnGetFile.FlatAppearance.BorderSize = 0;

            btnPrint.BackColor = Color.FromArgb(0, 180, 120);  // Green print button
            btnPrint.ForeColor = Color.White;
            btnPrint.FlatStyle = FlatStyle.Flat;
            btnPrint.FlatAppearance.BorderSize = 0;

            // Textboxes
            textBoxCode.BackColor = Color.White;
            textBoxCode.ForeColor = Color.Black;

            textBoxFile.BackColor = Color.White;
            textBoxFile.ForeColor = Color.Black;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            ResizeLogo();
            LayoutControls();
        }

        private void ResizeLogo()
        {
            if (logo.Image == null) return;

            // Make logo wide and prominent
            int maxWidth = (int)(this.ClientSize.Width * 0.25);   // 75% of window width

            // Preserve real image ratio
            float aspectRatio = (float)logo.Image.Height / logo.Image.Width;
            int calculatedHeight = (int)(maxWidth * aspectRatio);

            logo.Width = maxWidth;
            logo.Height = calculatedHeight;

            // Center horizontally and keep tight top spacing
            logo.Left = (this.ClientSize.Width - logo.Width) / 2;
            logo.Top = 10;

            logo.Refresh();
        }

        private void LayoutControls()
        {
            int baseTop = logo.Bottom;   // tight spacing
            int centerX = (this.ClientSize.Width - textBoxCode.Width) / 2;

            codeLabel.Top = baseTop;
            codeLabel.Left = centerX;

            textBoxCode.Top = baseTop + 32;
            textBoxCode.Left = centerX;
            textBoxCode.Font = new Font("Segoe UI", 24, FontStyle.Bold);

            btnGetFile.Top = baseTop + 95;
            btnGetFile.Left = centerX;
            btnGetFile.Font = new Font("Segoe UI", 20, FontStyle.Bold);

            fileLabel.Top = baseTop + 170;
            fileLabel.Left = centerX;

            textBoxFile.Top = baseTop + 200;
            textBoxFile.Left = centerX;
            textBoxFile.Font = new Font("Segoe UI", 14);

            btnPrint.Top = baseTop + 250;
            btnPrint.Left = centerX;
            btnPrint.Font = new Font("Segoe UI", 20, FontStyle.Bold);

            statusLabel.Top = baseTop + 315;
            statusLabel.Left = centerX;
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
    }
}
