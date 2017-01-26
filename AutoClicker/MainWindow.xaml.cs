using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace AutoClicker
{
    

    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        private const int MOUSEEVENTF_LEFTDOWN = 0x0002; /* left button down */
        private const int MOUSEEVENTF_LEFTUP = 0x0004; /* left button up */

        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        DispatcherTimer coolDownTimer = new DispatcherTimer();
        Point clickLocation = new Point(0, 0);
        Point previousLocation = new Point(0, 0);
        bool clicking = false;
        bool onCoolDown = false;
        int clicks = 0;
        int millis = 0;
        DateTime coolDown;
        long remainingCoolDown = 0;

        public MainWindow()
        {
            InitializeComponent();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            coolDownTimer.Tick += new EventHandler(coolDownTimer_Tick);
            coolDownTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            coolDown = new DateTime();
            coolDownTimer.Start();
        }        

        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void startStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (!clicking)
            {
                Int32.TryParse(timeBox.Text, out millis);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, millis);
                dispatcherTimer.Start();
                previousLocation = new Point(0, 0);
                startStopButton.Content = "Stop";
            }
            else
            {
                clicks = 0;
                dispatcherTimer.Stop();
                startStopButton.Content = "Start";
            }
            clicking = !clicking;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {

            if (!onCoolDown)
            {
                if (Point.Equals(clickLocation, previousLocation))
                {
                    clicks++;
                    LeftMouseClick((int)clickLocation.X, (int)clickLocation.Y);
                    UpdateLabels();
                }
                else
                {
                    onCoolDown = true;
                    coolDown = DateTime.Now;
                }
            }
        }

        private void coolDownTimer_Tick(object sender, EventArgs e)
        {
            clickLocation = GetMousePosition();

            if (onCoolDown)
            {
                if(Point.Equals(clickLocation, previousLocation))
                {
                    DateTime currentTime = DateTime.Now;
                    remainingCoolDown = ((coolDown + TimeSpan.FromSeconds(1)) - currentTime).Milliseconds;
                    if (currentTime > coolDown + TimeSpan.FromSeconds(1))
                    {
                        remainingCoolDown = 0;
                        onCoolDown = false;
                        //coolDownTimer.Stop();
                    }
                }
                else
                {
                    previousLocation = clickLocation;
                    coolDown = DateTime.Now;
                    remainingCoolDown = 1000;
                }
                
            }
            
            UpdateLabels();
        }

        public void UpdateLabels()
        {
            label1.Content = "Clicks: " + clicks;
            label2.Content = "Position: " + (int)clickLocation.X + ", " + (int)clickLocation.Y;
            if (onCoolDown)
            {
                label3.Content = "Cooldown: " + remainingCoolDown;
            }
            else
            {
                label3.Content = "";
            }
        }

        public static void LeftMouseClick(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            dispatcherTimer.Stop();
        }

    }
}
