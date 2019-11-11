using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace Plane_Wars
{
    public enum GameStatus
    {
        Running, Pausing,Ready
    }

    public class GameArgs
    {
        public double BorderWidth;
        public double BorderHeight;
        public Location PlaneLocation;
        public double PlaneRadius;
        public Location CannonLocation;
        public double BarrelLength;
        public double ShellRadius;
    }
    class GameController
    {
        private const double Precision=1d;
        private const int SleepSpan = 100;
        private const double PlaneDelta = 15d;
        private const double ShellDelta=10d;
        private readonly TranslateTransform _plane;
        private readonly TranslateTransform _shell;
        private readonly GameArgs _args;
        private readonly Dispatcher _uiDispatcher;
        private readonly Task[] _tasks;
        private bool _canFire;
        private readonly object _syncFire=new object();
        private MobileObject _planeObject;
        private MobileObject _shellObject;
        public GameStatus Status { get; private set; }

        public event EventHandler Loaded;
        public event EventHandler GameOver;

        public GameController(TranslateTransform plane, TranslateTransform shell,GameArgs args,Dispatcher uiDispatcher)
        {
            _plane = plane;
            _shell = shell;
            _args = args;
            _uiDispatcher = uiDispatcher;
            _tasks=new Task[2];
            _canFire = false;
            _planeObject =new MobileObject();
            _shellObject = new MobileObject();
            Status = GameStatus.Ready;
            _planeObject.MoveTo(args.PlaneLocation);
            _planeObject.Dx = PlaneDelta;
            _planeObject.Dy = 0d;
            _shellObject.MoveTo(args.CannonLocation);
        }

        public void Reset()
        {
            lock (this)
            {
                if (Status != GameStatus.Ready)
                {
                    Status = GameStatus.Ready;
                    Task.WaitAll(_tasks);
                }
            }
            _planeObject.MoveTo(_args.PlaneLocation);
            _shellObject.MoveTo(_args.CannonLocation);
            UpdatePlane();
            UpdateShell();
        }

        public void Start()
        {
            lock (this)
            {
                if(Status!=GameStatus.Ready)
                    throw new InvalidOperationException("Game is not in ready status");
                Status = GameStatus.Running;
                MovePlane();
            }
        }

        private void MovePlane()
        {
            _tasks[0]= Task.Run(() =>
            {
                double distance=(_args.ShellRadius+_args.PlaneRadius)*(_args.ShellRadius + _args.PlaneRadius);
                while (true)
                {
                    if (Status == GameStatus.Ready)
                        return Task.CompletedTask;
                    if (Status == GameStatus.Running)
                    {
                        _planeObject.Move();
                        double rx = (_planeObject.Location.X +_args.PlaneRadius- (_shellObject.Location.X+_args.ShellRadius));
                        double ry = (_planeObject.Location.Y + _args.PlaneRadius -(_shellObject.Location.Y+ _args.ShellRadius));
                        if (Math.Abs(distance-(rx*rx+ry*ry)) <Precision)
                        {
                            OnGameOver();
                            return Task.CompletedTask;
                        }
                        if (_planeObject.Location.X >= _args.BorderWidth)
                        {
                            _planeObject.MoveTo(-2*_args.PlaneRadius,_planeObject.Location.Y);
                        }
                        UpdatePlane();
                    }
                    Thread.Sleep(SleepSpan);
                }
            });
        }
        public void Fire(double angle)
        {
            lock (this)
            {
                if (Status != GameStatus.Running)
                    throw new InvalidOperationException("Game is not in running status");
            }
            lock (_syncFire)
            {
                if(!_canFire)
                    return;
                _canFire = false;
            }
            double radians = angle * (Math.PI / 180);
            _shellObject.Dx = _args.BarrelLength * Math.Sin(radians) * ShellDelta;
            _shellObject.Dy = _args.BarrelLength * Math.Cos(radians) * ShellDelta;
            _shellObject.MoveTo(_args.CannonLocation);
            _tasks[1]=Task.Run(() =>
            {
                while (true)
                {
                    if (Status == GameStatus.Ready)
                        return Task.CompletedTask;
                    if (Status == GameStatus.Running)
                    {
                        _shellObject.Move();
                        if (_shellObject.Location.X<=-2*_args.ShellRadius || _shellObject.Location.X>=_args.BorderWidth ||
                            _shellObject.Location.Y<=-2*_args.ShellRadius || _shellObject.Location.Y>=_args.BorderHeight)
                        {
                            OnLoaded();
                            return Task.CompletedTask;
                        }
                        UpdateShell();
                    }
                    Thread.Sleep(SleepSpan);
                }
            });
        }

        private void OnLoaded()
        {
            lock (_syncFire)
            {
                _canFire = true;
            }
            _uiDispatcher.Invoke(() => Loaded?.Invoke(this, new EventArgs()));
        }

        private void OnGameOver()
        {
            lock (this)
            {
                Status = GameStatus.Ready;
            }
            _uiDispatcher.Invoke(() => GameOver?.Invoke(this, new EventArgs()));
        }
        public void Pause()
        {
            lock (this)
            {
                if (Status != GameStatus.Running)
                    throw new InvalidOperationException("Game is not in running status");
                Status = GameStatus.Pausing;
            }
        }
        public void Continue()
        {
            lock (this)
            {
                if (Status != GameStatus.Pausing)
                    throw new InvalidOperationException("Game is not in pausing status");
                Status = GameStatus.Running;
            }
        }

        private void UpdateShell()
        {
            _uiDispatcher.Invoke(() =>
            {
                _shell.X = _shellObject.Location.X;
                _shell.Y = _shellObject.Location.Y;
            });
        }
        private void UpdatePlane()
        {
            _uiDispatcher.Invoke(() =>
            {
                _plane.X = _planeObject.Location.X;
                _plane.Y = _planeObject.Location.Y;
            });
        }
    }
}
