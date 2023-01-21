using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace AudioVisualizer
{
    internal class Slider
    {
        Point pos;
        int length;
        int width;
        int selectThreshold;
        double range;
        double value;
        Pen penBackground;
        Pen penMain;
        Pen penWhenSelected;
        Brush brushMain;
        bool selected;
        bool hover;
        int radius;
        

        public Slider(Point pos, int length, int width, int selectThreshold, double range, double value, Color colorBackground, Color colorMain, Color colorWhenSelected)
        {
            this.pos = pos;
            this.length = length;
            this.width = width;
            this.selectThreshold = selectThreshold;
            this.range = range;
            this.value = value;
            
            penBackground = new Pen(colorBackground, width);
            penMain = new Pen(colorMain, width);
            penWhenSelected = new Pen(colorWhenSelected, width);
            brushMain = new SolidBrush(colorMain);
            radius = width * 3 / 2;
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

        public double Update(Size picBoxSize, Point picBoxPos, Point mousePos, bool mouseDown)
        {
            int dX = picBoxPos.X + picBoxSize.Width - mousePos.X - pos.X - length / 2;
            int dY = picBoxPos.Y + picBoxSize.Height - mousePos.Y - pos.Y - width / 2;
            if (Math.Abs(dX) <= length / 2 + selectThreshold && Math.Abs(dY) <= width / 2 + selectThreshold)
            {
                hover = true;
                if (mouseDown)
                {
                    selected = true;
                }
                else
                {
                    selected = false;
                }
            }
            else
            {
                hover = false;
                if (!mouseDown)
                {
                    selected = false;
                }
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
