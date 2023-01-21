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
        //ez ez = new Over();
        Int16[] dataPCM;
        double[] dataFFT;
        int[] dataFFTValueCorrection = { 1600, 1250, 950, 675, 310, 250, 230, 210, 160, 150, 140, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 120, 120, 120, 120, 120, 115, 110, 110, 110, 110, 100, 100, 100, 95, 90, 90, 90, 80, 70, 70, 70, 70, 70, 75, 75, 75, 75, 75, 75, 70, 70, 70, 70, 70, 70, 70, 70, 70, 70, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 60, 60, 60, 60, 60, 60, 60, 55, 50, 45, 45, 40, 35, 30, 25, 20, 15, 13, 12, 10 };

        int sampleRate = 32_000;
        int bitRate = 16;
        int fftPoints = 128;

        float amplitude_factor = 0.5f;
        float max_amplitude_factor = 3.0f;
        bool amplitudeSliderSelected;
        Point pos = new Point();

        Pen pen1 = new Pen(Color.White);
        Pen pen2 = new Pen(Color.FromArgb(30, 215, 96));
        Pen pen3 = new Pen(Color.FromArgb(165, 165, 165), 2);
        Brush brush1 = new SolidBrush(Color.White);
        Brush brush2 = new SolidBrush(Color.FromArgb(30, 215, 96));

        Pen sliderPen1 = new Pen(Color.White, 4);
        Pen sliderPen2 = new Pen(Color.FromArgb(30, 215, 96), 4);
        Pen sliderPen3 = new Pen(Color.FromArgb(94, 94, 94), 4);

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
            
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            //g.DrawRectangle(pen2, 0, 0, pictureBox1.Width-1, pictureBox1.Height-1);
            DrawWav(g);
            DrawFFT(g);
            DrawAmplitudeSlider(g);
        }

        private void DrawWav(Graphics g, int x = 0, int y = 3, int width = 251, int height = 50, int threshold = 15)
        {
            g.FillRectangle(new SolidBrush(Color.Black), 20, 15, 35, 30);
            if (dataPCM != null)
            {
                int max_value = dataPCM.Max();
                int delta = dataPCM.Length / width;
                if (max_value > threshold)
                {
                    for (int i = dataPCM.Length; i >= 0; i--)
                    {
                        if (i % delta == 0 && i / delta <= width)
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

        private void DrawFFT(Graphics g, int bottomOffset = 91, int threshold = 15)
        {
            int amplitude = (int)(pictureBox1.Height * amplitude_factor);
            int y_min = pictureBox1.Height - bottomOffset;
            //g.DrawRectangle(pen2, 0, y_min-amplitude, pictureBox1.Width-1, amplitude);
            double delta = (double)pictureBox1.Width / (double)(fftPoints * 2);
            Pen pen = new Pen(Color.FromArgb(30, 215, 96), (int)delta-3);
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

        private void DrawAmplitudeSlider(Graphics g, int bottomOffset = 67, int rightOffset = 52, int width = 93)
        {
            int x = pictureBox1.Width - rightOffset - width;
            int y = pictureBox1.Height - bottomOffset;
            g.DrawLine(sliderPen3, x, y, x + width, y);
            int current_width = (int)(amplitude_factor / max_amplitude_factor * width);
            if (amplitudeSliderSelected)
            {
                g.DrawLine(sliderPen2, x, y, x + current_width, y);
                g.FillEllipse(brush1, new Rectangle(x + current_width - 6, y - 6, 12, 12));
            }
            else
            {
                g.DrawLine(sliderPen1, x, y, x + current_width, y);
            }
            g.DrawLine(pen3, x - 16, y - 8, x - 24, y + 8);
            g.DrawLine(pen3, x - 16, y - 8, x - 8, y + 8);
            g.DrawLine(pen3, x - 21, y + 2, x - 11, y + 2);
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


        private void UpdateAmplitudeSlider(int x, int y, int bottomOffset = 67, int rightOffset = 52, int width = 93)
        {
            if (Math.Abs(pictureBox1.Height - bottomOffset - y) < 6 && x < pictureBox1.Width - rightOffset + 6 && x > pictureBox1.Width - rightOffset - width - 6)
            {
                amplitudeSliderSelected = true;
                byte[] keys = BitConverter.GetBytes(GetAsyncKeyState(Keys.LButton));
                if (keys[1] == 128)
                {
                    amplitude_factor = ((float)(x - (pictureBox1.Width - width - rightOffset)) / (float)width) * max_amplitude_factor;
                    if (amplitude_factor < 0)
                    {
                        amplitude_factor = 0;
                    }
                }
            }
            else
            {
                amplitudeSliderSelected = false;
            }
        }

        private void UpdateMouse()
        {
            GetCursorPos(ref pos);
            int x = pos.X - this.Location.X;
            int y = pos.Y - this.Location.Y;
            if (x >= 0 && y >= 0)
            {
                if (x <= this.Width)
                {
                    if (y <= this.Height)
                    {
                        UpdateAmplitudeSlider(x ,y);
                    }
                }
            }
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateFFT();
            UpdateMouse();
            pictureBox1.Invalidate();
        }
    }
}
