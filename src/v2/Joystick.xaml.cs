using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace VirtualJoystick
{
    public sealed partial class Joystick : UserControl
    {
        private bool _lockY;

        private bool _pressed;

        private double _x, _y;

        private double _actualX, _actualY;

        private double _diameter;

        private double _radius;

        private double _sensitivity = 1;

        private readonly CompositeTransform _transform;

        private readonly Storyboard _storyboard;

        public bool LockY
        {
            get => _lockY;
            set => _lockY = value;
        }

        public bool Pressed => _pressed;

        public double X => _x;

        public double Y => _y;

        public double ActualX => _actualX;

        public double ActualY => _actualY;

        public double Diameter
        {
            get => _diameter;
            set => _diameter = value;
        }

        public double Radius
        {
            get => _radius;
            set => _radius = value;
        }

        public double Sensitivity
        {
            get => _sensitivity;
            set
            {
                value = Math.Abs(value);

                _sensitivity = value > 1 ? value / 100 : value;
            }
        }

        public double Size
        {
            get => (double)GetValue(SizeProperty);
            set
            {
                Width = Height = value;

                _diameter = value;

                _radius = _diameter / 2;

                SetValue(SizeProperty, value);
            }
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            nameof(Size),
            typeof(double),
            typeof(Joystick),
            new PropertyMetadata(default(double))
        );

        public Joystick()
        {
            InitializeComponent();

            _transform = (CompositeTransform)Stick.RenderTransform;

            _storyboard = Storyboard;

            Size = Width;

            _storyboard.Begin();
        }

        private void Stick_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            _storyboard.SpeedRatio *= 3;

            _pressed = true;
        }

        private void Stick_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            double x = e.Cumulative.Translation.X * _sensitivity,
                   y = e.Cumulative.Translation.Y * _sensitivity;

            double distance = Math.Sqrt(x * x + y * y);

            if (distance > _radius)
            {
                x = x / distance * _radius;
                y = y / distance * _radius;
            }

            _transform.TranslateX = _actualX = x;
            _transform.TranslateY = _actualY = y;

            _x =  _actualX / _radius;
            _y = -_actualY / _radius;
        }

        private void Stick_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            _transform.TranslateX = _x = _actualX = 0;
            _transform.TranslateY = _y = _actualY = 0;

            _storyboard.SpeedRatio /= 3;

            _pressed = false;
        }
    }
}