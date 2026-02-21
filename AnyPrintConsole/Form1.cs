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

        private GradientButton gradientGet;
        private GradientButton gradientPrint;

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
            this.TopMost = true;
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

            // Hide designer buttons
            titleLabel.Visible = false;
            btnGetFile.Visible = false;
            btnPrint.Visible = false;

            codeLabel.ForeColor = Color.White;
            fileLabel.ForeColor = Color.White;
            statusLabel.ForeColor = Color.White;

            // ===== MAIN CENTER PANEL =====
            Panel mainPanel = new Panel();
            mainPanel.Size = new Size(1000, 750);
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
            layout.Padding = new Padding(40, 30, 40, 30);
            layout.RowStyles.Clear();

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 22f)); // Logo
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 8f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 15f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 15f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 8f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 15f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 15f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 5f));

            mainPanel.Controls.Add(layout);

            // ===== LOGO =====
            PictureBox logo = new PictureBox();
            logo.Dock = DockStyle.Fill;
            logo.SizeMode = PictureBoxSizeMode.Zoom;
            logo.BackColor = Color.Transparent;
            logo.Margin = new Padding(0, 0, 0, 30);

            string logoPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "logo.png");

            if (File.Exists(logoPath))
                logo.Image = Image.FromFile(logoPath);

            // ===== FONTS =====
            codeLabel.Font = new Font("Segoe UI", 24F);
            textBoxCode.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            fileLabel.Font = new Font("Segoe UI", 24F);
            textBoxFile.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            statusLabel.Font = new Font("Segoe UI", 16F);

            textBoxCode.TextAlign = HorizontalAlignment.Center;

            // ===== FIXED WIDTH WRAPPER =====
            int controlWidth = 750;

            Panel WrapControl(Control ctrl)
            {
                Panel wrapper = new Panel();
                wrapper.Dock = DockStyle.Fill;
                wrapper.BackColor = Color.Transparent;

                ctrl.Width = controlWidth;
                ctrl.Anchor = AnchorStyles.None;

                wrapper.Resize += (s, e) =>
                {
                    ctrl.Left = (wrapper.Width - controlWidth) / 2;
                    ctrl.Top = (wrapper.Height - ctrl.Height) / 2;
                };

                wrapper.Controls.Add(ctrl);
                return wrapper;
            }

            Panel WrapLabel(Label ctrl)
            {
                Panel wrapper = new Panel();
                wrapper.Dock = DockStyle.Fill;
                wrapper.BackColor = Color.Transparent;

                ctrl.Width = controlWidth;
                ctrl.TextAlign = ContentAlignment.MiddleLeft;
                ctrl.Anchor = AnchorStyles.None;

                wrapper.Resize += (s, e) =>
                {
                    ctrl.Left = (wrapper.Width - controlWidth) / 2;
                    ctrl.Top = (wrapper.Height - ctrl.Height) / 2;
                };

                wrapper.Controls.Add(ctrl);
                return wrapper;
            }

            // ===== BUTTONS =====
            gradientGet = new GradientButton
            {
                Text = "GET FILE",
                Height = 80,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.White,
                Color1 = Color.FromArgb(255, 120, 80),
                Color2 = Color.FromArgb(30, 60, 120)
            };
            gradientGet.Click += button1_Click;

            gradientPrint = new GradientButton
            {
                Text = "PRINT",
                Height = 80,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.White,
                Color1 = Color.FromArgb(255, 120, 80),
                Color2 = Color.FromArgb(30, 60, 120),
                Enabled = false
            };
            gradientPrint.Click += button2_Click;

            textBoxFile.ReadOnly = true;
            textBoxFile.TabStop = false;          // Prevent tab focus
            textBoxFile.Cursor = Cursors.Default; // Remove I-beam cursor
            textBoxFile.BackColor = Color.FromArgb(235, 235, 235);
            textBoxFile.ForeColor = Color.Gray;

            // Prevent mouse focus
            textBoxFile.GotFocus += (s, e) =>
            {
                this.ActiveControl = null;
            };

            // ===== ADD CONTROLS =====
            layout.Controls.Add(logo, 0, 0);
            layout.Controls.Add(WrapLabel(codeLabel), 0, 1);
            layout.Controls.Add(WrapControl(textBoxCode), 0, 2);
            layout.Controls.Add(WrapControl(gradientGet), 0, 3);
            layout.Controls.Add(WrapLabel(fileLabel), 0, 4);
            layout.Controls.Add(WrapControl(textBoxFile), 0, 5);
            layout.Controls.Add(WrapControl(gradientPrint), 0, 6);
            layout.Controls.Add(WrapLabel(statusLabel), 0, 7);

            textBoxCode.Enter += TextBoxCode_Enter;
            textBoxCode.Leave += TextBoxCode_Leave;
        }

        private void SetStatus(string message, Color color)
        {
            statusLabel.Text = message;
            statusLabel.ForeColor = color;
        }

        // ================= ORIGINAL LOGIC BELOW =================

        private void button1_Click(object sender, EventArgs e)
        {
            SetStatus("Status: Downloading...", Color.Gold);
            textBoxFile.Text = "";
            filePath = null;

            string code = textBoxCode.Text.Trim();

            if (code.Length != 8 || !long.TryParse(code, out _))
            {
                MessageBox.Show("Invalid code. Code must be 8 digits.");
                SetStatus("Status: Invalid code", Color.Red);
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

                SetStatus("Status: Ready to print", Color.LimeGreen);

                textBoxFile.BackColor = Color.White;
                textBoxFile.ForeColor = Color.Black;

                gradientPrint.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Network error:\n\n" + ex.Message);
                SetStatus("Status: Network error", Color.Red);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("No file loaded");
                return;
            }

            SetStatus($"Status: Printing {copiesToPrint} copies...", Color.Gold);

            try
            {
                PrintWithGhostscript(filePath,
                    copiesToPrint,
                    printMode);
 
                // Force focus back
                this.TopMost = true;
                this.Activate();
                this.BringToFront();

                SetStatus(
                    $"Status: Print sent ({copiesToPrint} copies, {printMode})",
                    Color.LimeGreen);


                if (File.Exists(filePath))
                    File.Delete(filePath);

                filePath = null;
                textBoxCode.Text = "";
                textBoxFile.Text = "";

                textBoxFile.BackColor = Color.FromArgb(235, 235, 235);
                textBoxFile.ForeColor = Color.Gray;

                gradientPrint.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Print failed: " + ex.Message);
                SetStatus("Status: Print failed", Color.Red);
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
                    WindowStyle = ProcessWindowStyle.Hidden
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
