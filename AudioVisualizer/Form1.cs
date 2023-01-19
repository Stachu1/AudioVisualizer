using System;
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
    public partial class Form1 : Form
    {
        ez ez = new ez();
        Int16[] dataPCM;
        double[] dataFFT;

        Pen pen1 = new Pen(Color.White);
        Pen pen2 = new Pen(Color.FromArgb(30, 215, 96));
        Brush brush1 = new SolidBrush(Color.White);
        Brush brush2 = new SolidBrush(Color.FromArgb(30, 215, 96));
        public Form1()
        {
            InitializeComponent();
            AudioMonitorInitialize(0);
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

        private void DrawFFT(Graphics g, int bottomOffset = 91, float amplitude_factor = 0.8f, int threshold = 15)
        {
            int amplitude = (int)(pictureBox1.Height * amplitude_factor);
            int y_min = pictureBox1.Height - bottomOffset;
            //g.DrawRectangle(pen2, 0, y_min-amplitude, pictureBox1.Width-1, amplitude);
            if (dataFFT != null)
            {
                if (dataPCM.Max() > threshold)
                {
                    double[] data = NormalizeData(dataFFT);
                    for (int i = 0; i < dataFFT.Length; i++)
                    {
                        if (i < pictureBox1.Width)
                        {
                            g.DrawLine(new Pen(Color.FromArgb(30, 215, 96), 5), i*7, y_min, i*7, y_min - (int)(dataFFT[i] * amplitude));
                        }
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
            // the PCM size to be analyzed with FFT must be a power of 2
            int fftPoints = 2;
            while (fftPoints * 2 <= dataPCM.Length)
                fftPoints *= 2;

            // apply a Hamming window function as we load the FFT array then calculate the FFT
            NAudio.Dsp.Complex[] fftFull = new NAudio.Dsp.Complex[fftPoints];
            for (int i = 0; i < fftPoints; i++)
                fftFull[i].X = (float)(dataPCM[i] * NAudio.Dsp.FastFourierTransform.HammingWindow(i, fftPoints));
            NAudio.Dsp.FastFourierTransform.FFT(true, (int)Math.Log(fftPoints, 2.0), fftFull);

            // copy the complex values into the double array that will be plotted
            if (dataFFT == null)
                dataFFT = new double[fftPoints / 2];
            for (int i = 0; i < fftPoints / 2; i++)
            {
                double fftLeft = Math.Abs(fftFull[i].X + fftFull[i].Y);
                double fftRight = Math.Abs(fftFull[fftPoints - i - 1].X + fftFull[fftPoints - i - 1].Y);
                dataFFT[i] = fftLeft + fftRight;
            }
        }

        private double[] NormalizeData(double[] data)
        {
            double max_value = data.Max();
            for (int i = 0; i < data.Count(); i++)
            {
                data[i] = data[i] / max_value;
            }
            return data;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            updateFFT();
            pictureBox1.Invalidate();
        }
    }
}
