using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Text;

namespace AudioVisualizer
{
    public class OverLay
    {
        public static IntPtr handle;

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string IpClassName, string IpWindowName);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]

        public static extern bool GetWindowRect(IntPtr hwnd, out RECT IpRect);

        public static RECT rect;

        public bool threadrunning = true;
        public struct RECT
        {
            public int left, top, right, bottom;
        }

        public bool isMaximized;
        public bool isFullScreen;

        public void setHandle(string window_name)
        {
            handle = FindWindow(null, window_name);
        }

        public void SetInvisibility(Form form)
        {
            form.BackColor = Color.Wheat;
            form.TransparencyKey = Color.Wheat;
            form.TopMost = true;
            form.FormBorderStyle = FormBorderStyle.None;
            ClickThrough(form.Handle);
        }

        public void GetRect()
        {
            GetWindowRect(handle, out rect);
        }

        public void ClickThrough(IntPtr formHandle)
        {
            int initialStyle = GetWindowLong(formHandle, -20);
            SetWindowLong(formHandle, -20, initialStyle | 0x8000 | 0x20);

        }

        public Size CalibrateSize()
        {
            Size size = new Size(rect.right - rect.left, rect.bottom - rect.top);
            return size;
        }

        public void Calibrate(Form form)
        {
            GetRect();
            form.Size = CalibrateSize();
            form.Left = rect.left;
            form.Top = rect.top;
            UpdateWindowState();
        }

        private void UpdateWindowState()
        {
            int windowStyle = GetWindowLong(handle, -16);
            if ((windowStyle & 0x00040000L) == 0x00040000L && (windowStyle & 0x01000000L) != 0x01000000L)
            {
                isMaximized = false;
                isFullScreen = false;
            }
            else if ((windowStyle & 0x00040000L) == 0x00040000L && (windowStyle & 0x01000000L) == 0x01000000L)
            {
                isMaximized = true;
                isFullScreen = false;
            }
            else
            {
                isMaximized = false;
                isFullScreen = true;
            }
        }

        public void PauseLoop()
        {
            threadrunning = false;
        }

        public void UnPauseLoop()
        {
            threadrunning = true;
        }

        //public void StartLoop(int frequency, string WindowName, Form form)
        //{
        //    while (handle == IntPtr.Zero)
        //    {
        //        setHandle(WindowName);
        //    }
        //    Thread lp = new Thread(() => LOOP(frequency, WindowName, form)) { IsBackground = true };
        //    lp.Start();

        //}

        public void StartLoop(IntPtr hndl, Form form, int frequency = 10)
        {
            handle = hndl;
            Thread lp = new Thread(() => LOOP(form, frequency)) { IsBackground = true };
            lp.Start();

        }

        public void LOOP(Form form, int frequency)
        {
            while (true)
            {
                if (threadrunning == true)
                {
                    Calibrate(form);

                }
                Thread.Sleep(frequency);

            }

        }
    }
}