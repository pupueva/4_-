using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace _4_tipis
{
    public partial class Form1 : Form
    {
        private static readonly Random rnd = new Random();

        private Button btnRun1, btnRun2;
        private PictureBox pbGraph1, pbGraph2;
        private Label lblInfo1, lblInfo2;
        private NumericUpDown nudN;

        private readonly Color C_BG = Color.FromArgb(30, 30, 40);
        private readonly Color C_PANEL = Color.FromArgb(45, 45, 55);
        private readonly Color C_BLUE = Color.FromArgb(70, 130, 230);
        private readonly Color C_RED = Color.FromArgb(230, 70, 70);
        private readonly Color C_GREEN = Color.FromArgb(70, 200, 100);
        private readonly Color C_ORANGE = Color.FromArgb(255, 140, 50);
        private readonly Color C_TEXT = Color.White;

        public Form1()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Рекуррентная оценка математического ожидания и дисперсии";
            this.Size = new Size(1200, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = C_BG;

            // Верхняя панель
            Panel pnlTop = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(35, 35, 45)
            };

            Label lblN = new Label
            {
                Text = "Количество отсчетов N:",
                Location = new Point(15, 18),
                Size = new Size(140, 25),
                ForeColor = C_TEXT
            };
            nudN = new NumericUpDown
            {
                Location = new Point(160, 16),
                Width = 100,
                Minimum = 100,
                Maximum = 10000,
                Value = 1000,
                Increment = 100,
                BackColor = C_PANEL,
                ForeColor = C_TEXT
            };

            btnRun1 = new Button
            {
                Text = "Задание 1: Оценка M[Y]",
                Location = new Point(300, 12),
                Size = new Size(200, 35),
                BackColor = C_BLUE,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnRun1.Click += (s, e) => RunTask1();

            btnRun2 = new Button
            {
                Text = "Задание 2: Оценка D[Y]",
                Location = new Point(520, 12),
                Size = new Size(200, 35),
                BackColor = C_ORANGE,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnRun2.Click += (s, e) => RunTask2();

            pnlTop.Controls.AddRange(new Control[] { lblN, nudN, btnRun1, btnRun2 });

            TableLayoutPanel tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(5)
            };
            tlp.RowStyles.Clear();
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            pbGraph1 = CreatePictureBox();
            pbGraph2 = CreatePictureBox();

            tlp.Controls.Add(pbGraph1, 0, 0);
            tlp.Controls.Add(pbGraph2, 0, 1);

            Panel pnlInfo = new Panel { Dock = DockStyle.Bottom, Height = 80, BackColor = C_PANEL };

            lblInfo1 = new Label
            {
                Text = "Задание 1: Нажмите кнопку для запуска",
                Location = new Point(10, 5),
                Size = new Size(800, 30),
                ForeColor = C_TEXT,
                Font = new Font("Segoe UI", 9)
            };

            lblInfo2 = new Label
            {
                Text = "Задание 2: Нажмите кнопку для запуска",
                Location = new Point(10, 40),
                Size = new Size(800, 30),
                ForeColor = C_TEXT,
                Font = new Font("Segoe UI", 9)
            };

            pnlInfo.Controls.AddRange(new Control[] { lblInfo1, lblInfo2 });

            this.Controls.Add(tlp);
            this.Controls.Add(pnlInfo);
            this.Controls.Add(pnlTop);
        }

        private PictureBox CreatePictureBox()
        {
            return new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = C_PANEL,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private double Gauss()
        {
            double u1 = rnd.NextDouble();
            double u2 = rnd.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        }

        
        private void RunTask1()
        {
            int N = (int)nudN.Value;
            double trueMean = 0.5;  

            double[] estimates = new double[N];
            double currentEstimate = 0;

            for (int n = 1; n <= N; n++)
            {
                double x = Gauss();

                double y = (x > 0) ? x * x : 0;

                currentEstimate = currentEstimate + (y - currentEstimate) / n;
                estimates[n - 1] = currentEstimate;
            }

            double finalEstimate = estimates[N - 1];
            double error = Math.Abs(finalEstimate - trueMean);

            lblInfo1.Text = string.Format(
                "Задание 1: M[Y] = 0.5 (теоретическое) | Оценка = {0:F6} | Ошибка = {1:F6} | N = {2}",
                finalEstimate, error, N);

            DrawGraph(pbGraph1, estimates, trueMean, N,
                "Рекуррентная оценка математического ожидания M[Y]",
                "M[Y] = 0.5 (теоретическое)", C_BLUE, C_RED);
        }

        
        private void RunTask2()
        {
            int N = (int)nudN.Value;


            double trueMean = 1.0 / Math.Sqrt(2.0 * Math.PI);  
            double trueVariance = 0.5 - trueMean * trueMean;    

            double[] varEstimates = new double[N];
            double currentMean = 0;
            double currentM2 = 0;  

            for (int n = 1; n <= N; n++)
            {
                double x = Gauss();

                double y = (x > 0) ? x : 0;

                double newMean = currentMean + (y - currentMean) / n;

                double delta = y - currentMean;
                currentM2 += delta * (y - newMean);
                currentMean = newMean;

                if (n > 1)
                    varEstimates[n - 1] = currentM2 / (n - 1);
                else
                    varEstimates[n - 1] = 0;
            }

            double finalEstimate = varEstimates[N - 1];
            double error = Math.Abs(finalEstimate - trueVariance);

            lblInfo2.Text = string.Format(
                "Задание 2: D[Y] = {0:F6} (теоретическое) | Оценка = {1:F6} | Ошибка = {2:F6} | N = {3}",
                trueVariance, finalEstimate, error, N);

            DrawGraph(pbGraph2, varEstimates, trueVariance, N,
                "Рекуррентная оценка дисперсии D[Y]",
                "D[Y] = 0.34085 (теоретическое)", C_ORANGE, C_GREEN);
        }

        private void DrawGraph(PictureBox pb, double[] estimates, double trueValue, int N,
            string title, string legend, Color colorEstimate, Color colorTrue)
        {
            if (pb.Width < 10 || pb.Height < 10) return;

            Bitmap bmp = new Bitmap(pb.Width, pb.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(C_PANEL);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                int left = 70, right = 30, top = 40, bottom = 50;
                int width = pb.Width - left - right;
                int height = pb.Height - top - bottom;

                using (Font titleFont = new Font("Segoe UI", 10, FontStyle.Bold))
                    g.DrawString(title, titleFont, Brushes.White, left, 8);

                double yMin = estimates[0], yMax = estimates[0];
                for (int i = 0; i < estimates.Length; i++)
                {
                    if (estimates[i] < yMin) yMin = estimates[i];
                    if (estimates[i] > yMax) yMax = estimates[i];
                }
                if (trueValue < yMin) yMin = trueValue;
                if (trueValue > yMax) yMax = trueValue;

                double range = yMax - yMin;
                if (range < 0.01) range = 1;
                yMin -= range * 0.1;
                yMax += range * 0.1;
                double yr = yMax - yMin;

                using (Pen gridPen = new Pen(Color.FromArgb(50, 100, 100, 120)))
                using (Font font = new Font("Consolas", 7))
                using (Brush brush = new SolidBrush(Color.FromArgb(150, 150, 170)))
                {
                    for (int i = 0; i <= 5; i++)
                    {
                        float yy = top + (i / 5f) * height;
                        g.DrawLine(gridPen, left, yy, left + width, yy);
                        double val = yMax - (i / 5f) * yr;
                        g.DrawString(val.ToString("F3"), font, brush, left - 50, yy - 5);
                    }
                }

                float yTrue = top + (float)((yMax - trueValue) / yr * height);
                using (Pen dashPen = new Pen(colorTrue, 2f))
                {
                    dashPen.DashStyle = DashStyle.Dash;
                    dashPen.DashPattern = new float[] { 8, 4 };
                    g.DrawLine(dashPen, left, yTrue, left + width, yTrue);
                }

                int step = Math.Max(1, estimates.Length / 2000);
                int points = estimates.Length / step;
                if (points > 1)
                {
                    PointF[] pts = new PointF[points];
                    float xStep = (float)width / (points - 1);
                    for (int i = 0; i < points; i++)
                    {
                        float px = left + i * xStep;
                        float py = top + (float)((yMax - estimates[i * step]) / yr * height);
                        pts[i] = new PointF(px, Math.Max(top, Math.Min(top + height, py)));
                    }
                    using (Pen linePen = new Pen(colorEstimate, 1.5f))
                        g.DrawLines(linePen, pts);
                }

                using (Pen axisPen = new Pen(Color.FromArgb(100, 100, 120)))
                {
                    g.DrawRectangle(axisPen, left, top, width, height);

                    for (int i = 0; i <= 4; i++)
                    {
                        float px = left + (i / 4f) * width;
                        double val = (i / 4f) * N;
                        g.DrawString(val.ToString("F0"), new Font("Consolas", 7),
                            Brushes.Gray, px - 10, top + height + 5);
                    }
                }

                g.DrawString("n (количество отсчетов)", new Font("Segoe UI", 8),
                    Brushes.Gray, left + width / 2 - 60, pb.Height - 20);
                g.DrawString("Оценка", new Font("Segoe UI", 8),
                    Brushes.Gray, 8, top + height / 2);

                using (Font legendFont = new Font("Segoe UI", 8))
                {
                    // Экспериментальная оценка
                    g.DrawLine(new Pen(colorEstimate, 2), left + width - 180, 25, left + width - 150, 25);
                    g.DrawString("Рекуррентная оценка", legendFont, Brushes.White, left + width - 145, 20);

                    // Теоретическое значение
                    g.DrawLine(new Pen(colorTrue, 2), left + width - 180, 45, left + width - 150, 45);
                    g.DrawString(legend, legendFont, Brushes.White, left + width - 145, 40);
                }
            }
            pb.Image = bmp;
        }
    }
}