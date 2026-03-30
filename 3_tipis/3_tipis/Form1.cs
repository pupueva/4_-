using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace _3_tipis
{
    public partial class Form1 : Form
    {
        private static readonly Random rnd = new Random();

        // Элементы управления
        private Button btnRun;
        private PictureBox pbImpulse, pbInput, pbOutput;
        private Label lblInfo;
        private NumericUpDown nudN, nudDt;

        // Цвета
        private readonly Color C_BG = Color.FromArgb(30, 30, 40);
        private readonly Color C_PANEL = Color.FromArgb(45, 45, 55);
        private readonly Color C_BLUE = Color.FromArgb(70, 130, 230);
        private readonly Color C_RED = Color.FromArgb(230, 70, 70);
        private readonly Color C_GREEN = Color.FromArgb(70, 200, 100);
        private readonly Color C_ORANGE = Color.FromArgb(255, 140, 50);
        private readonly Color C_TEXT = Color.White;

        public Form1()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Прохождение белого шума через линейную систему";
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = C_BG;

            // Верхняя панель
            Panel pnlTop = new Panel
            {
                Height = 70,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(35, 35, 45)
            };

            // Количество точек
            Label lblN = new Label
            {
                Text = "Количество точек:",
                Location = new Point(15, 25),
                Size = new Size(110, 25),
                ForeColor = C_TEXT
            };
            nudN = new NumericUpDown
            {
                Location = new Point(130, 23),
                Width = 100,
                Minimum = 100,
                Maximum = 5000,
                Value = 1000,
                Increment = 100,
                BackColor = C_PANEL,
                ForeColor = C_TEXT
            };

            // Шаг по времени
            Label lblDt = new Label
            {
                Text = "Шаг dt:",
                Location = new Point(250, 25),
                Size = new Size(60, 25),
                ForeColor = C_TEXT
            };
            nudDt = new NumericUpDown
            {
                Location = new Point(310, 23),
                Width = 80,
                Minimum = 0.001m,
                Maximum = 0.1m,
                Value = 0.01m,
                Increment = 0.005m,
                DecimalPlaces = 3,
                BackColor = C_PANEL,
                ForeColor = C_TEXT
            };

            // Кнопка запуска
            btnRun = new Button
            {
                Text = "▶  ВЫПОЛНИТЬ МОДЕЛИРОВАНИЕ",
                Location = new Point(420, 18),
                Size = new Size(220, 35),
                BackColor = C_GREEN,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnRun.Click += (s, e) => RunSimulation();

            pnlTop.Controls.AddRange(new Control[] { lblN, nudN, lblDt, nudDt, btnRun });

            // Информационная панель
            lblInfo = new Label
            {
                Text = "Нажмите кнопку для запуска моделирования",
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = C_PANEL,
                ForeColor = C_TEXT,
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 9)
            };

            // Панель с графиками
            TableLayoutPanel tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(5)
            };
            tlp.RowStyles.Clear();
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 34));

            pbImpulse = CreatePictureBox();
            pbInput = CreatePictureBox();
            pbOutput = CreatePictureBox();

            tlp.Controls.Add(pbImpulse, 0, 0);
            tlp.Controls.Add(pbInput, 0, 1);
            tlp.Controls.Add(pbOutput, 0, 2);

            this.Controls.Add(tlp);
            this.Controls.Add(lblInfo);
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

        // Генерация белого шума (нормальное распределение)
        private double Gauss()
        {
            double u1 = rnd.NextDouble();
            double u2 = rnd.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        }

        // Импульсная характеристика k(t) = 0.2 * cos(0.5*t + 7)
        private double ImpulseResponse(double t)
        {
            if (t < 0 || t > 5) return 0;
            return 0.2 * Math.Cos(0.5 * t + 7);
        }

        // Свертка сигнала с импульсной характеристикой
        private double[] Convolve(double[] input, double dt)
        {
            int N = input.Length;
            double[] output = new double[N];

            // Длина импульсной характеристики
            int impulseLen = (int)(5.0 / dt) + 1;
            double[] h = new double[impulseLen];
            for (int i = 0; i < impulseLen; i++)
            {
                double t = i * dt;
                h[i] = ImpulseResponse(t);
            }

            // Вычисление свертки (линейная свертка)
            for (int n = 0; n < N; n++)
            {
                double sum = 0;
                for (int k = 0; k < impulseLen && k <= n; k++)
                {
                    sum += h[k] * input[n - k];
                }
                output[n] = sum * dt;  // Умножаем на dt для приближения интеграла
            }

            return output;
        }

        // Расчет дисперсии
        private double Variance(double[] x)
        {
            double mean = 0;
            for (int i = 0; i < x.Length; i++) mean += x[i];
            mean /= x.Length;

            double var = 0;
            for (int i = 0; i < x.Length; i++) var += (x[i] - mean) * (x[i] - mean);
            return var / x.Length;
        }

        // Расчет корреляционной функции
        private double[] Correlation(double[] x, int maxLag)
        {
            int N = x.Length;
            double mu = 0;
            for (int i = 0; i < N; i++) mu += x[i];
            mu /= N;

            double[] R = new double[maxLag + 1];
            for (int m = 0; m <= maxLag; m++)
            {
                double sum = 0;
                for (int n = 0; n < N - m; n++)
                    sum += (x[n] - mu) * (x[n + m] - mu);
                R[m] = sum / N;
            }
            return R;
        }

        // Главная функция моделирования
        private void RunSimulation()
        {
            int N = (int)nudN.Value;
            double dt = (double)nudDt.Value;

            // 1. Временная шкала
            double[] t = new double[N];
            for (int i = 0; i < N; i++)
                t[i] = i * dt;

            // 2. Импульсная характеристика
            double[] h = new double[N];
            for (int i = 0; i < N; i++)
                h[i] = ImpulseResponse(t[i]);

            // 3. Белый шум на входе
            double[] input = new double[N];
            for (int i = 0; i < N; i++)
                input[i] = Gauss();

            // 4. Сигнал на выходе (свертка)
            double[] output = Convolve(input, dt);

            // 5. Расчет статистик
            double inputVar = Variance(input);
            double outputVar = Variance(output);

            // 6. Корреляционные функции
            int maxLag = Math.Min(100, N / 4);
            double[] Rinput = Correlation(input, maxLag);
            double[] Routput = Correlation(output, maxLag);

            // 7. Обновление информации
            lblInfo.Text = string.Format(
                "Результаты: Дисперсия входа = {0:F4} | Дисперсия выхода = {1:F4} | " +
                "Отношение D_out/D_in = {2:F4} | Количество точек = {3} | dt = {4:F3}",
                inputVar, outputVar, outputVar / inputVar, N, dt);

            // 8. Отрисовка графиков
            DrawGraph(pbImpulse, t, h, "Импульсная характеристика k(t) = 0.2·cos(0.5t + 7)",
                C_ORANGE, 0, 0.25);
            DrawGraph(pbInput, t, input, "Входной сигнал: Белый шум (нормальный)",
                C_BLUE, -4, 4);
            DrawGraph(pbOutput, t, output, "Выходной сигнал: Результат свертки с k(t)",
                C_GREEN, -2, 2);

            // Дополнительно: показать корреляцию на выходе во всплывающем окне
            ShowCorrelationWindow(Routput, maxLag);
        }

        // Отрисовка графика
        private void DrawGraph(PictureBox pb, double[] x, double[] y, string title, Color color, double yMin, double yMax)
        {
            if (pb.Width < 10 || pb.Height < 10) return;

            Bitmap bmp = new Bitmap(pb.Width, pb.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(C_PANEL);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                int left = 60, right = 20, top = 30, bottom = 40;
                int width = pb.Width - left - right;
                int height = pb.Height - top - bottom;

                // Заголовок
                using (Font titleFont = new Font("Segoe UI", 9, FontStyle.Bold))
                    g.DrawString(title, titleFont, Brushes.White, left, 5);

                // Автоматическое определение Y-границ если не заданы
                if (yMin == 0 && yMax == 0)
                {
                    yMin = y[0]; yMax = y[0];
                    for (int i = 0; i < y.Length; i++)
                    {
                        if (y[i] < yMin) yMin = y[i];
                        if (y[i] > yMax) yMax = y[i];
                    }
                    double range = yMax - yMin;
                    yMin -= range * 0.1;
                    yMax += range * 0.1;
                }
                double yr = yMax - yMin;

                // Сетка
                using (Pen gridPen = new Pen(Color.FromArgb(50, 80, 80, 100)))
                using (Font font = new Font("Consolas", 7))
                using (Brush brush = new SolidBrush(Color.FromArgb(150, 150, 170)))
                {
                    for (int i = 0; i <= 5; i++)
                    {
                        float yy = top + (i / 5f) * height;
                        g.DrawLine(gridPen, left, yy, left + width, yy);
                        double val = yMax - (i / 5f) * yr;
                        g.DrawString(val.ToString("F2"), font, brush, left - 45, yy - 5);
                    }
                }

                // Нулевая линия
                float yZero = top + (float)((yMax) / yr * height);
                using (Pen zeroPen = new Pen(Color.FromArgb(100, 255, 255, 255)))
                    g.DrawLine(zeroPen, left, yZero, left + width, yZero);

                // График сигнала
                int step = Math.Max(1, x.Length / 2000);
                int points = x.Length / step;
                if (points > 1)
                {
                    PointF[] pts = new PointF[points];
                    float xStep = (float)width / (points - 1);
                    for (int i = 0; i < points; i++)
                    {
                        float px = left + i * xStep;
                        float py = top + (float)((yMax - y[i * step]) / yr * height);
                        pts[i] = new PointF(px, Math.Max(top, Math.Min(top + height, py)));
                    }
                    using (Pen linePen = new Pen(color, 1.2f))
                        g.DrawLines(linePen, pts);
                }

                // Оси
                using (Pen axisPen = new Pen(Color.FromArgb(100, 100, 120)))
                {
                    g.DrawRectangle(axisPen, left, top, width, height);

                    // Подписи по оси X
                    for (int i = 0; i <= 4; i++)
                    {
                        float px = left + (i / 4f) * width;
                        double val = (i / 4f) * x[x.Length - 1];
                        g.DrawString(val.ToString("F1"), new Font("Consolas", 7),
                            Brushes.Gray, px - 10, top + height + 5);
                    }
                }

                // Подписи осей
                g.DrawString("t (сек)", new Font("Segoe UI", 8), Brushes.Gray,
                    left + width / 2 - 20, pb.Height - 20);
                g.DrawString("k(t)", new Font("Segoe UI", 8), Brushes.Gray, 5, top + height / 2);
            }
            pb.Image = bmp;
        }

        // Отображение корреляционной функции в отдельном окне
        private void ShowCorrelationWindow(double[] R, int maxLag)
        {
            Form corrForm = new Form
            {
                Text = "Корреляционная функция выходного сигнала",
                Size = new Size(800, 500),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = C_BG
            };

            PictureBox pb = new PictureBox { Dock = DockStyle.Fill, BackColor = C_PANEL };
            corrForm.Controls.Add(pb);

            // Отрисовка корреляции
            Bitmap bmp = new Bitmap(pb.Width, pb.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(C_PANEL);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                int left = 70, right = 30, top = 40, bottom = 50;
                int width = pb.Width - left - right;
                int height = pb.Height - top - bottom;

                g.DrawString("Корреляционная функция R_Y(m)",
                    new Font("Segoe UI", 12, FontStyle.Bold), Brushes.White, left, 10);

                // Находим максимум
                double maxVal = 0;
                for (int m = 0; m <= maxLag; m++)
                    if (Math.Abs(R[m]) > maxVal) maxVal = Math.Abs(R[m]);
                maxVal *= 1.1;
                if (maxVal < 0.01) maxVal = 1;

                // Сетка
                for (int i = -5; i <= 5; i++)
                {
                    float y = top + (1 - (i + 5) / 10f) * height;
                    g.DrawLine(Pens.Gray, left, y, left + width, y);
                    double val = maxVal * i / 5;
                    g.DrawString(val.ToString("F3"), new Font("Consolas", 8),
                        Brushes.Gray, left - 55, y - 5);
                }

                // Нулевая линия
                float yZero = top + (float)((maxVal) / (2 * maxVal) * height);
                g.DrawLine(new Pen(Color.White), left, yZero, left + width, yZero);

                // Столбцы корреляции
                float barWidth = (float)width / (maxLag + 1);
                for (int m = 0; m <= maxLag; m++)
                {
                    float x = left + m * barWidth + barWidth * 0.15f;
                    float barH = (float)(Math.Abs(R[m]) / maxVal * height);
                    float y = (R[m] >= 0) ? yZero - barH : yZero;
                    using (Brush br = new SolidBrush(Color.FromArgb(180, C_GREEN)))
                        g.FillRectangle(br, x, y, barWidth * 0.7f, barH);
                }

                // Подписи
                for (int m = 0; m <= maxLag; m += 10)
                {
                    float x = left + m * barWidth + barWidth / 2;
                    g.DrawString(m.ToString(), new Font("Consolas", 8),
                        Brushes.Gray, x - 5, top + height + 5);
                }
                g.DrawString("m (задержка)", new Font("Segoe UI", 10),
                    Brushes.Gray, left + width / 2 - 40, pb.Height - 25);

                g.DrawRectangle(Pens.DarkGray, left, top, width, height);
            }
            pb.Image = bmp;

            corrForm.ShowDialog();
        }
    }
}
