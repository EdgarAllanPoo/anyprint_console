using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class GradientButton : Button
{
    public Color Color1 { get; set; }
    public Color Color2 { get; set; }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        using (LinearGradientBrush brush =
            new LinearGradientBrush(this.ClientRectangle, Color1, Color2, 0f))
        {
            pevent.Graphics.FillRectangle(brush, this.ClientRectangle);
        }

        TextRenderer.DrawText(
            pevent.Graphics,
            this.Text,
            this.Font,
            this.ClientRectangle,
            this.ForeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}
