namespace press_double
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Threading;

    using press_double.Models;

    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr windowHandle, int hotkeyId, uint modifierKeys, uint virtualKey);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr windowHandle, int hotkeyId);
    }

    public partial class MainWindow : Window
    {
        static readonly int MyHotKeyId = 0x3000;

        static readonly int WM_HOTKEY = 0x312;

        public SpeedValueModel Speedval;

        private Thread _autoclickthread;

        private bool _isactive;

        private bool _ismousebuttonleft;

        private bool _timerneeded;

        private int _timertime;

        private int dispatchersecs = 0;

        public MainWindow()
        {
            this.InitializeComponent();
            this.IsActive = false;
            this.TimerNeeded = false;
            this.IsMouseButtonLeft = true;
            this.AutoClickThread = new Thread(this.AutoClick);
            this.AutoClickThread.IsBackground = true;
            this.Speedval = new SpeedValueModel(1);
            this.AutoClickThread.Start();
        }

        public Thread AutoClickThread
        {
            get
            {
                return this._autoclickthread;
            }

            protected set
            {
                this._autoclickthread = value;
            }
        }

        public bool IsActive
        {
            get
            {
                return this._isactive;
            }

            set
            {
                this._isactive = value;
            }
        }

        public bool IsMouseButtonLeft
        {
            get
            {
                return this._ismousebuttonleft;
            }

            set
            {
                this._ismousebuttonleft = value;
            }
        }

        public bool TimerNeeded
        {
            get
            {
                return this._timerneeded;
            }

            set
            {
                this._timerneeded = value;
            }
        }

        public int TimerTime
        {
            get
            {
                return this._timertime;
            }

            set
            {
                this._timertime = value;
            }
        }

        internal void AutoClick()
        {
            SpinWait.SpinUntil(() => this.IsActive);
            if (this.IsMouseButtonLeft)
            {
                while (this.IsActive)
                {
                    MouseSimulator.ClickLeftMouseButton();

                    Thread.Sleep((int)(1000 - this.Speedval.SpeedValue * this.Speedval.Multiplier));
                    Thread.Sleep(1);
                }
            }
            else
            {
                while (this.IsActive)
                {
                    MouseSimulator.ClickRightMouseButton();

                    Thread.Sleep((int)(1000 - this.Speedval.SpeedValue * this.Speedval.Multiplier));
                    Thread.Sleep(1);
                }
            }

            this.AutoClick();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.InitializeHook();
            this.InitializeHotKey();
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            this.TimerNeeded = true;
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.TimerNeeded = false;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            this.dispatchersecs++;
            if (this.TimerTime < this.dispatchersecs)
            {
                this.IsActive = false;
            }
        }

        void InitializeHook()
        {
            var windowHelper = new WindowInteropHelper(this);
            var windowSource = HwndSource.FromHwnd(windowHelper.Handle);

            windowSource.AddHook(this.MessagePumpHook);
        }

        void InitializeHotKey()
        {
            var windowHelper = new WindowInteropHelper(this);
            uint modifiers = (uint)ModifierKeys.None;
            uint virtualKey = (uint)KeyInterop.VirtualKeyFromKey(Key.F8);

            NativeMethods.RegisterHotKey(windowHelper.Handle, MyHotKeyId, modifiers, virtualKey);
        }

        IntPtr MessagePumpHook(IntPtr handle, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                if ((int)wParam == MyHotKeyId)
                {
                    if (this.TimerNeeded)
                    {
                        try
                        {
                            this.TimerTime = Convert.ToInt32(this.textBox.Text);
                            this.dispatchersecs = 0;
                            DispatcherTimer dispatcherTimer = new DispatcherTimer();
                            dispatcherTimer.Tick += new EventHandler(this.dispatcherTimer_Tick);
                            dispatcherTimer.Interval = new TimeSpan(0, 0, this.TimerTime);
                            dispatcherTimer.Start();
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Timer not set, maybe error with the input!", "Error");
                        }
                    }

                    // The hotkey has been pressed, do something!
                    switch (this.IsActive)
                    {
                        case false:
                            this.IsActive = true;
                            break;
                        case true:
                            this.IsActive = false;
                            break;
                    }

                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        private void Rectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsMouseButtonLeft)
            {
                this.recmouseleft.Fill = new SolidColorBrush(Color.FromArgb(150, 255, 0, 0));
                this.recmouseright.Fill = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0));
                this.IsMouseButtonLeft = true;
            }
        }

        private void Rectangle_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            if (this.IsMouseButtonLeft)
            {
                this.recmouseright.Fill = new SolidColorBrush(Color.FromArgb(150, 255, 0, 0));
                this.recmouseleft.Fill = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0));
                this.IsMouseButtonLeft = false;
            }
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.Speedval.SpeedValue = (int)this.slider.Value;
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        void UninitializeHotKey()
        {
            var windowHelper = new WindowInteropHelper(this);
            NativeMethods.UnregisterHotKey(windowHelper.Handle, MyHotKeyId);
        }
    }
}