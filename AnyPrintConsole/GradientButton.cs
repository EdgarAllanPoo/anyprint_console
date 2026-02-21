using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class GradientButton : Button
{
    public Color Color1 { get; set; }
    public Color Color2 { get; set; }

    private bool isHovered = false;
    private bool isPressed = false;

    public GradientButton()
    {
        this.FlatStyle = FlatStyle.Flat;
        this.FlatAppearance.BorderSize = 0;
        this.BackColor = Color.Transparent;
        this.ForeColor = Color.White;
        this.DoubleBuffered = true;

        this.MouseEnter += (s, e) =>
        {
            isHovered = true;
            this.Invalidate();
        };

        this.MouseLeave += (s, e) =>
        {
            isHovered = false;
            isPressed = false;
            this.Invalidate();
        };

        this.MouseDown += (s, e) =>
        {
            isPressed = true;
            this.Invalidate();
        };

        this.MouseUp += (s, e) =>
        {
            isPressed = false;
            this.Invalidate();
        };
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        Graphics g = pevent.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        Rectangle rect = this.ClientRectangle;

        Color c1 = Color1;
        Color c2 = Color2;

        // Hover effect (slightly brighter)
        if (isHovered && this.Enabled)
        {
            c1 = ControlPaint.Light(Color1, 0.2f);
            c2 = ControlPaint.Light(Color2, 0.2f);
        }

        // Press effect (slightly darker)
        if (isPressed && this.Enabled)
        {
            c1 = ControlPaint.Dark(Color1, 0.2f);
            c2 = ControlPaint.Dark(Color2, 0.2f);
        }

        using (LinearGradientBrush brush =
            new LinearGradientBrush(rect, c1, c2, 0f))
        {
            g.FillRectangle(brush, rect);
        }

        // Draw text
        TextRenderer.DrawText(
            g,
            this.Text,
            this.Font,
            rect,
            this.ForeColor,
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter);

        // Disabled overlay
        if (!this.Enabled)
        {
            using (SolidBrush overlay =
                new SolidBrush(Color.FromArgb(120, Color.Black)))
            {
                g.FillRectangle(overlay, rect);
            }
        }
    }
}
