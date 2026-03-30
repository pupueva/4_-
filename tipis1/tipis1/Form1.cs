using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using System;

//  ЛР №1 — Моделирование случайных величин
//  C# 7.3 / .NET Framework 4.x совместимый код.
//  Графики через GDI+ (PictureBox). Без внешних зависимостей.
// ============================================================
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
 
namespace tipis1
{
    public partial class Form1 : Form
    {
        // ── RNG ─────────────────────────────────────────────────────
        private static readonly Random rnd = new Random();

        // ── Палитра ─────────────────────────────────────────────────
        private readonly Color C_BG = Color.FromArgb(14, 14, 26);
        private readonly Color C_PANEL = Color.FromArgb(20, 20, 38);
        private readonly Color C_SIDE = Color.FromArgb(17, 17, 32);
        private readonly Color C_BORDER = Color.FromArgb(38, 42, 64);
        private readonly Color C_TEXT = Color.FromArgb(205, 210, 230);
        private readonly Color C_MUTED = Color.FromArgb(85, 90, 118);
        private readonly Color C_BLUE = Color.FromArgb(60, 150, 255);
        private readonly Color C_ORANGE = Color.FromArgb(255, 110, 60);
        private readonly Color C_GREEN = Color.FromArgb(65, 210, 140);

        // ── Контролы ────────────────────────────────────────────────
        private Panel pnlSide, pnlMain, pnlParams;
        private Button btnT1, btnT2, btnT7, btnRun;
        private Label lblTitle, lblSub, lblDesc;
        private NumericUpDown nudN1, nudN2;
        private TabControl tabs;
        private TabPage pageG1, pageG2, pageStats;
        private PictureBox pbG1, pbG2;
        private DataGridView grid;

        private int _task = 0;
        private ChartData _cd1, _cd2;

        // ═════════════════════════════════════════════════════════════
        public Form1()
        {
            InitializeComponent();
            Build();
            this.Resize += (s, e) => DoLayout();
        }
        private void Form1_Load(object sender, EventArgs e) { }

        // ═════════════════════════════════════════════════════════════
        //  ПОСТРОЕНИЕ UI
        // ═════════════════════════════════════════════════════════════
        private void Build()
        {
            this.Text = "ЛР №1 — Моделирование случайных величин";
            this.Size = new Size(1380, 860);
            this.MinimumSize = new Size(1000, 660);
            this.BackColor = C_BG;
            this.ForeColor = C_TEXT;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10f);

            // ── Sidebar ─────────────────────────────────────────────
            pnlSide = new Panel { BackColor = C_SIDE, Width = 252 };

            var pnlHead = new Panel
            {
                BackColor = Color.FromArgb(19, 19, 40),
                Height = 86,
                Dock = DockStyle.Top
            };
            lblTitle = new Label
            {
                Text = "ЛР №1",
                ForeColor = C_BLUE,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                AutoSize = true,
                Top = 10,
                Left = 16
            };
            lblSub = new Label
            {
                Text = "Моделирование\nслучайных величин",
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = true,
                Top = 46,
                Left = 16
            };
            pnlHead.Controls.AddRange(new Control[] { lblTitle, lblSub });
            pnlSide.Controls.Add(pnlHead);

            var lbMenu = MakeLabel("  ЗАДАНИЯ", C_MUTED, 7.5f, true);
            lbMenu.Top = 92; lbMenu.Height = 24;
            lbMenu.AutoSize = false; lbMenu.Width = 252;
            pnlSide.Controls.Add(lbMenu);

            btnT1 = SideBtn("Задание 1", "Равномерное [-2, 7]", 118, C_ORANGE);
            btnT2 = SideBtn("Задание 2", "Дискретное распределение", 192, C_BLUE);
            btnT7 = SideBtn("Задание 7", "Гаусс + Равномерное [5,7]", 266, C_GREEN);
            btnT1.Click += (s, e) => Pick(1);
            btnT2.Click += (s, e) => Pick(2);
            btnT7.Click += (s, e) => Pick(7);
            pnlSide.Controls.AddRange(new Control[] { btnT1, btnT2, btnT7 });

            // Панель параметров
            pnlParams = new Panel
            {
                BackColor = Color.FromArgb(17, 17, 34),
                Width = 252,
                Height = 174,
                Left = 0
            };

