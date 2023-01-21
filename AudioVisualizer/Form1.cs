using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AudioVisualizer
{
    public partial class SpotifyAudioVisualizer : Form
    {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);

        OverLay overLay = new OverLay();
        Int16[] dataPCM;
        double[] dataFFT;
        int[] dataFFTValueCorrection = { 1600, 1250, 950, 675, 310, 250, 230, 210, 160, 150, 140, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 120, 120, 120, 120, 120, 115, 110, 110, 110, 110, 100, 100, 100, 95, 90, 90, 90, 80, 70, 70, 70, 70, 70, 75, 75, 75, 75, 75, 75, 70, 70, 70, 70, 70, 70, 70, 70, 70, 70, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 60, 60, 60, 60, 60, 60, 60, 55, 50, 45, 45, 40, 35, 30, 25, 20, 15, 13, 12, 10 };

        int sampleRate = 32_000;
        int bitRate = 16;
        int fftPoints = 128;

        double amplitude_factor = 0.5;
        double max_amplitude_factor = 5.0;
        Point mousePos = new Point();
        bool mouseDown;

        Pen pen1;
        Pen pen2;
        Pen pen3;

        Brush brush1;
        Brush brush2;
        Brush brush3;

        Color colorTheme = Color.FromArgb(30, 215, 96);
        Color colorBackground = Color.FromArgb(94, 94, 94);

        private static Random rand = new Random();

        Slider amplitudeSlider;


        public SpotifyAudioVisualizer()
        {
            InitializeComponent();
            AudioMonitorInitialize(0, sampleRate, bitRate);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            this.WindowState = FormWindowState.Minimized;
            overLay.SetInvisibility(this);
            overLay.StartLoop(10, "Spotify Premium", this);
            this.WindowState = FormWindowState.Normal;
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Size = this.Size;
            UpdatePens();
            amplitudeSlider = new Slider(new Point(52, 68), 180, 4, 6, max_amplitude_factor, amplitude_factor, colorBackground, Color.White, colorTheme);
        }

        private void UpdatePens()
        {
            pen1 = new Pen(Color.White);
            pen2 = new Pen(colorTheme);
            pen3 = new Pen(colorBackground, 2);

            brush1 = new SolidBrush(Color.White);
            brush2 = new SolidBrush(colorTheme);
            brush3 = new SolidBrush(colorBackground);
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (overLay.isFullScreen)
            {
                DrawFFT(g, 0);
            }
            else if (overLay.isMaximized)
            {
                DrawFFT(g, 99);
                DrawWav(g, 7);
                amplitudeSlider.pos = new Point(59, 77);
                amplitudeSlider.Draw(g, pictureBox1.Size);
            }
            else
            {
                DrawFFT(g);
                DrawWav(g);
                amplitudeSlider.pos = new Point(52, 68);
                amplitudeSlider.Draw(g, pictureBox1.Size);
            }
        }

        private void DrawWav(Graphics g, int x = 0, int y = 3, int width = 252, int height = 50, int threshold = 15, int frame = 3)
        {
            g.FillRectangle(new SolidBrush(Color.Black), x, y - frame, width, height + 2 * frame);
            if (dataPCM != null)
            {
                int max_value = dataPCM.Max();
                int delta = dataPCM.Length / width;
                if (max_value > threshold)
                {
                    for (int i = dataPCM.Length; i >= 0; i--)
                    {
                        if (i % delta == 0 && i / delta < width)
                        {
                            g.FillRectangle(brush2, x + i / delta, y + height / 2 + (dataPCM[i] * height) / (2 * max_value), 1, 1);
                        }
                    }
                }
                else
                {
                    g.DrawLine(pen2, x, y + height / 2, x + width, y + height / 2);
                }
            }
        }

        private void DrawFFT(Graphics g, int bottomOffset = 90, int threshold = 15)
        {
            int amplitude = (int)(pictureBox1.Height * amplitude_factor);
            int y_min = pictureBox1.Height - bottomOffset;

            double delta = (double)pictureBox1.Width / (double)(fftPoints * 2);
            Pen pen = new Pen(colorTheme, (int)delta-3);
            if (dataFFT != null)
            {
                if (dataPCM.Max() > threshold)
                {
                    for (int i = fftPoints - 1; i >= 0; i--)
                    {
                        int xL = (int)(pictureBox1.Width / 2) - (int)(i * delta);
                        int xR = (int)(pictureBox1.Width / 2) + (int)(i * delta);
                        int y = y_min - (int)(dataFFT[fftPoints - 1 - i] * amplitude / dataFFTValueCorrection[fftPoints - 1 - i]);
                        g.DrawLine(pen, xL, y_min, xL, y);
                        g.DrawLine(pen, xR, y_min, xR, y);
                    }
                }
            }
        }

        private NAudio.Wave.WaveInEvent wvin;
        private void AudioMonitorInitialize(
                int DeviceIndex, int sampleRate = 32_000,
                int bitRate = 16, int channels = 1,
                int bufferMilliseconds = 30, bool start = true)
        {
            if (wvin == null)
            {
                wvin = new NAudio.Wave.WaveInEvent();
                wvin.DeviceNumber = DeviceIndex;
                wvin.WaveFormat = new NAudio.Wave.WaveFormat(sampleRate, bitRate, channels);
                wvin.DataAvailable += OnDataAvailable;
                wvin.BufferMilliseconds = bufferMilliseconds;
                if (start)
                    wvin.StartRecording();
            }
        }

        private void OnDataAvailable(object sender, NAudio.Wave.WaveInEventArgs args)
        {
            int bytesPerSample = wvin.WaveFormat.BitsPerSample / 8;
            int samplesRecorded = args.BytesRecorded / bytesPerSample;
            if (dataPCM == null)
                dataPCM = new Int16[samplesRecorded];
            for (int i = 0; i < samplesRecorded; i++)
                dataPCM[i] = BitConverter.ToInt16(args.Buffer, i * bytesPerSample);
        }

        private void UpdateFFT()
        {
            NAudio.Dsp.Complex[] fftFull = new NAudio.Dsp.Complex[fftPoints*2];
            for (int i = 0; i < fftPoints*2; i++)
                fftFull[i].X = (float)(dataPCM[i] * NAudio.Dsp.FastFourierTransform.HammingWindow(i, fftPoints*2));
            NAudio.Dsp.FastFourierTransform.FFT(true, (int)Math.Log(fftPoints*2, 2.0), fftFull);

            if (dataFFT == null)
                dataFFT = new double[fftPoints];
            for (int i = 0; i < fftPoints; i++)
            {
                double fftLeft = Math.Abs(fftFull[i].X + fftFull[i].Y);
                double fftRight = Math.Abs(fftFull[fftPoints*2 - i - 1].X + fftFull[fftPoints*2 - i - 1].Y);
                dataFFT[i] = fftLeft + fftRight;
            }
        }

        private void UpdateMouse()
        {
            GetCursorPos(ref mousePos);
            if (BitConverter.GetBytes(GetAsyncKeyState(Keys.LButton))[1] == 128)
            {
                mouseDown = true;
            }
            else
            {
                mouseDown = false;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateFFT();
            UpdateMouse();
            if (!overLay.isFullScreen)
            {
                amplitude_factor = amplitudeSlider.Update(pictureBox1.Size, this.Location, mousePos, mouseDown);
            }
            pictureBox1.Invalidate();
        }
    }
}
