using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.InteropServices;

public class NeonPopup : Form
{
    private string message;
    private System.Windows.Forms.Timer fadeTimer;
    private float opacityStep = 0.05f;

    public NeonPopup(string message)
    {
        this.message = message;
        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.Manual;
        this.TopMost = true;
        this.BackColor = Color.Black;
        this.TransparencyKey = Color.Black;
        this.Opacity = 1;
        this.ShowInTaskbar = false;

        // SIZE AND POSITION
        int width = 110;
        int height = 50;
        this.Size = new Size(width, height);
        var screen = Screen.PrimaryScreen!;
        var workingArea = screen.WorkingArea;
        this.Location = new Point(workingArea.Right - width - 10, workingArea.Bottom - height - 10);

        // FADE TIMER
        fadeTimer = new System.Windows.Forms.Timer();
        fadeTimer.Interval = 5; // MS
        fadeTimer.Tick += FadeTimer_Tick;

        this.Paint += NeonPopup_Paint;
    }

    protected override CreateParams CreateParams
    {
        get
        {
            // KEEP UNFOCUS
            const int WS_EX_TOOLWINDOW = 0x00000080;
            var cp = base.CreateParams;
            cp.ExStyle |= WS_EX_TOOLWINDOW;
            return cp;
        }
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        // POPUP DURATION
        var showTimer = new System.Windows.Forms.Timer();
        showTimer.Interval = 2000;
        showTimer.Tick += (s, args) =>
        {
            showTimer.Stop();
            fadeTimer.Start();
        };
        showTimer.Start();
    }

    private void FadeTimer_Tick(object? sender, EventArgs e)
    {
        this.Opacity -= opacityStep;
        if (this.Opacity <= 0)
        {
            fadeTimer.Stop();
            this.Close();
            this.Dispose();
        }
    }

    private void NeonPopup_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // FONT
        Font font = new Font("Segoe UI", 18, FontStyle.Bold);
        string text = message;
        PointF point = new PointF(10, 5);

        // COLOR
        Color neonGreen = Color.FromArgb(57, 255, 20);

        using (var path = new GraphicsPath())
        using (var pen = new Pen(Color.Black, 6) { LineJoin = LineJoin.Round })
        using (var brush = new SolidBrush(neonGreen))
        {
            path.AddString(text, font.FontFamily, (int)font.Style, g.DpiY * font.Size / 72, point, StringFormat.GenericDefault);
            g.DrawPath(pen, path);
            g.FillPath(brush, path);
        }
    }
}