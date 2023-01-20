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
    public partial class Form1 : Form
    {
        ez ez = new ez();
        Int16[] dataPCM;
        double[] dataFFT;
        double[] dataFFTValueCorrection = { 1744.44561767578, 949.0498046875, 671.035949707031, 441.154159545898, 416.147842407227, 282.484802246094, 220.120414733887, 286.392852783203, 236.505523681641, 225.483818054199, 248.550323486328, 225.959716796875, 172.542110443115, 170.821952819824, 174.250930786133, 165.000381469727, 136.69123840332, 106.758144378662, 108.441200256348, 140.780607223511, 126.643142700195, 124.244541168213, 123.862510681152, 108.45686340332, 102.836292266846, 83.0367851257324, 140.644044876099, 126.801746368408, 123.317764282227, 107.77293586731, 102.825801849365, 85.311595916748, 68.954719543457, 73.318058013916, 100.023307800293, 89.4388465881348, 75.352352142334, 46.7224063873291, 41.6807613372803, 62.4423885345459, 71.3994216918945, 61.8595943450928, 91.3947715759277, 122.447257995605, 133.924171447754, 86.2712554931641, 101.191776275635, 105.062107086182, 78.8742036819458, 79.3381175994873, 91.7759056091309, 93.5965480804443, 94.718822479248, 121.451545715332, 123.623573303223, 108.323925018311, 94.396167755127, 74.1161785125732, 103.126308441162, 126.481956481934, 114.928871154785, 86.6425094604492, 92.6955909729004, 83.7459831237793, 79.2258586883545, 119.438385009766, 96.0564422607422, 102.328227996826, 97.4920425415039, 91.2356224060059, 79.058521270752, 68.7654190063477, 93.053825378418, 64.5229778289795, 50.9297161102295, 48.6778564453125, 64.2419319152832, 68.8266525268555, 46.4721374511719, 45.818489074707, 51.8794612884521, 46.979305267334, 57.4979190826416, 45.9525928497314, 41.8240795135498, 40.3243026733398, 48.2340335845947, 42.9734945297241, 44.4311971664429, 43.1187171936035, 36.8598728179932, 43.5892162322998, 38.3463478088379, 44.9562702178955, 40.4747829437256, 33.6572341918945, 49.6550006866455, 46.0744752883911, 49.3917007446289, 44.9337892532349, 35.2822494506836, 45.0261058807373, 30.0520086288452, 34.0560646057129, 33.1774215698242, 40.2188320159912, 43.9030618667603, 51.7645568847656, 33.1556529998779, 49.2877464294434, 40.53688621521, 38.7720775604248, 35.7769346237183, 40.3191776275635, 50.1834526062012, 40.1870555877686, 35.7214107513428, 34.2014961242676, 35.0673246383667, 26.8559713363647, 29.7399091720581, 38.1883134841919, 27.607780456543, 19.0609855651855, 18.0833511352539, 12.7863068580627, 10.7232823371887, 11.1550121307373 };

        int sampleRate = 32_000;
        int bitRate = 16;
        int fftPoints = 128;


        Pen pen1 = new Pen(Color.White);
        Pen pen2 = new Pen(Color.FromArgb(30, 215, 96));
        Brush brush1 = new SolidBrush(Color.White);
        Brush brush2 = new SolidBrush(Color.FromArgb(30, 215, 96));
        public Form1()
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
            int delta = (int)Math.Ceiling((double)pictureBox1.Width / (double)fftPoints);
            Pen pen = new Pen(Color.FromArgb(30, 215, 96), delta-1);
            if (dataFFT != null)
            {
                if (dataPCM.Max() > threshold)
                {
                    //double[] data = NormalizeData(dataFFT);
                    for (int i = 0; i < fftPoints; i++)
                    {    
                        //if (dataFFT[i] > dataFFTValueCorrection[i])
                        //{
                        //    dataFFTValueCorrection[i] = dataFFT[i];
                        //}
                        g.DrawLine(pen, i*delta, y_min, i*delta, y_min - (int)(dataFFT[i] * amplitude / dataFFTValueCorrection[i]));
                    }
                }
                //else
                //{
                //    using (StreamWriter sr = new StreamWriter("data.txt"))
                //    {
                //        foreach (var item in dataFFTValueCorrection)
                //        {
                //            sr.Write(item);
                //            sr.Write(", ");
                //        }
                //    }
                //}
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
