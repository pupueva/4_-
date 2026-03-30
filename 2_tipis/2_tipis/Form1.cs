using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace tipis2
{
    public partial class Form1 : Form
    {
        private static readonly Random rnd = new Random();

        private TabControl tabs;
        private TabPage tab1, tab2;
        private PictureBox pb1proc, pb1corr, pb2proc, pb2corr;
        private Button btnRun1, btnRun2;
        private Label lbl1Info, lbl2Info;
        private NumericUpDown nudN;

        private readonly Color C_BG = Color.FromArgb(18, 20, 35);
        private readonly Color C_PANEL = Color.FromArgb(24, 26, 44);
        private readonly Color C_BLUE = Color.FromArgb(60, 145, 255);
        private readonly Color C_ORNG = Color.FromArgb(255, 120, 50);
        private readonly Color C_GRN = Color.FromArgb(55, 205, 130);
        private readonly Color C_TEXT = Color.FromArgb(210, 215, 235);
        private readonly Color C_MUTED = Color.FromArgb(90, 95, 120);

        public Form1()
        {
            InitializeComponent();
            BuildUI();
            this.Resize += (s, e) => ArrangeLayout();
        }

        private void Form1_Load(object sender, EventArgs e) { }

        private void BuildUI()
        {
            this.Text = "ЛР №2 — Моделирование случайных процессов";
            this.Size = new Size(1200, 820);
            this.MinimumSize = new Size(900, 600);
            this.BackColor = C_BG;
            this.ForeColor = C_TEXT;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10f);

            var pnlTop = new Panel
            {
                BackColor = Color.FromArgb(20, 22, 40),
                Height = 52,
                Dock = DockStyle.Top
            };

            var lblN = new Label
            {
                Text = "Длина реализации N:",
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 9.5f),
                AutoSize = true,
                Top = 16,
                Left = 12
            };

            nudN = new NumericUpDown
            {
                Minimum = 100,
                Maximum = 10000,
                Value = 500,
                Increment = 100,
                ThousandsSeparator = true,
                Top = 12,
                Left = 192,
                Width = 100,
                BackColor = Color.FromArgb(28, 30, 52),
                ForeColor = C_TEXT,
                Font = new Font("Consolas", 9.5f)
            };

            btnRun1 = MakeTopBtn("Задание 1", 314, C_BLUE);
            btnRun2 = MakeTopBtn("Задание 2", 444, C_ORNG);
            btnRun1.Click += (s, e) => RunTask1();
            btnRun2.Click += (s, e) => RunTask2();

            pnlTop.Controls.AddRange(new Control[] { lblN, nudN, btnRun1, btnRun2 });

            tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                DrawMode = TabDrawMode.OwnerDrawFixed,
                Appearance = TabAppearance.FlatButtons,
                ItemSize = new Size(260, 32),
                SizeMode = TabSizeMode.Fixed
            };
            tabs.DrawItem += DrawTab;

            tab1 = new TabPage("  Задание 1 — Гауссовский процесс  ") { BackColor = C_PANEL };
            tab2 = new TabPage("  Задание 2 — Линейная система Y=X-bX(n-1)  ") { BackColor = C_PANEL };

            lbl1Info = MakeInfoLabel(); pb1proc = MakePb(); pb1corr = MakePb();
            tab1.Controls.AddRange(new Control[] { lbl1Info, pb1proc, pb1corr });

            lbl2Info = MakeInfoLabel(); pb2proc = MakePb(); pb2corr = MakePb();
            tab2.Controls.AddRange(new Control[] { lbl2Info, pb2proc, pb2corr });

            tabs.TabPages.AddRange(new TabPage[] { tab1, tab2 });

            this.Controls.Add(tabs);
            this.Controls.Add(pnlTop);

            ArrangeLayout();
        }

        private void ArrangeLayout()
        {
            foreach (TabPage tp in tabs.TabPages)
            {
                int tw = tp.ClientSize.Width - 16;
                int th = tp.ClientSize.Height;
                int h2 = (th - 50) / 2;
                if (h2 < 10) return;
                tp.Controls[0].SetBounds(8, 6, tw, 26);
                tp.Controls[1].SetBounds(8, 36, tw, h2);
                tp.Controls[2].SetBounds(8, 38 + h2, tw, h2);
            }
        }

        // ════════════════════════════════════════════════════════════
        //  ЗАДАНИЕ 1
        // ════════════════════════════════════════════════════════════
        private void RunTask1()
        {
            int N = (int)nudN.Value;
            double mu = 2.0;
            double D = 5.0;
            double rho = 0.95;
            double a = -Math.Log(rho);

            double sigW = Math.Sqrt(D * (1.0 - rho * rho));
            double[] X = new double[N];
            X[0] = mu + Math.Sqrt(D) * Gauss();
            for (int n = 1; n < N; n++)
                X[n] = mu + rho * (X[n - 1] - mu) + sigW * Gauss();

            int maxLag = Math.Min(60, N / 4);
            double[] Rx = EstCorr(X, maxLag);
            double[] RxT = new double[maxLag + 1];
            for (int m = 0; m <= maxLag; m++)
                RxT[m] = D * Math.Exp(-a * m);

            lbl1Info.Text = string.Format(
                "Параметры: M={0}, D={1}, rho(1)={2}  |  a={3:F4}  |  Оценки: M={4:F3}, D={5:F3}",
                mu, D, rho, a, Mean(X), Var(X));

            DrawProc(pb1proc, X, "Реализация гауссовского процесса X[n]", C_BLUE, mu, D);
            DrawCorr(pb1corr, Rx, RxT, maxLag,
                "КФ R(m): синий=эксп., зелёный=теория D*exp(-a*m)", C_BLUE, C_GRN);

            tabs.SelectedIndex = 0;
        }

        // ════════════════════════════════════════════════════════════
        //  ЗАДАНИЕ 2
        // ════════════════════════════════════════════════════════════
        private void RunTask2()
        {
            int N = (int)nudN.Value;
            double D = 7.0;
            double a = 0.05;
            double rho = Math.Exp(-a);

            double sigW = Math.Sqrt(D * (1.0 - rho * rho));
            double[] X = new double[N];
            X[0] = Math.Sqrt(D) * Gauss();
            for (int n = 1; n < N; n++)
                X[n] = rho * X[n - 1] + sigW * Gauss();

            double Rx1 = D * rho;
            double b = Rx1 / D;
            double DyTheor = D * (1.0 + b * b) - 2.0 * b * Rx1;

            double[] Y = new double[N];
            Y[0] = X[0];
            for (int n = 1; n < N; n++)
                Y[n] = X[n] - b * X[n - 1];

            int maxLag = Math.Min(60, N / 4);
            double[] Ry = EstCorr(Y, maxLag);
            double[] RyT = new double[maxLag + 1];
            for (int m = 0; m <= maxLag; m++)
            {
                double r0 = D * Math.Exp(-a * m);
                double rm = m > 0 ? D * Math.Exp(-a * (m - 1)) : D;
                double rp = D * Math.Exp(-a * (m + 1));
                RyT[m] = (1.0 + b * b) * r0 - b * rm - b * rp;
            }

            lbl2Info.Text = string.Format(
                "D={0}, a={1}  |  b_opt={2:F4}  |  Dy_теор={3:F4}  |  Dy_оценка={4:F4}  |  My={5:F4}",
                D, a, b, DyTheor, Var(Y), Mean(Y));

            DrawProc(pb2proc, Y, "Реализация Y[n] = X[n] - b*X[n-1]", C_ORNG, 0.0, DyTheor);
            DrawCorr(pb2corr, Ry, RyT, maxLag,
                "КФ R_Y(m): оранжевый=эксп., зелёный=теория", C_ORNG, C_GRN);

            tabs.SelectedIndex = 1;
        }

        // ════════════════════════════════════════════════════════════
        //  ОТРИСОВКА
        // ════════════════════════════════════════════════════════════
        private void DrawProc(PictureBox pb, double[] data,
            string title, Color col, double mu, double D)
        {
            if (pb.Width < 40 || pb.Height < 40) return;
            int W = pb.Width, H = pb.Height;
            var bmp = new Bitmap(W, H);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(C_PANEL);
                int PL = 10, PR = 10, PT = 26, PB = 18;
                int cw = W - PL - PR, ch = H - PT - PB;
                if (cw < 10 || ch < 10) return;

                g.DrawString(title, new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    new SolidBrush(C_TEXT), PL, 4);

                double sig3 = 3.0 * Math.Sqrt(Math.Abs(D));
                double yMin = mu - sig3, yMax = mu + sig3;
                foreach (double v in data)
                {
                    if (v < yMin) yMin = v;
                    if (v > yMax) yMax = v;
                }
                double yr = yMax - yMin;
                if (yr < 1e-10) yr = 1;
                yMin -= yr * 0.04; yMax += yr * 0.04; yr = yMax - yMin;

                var penG = new Pen(Color.FromArgb(33, 40, 62));
                var fntS = new Font("Consolas", 7f);
                var brM = new SolidBrush(C_MUTED);
                for (int i = 0; i <= 4; i++)
                {
                    float yy = PT + (float)(i / 4.0 * ch);
                    g.DrawLine(penG, PL, yy, PL + cw, yy);
                    double v = yMax - i / 4.0 * yr;
                    g.DrawString(v.ToString("F1"), fntS, brM, 0, yy - 7);
                }

                float yMu = PT + (float)((yMax - mu) / yr * ch);
                g.DrawLine(new Pen(Color.FromArgb(50, C_TEXT)), PL, yMu, PL + cw, yMu);

                int step = Math.Max(1, data.Length / 2000);
                int cnt = data.Length / step;
                if (cnt < 2) return;
                var pts = new PointF[cnt];
                float bwPt = (float)cw / (cnt - 1);
                for (int i = 0; i < cnt; i++)
                {
                    float py = PT + (float)((yMax - data[i * step]) / yr * ch);
                    py = Math.Max(PT, Math.Min(PT + ch, py));
                    pts[i] = new PointF(PL + i * bwPt, py);
                }
                g.DrawLines(new Pen(col, 1.1f), pts);
                g.DrawRectangle(new Pen(Color.FromArgb(42, 46, 72)), PL, PT, cw, ch);
            }
            pb.Image = bmp;
        }

        private void DrawCorr(PictureBox pb, double[] Rx, double[] RxT,
            int maxLag, string title, Color colExp, Color colT)
        {
            if (pb.Width < 40 || pb.Height < 40) return;
            int W = pb.Width, H = pb.Height;
            var bmp = new Bitmap(W, H);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(C_PANEL);
                int PL = 58, PR = 16, PT = 26, PB = 18;
                int cw = W - PL - PR, ch = H - PT - PB;
                if (cw < 10 || ch < 10) return;

                g.DrawString(title, new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    new SolidBrush(C_TEXT), PL, 4);

                double vMax = 0, vMin = 0;
                for (int i = 0; i <= maxLag; i++)
                {
                    vMax = Math.Max(vMax, Math.Max(Rx[i], RxT[i]));
                    vMin = Math.Min(vMin, Math.Min(Rx[i], RxT[i]));
                }
                vMax *= 1.12;
                double yMin = vMin < 0 ? vMin * 1.12 : -vMax * 0.08;
                double yMax = vMax;
                double yr = yMax - yMin;
                if (yr < 1e-10) yr = 1;

                var penG = new Pen(Color.FromArgb(33, 40, 62));
                var fntS = new Font("Consolas", 7f);
                var brM = new SolidBrush(C_MUTED);

                for (int i = 0; i <= 4; i++)
                {
                    float yy = PT + (float)(i / 4.0 * ch);
                    double v = yMax - i / 4.0 * yr;
                    g.DrawLine(penG, PL, yy, PL + cw, yy);
                    string s = v.ToString("F2");
                    SizeF sz = g.MeasureString(s, fntS);
                    g.DrawString(s, fntS, brM, PL - sz.Width - 3, yy - sz.Height / 2);
                }

                float y0 = PT + (float)(yMax / yr * ch);
                float barW = (float)cw / (maxLag + 1);

                g.DrawLine(new Pen(Color.FromArgb(50, C_TEXT)), PL, y0, PL + cw, y0);

                var brExp = new SolidBrush(Color.FromArgb(155, colExp));
                for (int m = 0; m <= maxLag; m++)
                {
                    float px = PL + m * barW + barW * 0.15f;
                    float py = PT + (float)((yMax - Rx[m]) / yr * ch);
                    py = Math.Max(PT, Math.Min(PT + ch, py));
                    float by = Math.Min(py, y0);
                    float bh = Math.Abs(py - y0);
                    g.FillRectangle(brExp, px, by, Math.Max(barW * 0.7f, 1f), bh);
                }

                var ptsT = new PointF[maxLag + 1];
                for (int m = 0; m <= maxLag; m++)
                {
                    float px = PL + m * barW + barW / 2f;
                    float py = PT + (float)((yMax - RxT[m]) / yr * ch);
                    py = Math.Max(PT, Math.Min(PT + ch, py));
                    ptsT[m] = new PointF(px, py);
                }
                if (ptsT.Length > 1)
                    g.DrawCurve(new Pen(colT, 2f), ptsT, 0.3f);

                int stepX = Math.Max(1, maxLag / 10);
                for (int m = 0; m <= maxLag; m += stepX)
                {
                    float px = PL + m * barW + barW / 2f;
                    g.DrawString(m.ToString(), fntS, brM, px - 6, PT + ch + 3);
                }
                g.DrawString("m", new Font("Segoe UI", 8f), brM, PL + cw + 2, PT + ch - 6);
                g.DrawRectangle(new Pen(Color.FromArgb(42, 46, 72)), PL, PT, cw, ch);
            }
            pb.Image = bmp;
        }

        // ════════════════════════════════════════════════════════════
        //  МАТЕМАТИКА
        // ════════════════════════════════════════════════════════════
        private static double Gauss()
        {
            double u1 = 1.0 - rnd.NextDouble();
            double u2 = 1.0 - rnd.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        }

        private static double Mean(double[] x)
        {
            double s = 0;
            for (int i = 0; i < x.Length; i++) s += x[i];
            return s / x.Length;
        }

        private static double Var(double[] x)
        {
            double m = Mean(x), s = 0;
            for (int i = 0; i < x.Length; i++) s += (x[i] - m) * (x[i] - m);
            return s / x.Length;
        }

        private static double[] EstCorr(double[] x, int maxLag)
        {
            int N = x.Length;
            double mu = Mean(x);
            double[] R = new double[maxLag + 1];
            for (int m = 0; m <= maxLag; m++)
            {
                double s = 0;
                for (int n = 0; n < N - m; n++)
                    s += (x[n] - mu) * (x[n + m] - mu);
                R[m] = s / N;
            }
            return R;
        }

        // ════════════════════════════════════════════════════════════
        //  UI-ХЕЛПЕРЫ
        // ════════════════════════════════════════════════════════════
        private Button MakeTopBtn(string text, int left, Color col)
        {
            var b = new Button
            {
                Text = text,
                Top = 10,
                Left = left,
                Width = 120,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = col,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private Label MakeInfoLabel()
        {
            return new Label
            {
                Text = "Нажмите кнопку запуска выше",
                ForeColor = C_TEXT,
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = false,
                Height = 22,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private PictureBox MakePb()
        {
            return new PictureBox { BackColor = C_PANEL };
        }

        private void DrawTab(object sender, DrawItemEventArgs e)
        {
            var tc = (TabControl)sender;
            bool sel = e.Index == tc.SelectedIndex;
            e.Graphics.FillRectangle(
                new SolidBrush(sel
                    ? Color.FromArgb(26, 28, 50)
                    : Color.FromArgb(18, 18, 34)),
                e.Bounds);
            if (sel)
                e.Graphics.FillRectangle(new SolidBrush(C_BLUE),
                    e.Bounds.Left, e.Bounds.Bottom - 3, e.Bounds.Width, 3);
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            e.Graphics.DrawString(tc.TabPages[e.Index].Text,
                new Font("Segoe UI", 9f, sel ? FontStyle.Bold : FontStyle.Regular),
                new SolidBrush(sel ? C_BLUE : C_MUTED), e.Bounds, sf);
        }
    }
}