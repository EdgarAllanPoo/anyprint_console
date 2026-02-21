using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class GradientButton : Button
{
  public Color Color1 { get; set; }
  public Color Color2 { get; set; }

  public GradientButton()
  {
      this.FlatStyle = FlatStyle.Flat;
      this.FlatAppearance.BorderSize = 0;
      this.BackColor = Color.Transparent;
      this.ForeColor = Color.White;
  }

  protected override void OnPaint(PaintEventArgs pevent)
  {
    Graphics g = pevent.Graphics;
    g.SmoothingMode = SmoothingMode.AntiAlias;

    // Draw gradient background
    using (LinearGradientBrush brush =
      new LinearGradientBrush(this.ClientRectangle, Color1, Color2, 0f))
    {
      g.FillRectangle(brush, this.ClientRectangle);
    }

    // Draw centered text
    TextRenderer.DrawText(
      g,
      this.Text,
      this.Font,
      this.ClientRectangle,
      this.ForeColor,
      TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

    // Disabled overlay
    if (!this.Enabled)
    {
      using (SolidBrush overlay =
        new SolidBrush(Color.FromArgb(120, Color.Black)))
      {
        g.FillRectangle(overlay, this.ClientRectangle);
      }
    }
  }
}