            var lhN = MakeLabel("  ПАРАМЕТРЫ", C_MUTED, 7.5f, true);
            lhN.Top = 6; lhN.AutoSize = false; lhN.Width = 252; lhN.Height = 22;

            var l1 = MakeLabel("N₁  (малое число экспериментов):", C_MUTED, 8.5f);
            l1.Top = 30; l1.Left = 10; l1.AutoSize = false; l1.Width = 230; l1.Height = 18;

            nudN1 = MakeNud(100, 2000000, 10000);
            nudN1.Top = 48; nudN1.Left = 10; nudN1.Width = 230;

            var l2 = MakeLabel("N₂  (большое число экспериментов):", C_MUTED, 8.5f);
            l2.Top = 78; l2.Left = 10; l2.AutoSize = false; l2.Width = 230; l2.Height = 18;

            nudN2 = MakeNud(1000, 10000000, 100000);
            nudN2.Top = 96; nudN2.Left = 10; nudN2.Width = 230;

            btnRun = new Button
            {
                Text = "▶   Запустить",
                Top = 134,
                Left = 10,
                Width = 230,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = C_BLUE,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRun.FlatAppearance.BorderSize = 0;
            btnRun.FlatAppearance.MouseOverBackColor = Color.FromArgb(88, 170, 255);
            btnRun.Click += (s, e) => RunTask();

            pnlParams.Controls.AddRange(new Control[] { lhN, l1, nudN1, l2, nudN2, btnRun });
            pnlSide.Controls.Add(pnlParams);

            // ── Основная область ────────────────────────────────────
            pnlMain = new Panel { BackColor = C_BG };

            lblDesc = new Label
            {
                Text = "← Выберите задание слева и нажмите «Запустить»",
                ForeColor = C_MUTED,
                Font = new Font("Segoe UI", 11f, FontStyle.Italic),
                AutoSize = false,
                Height = 36,
                Top = 7,
                Left = 12,
                TextAlign = ContentAlignment.MiddleLeft
            };

            tabs = new TabControl
            {
                DrawMode = TabDrawMode.OwnerDrawFixed,
                Appearance = TabAppearance.FlatButtons,
                ItemSize = new Size(180, 36),
                SizeMode = TabSizeMode.Fixed,
                Top = 46,
                Left = 8
            };
            tabs.DrawItem += DrawTab;

            pageG1 = new TabPage("График  N\u2081") { BackColor = C_PANEL };
            pageG2 = new TabPage("График  N\u2082") { BackColor = C_PANEL };
            pageStats = new TabPage("  Статистика  ") { BackColor = C_PANEL };

            pbG1 = new PictureBox { Dock = DockStyle.Fill, BackColor = C_PANEL };
            pbG2 = new PictureBox { Dock = DockStyle.Fill, BackColor = C_PANEL };
            pbG1.Resize += (s, e) => Redraw(pbG1, _cd1);
            pbG2.Resize += (s, e) => Redraw(pbG2, _cd2);

            pageG1.Controls.Add(pbG1);
            pageG2.Controls.Add(pbG2);

            grid = BuildGrid();
            grid.Dock = DockStyle.Fill;
            pageStats.Controls.Add(grid);

            tabs.TabPages.AddRange(new[] { pageG1, pageG2, pageStats });
            pnlMain.Controls.AddRange(new Control[] { lblDesc, tabs });
            this.Controls.AddRange(new Control[] { pnlSide, pnlMain });

            DoLayout();
        }

        private void DoLayout()
        {
            int sw = 252;
            int cw = this.ClientSize.Width - sw;
            int ch = this.ClientSize.Height;

            pnlSide.SetBounds(0, 0, sw, ch);
            pnlMain.SetBounds(sw, 0, cw, ch);
            lblDesc.SetBounds(12, 7, cw - 20, 36);
            tabs.SetBounds(8, 46, cw - 14, ch - 52);
            pnlParams.SetBounds(0, ch - pnlParams.Height - 10, sw, pnlParams.Height);
        }

