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
        private const double _interval = 15d;
        private const double _unitSpan = 2d;

        public MainWindow()
        {
            InitializeComponent();
            _leftTimer = new DispatcherTimer();
            _rightTimer = new DispatcherTimer();
            Prepare();
        }

        private void Prepare()
        {
            _leftTimer.Interval = _rightTimer.Interval = TimeSpan.FromMilliseconds(_interval);
            _leftTimer.Tick += (s, e) => TurnBarrelLeft();
            _rightTimer.Tick += (s, e) => TurnBarrelRight();
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double x = 92.23d % 90d;
            double result = Math.Tan(90 * (Math.PI / 180));
            myTranslateTransform.X = myTranslateTransform.X + 15;
            myTranslateTransform.Y = myTranslateTransform.Y + 15;
        }

        private void TurnBarrelLeft()
        {
            var angle = Barrel.Angle - _unitSpan;
            if (angle < 0d)
                angle += 360d;
            Barrel.Angle = angle;
        }
        private void TurnBarrelRight()
        {
            var angle = Barrel.Angle + _unitSpan;
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
    }
}
