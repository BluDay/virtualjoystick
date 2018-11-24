/*************************************************************
 *                                                           *
 *                    VirtualJoystick                        *
 *                                                           *
 *             Futuristic UWP Joystick Control               *
 *                                                           *
 *           No comments are provided throughout             *
 *       this file. I require the privileged user to         *
 *    fully comprehend this code by closely examining it.    *
 *                                                           *
 *            https://github.com/engineer-186f               *
 *                                                           *
 *                          (0.o)                            *
 *                                                           *
 * ***********************************************************
 */

using System;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

namespace PX99_Interface
{
    public sealed partial class Joystick : UserControl
    {
        private Timer               timer;
        private Storyboard          storyboard;
        private GamepadManager      gamepad;
        private TranslateTransform  translateTransform;

        private bool enabled        = true;
        private bool masterMode     = true;
        private bool deviceIsLowEnd = false;
        private bool pressed        = false;
        private bool animated       = true;
        private bool gamepadEnabled = false;
        private bool yLocked        = false;
        private bool backColor      = true;

        private double defaultValue = 0;
        private double sensitivity  = 0;
        private double diameter     = 0;
        private double actualX      = 0;
        private double actualY      = 0;
        private double radius       = 0;
        private double x            = 0;
        private double y            = 0;

        private Int32 roundDecimal = 1;

        private Enum side;

        private const double BACK_JOYSTICK_NORMAL_SCALE     = 0.9;
        private const double BACK_JOYSTICK_ANIM_SPEED       = 5;
        private const double BACK_JOYSTICK_BIG_SCALE        = 1.15;
        private const double JOYSTICK_NORMAL_OPACITY        = 1;
        private const double JOYSTICK_DELTA_OPACITY         = 0.75;
        private const double JOYSTICK_SKEW_ANIM_VALUE       = 50;
        private const double JOYSTICK_HOVER_BG_ANIM_SPEED   = 0.1;
        private const double JOYSTICK_BG_ANIM_SPEED         = 0.05;

        public enum Sides { Left, Right };

        public Enum Side
        {
            get
            {
                return side;
            }
            set
            {
                side = value;
            }
        }

        public Point Position
        {
            get
            {
                return new Point(x, y);
            }
        }

        public bool Pressed
        {
            get
            {
                return pressed;
            }
        }

        public bool LockY
        {
            get
            {
                return yLocked;
            }
            set
            {
                yLocked = value;
            }
        }