        // ═════════════════════════════════════════════════════════════
        //  UI-ФАБРИКИ
        // ═════════════════════════════════════════════════════════════
        private Button SideBtn(string title, string sub, int top, Color accent)
        {
            var b = new Button
            {
                Top = top,
                Left = 0,
                Width = 252,
                Height = 66,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = C_TEXT,
                Cursor = Cursors.Hand,
                Text = title + "\n" + sub
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(26, 26, 52);
            b.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 30, 60);
            b.Paint += (sender, e) =>
            {
                var btn = (Button)sender;
                int tk = btn == btnT1 ? 1 : btn == btnT2 ? 2 : 7;
                bool sel = _task == tk;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                if (sel)
                    e.Graphics.FillRectangle(
                        new SolidBrush(Color.FromArgb(26, 26, 52)), e.ClipRectangle);
                e.Graphics.FillRectangle(
                    new SolidBrush(sel ? accent : Color.FromArgb(45, accent)),
                    0, 0, 4, btn.Height);
                string[] lines = btn.Text.Split('\n');
                e.Graphics.DrawString(lines[0],
                    new Font("Segoe UI", 10.5f, FontStyle.Bold),
                    new SolidBrush(sel ? accent : C_TEXT), 14, 12);
                if (lines.Length > 1)
                    e.Graphics.DrawString(lines[1],
                        new Font("Segoe UI", 8.5f),
                        new SolidBrush(C_MUTED), 14, 34);
            };
            return b;
        }

        private Label MakeLabel(string text, Color fore, float size, bool bold = false)
        {
            return new Label
            {
                Text = text,
                ForeColor = fore,
                AutoSize = true,
                Font = new Font("Segoe UI", size,
                    bold ? FontStyle.Bold : FontStyle.Regular)
            };
        }

        private NumericUpDown MakeNud(decimal min, decimal max, decimal val)
        {
            return new NumericUpDown
            {
                Minimum = min,
                Maximum = max,
                Value = val,
                Increment = min,
                ThousandsSeparator = true,
                BackColor = Color.FromArgb(26, 26, 48),
                ForeColor = C_TEXT,
                Font = new Font("Consolas", 9.5f)
            };
        }

        private DataGridView BuildGrid()
        {
            var hdrStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(24, 24, 46),
                ForeColor = C_BLUE,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                SelectionBackColor = Color.FromArgb(24, 24, 46),
                SelectionForeColor = C_BLUE,
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };
            var cellStyle = new DataGridViewCellStyle
            {
                BackColor = C_PANEL,
                ForeColor = C_TEXT,
                SelectionBackColor = Color.FromArgb(30, 30, 65),
                SelectionForeColor = C_TEXT,
                Alignment = DataGridViewContentAlignment.MiddleRight
            };
            var altStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(22, 22, 42),
                ForeColor = C_TEXT,
                SelectionBackColor = Color.FromArgb(30, 30, 65),
                SelectionForeColor = C_TEXT,
                Alignment = DataGridViewContentAlignment.MiddleRight
            };

