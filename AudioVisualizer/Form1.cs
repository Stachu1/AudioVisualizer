using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;

namespace AudioVisualizer
{
    public partial class SpotifyAudioVisualizer : Form
    {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string IpClassName, string IpWindowName);


        OverLay overLay = new OverLay();
        Int16[] dataPCM;
        double[] dataFFT;
        int[] dataFFTValueCorrection = { 1600, 1250, 950, 675, 310, 250, 230, 210, 160, 150, 140, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 130, 120, 120, 120, 120, 120, 115, 110, 110, 110, 110, 100, 100, 100, 95, 90, 90, 90, 80, 70, 70, 70, 70, 70, 75, 75, 75, 75, 75, 75, 70, 70, 70, 70, 70, 70, 70, 70, 70, 70, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 60, 60, 60, 60, 60, 60, 60, 55, 50, 45, 45, 40, 35, 30, 25, 20, 15, 13, 12, 10 };

        int sampleRate = 32_000;
        int bitRate = 16;
        int fftPoints = 128;

        double amplitude_factor = 0.5;
        double max_amplitude_factor = 3.0;

        IntPtr spotifyPtr;

        Point mousePos = new Point();
        bool mouseDown;
        bool mouseClicked;

        Pen colorPen;
        Brush colorBrush;

        Color colorTheme = Color.FromArgb(30, 215, 96);
        Color colorBackground = Color.FromArgb(94, 94, 94);

        private static Random rand = new Random();

        Slider amplitudeSlider;
        Slider RSlider;
        Slider GSlider;
        Slider BSlider;
        List<Slider> SliderList = new List<Slider>();




        public SpotifyAudioVisualizer()
        {
            spotifyPtr = StartSpotify();
            InitializeComponent();
            AudioMonitorInitialize(0, sampleRate, bitRate);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            this.WindowState = FormWindowState.Minimized;
            overLay.SetInvisibility(this);
            overLay.StartLoop(spotifyPtr, this);
            this.WindowState = FormWindowState.Normal;
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Size = this.Size;
            UpdateColor();
            amplitudeSlider = new Slider(new Point(59, 86), new Point(67, 97), 180, 4, 6, max_amplitude_factor, amplitude_factor, colorBackground, Color.White, colorTheme);
            RSlider = new Slider(new Point(59, 22), new Point(67, 32), 54, 4, 4, 255, 30, colorBackground, Color.White, Color.FromArgb(255, 0, 0));
            GSlider = new Slider(new Point(122, 22), new Point(130, 32), 54, 4, 4, 255, 215, colorBackground, Color.White, Color.FromArgb(0, 255, 0));
            BSlider = new Slider(new Point(185, 22), new Point(193, 32), 54, 4, 4, 255, 96, colorBackground, Color.White, Color.FromArgb(0, 0, 255));
            SliderList.Add(amplitudeSlider);
            SliderList.Add(RSlider);
            SliderList.Add(GSlider);
            SliderList.Add(BSlider);
        }

        private IntPtr StartSpotify()
        {
            foreach (var process in Process.GetProcessesByName("Spotify"))
            {
                process.Kill();
            }
            string username = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString().Split('\\')[1];
            string path = "C:\\Users\\" + username + "\\AppData\\Roaming\\Spotify\\Spotify.exe";
            Process proc = Process.Start(path, "--win7");
            IntPtr hndl = IntPtr.Zero;
            while(hndl == IntPtr.Zero)
            {
                hndl = FindWindow(null, "Spotify Premium");
            }
            return hndl;
        }

        private void UpdateColor()
        {
            colorPen = new Pen(colorTheme);
            colorBrush = new SolidBrush(colorTheme);
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (overLay.isFullScreen)
            {   
                DrawFFT(g, 0, 15, 0);
            }
            else if (overLay.isMaximized)
            {
                DrawFFT(g, 120, 15, 5);
                DrawWav(g, 79, 8, 400, 48);
                foreach (Slider slider in SliderList)
                {
                    slider.Draw(g, pictureBox1.Size);
                }
            }
            else
            {
                DrawFFT(g, 109, 15, 0);
                DrawWav(g, 80, 1, 400, 48);
                foreach (Slider slider in SliderList)
                {
                    slider.Draw(g, pictureBox1.Size);
                }
            }
        }

        private void DrawWav(Graphics g, int x, int y, int width, int height, int threshold = 15)
        {
            g.FillRectangle(new SolidBrush(Color.Wheat), x, y, width, height);
            if (dataPCM != null)
            {
                int max_value = Math.Max(dataPCM.Max(), -dataPCM.Min());
                if (max_value > threshold)
                {
                    int delta = dataPCM.Length / width;
                    for (int i = dataPCM.Length; i >= 0; i--)
                    {
                        if (i % delta == 0 && i / delta <= width - 1)
                        {
                            g.FillRectangle(colorBrush, x + i / delta, y + height / 2 + (dataPCM[i] * height) / (2 * max_value) - 1, 1, 1);
                        }
                    }
                }
                else
                {
                    g.DrawLine(colorPen, x, y + height / 2, x + width - 1, y + height / 2);
                }
            }
        }

        private void DrawFFT(Graphics g, int bottomOffset = 109, int threshold = 15, int sideOffset = 0)
        {
            int amplitude = (int)(pictureBox1.Height * amplitude_factor);
            int y_min = pictureBox1.Height - bottomOffset;

            double delta = (double)(pictureBox1.Width - sideOffset * 2) / (double)(fftPoints * 2);
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
            byte[] values = BitConverter.GetBytes(GetAsyncKeyState(Keys.LButton));
            if (values[0] == 1)
            {
                mouseClicked = true;
            }
            else
            {
                mouseClicked = false;
            }
            if (values[1] == 128)
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
            if (overLay.isMaximized)
            {
                foreach (Slider slider in SliderList)
                {
                    slider.pos = slider.pos2;
                }
            }
            if (!overLay.isFullScreen && !overLay.isMaximized)
            {
                foreach (Slider slider in SliderList)
                {
                    slider.pos = slider.pos1;
                }
            }
            if (!overLay.isFullScreen)
            {
                amplitude_factor = amplitudeSlider.Update(pictureBox1.Size, this.Location, mousePos, mouseClicked, mouseDown);
                colorTheme = Color.FromArgb((int)RSlider.Update(pictureBox1.Size, this.Location, mousePos, mouseClicked, mouseDown),
                                            (int)GSlider.Update(pictureBox1.Size, this.Location, mousePos, mouseClicked, mouseDown),
                                            (int)BSlider.Update(pictureBox1.Size, this.Location, mousePos, mouseClicked, mouseDown));
            }
            UpdateColor();
            amplitudeSlider.colorWhenSelected = colorTheme;
            amplitudeSlider.ApplyColor();
            pictureBox1.Invalidate();
        }
    }
}
