using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Threading.Tasks;
using System.Net;

namespace AnyPrintConsole
{
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
        }
    }

    public partial class Form1 : Form
    {
        private AnyPrintApiClient apiClient = new AnyPrintApiClient();

        private string filePath;
        private int copiesToPrint = 1;
        private string printMode = "BW";

        private GradientButton gradientGet;
        private GradientButton gradientPrint;

        private Panel loadingOverlay;
        private Label loadingLabel;
        private Timer spinnerTimer;
        private int spinnerAngle = 0;

        private Process onScreenKeyboardProc;

        private readonly string ghostscriptPath =
            @"C:\Program Files\gs\gs10.06.0\bin\gswin64c.exe";

        public Form1()
        {
            InitializeComponent();

            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);

            this.UpdateStyles();

            SetupUI();
        }

        // ======================================================
        // ====================== UI SETUP ======================
        // ======================================================

        private void SetupUI()
        {
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

            string bgPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "background.png");

            if (File.Exists(bgPath))
            {
                this.BackgroundImage = Image.FromFile(bgPath);
                this.BackgroundImageLayout = ImageLayout.Stretch;
            }

            titleLabel.Visible = false;
            btnGetFile.Visible = false;
            btnPrint.Visible = false;

            codeLabel.ForeColor = Color.White;
            fileLabel.ForeColor = Color.White;
            statusLabel.ForeColor = Color.White;

            Panel mainPanel = new Panel
            {
                Size = new Size(1000, 750),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.None
            };

            this.Controls.Add(mainPanel);

            this.Resize += (s, e) =>
            {
                mainPanel.Left = (this.ClientSize.Width - mainPanel.Width) / 2;
                mainPanel.Top = (this.ClientSize.Height - mainPanel.Height) / 2;
            };

            mainPanel.Left = (this.ClientSize.Width - mainPanel.Width) / 2;
            mainPanel.Top = (this.ClientSize.Height - mainPanel.Height) / 2;

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 8,
                BackColor = Color.Transparent,
                Padding = new Padding(40, 30, 40, 30)
            };

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 22f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 8f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 15f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 15f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 8f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 15f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 15f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 5f));

            mainPanel.Controls.Add(layout);

            PictureBox logo = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Margin = new Padding(0, 0, 0, 30)
            };

            string logoPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "logo.png");

            if (File.Exists(logoPath))
                logo.Image = Image.FromFile(logoPath);

            codeLabel.Font = new Font("Segoe UI", 24F);
            textBoxCode.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            fileLabel.Font = new Font("Segoe UI", 24F);
            textBoxFile.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            statusLabel.Font = new Font("Segoe UI", 16F);

            textBoxCode.TextAlign = HorizontalAlignment.Center;

            int controlWidth = 750;

            Panel Wrap(Control ctrl)
            {
                Panel wrapper = new Panel { Dock = DockStyle.Fill };
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

            gradientGet = new GradientButton
            {
                Text = "GET FILE",
                Height = 80,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                Color1 = Color.FromArgb(255, 120, 80),
                Color2 = Color.FromArgb(30, 60, 120)
            };
            gradientGet.Click += button1_Click;

            gradientPrint = new GradientButton
            {
                Text = "PRINT",
                Height = 80,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                Color1 = Color.FromArgb(255, 120, 80),
                Color2 = Color.FromArgb(30, 60, 120),
                Enabled = false
            };
            gradientPrint.Click += button2_Click;

            textBoxFile.ReadOnly = true;
            textBoxFile.TabStop = false;           // Prevent tab focus
            textBoxFile.Cursor = Cursors.Default;  // Remove I-beam
            textBoxFile.BackColor = Color.FromArgb(235, 235, 235);
            textBoxFile.ForeColor = Color.Gray;

            // Prevent mouse focus
            textBoxFile.GotFocus += (s, e) =>
            {
                this.ActiveControl = null;
            };

            layout.Controls.Add(logo, 0, 0);
            layout.Controls.Add(Wrap(codeLabel), 0, 1);
            layout.Controls.Add(Wrap(textBoxCode), 0, 2);
            layout.Controls.Add(Wrap(gradientGet), 0, 3);
            layout.Controls.Add(Wrap(fileLabel), 0, 4);
            layout.Controls.Add(Wrap(textBoxFile), 0, 5);
            layout.Controls.Add(Wrap(gradientPrint), 0, 6);
            layout.Controls.Add(Wrap(statusLabel), 0, 7);

            textBoxCode.Enter += TextBoxCode_Enter;
            textBoxCode.Leave += TextBoxCode_Leave;

            InitializeLoadingOverlay();
        }

        // ======================================================
        // ================= LOADING OVERLAY ====================
        // ======================================================

        private void InitializeLoadingOverlay()
        {
            loadingOverlay = new DoubleBufferedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(160, 0, 0, 0),
                Visible = false
            };

            loadingLabel = new Label
            {
                AutoSize = false,
                Width = 400,
                Height = 50,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            loadingOverlay.Controls.Add(loadingLabel);
            this.Controls.Add(loadingOverlay);

            spinnerTimer = new Timer { Interval = 30 };
            spinnerTimer.Tick += (s, e) =>
            {
                spinnerAngle += 8;
                loadingOverlay.Invalidate();
            };

            loadingOverlay.Paint += DrawSpinner;
        }

        private void DrawSpinner(object sender, PaintEventArgs e)
        {
            if (!loadingOverlay.Visible) return;

            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int size = 60;
            int centerX = loadingOverlay.Width / 2;
            int centerY = loadingOverlay.Height / 2 - 40;

            Rectangle rect = new Rectangle(
                centerX - size / 2,
                centerY - size / 2,
                size,
                size);

            using (Pen pen = new Pen(Color.White, 6))
            {
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                g.DrawArc(pen, rect, spinnerAngle, 270);
            }

            loadingLabel.Left = (loadingOverlay.Width - loadingLabel.Width) / 2;
            loadingLabel.Top = centerY + 50;
        }

        private void ShowLoading(string message)
        {
            loadingLabel.Text = message;
            loadingOverlay.Visible = true;
            loadingOverlay.BringToFront();
            spinnerTimer.Start();
        }

        private void HideLoading()
        {
            spinnerTimer.Stop();

            this.SuspendLayout();
            loadingOverlay.Visible = false;
            this.ResumeLayout(true);

            this.Refresh();
        }

        private void SetStatus(string message, Color color)
        {
            statusLabel.Text = message;
            statusLabel.ForeColor = color;
        }

        // ======================================================
        // ================= DOWNLOAD ============================
        // ======================================================

        private async void button1_Click(object sender, EventArgs e)
        {
            string code = textBoxCode.Text.Trim();

            if (code.Length != 8 || !long.TryParse(code, out _))
            {
                SetStatus("Status: Invalid code", Color.Red);
                return;
            }

            try
            {
                ShowLoading("Downloading...");
                SetStatus("Status: Downloading...", Color.Gold);

                var job = await apiClient.GetJobAsync(code);

                filePath = await apiClient.DownloadFileAsync(
                    job.fileUrl,
                    @"C:\AnyPrintFolder\FilesToPrint");

                copiesToPrint = job.copies;
                printMode = string.IsNullOrEmpty(job.printMode)
                    ? "BW"
                    : job.printMode;

                textBoxFile.Text =
                    job.filename +
                    $"  (Copies: {job.copies}, Mode: {job.printMode})";

                textBoxFile.BackColor = Color.White;
                textBoxFile.ForeColor = Color.Black;

                gradientPrint.Enabled = true;

                SetStatus("Status: Ready to print", Color.LimeGreen);
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse response)
            {
                string message;

                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:        // 404
                        message = "Code not found.";
                        break;

                    case HttpStatusCode.Conflict:        // 409
                        message = "Code already used.";
                        break;

                    default:
                        message = "Network error.";
                        break;
                }

                MessageBox.Show(message, "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                SetStatus($"Status: {message}", Color.Red);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected error:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                SetStatus("Status: Unexpected error", Color.Red);
            }
            finally
            {
                HideLoading();
            }
        }

        // ======================================================
        // ================= PRINT ===============================
        // ======================================================

        private async void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            try
            {
                ShowLoading("Sending to printer...");
                SetStatus("Status: Printing...", Color.Gold);

                var printTask = Task.Run(() =>
                {
                    PrintWithGhostscript(filePath, copiesToPrint, printMode);
                });

                // 30 second safety timeout
                if (await Task.WhenAny(printTask, Task.Delay(60000)) != printTask)
                {
                    throw new Exception("Printing timed out.");
                }

                // Await again to rethrow internal exceptions
                await printTask;

                this.Activate();
                this.BringToFront();

                SetStatus("Status: Print sent", Color.LimeGreen);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Print failed:\n\n" + ex.Message);
                SetStatus("Status: Print failed", Color.Red);
            }
            finally
            {
                HideLoading();

                // Reset UI state
                textBoxCode.Text = "";
                textBoxFile.Text = "";
                textBoxFile.BackColor = Color.FromArgb(235, 235, 235);
                textBoxFile.ForeColor = Color.Gray;

                gradientPrint.Enabled = false;

                filePath = null;
            }
        }

        // ======================================================
        // ================= GHOSTSCRIPT =========================
        // ======================================================

        private void PrintWithGhostscript(string pdfPath, int copies, string printMode)
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
                throw new Exception($"Printer '{printerName}' not found.");

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
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (Process p = Process.Start(psi))
                {
                    string error = p.StandardError.ReadToEnd();
                    p.WaitForExit();

                    if (p.ExitCode != 0)
                        throw new Exception(error);
                }
            }
        }

        // ======================================================
        // ================= KEYBOARD ============================
        // ======================================================

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
                    onScreenKeyboardProc = Process.Start(tabTipPath);
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