            var g = new DataGridView
            {
                BackgroundColor = C_PANEL,
                GridColor = C_BORDER,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Consolas", 9f),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersDefaultCellStyle = hdrStyle,
                DefaultCellStyle = cellStyle,
                AlternatingRowsDefaultCellStyle = altStyle
            };
            g.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 34;
            g.RowTemplate.Height = 26;
            return g;
        }

        private void DrawTab(object sender, DrawItemEventArgs e)
        {
            var tc = (TabControl)sender;
            var page = tc.TabPages[e.Index];
            bool sel = e.Index == tc.SelectedIndex;
            e.Graphics.FillRectangle(
                new SolidBrush(sel
                    ? Color.FromArgb(26, 26, 50)
                    : Color.FromArgb(17, 17, 32)),
                e.Bounds);
            if (sel)
                e.Graphics.FillRectangle(new SolidBrush(C_BLUE),
                    e.Bounds.Left, e.Bounds.Bottom - 3, e.Bounds.Width, 3);
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            e.Graphics.DrawString(page.Text,
                new Font("Segoe UI", 9.5f,
                    sel ? FontStyle.Bold : FontStyle.Regular),
                new SolidBrush(sel ? C_BLUE : C_MUTED),
                e.Bounds, sf);
        }

        // ═════════════════════════════════════════════════════════════
        //  ЛОГИКА ЗАДАНИЙ
        // ═════════════════════════════════════════════════════════════
        private void Pick(int t)
        {
            _task = t;
            btnT1.Invalidate(); btnT2.Invalidate(); btnT7.Invalidate();
            lblDesc.ForeColor = C_TEXT;
            switch (t)
            {
                case 1: lblDesc.Text = "Задание 1 — Равномерное распределение на интервале [-2, 7]"; break;
                case 2: lblDesc.Text = "Задание 2 — Дискретное распределение  X ∈ {5, 7, 17, 19, 21, 25, 55}"; break;
                case 7: lblDesc.Text = "Задание 7 — Y = X (Гаусс, M=3, D=2) + Z (Равном., [5, 7])"; break;
            }
        }

        private void RunTask()
        {
            if (_task == 0) Pick(1);
            btnRun.Enabled = false;
            btnRun.Text = "  Считаю...";
            Application.DoEvents();
            try
            {
                switch (_task)
                {
                    case 1: DoTask1(); break;
                    case 2: DoTask2(); break;
                    case 7: DoTask7(); break;
                }
            }
            finally
            {
                btnRun.Enabled = true;
                btnRun.Text = "▶   Запустить";
            }
        }

        // ═════════════════════════════════════════════════════════════
        //  ГЕНЕРАТОРЫ
        // ═════════════════════════════════════════════════════════════
        private static double Uniform(double a, double b)
        {
            return a + (b - a) * rnd.NextDouble();
        }

        private static double Discrete()
        {
            double[] v = { 5, 7, 17, 19, 21, 25, 55 };
            double[] c = { 0.01, 0.06, 0.36, 0.66, 0.96, 0.98 };
            double e = rnd.NextDouble();
            for (int i = 0; i < 6; i++) if (e < c[i]) return v[i];
            return v[6];
        }

        private static double Gauss(double mean, double variance)
        {
            double x = 0;
            for (int i = 0; i < 12; i++) x += rnd.NextDouble();
            return (x - 6.0) * Math.Sqrt(variance) + mean;
        }

        private static double[] BuildHist(int bins, long N,
            double mn, double mx, Func<double> gen)
        {
            double[] h = new double[bins];
            double bw = (mx - mn) / bins;
            for (long m = 0; m < N; m++)
            {
                int idx = (int)((gen() - mn) / bw);
                if (idx >= 0 && idx < bins) h[idx]++;
            }
            for (int i = 0; i < bins; i++) h[i] /= (N * bw);
            return h;
        }

        private static double[] FreqDiscrete(int[] vals, long N)
        {
            double[] f = new double[vals.Length];
            for (long m = 0; m < N; m++)
            {
                double v = Discrete();
                for (int i = 0; i < vals.Length; i++)
                    if ((int)v == vals[i]) { f[i]++; break; }
            }
            for (int i = 0; i < f.Length; i++) f[i] /= N;
            return f;
        }

        // ═════════════════════════════════════════════════════════════
        //  ЗАДАНИЕ 1
        // ═════════════════════════════════════════════════════════════
        private void DoTask1()
        {
            double a = -2, b = 7;
            int bins = 35;
            long N1 = (long)nudN1.Value, N2 = (long)nudN2.Value;
            double th = 1.0 / (b - a);
            double bw = (b - a) / bins;

            double[] h1 = BuildHist(bins, N1, a, b, () => Uniform(a, b));
            double[] h2 = BuildHist(bins, N2, a, b, () => Uniform(a, b));

            _cd1 = new ChartData
            {
                Title = string.Format("N\u2081 = {0:N0}  |  Равномерное [-2, 7]", N1),
                MinX = a,
                BinW = bw,
                Bins = bins,
                Bars1 = h1,
                Color1 = C_ORANGE,
                Theory = th,
                TheoryColor = C_GREEN,
                AxisX = "x",
                AxisY = "f(x)"
            };
            _cd2 = new ChartData
            {
                Title = string.Format("N\u2082 = {0:N0}  |  Равномерное [-2, 7]", N2),
                MinX = a,
                BinW = bw,
                Bins = bins,
                Bars1 = h2,
                Color1 = C_BLUE,
                Theory = th,
                TheoryColor = C_GREEN,
                AxisX = "x",
                AxisY = "f(x)"
            };

            pageG1.Text = string.Format("N\u2081 = {0:N0}", N1);
            pageG2.Text = string.Format("N\u2082 = {0:N0}", N2);
            Redraw(pbG1, _cd1);
            Redraw(pbG2, _cd2);

            GridSetup("Интервал",
                string.Format("N\u2081={0:N0}", N1),
                string.Format("N\u2082={0:N0}", N2),
                "Теория", "Откл. N\u2081", "Откл. N\u2082");

            double sE1 = 0, sE2 = 0;
            for (int i = 0; i < bins; i++)
            {
                double x1 = a + i * bw, x2 = x1 + bw;
                double e1 = Math.Abs(h1[i] - th);
                double e2 = Math.Abs(h2[i] - th);
                sE1 += e1; sE2 += e2;
                grid.Rows.Add(
                    string.Format("[{0:F2}; {1:F2}]", x1, x2),
                    h1[i].ToString("F5"), h2[i].ToString("F5"),
                    th.ToString("F5"),
                    e1.ToString("F5"), e2.ToString("F5"));
            }
            SumRow("СРЕДНЕЕ", "", "", "",
                (sE1 / bins).ToString("F5"), (sE2 / bins).ToString("F5"));
            tabs.SelectedIndex = 0;
        }

        // ═════════════════════════════════════════════════════════════
        //  ЗАДАНИЕ 2
        // ═════════════════════════════════════════════════════════════
        private void DoTask2()
        {
            int[] vals = { 5, 7, 17, 19, 21, 25, 55 };
            double[] probs = { 0.01, 0.05, 0.3, 0.3, 0.3, 0.02, 0.02 };
            long N1 = (long)nudN1.Value, N2 = (long)nudN2.Value;

            double[] f1 = FreqDiscrete(vals, N1);
            double[] f2 = FreqDiscrete(vals, N2);

            _cd1 = new ChartData
            {
                Title = string.Format("N\u2081 = {0:N0}  |  Дискретное распределение", N1),
                IsDiscrete = true,
                DiscreteX = vals,
                Bars1 = f1,
                Color1 = C_ORANGE,
                Theory2 = probs,
                TheoryColor = C_GREEN,
                AxisX = "X",
                AxisY = "P(X = x)"
            };
            _cd2 = new ChartData
            {
                Title = string.Format("N\u2082 = {0:N0}  |  Дискретное распределение", N2),
                IsDiscrete = true,
                DiscreteX = vals,
                Bars1 = f2,
                Color1 = C_BLUE,
                Theory2 = probs,
                TheoryColor = C_GREEN,
                AxisX = "X",
                AxisY = "P(X = x)"
            };

            pageG1.Text = string.Format("N\u2081 = {0:N0}", N1);
            pageG2.Text = string.Format("N\u2082 = {0:N0}", N2);
            Redraw(pbG1, _cd1);
            Redraw(pbG2, _cd2);

            GridSetup("X", "P(X) теория",
                string.Format("N\u2081={0:N0}", N1),
                string.Format("N\u2082={0:N0}", N2),
                "Откл. N\u2081", "Откл. N\u2082");

            double sE1 = 0, sE2 = 0;
            for (int i = 0; i < vals.Length; i++)
            {
                double e1 = Math.Abs(f1[i] - probs[i]);
                double e2 = Math.Abs(f2[i] - probs[i]);
                sE1 += e1; sE2 += e2;
                grid.Rows.Add(vals[i].ToString(), probs[i].ToString("F4"),
                    f1[i].ToString("F4"), f2[i].ToString("F4"),
                    e1.ToString("F4"), e2.ToString("F4"));
            }
            SumRow("СРЕДНЕЕ", "", "", "",
                (sE1 / vals.Length).ToString("F5"),
                (sE2 / vals.Length).ToString("F5"));
            tabs.SelectedIndex = 0;
        }

        // ═════════════════════════════════════════════════════════════
        //  ЗАДАНИЕ 7
        // ═════════════════════════════════════════════════════════════
        private void DoTask7()
        {
            double mx = 3, dx = 2, az = 5, bz = 7;
            double sig = Math.Sqrt(dx);
            double mn = (mx - 4.5 * sig) + az;
            double mxY = (mx + 4.5 * sig) + bz;
            int bins = 50;
            long N1 = (long)nudN1.Value, N2 = (long)nudN2.Value;
            double bw = (mxY - mn) / bins;

            double[] h1 = BuildHist(bins, N1, mn, mxY,
                () => Gauss(mx, dx) + Uniform(az, bz));
            double[] h2 = BuildHist(bins, N2, mn, mxY,
                () => Gauss(mx, dx) + Uniform(az, bz));

            _cd1 = new ChartData
            {
                Title = string.Format("N\u2081 = {0:N0}  |  Y = Гаусс(3,2) + Равн.[5,7]", N1),
                MinX = mn,
                BinW = bw,
                Bins = bins,
                Bars1 = h1,
                Color1 = C_ORANGE,
                AxisX = "y",
                AxisY = "f(y)"
            };
            // Второй график — оба N поверх друг друга
            _cd2 = new ChartData
            {
                Title = string.Format(
                    "Сравнение N\u2081 vs N\u2082  |  Y = Гаусс(3,2) + Равн.[5,7]"),
                MinX = mn,
                BinW = bw,
                Bins = bins,
                Bars1 = h1,
                Color1 = C_ORANGE,
                Bars2 = h2,
                Color2 = C_BLUE,
                AxisX = "y",
                AxisY = "f(y)"
            };

            pageG1.Text = string.Format("N\u2081 = {0:N0}", N1);
            pageG2.Text = "N\u2081  vs  N\u2082";
            Redraw(pbG1, _cd1);
            Redraw(pbG2, _cd2);

            GridSetup("Интервал",
                string.Format("N\u2081={0:N0}", N1),
                string.Format("N\u2082={0:N0}", N2),
                "|Разница|");

            double maxD = 0, sumD = 0;
            for (int i = 0; i < bins; i++)
            {
                double x1 = mn + i * bw, x2 = x1 + bw;
                double diff = Math.Abs(h2[i] - h1[i]);
                sumD += diff;
                if (diff > maxD) maxD = diff;
                grid.Rows.Add(
                    string.Format("[{0:F2}; {1:F2}]", x1, x2),
                    h1[i].ToString("F5"), h2[i].ToString("F5"),
                    diff.ToString("F5"));
            }
            SumRow(
                string.Format("Среднее = {0:F5}    Максимум = {1:F5}",
                    sumD / bins, maxD),
                "", "", "");
            tabs.SelectedIndex = 0;
        }

        // ═════════════════════════════════════════════════════════════
        //  РИСОВАНИЕ ГРАФИКА (GDI+)
        // ═════════════════════════════════════════════════════════════
        private void Redraw(PictureBox pb, ChartData cd)
        {
            if (cd == null || pb.Width < 80 || pb.Height < 60) return;

            int W = pb.Width, H = pb.Height;
            Bitmap bmp = new Bitmap(W, H,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.Clear(C_PANEL);

                int PL = 68, PR = 20, PT = 44, PB = 40;
                int cw = W - PL - PR;
                int ch = H - PT - PB;
                if (cw < 10 || ch < 10) return;

                // Заголовок
                g.DrawString(cd.Title,
                    new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    new SolidBrush(C_TEXT), PL, 8);

                // Найти максимум
                double maxV = 0;
                if (cd.Bars1 != null && cd.Bars1.Length > 0) maxV = Math.Max(maxV, cd.Bars1.Max());
                if (cd.Bars2 != null && cd.Bars2.Length > 0) maxV = Math.Max(maxV, cd.Bars2.Max());
                if (cd.Theory > 0) maxV = Math.Max(maxV, cd.Theory);
                if (cd.Theory2 != null && cd.Theory2.Length > 0) maxV = Math.Max(maxV, cd.Theory2.Max());
                if (maxV == 0) maxV = 1;
                maxV *= 1.12;

                // Горизонтальные линии сетки + метки Y
                Font fntS = new Font("Consolas", 7.8f);
                Brush brM = new SolidBrush(C_MUTED);
                Pen penGr = new Pen(Color.FromArgb(33, 40, 60));
                Pen penAx = new Pen(C_BORDER);
                int gLines = 5;

                for (int i = 0; i <= gLines; i++)
                {
                    float yy = PT + ch - (float)(i / (double)gLines * ch);
                    g.DrawLine(penGr, PL, yy, PL + cw, yy);
                    g.DrawLine(penAx, PL - 4, yy, PL, yy);
                    string lbl = (maxV * i / gLines).ToString("F4");
                    SizeF sz = g.MeasureString(lbl, fntS);
                    g.DrawString(lbl, fntS, brM, PL - sz.Width - 5, yy - sz.Height / 2);
                }

                // Рамка
                g.DrawRectangle(penAx, PL, PT, cw, ch);

                // Данные
                if (cd.IsDiscrete)
                    DrawDiscrete(g, cd, PL, PT, cw, ch, maxV);
                else
                    DrawContinuous(g, cd, PL, PT, cw, ch, maxV);

                // Метки X
                if (!cd.IsDiscrete)
                    DrawXLabels(g, cd, PL, PT, cw, ch, fntS, brM, penAx);

                // Подписи осей
                g.DrawString(cd.AxisY,
                    new Font("Segoe UI", 8.5f), brM, 2, PT + ch / 2 - 8);
                g.DrawString(cd.AxisX,
                    new Font("Segoe UI", 8.5f), brM, PL + cw / 2 - 8, H - 16);

                // Легенда
                DrawLegend(g, cd, W, PT);
            }

            pb.Image = bmp;
        }

        private void DrawContinuous(Graphics g, ChartData cd,
            int PL, int PT, int cw, int ch, double maxV)
        {
            int bins = cd.Bins;
            float bw = (float)cw / bins;

            if (cd.Bars1 != null)
            {
                SolidBrush br1 = new SolidBrush(Color.FromArgb(175, cd.Color1));
                for (int i = 0; i < bins; i++)
                {
                    float bh = (float)(cd.Bars1[i] / maxV * ch);
                    g.FillRectangle(br1,
                        PL + i * bw + 0.5f, PT + ch - bh, bw - 1f, bh);
                }
            }

            if (cd.Bars2 != null)
            {
                SolidBrush br2 = new SolidBrush(Color.FromArgb(140, cd.Color2));
                for (int i = 0; i < bins; i++)
                {
                    float bh = (float)(cd.Bars2[i] / maxV * ch);
                    g.FillRectangle(br2,
                        PL + i * bw + 0.5f, PT + ch - bh, bw - 1f, bh);
                }
                DrawSpline(g, cd.Bars2, cd.Color2, bins, PL, PT, cw, ch, maxV, 2f);
            }

            if (cd.Bars1 != null && cd.Bars2 != null)
                DrawSpline(g, cd.Bars1, cd.Color1, bins, PL, PT, cw, ch, maxV, 2f);

            // Теоретическая линия
            if (cd.Theory > 0)
            {
                float ty = PT + ch - (float)(cd.Theory / maxV * ch);
                Pen penT = new Pen(cd.TheoryColor, 2f);
                penT.DashStyle = DashStyle.Dash;
                penT.DashPattern = new float[] { 6, 4 };
                g.DrawLine(penT, PL, ty, PL + cw, ty);
            }
        }

        private void DrawSpline(Graphics g, double[] data, Color col,
            int bins, int PL, int PT, int cw, int ch, double maxV, float penW)
        {
            if (data == null || data.Length < 2) return;
            float bw = (float)cw / bins;
            PointF[] pts = new PointF[bins];
            for (int i = 0; i < bins; i++)
                pts[i] = new PointF(
                    PL + (i + 0.5f) * bw,
                    PT + ch - (float)(data[i] / maxV * ch));
            g.DrawCurve(new Pen(col, penW), pts, 0.4f);
        }

        private void DrawDiscrete(Graphics g, ChartData cd,
            int PL, int PT, int cw, int ch, double maxV)
        {
            int n = cd.DiscreteX.Length;
            float grpW = (float)cw / n;
            float barW = grpW * 0.34f;
            Font fnt = new Font("Consolas", 7.8f);
            Brush brM = new SolidBrush(C_MUTED);

            for (int i = 0; i < n; i++)
            {
                float gx = PL + i * grpW + grpW * 0.04f;

                // Теория
                if (cd.Theory2 != null)
                {
                    float bhT = (float)(cd.Theory2[i] / maxV * ch);
                    g.FillRectangle(
                        new SolidBrush(Color.FromArgb(150, cd.TheoryColor)),
                        gx, PT + ch - bhT, barW, bhT);
                }

                // Эксп.
                if (cd.Bars1 != null)
                {
                    float bhE = (float)(cd.Bars1[i] / maxV * ch);
                    g.FillRectangle(
                        new SolidBrush(Color.FromArgb(180, cd.Color1)),
                        gx + barW + 2, PT + ch - bhE, barW, bhE);
                }

                // Подпись X
                g.DrawString(cd.DiscreteX[i].ToString(), fnt, brM,
                    gx + barW / 2, PT + ch + 4);
            }
        }

        private void DrawXLabels(Graphics g, ChartData cd,
            int PL, int PT, int cw, int ch,
            Font fnt, Brush brM, Pen penAx)
        {
            int step = Math.Max(1, cd.Bins / 9);
            float bw = (float)cw / cd.Bins;
            for (int i = 0; i <= cd.Bins; i += step)
            {
                double val = cd.MinX + cd.BinW * i;
                string lbl = val.ToString("F1");
                float x = PL + i * bw;
                SizeF sz = g.MeasureString(lbl, fnt);
                g.DrawString(lbl, fnt, brM, x - sz.Width / 2, PT + ch + 4);
                g.DrawLine(penAx, x, PT + ch, x, PT + ch + 4);
            }
        }

        private void DrawLegend(Graphics g, ChartData cd, int W, int PT)
        {
            var items = new List<KeyValuePair<Color, string>>();

            if (cd.Bars1 != null)
            {
                string lbl = (cd.Bars2 != null) ? "N\u2081" : "Эксп.";
                items.Add(new KeyValuePair<Color, string>(cd.Color1, lbl));
            }
            if (cd.Bars2 != null)
                items.Add(new KeyValuePair<Color, string>(cd.Color2, "N\u2082"));
            if (cd.Theory > 0)
                items.Add(new KeyValuePair<Color, string>(cd.TheoryColor, "Теория"));
            if (cd.Theory2 != null)
                items.Add(new KeyValuePair<Color, string>(cd.TheoryColor, "Теория"));

            Font fnt = new Font("Segoe UI", 8.5f);
            int lx = W - 16;
            int ly = PT + 4;

            for (int i = items.Count - 1; i >= 0; i--)
            {
                Color col = items[i].Key;
                string lbl = items[i].Value;
                SizeF sz = g.MeasureString(lbl, fnt);
                lx -= (int)(sz.Width + 22);
                g.FillRectangle(
                    new SolidBrush(Color.FromArgb(200, col)),
                    lx, ly + 3, 13, 11);
                g.DrawString(lbl, fnt,
                    new SolidBrush(Color.FromArgb(190, 190, 210)),
                    lx + 16, ly);
            }
        }

        // ═════════════════════════════════════════════════════════════
        //  ТАБЛИЦА
        // ═════════════════════════════════════════════════════════════
        private void GridSetup(params string[] headers)
        {
            grid.Columns.Clear();
            grid.Rows.Clear();
            foreach (string h in headers)
            {
                var col = new DataGridViewTextBoxColumn { HeaderText = h };
                col.DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleRight;
                grid.Columns.Add(col);
            }
            if (grid.Columns.Count > 0)
                grid.Columns[0].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleLeft;
        }

        private void SumRow(params string[] vals)
        {
            grid.Rows.Add(vals);
            DataGridViewRow row = grid.Rows[grid.Rows.Count - 1];
            foreach (DataGridViewCell cell in row.Cells)
            {
                cell.Style.BackColor = Color.FromArgb(26, 26, 56);
                cell.Style.ForeColor = C_BLUE;
                cell.Style.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                cell.Style.SelectionBackColor = Color.FromArgb(26, 26, 56);
                cell.Style.SelectionForeColor = C_BLUE;
            }
        }
    }

    // ═════════════════════════════════════════════════════════════════
    //  Данные для графика
    // ═════════════════════════════════════════════════════════════════
    internal class ChartData
    {
        public string Title = "";
        public string AxisX = "x";
        public string AxisY = "f(x)";

        // Непрерывный режим
        public double MinX;
        public double BinW;
        public int Bins;

        // Дискретный режим
        public bool IsDiscrete;
        public int[] DiscreteX;
        public double[] Theory2;

        // Серии данных
        public double[] Bars1;
        public Color Color1 = Color.Gray;
        public double[] Bars2;
        public Color Color2 = Color.Gray;

        // Теоретическая (горизонтальная) линия
        public double Theory = -1;
        public Color TheoryColor = Color.Yellow;
    }
}
