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
        Int16[] dataPcm;

        Pen pen1 = new Pen(Color.White);
        Pen pen2 = new Pen(Color.LightGreen);
        Brush brush1 = new SolidBrush(Color.White);
        Brush brush2 = new SolidBrush(Color.LightGreen);
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
            g.DrawRectangle(pen2, 0, 0, pictureBox1.Width-1, pictureBox1.Height-1);
            DrawWav(g);
        }

        private void DrawWav(Graphics g, int x = 0, int y = 3, int width = 251, int height = 50)
        {
            g.FillRectangle(new SolidBrush(Color.Black), 20, 15, 30, 30);
            if (dataPcm != null)
            {
                int max_value = dataPcm.Max();
                int delta = dataPcm.Length / width;
                if (max_value > 15)
                {
                    for (int i = dataPcm.Length; i >= 0; i--)
                    {
                        if (i % delta == 0 && i / delta <= width)
                        {
                            g.FillRectangle(brush2, x + i / delta, y + height / 2 + (dataPcm[i] * height) / (2 * max_value), 1, 1);
                        }
                    }
                }
                else
                {
                    g.DrawLine(pen2, x, y + height / 2, x + width, y + height / 2);
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
            if (dataPcm == null)
                dataPcm = new Int16[samplesRecorded];
            for (int i = 0; i < samplesRecorded; i++)
                dataPcm[i] = BitConverter.ToInt16(args.Buffer, i * bytesPerSample);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }
    }
}
