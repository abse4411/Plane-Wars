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
                BorderWidth = 800,
                BorderHeight = 600,
                PlaneLocation = new Location {X=0,Y=0 },
                PlaneRadius=25d,
                CannonLocation= new Location { X = 400d, Y = 540 },
                BarrelLength=80,
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
                Status.Text = "Loaded";
            };
            _controller.GameOver += (s, e) =>
            {
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
            _controller.Start();
        }

        private void Fire_OnClick(object sender, RoutedEventArgs e)
        {
            Status.Text = "Reloading";
            _controller.Fire(Barrel.Angle);
        }
    }
}