        public bool GamepadEnabled
        {
            get
            {
                return gamepadEnabled;
            }
            set
            {
                gamepadEnabled = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }

        public double DefaultValue
        {
            get
            {
                return defaultValue;
            }
            set
            {
                defaultValue = value;
            }
        }

        public double ActualX
        {
            get
            {
                return actualX;
            }
        }

        public double ActualY
        {
            get
            {
                return actualY;
            }
        }

        public double X
        {
            get
            {
                return x;
            }
        }

        public double Y
        {
            get
            {
                return y;
            }
        }

        public double Sensitivity
        {
            get
            {
                return sensitivity;
            }
            set
            {
                sensitivity = value / 100;
            }
        }

        public Joystick(Sides side)
        {
            InitializeComponent();

            this.side = side;

            diameter = joystick.Width;

            translateTransform = new TranslateTransform();

            stick.RenderTransform = translateTransform;

            gamepad = new GamepadManager();

            timer = new Timer();

            timer.TickAction = () => OnTimerTick();

            if (gamepad != null)
            {
                Size size = new Size(joystick.Height, joystick.Width);

                if (size.Width == size.Height + (size.Width / 10) ||
                    size.Width == size.Width - (size.Width / 10) ||
                    size.Width == size.Height)
                    gamepad.SetJoystickDimensions(size);

                gamepadEnabled = true;

                TimerStart();
            }
        }

        public void TimerStart() { timer.Start(); }

        public void TimerStop() { timer.Stop(); }

        private async void OnTimerTick()
        {
            if (enabled && gamepad != null)
            {
                if (gamepad.Available && gamepadEnabled && !pressed)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        radius = joystick.Width / 3;

                        gamepad.Read();

                        double x = 0, y = 0;

                        switch (side)
                        {
                            case Sides.Left:
                                x = gamepad.LeftstickX;
                                y = gamepad.LeftstickY;
                                break;
                            case Sides.Right:
                                x = gamepad.RightstickX;
                                y = gamepad.RightstickY;
                                break;
                        }

                        if (x > -0.03 && x < 0.03 && x != 0 &&
                            y > -0.03 && y < 0.03 && y != 0)
                        {
                            translateTransform.X = actualX = 0;
                            translateTransform.Y = actualY = 0;
                        }
                        else
                        {
                            translateTransform.X = actualX = x;
                            translateTransform.Y = actualY = -y;
                        }

                        this.x = Math.Round((actualX / radius), roundDecimal);
                        this.y = Math.Round(-(actualY / radius), roundDecimal);
                    });
                }
            }
        }

        private void Joystick_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (!enabled) return;

            pressed = true;

            OnManipulationStarted();

            if (gamepadEnabled) TimerStop();
        }

        private void Joystick_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (!enabled) return;

            double x = e.Cumulative.Translation.X * sensitivity,
                   y = e.Cumulative.Translation.Y * sensitivity;

            OnManipulationDelta(x, y);
        }

        private void Joystick_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (!enabled) return;

            pressed = false;

            OnManipulationCompleted();

            if (gamepadEnabled) TimerStart();
        }

        private void OnManipulationDelta(double x, double y)
        {
            if (pressed)
            {
                double displacement = Math.Sqrt(x * x + y * y);

                if (displacement < radius)
                {
                    translateTransform.X = actualX = x;
                    translateTransform.Y = actualY = y;
                }
                else
                {
                    translateTransform.X = actualX = radius * (x / displacement);
                    translateTransform.Y = actualY = radius * (y / displacement);
                }

                this.x = Math.Round((actualX / radius), roundDecimal);
                this.y = Math.Round(-(actualY / radius), roundDecimal);
            }
        }

        private void OnManipulationStarted()
        {
            stick.RenderTransform = translateTransform;

            radius = joystick.Width / 3;

            if (!deviceIsLowEnd && animated && masterMode)
            {
                storyboard = null;

                BeginAnimation(hoverBackground, false, false, true, JOYSTICK_HOVER_BG_ANIM_SPEED * 10);
                BeginAnimation(background, true, false, true, JOYSTICK_BG_ANIM_SPEED * 10);

                Storyboard sBoard = new Storyboard();

                DoubleAnimation xAnim   = new DoubleAnimation(),
                                yAnim   = new DoubleAnimation(),
                                opacity = new DoubleAnimation(),
                                skewX   = new DoubleAnimation();

                DoubleAnimation[] animations =
                {
                    xAnim, yAnim,
                    opacity,
                    skewX
                };

                for (Int32 i = 0; i < animations.Length; i++)
                {
                    double from = i == 2 ? JOYSTICK_NORMAL_OPACITY  : BACK_JOYSTICK_NORMAL_SCALE;
                    double to   = i == 2 ? JOYSTICK_DELTA_OPACITY   : BACK_JOYSTICK_BIG_SCALE;

                    if (i == 3)
                    {
                        from = side.Equals(Sides.Left) 
                            ? JOYSTICK_SKEW_ANIM_VALUE 
                            : -JOYSTICK_SKEW_ANIM_VALUE;

                        to = 0;
                    }

                    animations[i].To            = to;
                    animations[i].From          = from;
                    animations[i].SpeedRatio    = 
                        i == 3 
                            ? BACK_JOYSTICK_ANIM_SPEED / 3 
                            : BACK_JOYSTICK_ANIM_SPEED;

                    string property =   $"(UIElement.RenderTransform)." + 
                                        $"(CompositeTransform.Scale{(i == 0 ? "X)" : "Y)")}";

                    DependencyObject target = background;

                    if (i == 2)
                    {
                        property = "(UIElement.Opacity)";

                        target = stick;
                    }
                    else if (i == 3)
                    {
                        property = "(UIElement.RenderTransform).(CompositeTransform.SkewX)";

                        target = hoverBackground;
                    }

                    Storyboard.SetTarget(animations[i], target);
                    Storyboard.SetTargetProperty(animations[i], property);

                    sBoard.Children.Add(animations[i]);
                }

                sBoard.Begin();
            }
            else
            {
                CompositeTransform cTrans = new CompositeTransform();

                cTrans.ScaleX = cTrans.ScaleY = BACK_JOYSTICK_BIG_SCALE;

                stick.Opacity = JOYSTICK_DELTA_OPACITY;

                background.RenderTransform = cTrans;
            }
        }

        private void OnManipulationCompleted()
        {
            CompositeTransform cTrans;

            double tempY = y;

            actualX = actualY = 0;

            if (!deviceIsLowEnd && animated && masterMode)
            {
                storyboard = null;

                BeginAnimation(hoverBackground, false, false, true, JOYSTICK_HOVER_BG_ANIM_SPEED);
                BeginAnimation(background, true, false, true, JOYSTICK_BG_ANIM_SPEED);

                Storyboard sBoard = new Storyboard();

                DoubleAnimation xAnim   = new DoubleAnimation(),
                                yAnim   = new DoubleAnimation(),
                                opacity = new DoubleAnimation(),
                                stickX  = new DoubleAnimation(),
                                stickY  = new DoubleAnimation();

                DoubleAnimation[] animations =
                {
                    xAnim, yAnim,
                    opacity,
                    stickX, stickY
                };

                for (Int32 i = 0; i < animations.Length; i++)
                {
                    if (yLocked && i == 4) break;

                    double  from = i == 2 ? JOYSTICK_DELTA_OPACITY  : BACK_JOYSTICK_BIG_SCALE,
                            to   = i == 2 ? JOYSTICK_NORMAL_OPACITY : BACK_JOYSTICK_NORMAL_SCALE;

                    if (i > 2)
                    {
                        from = i == 3 ? translateTransform.X : translateTransform.Y;

                        to = 0;
                    }

                    animations[i].To            = to;
                    animations[i].From          = from;
                    animations[i].SpeedRatio    = 
                        i > 2 
                            ? BACK_JOYSTICK_ANIM_SPEED * 2 
                            : BACK_JOYSTICK_ANIM_SPEED;

                    string property =   $"(UIElement.RenderTransform)." +
                                        $"(CompositeTransform.Scale{(i == 0 ? "X)" : "Y)")}";

                    if (i > 2)
                    {
                        cTrans = new CompositeTransform
                        {
                            TranslateY = translateTransform.Y,
                            TranslateX = translateTransform.X
                        };

                        stick.RenderTransform = cTrans;

                        property = "(UIElement.RenderTransform).(CompositeTransform.Translate";

                        property += i == 3 ? "X)" : "Y)";
                    }
                    else if (i == 2) property = "(UIElement.Opacity)";

                    sBoard.Children.Add(animations[i]);

                    Storyboard.SetTarget(animations[i], i >= 2 ? stick : background);
                    Storyboard.SetTargetProperty(animations[i], property);
                }

                sBoard.Begin();

                translateTransform.X = actualX = x = 0;

                if (!yLocked) translateTransform.Y = actualY = y = yLocked ? tempY : 0;

                stick.RenderTransform = translateTransform;
            }
            else
            {
                cTrans = new CompositeTransform();

                cTrans.ScaleX = cTrans.ScaleY = BACK_JOYSTICK_NORMAL_SCALE;

                stick.Opacity = JOYSTICK_NORMAL_OPACITY;

                background.RenderTransform = cTrans;

                translateTransform.X = actualX = x = 0;

                if (!yLocked) translateTransform.Y = actualY = y = yLocked ? tempY : 0;
            }
        }

        private void JoystickContainer_Loaded(object sender, RoutedEventArgs e)
        {
            if (!backColor) transBackground.Opacity = 0;

            if (animated && !deviceIsLowEnd && masterMode)
            {
                BeginAnimation(hoverBackground, false, false, true, JOYSTICK_HOVER_BG_ANIM_SPEED);
                BeginAnimation(background, true, false, true, JOYSTICK_BG_ANIM_SPEED);
            }

            if (masterMode)
            {
                background.Opacity      = 0.5;
                hoverBackground.Opacity = 1;
            }

            stick.Height = joystick.Height / 2.5;
            stick.Width  = joystick.Width / 2.5;

            stick.StrokeThickness   = ((stick.Width / 2) / 10) - 1;
            stroke.StrokeThickness  = stick.StrokeThickness;

            if (gamepad != null)
            {
                if (side.Equals(Sides.Left))
                    gamepad.JoystickSide = "Left";
                else if (side.Equals(Sides.Right))
                    gamepad.JoystickSide = "Right";
            }
        }

        private void BeginAnimation(
            Ellipse ellipse, 
            bool    reverse, 
            bool    reset, 
            bool    forever, 
            double  speed)
        {
            if (!enabled) return;

            storyboard = new Storyboard();

            DoubleAnimation animation = new DoubleAnimation
            {
                From        = 0.0,
                To          = reverse ? -360.0 : 360.0,
                SpeedRatio  = speed
            };

            if (forever) animation.RepeatBehavior = RepeatBehavior.Forever;

            Storyboard.SetTarget(animation, ellipse);

            Storyboard.SetTargetProperty(
                animation, 
                "(UIElement.RenderTransform)." + 
                "(CompositeTransform.Rotation)"
            );

            storyboard.Children.Add(animation);

            storyboard.Begin();
        }
    }
}
