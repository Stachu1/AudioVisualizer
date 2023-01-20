using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ezOverLay;
using NAudio;

namespace AudioVisualizer
{
    public partial class SpotifyAudioVisualizer : Form
    {
        ez ez = new ez();
        Int16[] dataPCM;
        double[] dataFFT;
        int[] dataFFTValueCorrection = { 2000, 1243, 951, 674, 308, 250, 208, 209, 158, 148, 142, 104, 124, 115, 120, 139, 145, 134, 109, 131, 126, 141, 135, 186, 198, 149, 106, 133, 175, 152, 104, 139, 108, 101, 90, 124, 100, 124, 107, 103, 112, 121, 107, 95, 85, 102, 94, 89, 84, 89, 70, 59, 66, 105, 88, 98, 63, 62, 72, 82, 88, 65, 67, 63, 71, 66, 73, 76, 81, 73, 82, 68, 61, 58, 61, 65, 69, 82, 77, 81, 73, 62, 91, 93, 89, 91, 78, 76, 67, 66, 72, 71, 66, 62, 79, 104, 70, 50, 46, 60, 77, 85, 79, 96, 69, 58, 73, 70, 61, 56, 56, 61, 93, 93, 91, 55, 51, 41, 33, 38, 40, 35, 24, 18, 14, 11, 12, 11 };
        //double[] dataFFTValueCorrection = new double[128];

        int sampleRate = 32_000;
        int bitRate = 16;
        int fftPoints = 128;


        Pen pen1 = new Pen(Color.White);
        Pen pen2 = new Pen(Color.FromArgb(30, 215, 96));
        Brush brush1 = new SolidBrush(Color.White);
        Brush brush2 = new SolidBrush(Color.FromArgb(30, 215, 96));
        public SpotifyAudioVisualizer()
        {
            InitializeComponent();
            AudioMonitorInitialize(0, sampleRate, bitRate);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            ez.SetInvi(this);
            ez.StartLoop(10, "Spotify Premium", this);
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Size = this.Size;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            //g.DrawRectangle(pen2, 0, 0, pictureBox1.Width-1, pictureBox1.Height-1);
            DrawWav(g);
            DrawFFT(g);
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

        private void DrawFFT(Graphics g, int bottomOffset = 91, float amplitude_factor = 0.4f, int threshold = 15)
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

        private void updateFFT()
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            updateFFT();
            pictureBox1.Invalidate();
        }
    }
}
