using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Printing;

namespace AnyPrintConsole
{
    public partial class Form1 : Form
    {
        private AnyPrintApiClient apiClient = new AnyPrintApiClient();

        private string filePath;
        private int copiesToPrint = 1;
        private string printMode = "BW";

        private Process onScreenKeyboardProc;
        private readonly string ghostscriptPath =
            @"C:\Program Files\gs\gs10.06.0\bin\gswin64c.exe";

        public Form1()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            // ===== FORM STYLE =====
            this.Text = "";
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.Black;
            this.KeyPreview = true;

            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                    this.Close();
            };

            // ===== BACKGROUND =====
            string bgPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "background.png");

            if (File.Exists(bgPath))
            {
                this.BackgroundImage = Image.FromFile(bgPath);
                this.BackgroundImageLayout = ImageLayout.Stretch;
            }

            // Hide old designer buttons/title
            titleLabel.Visible = false;
            btnGetFile.Visible = false;
            btnPrint.Visible = false;

            codeLabel.ForeColor = Color.White;
            fileLabel.ForeColor = Color.White;
            statusLabel.ForeColor = Color.White;

            // ===== MAIN CENTER PANEL =====
            Panel mainPanel = new Panel();
            mainPanel.Size = new Size(750, 600);
            mainPanel.BackColor = Color.Transparent;
            mainPanel.Anchor = AnchorStyles.None;

            this.Controls.Add(mainPanel);

            this.Resize += (s, e) =>
            {
                mainPanel.Left = (this.ClientSize.Width - mainPanel.Width) / 2;
                mainPanel.Top = (this.ClientSize.Height - mainPanel.Height) / 2;
            };

            mainPanel.Left = (this.ClientSize.Width - mainPanel.Width) / 2;
            mainPanel.Top = (this.ClientSize.Height - mainPanel.Height) / 2;

            // ===== TABLE LAYOUT =====
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 1;
            layout.RowCount = 8;
            layout.BackColor = Color.Transparent;
            layout.Padding = new Padding(20);

            for (int i = 0; i < 8; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5f));

            mainPanel.Controls.Add(layout);

            // ===== LOGO =====
            PictureBox logo = new PictureBox();
            logo.Height = 120;
            logo.Dock = DockStyle.Fill;
            logo.SizeMode = PictureBoxSizeMode.Zoom;
            logo.BackColor = Color.Transparent;
            logo.Margin = new Padding(0, 0, 0, 20);

            string logoPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "logo.png");

            if (File.Exists(logoPath))
                logo.Image = Image.FromFile(logoPath);

            // ===== FONTS =====
            codeLabel.Font = new Font("Segoe UI", 22F);
            textBoxCode.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            fileLabel.Font = new Font("Segoe UI", 20F);
            textBoxFile.Font = new Font("Segoe UI", 16F);
            statusLabel.Font = new Font("Segoe UI", 16F);

            textBoxCode.Width = 650;
            textBoxFile.Width = 650;
            textBoxCode.TextAlign = HorizontalAlignment.Center;

            // ===== GRADIENT BUTTONS =====
            GradientButton gradientGet = new GradientButton
            {
                Text = "GET FILE",
                Height = 70,
                Width = 650,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                Color1 = Color.FromArgb(255, 120, 80),
                Color2 = Color.FromArgb(30, 60, 120)
            };
            gradientGet.Click += button1_Click;

            GradientButton gradientPrint = new GradientButton
            {
                Text = "PRINT",
                Height = 70,
                Width = 650,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                Color1 = Color.FromArgb(255, 120, 80),
                Color2 = Color.FromArgb(30, 60, 120)
            };
            gradientPrint.Click += button2_Click;

            // ===== ADD CONTROLS IN ORDER =====
            layout.Controls.Add(logo);
            layout.Controls.Add(codeLabel);
            layout.Controls.Add(textBoxCode);
            layout.Controls.Add(gradientGet);
            layout.Controls.Add(fileLabel);
            layout.Controls.Add(textBoxFile);
            layout.Controls.Add(gradientPrint);
            layout.Controls.Add(statusLabel);

            // ===== KEYBOARD EVENTS =====
            textBoxCode.Enter += TextBoxCode_Enter;
            textBoxCode.Leave += TextBoxCode_Leave;
        }

        // ================= ORIGINAL LOGIC BELOW =================

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
                printMode = string.IsNullOrEmpty(job.printMode)
                    ? "BW"
                    : job.printMode;

                textBoxFile.Text =
                    job.filename +
                    $"  (Copies: {job.copies}, Mode: {job.printMode})";

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

            statusLabel.Text =
                $"Status: Printing {copiesToPrint} copies...";

            try
            {
                PrintWithGhostscript(filePath,
                    copiesToPrint,
                    printMode);

                statusLabel.Text =
                    $"Status: Print sent ({copiesToPrint} copies, {printMode})";

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

        private void PrintWithGhostscript(string pdfPath,
            int copies,
            string printMode)
        {
            if (!File.Exists(ghostscriptPath))
                throw new Exception("Ghostscript not found.");

            string printerName =
                printMode == "BW"
                ? "Anyprint BW"
                : "Anyprint Color";

            bool printerExists =
                PrinterSettings.InstalledPrinters
                .Cast<string>()
                .Any(p => p == printerName);

            if (!printerExists)
                throw new Exception(
                    $"Printer '{printerName}' not found.");

            for (int i = 0; i < copies; i++)
            {
                string printArgs =
                    "-dPrinted -dBATCH -dNOPAUSE " +
                    "-sDEVICE=mswinpr2 " +
                    $"-sOutputFile=\"%printer%{printerName}\" " +
                    $"\"{pdfPath}\"";

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = ghostscriptPath,
                    Arguments = printArgs,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true
                };

                using (Process p = Process.Start(psi))
                {
                    string error =
                        p.StandardError.ReadToEnd();
                    p.WaitForExit();

                    if (p.ExitCode != 0)
                        throw new Exception(error);
                }
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

        private void ShowKeyboard()
        {
            try
            {
                foreach (var proc in Process.GetProcessesByName("TabTip"))
                    proc.Kill();

                string tabTipPath =
                    @"C:\Program Files\Common Files\microsoft shared\ink\TabTip.exe";

                if (File.Exists(tabTipPath))
                    onScreenKeyboardProc =
                        Process.Start(tabTipPath);
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
