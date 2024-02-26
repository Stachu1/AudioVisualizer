using System;
using System.Drawing;

namespace AudioVisualizer
{
    public class Slider
    {
        public Point pos;
        public Point pos1;
        public Point pos2;
        public int length;
        public int width;
        public int selectThreshold;
        public double range;
        public double value;
        public Color colorBackground;
        public Color colorMain;
        public Color colorWhenSelected;
        public Pen penBackground;
        public Pen penMain;
        public Pen penWhenSelected;
        public Brush brushMain;
        bool selected;
        bool hover;
        public int radius;


        public Slider(Point pos1, Point pos2, int length, int width, int selectThreshold, double range, double value, Color colorBackground, Color colorMain, Color colorWhenSelected)
        {
            this.pos1 = pos1;
            this.pos2 = pos2;
            this.length = length;
            this.width = width;
            this.selectThreshold = selectThreshold;
            this.range = range;
            this.value = value;
            this.colorBackground = colorBackground;
            this.colorMain = colorMain;
            this.colorWhenSelected = colorWhenSelected;
            radius = width * 3 / 2;
            ApplyColor();
        }

        public void ApplyColor()
        {
            penBackground = new Pen(colorBackground, width);
            penMain = new Pen(colorMain, width);
            penWhenSelected = new Pen(colorWhenSelected, width);
            brushMain = new SolidBrush(colorMain);
        }

        public void Draw(Graphics g, Size picBoxSize)
        {
            int x = picBoxSize.Width - pos.X - length;
            int y = picBoxSize.Height - pos.Y;
            g.DrawLine(penBackground, x, y, x + length, y);
            int current_lenght = (int)(value * length / range);
            if (selected || hover)
            {
                g.DrawLine(penWhenSelected, x, y, x + current_lenght, y);
                g.FillEllipse(brushMain, new Rectangle(x + current_lenght - radius, y - radius, radius * 2, radius * 2));
            }
            else
            {
                g.DrawLine(penMain, x, y, x + current_lenght, y);
            }
        }

        public double Update(Size picBoxSize, Point picBoxPos, Point mousePos, bool mouseClicked, bool mouseDown)
        {
            int dX = picBoxPos.X + picBoxSize.Width - mousePos.X - pos.X - length / 2;
            int dY = picBoxPos.Y + picBoxSize.Height - mousePos.Y - pos.Y - width / 2;
            if (!mouseDown)
            {
                selected = false;
            }
            if (Math.Abs(dX) <= length / 2 + selectThreshold && Math.Abs(dY) <= width / 2 + selectThreshold)
            {
                hover = true;
                if (mouseClicked)
                {
                    selected = true;
                }
            }
            else
            {
                hover = false;
            }
            if (selected)
            {
                int x = mousePos.X + pos.X + length - picBoxPos.X - picBoxSize.Width;
                if (x <= 0)
                {
                    value = 0;
                }
                else if (x >= length)
                {
                    value = range;
                }
                else
                {
                    value = (double)x * range / (double)length;
                }
            }
            return value;
        }
    }
}
