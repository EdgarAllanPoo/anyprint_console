namespace AnyPrintConsole
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label codeLabel;
        private System.Windows.Forms.TextBox textBoxCode;
        private System.Windows.Forms.Button btnGetFile;
        private System.Windows.Forms.Label fileLabel;
        private System.Windows.Forms.TextBox textBoxFile;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.Label statusLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.titleLabel = new System.Windows.Forms.Label();
            this.codeLabel = new System.Windows.Forms.Label();
            this.textBoxCode = new System.Windows.Forms.TextBox();
            this.btnGetFile = new System.Windows.Forms.Button();
            this.fileLabel = new System.Windows.Forms.Label();
            this.textBoxFile = new System.Windows.Forms.TextBox();
            this.btnPrint = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // Form
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Text = "AnyPrint POS";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.BackColor = System.Drawing.Color.White;

            // Title
            this.titleLabel.Text = "ANYPRINT POS";
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 28F, System.Drawing.FontStyle.Bold);
            this.titleLabel.AutoSize = true;
            this.titleLabel.Location = new System.Drawing.Point(250, 30);

            // Code Label
            this.codeLabel.Text = "Enter Print Code:";
            this.codeLabel.Font = new System.Drawing.Font("Segoe UI", 16F);
            this.codeLabel.AutoSize = true;
            this.codeLabel.Location = new System.Drawing.Point(300, 110);

            // Code TextBox
            this.textBoxCode.Font = new System.Drawing.Font("Segoe UI", 24F);
            this.textBoxCode.Location = new System.Drawing.Point(200, 150);
            this.textBoxCode.Size = new System.Drawing.Size(400, 50);
            this.textBoxCode.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;

            // Get File Button
            this.btnGetFile.Text = "GET FILE";
            this.btnGetFile.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.btnGetFile.Size = new System.Drawing.Size(400, 60);
            this.btnGetFile.Location = new System.Drawing.Point(200, 220);
            this.btnGetFile.Click += new System.EventHandler(this.button1_Click);

            // File Label
            this.fileLabel.Text = "File:";
            this.fileLabel.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.fileLabel.AutoSize = true;
            this.fileLabel.Location = new System.Drawing.Point(200, 310);

            // File TextBox
            this.textBoxFile.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.textBoxFile.Location = new System.Drawing.Point(200, 340);
            this.textBoxFile.Size = new System.Drawing.Size(400, 32);
            this.textBoxFile.ReadOnly = true;

            // Print Button
            this.btnPrint.Text = "PRINT";
            this.btnPrint.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.btnPrint.Size = new System.Drawing.Size(400, 60);
            this.btnPrint.Location = new System.Drawing.Point(200, 390);
            this.btnPrint.Click += new System.EventHandler(this.button2_Click);

            // Status Label
            this.statusLabel.Text = "Status: Ready";
            this.statusLabel.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(200, 470);

            // Add Controls
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.codeLabel);
            this.Controls.Add(this.textBoxCode);
            this.Controls.Add(this.btnGetFile);
            this.Controls.Add(this.fileLabel);
            this.Controls.Add(this.textBoxFile);
            this.Controls.Add(this.btnPrint);
            this.Controls.Add(this.statusLabel);

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
