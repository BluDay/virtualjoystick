// ( 0 _ o )

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace DroneController.Controls
{
    public sealed partial class Joystick : UserControl
    {
        private bool _lockY   = false;
        private bool _pressed = false;

        private double _x   = 0;
        private double _y   = 0;
        private double _ax  = 0;
        private double _ay  = 0;
        private double _dia = 0;
        private double _rad = 0;
        private double _sen = 1;

        private readonly CompositeTransform _t;

        public double X => _x;
        public double Y => _y;

        public double ActualX => _ax;
        public double ActualY => _ay;

        public double Diameter
        {
            get => _dia;
            set => _dia = value;
        }

        public double Radius
        {
            get => _rad;
            set => _rad = value;
        }

        public double Sensitivity
        {
            get => _sen;
            set
            {
                value = Math.Abs(value);

                _sen = value > 1 ? value / 100 : value;
            }
        }

        public double Size
        {
            get => (double)GetValue(SizeProperty);
            set
            {
                base.Width = base.Height = value;

                _dia = Width;
                _rad = _dia / 2;

                SetValue(SizeProperty, value);
            }
        }

        public new double Width  => base.Width;
        public new double Height => base.Height;

        public bool LockY
        {
            get => _lockY;
            set => _lockY = value;
        }

        public bool Pressed => _pressed;

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size",
            typeof(double),
            typeof(Joystick),
            new PropertyMetadata(default(double))
        );

        public Joystick()
        {
            InitializeComponent();

            _t = (CompositeTransform)Stick.RenderTransform;

            Size = base.Width;

            Storyboard.Begin();
        }

        private void Stick_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            Storyboard.SpeedRatio *= 3;

            _pressed = true;
        }

        private void Stick_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            double x = e.Cumulative.Translation.X * _sen,
                   y = e.Cumulative.Translation.Y * _sen;

            double r = _rad;

            double distance = Math.Sqrt(x * x + y * y);

            if (distance > r)
            {
                x = x / distance * r;
                y = y / distance * r;
            }

            _t.TranslateX = _ax = x;
            _t.TranslateY = _ay = y;

            _x =  _ax / r;
            _y = -_ay / r;
        }

        private void Stick_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            _t.TranslateX = _x = _ax = 0;
            _t.TranslateY = _y = _ay = 0;

            Storyboard.SpeedRatio /= 3;

            _pressed = false;
        }
    }
}