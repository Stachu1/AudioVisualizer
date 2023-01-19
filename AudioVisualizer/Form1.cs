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

namespace AudioVisualizer
{
    public partial class Form1 : Form
    {
        ez ez = new ez();
        public Form1()
        {
            InitializeComponent();
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
            g.DrawRectangle(new Pen(Color.Red), 0, 0, pictureBox1.Width-1, pictureBox1.Height-1);
        }
    }
}
