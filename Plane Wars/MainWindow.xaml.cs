using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Plane_Wars
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _leftTimer;
        private readonly DispatcherTimer _rightTimer;
        private const double Interval = 15d;
        private const double UnitSpan = 2d;
        private readonly GameController _controller;

        public MainWindow()
        {
            InitializeComponent();
            _leftTimer = new DispatcherTimer();
            _rightTimer = new DispatcherTimer();
            _controller=new GameController(Plane,Shell,new GameArgs
            {
                BorderWidth = 800d,
                BorderHeight = 600d,
                PlaneLocation = new Location {X=0d,Y=0d },
                ShellLocation = new Location { X = 400d, Y = 560d },
                PlaneRadius =32d,
                BarrelLength=80d,
                ShellRadius=10d
            },this.Dispatcher);
            Prepare();
        }

        private void Prepare()
        {
            _leftTimer.Interval = _rightTimer.Interval = TimeSpan.FromMilliseconds(Interval);
            _leftTimer.Tick += (s, e) => TurnBarrelLeft();
            _rightTimer.Tick += (s, e) => TurnBarrelRight();
            _controller.Loaded += (s, e) =>
            {
                Fire.IsEnabled = true;
                Status.Text = "Loaded";
            };
            _controller.GameOver += (s, e) =>
            {
                Start.IsEnabled = true;
                Pause.IsEnabled = false;
                Continue.IsEnabled = false;
                Fire.IsEnabled = false;
                Status.Text = "Game Over";
            };
        }


        private void TurnBarrelLeft()
        {
            var angle = Barrel.Angle - UnitSpan;
            if (angle < 0d)
                angle += 360d;
            Barrel.Angle = angle;
        }
        private void TurnBarrelRight()
        {
            var angle = Barrel.Angle + UnitSpan;
            if (angle > 360d)
                angle -= 360d;
            Barrel.Angle = angle;
        }

        private void TurnLeftBtn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TurnBarrelLeft();
            _leftTimer.Start();
        }

        private void TurnLeftBtn_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _leftTimer.Stop();
        }

        private void TurnRightBtn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TurnBarrelRight();
            _rightTimer.Start();
        }

        private void TurnRightBtn_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _rightTimer.Stop();
        }

        private void Start_OnClick(object sender, RoutedEventArgs e)
        {
            Start.IsEnabled = false;
            Pause.IsEnabled = true;
            Continue.IsEnabled = false;
            Status.Text = "Running";
            _controller.Start();
        }

        private void Fire_OnClick(object sender, RoutedEventArgs e)
        {
            Fire.IsEnabled = false;
            Status.Text = "Reloading";
            _controller.Fire(Barrel.Angle);
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            Pause.IsEnabled = false;
            Continue.IsEnabled = true;
            Fire.IsEnabled = false;
            Status.Text = "Pausing";
            _controller.Pause();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Start.IsEnabled = true;
            Pause.IsEnabled = false;
            Continue.IsEnabled = false;
            Fire.IsEnabled = false;
            Status.Text = "Ready";
            _controller.Reset();
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            Pause.IsEnabled = true;
            Continue.IsEnabled = false;
            if(_controller.CanFire)
                Fire.IsEnabled = true;
            else
                Fire.IsEnabled = false;
            Status.Text = "Running";
            _controller.Continue();
        }
    }
}
