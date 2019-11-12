using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace Plane_Wars
{
    public enum GameStatus
    {
        Running, Pausing, Ready
    }

    public class GameArgs
    {
        public double BorderWidth;
        public double BorderHeight;
        public Location PlaneLocation;
        public Location ShellLocation;
        public double PlaneRadius;
        public double BarrelLength;
        public double ShellRadius;
    }
    class GameController
    {
        private const int SleepSpan = 30;
        private const double PlaneDelta = 20d;
        private const double ShellDelta = 0.1d;
        private readonly TranslateTransform _plane;
        private readonly TranslateTransform _shell;
        private readonly GameArgs _args;
        private readonly Dispatcher _uiDispatcher;
        private readonly Task[] _tasks;
        public bool CanFire { get; private set; }
        private readonly object _syncFire = new object();
        private MobileObject _planeObject;
        private MobileObject _shellObject;
        public GameStatus Status { get; private set; }

        public event EventHandler Loaded;
        public event EventHandler GameOver;

        public GameController(TranslateTransform plane, TranslateTransform shell, GameArgs args, Dispatcher uiDispatcher)
        {
            _plane = plane;
            _shell = shell;
            _args = args;
            _uiDispatcher = uiDispatcher;
            _tasks = new Task[2];
            CanFire = false;
            _planeObject = new MobileObject();
            _shellObject = new MobileObject();
            Status = GameStatus.Ready;
            _planeObject.Dx = PlaneDelta;
            _planeObject.Dy = 0d;
            ResetLocation();
        }

        public void Reset()
        {
            lock (this)
            {
                if (Status != GameStatus.Ready)
                {
                    Status = GameStatus.Ready;
                    _tasks[0]?.Wait();
                    _tasks[1]?.Wait();
                }
                ResetLocation();
            }
        }

        private void ResetLocation()
        {
            _planeObject.MoveTo(0, 0);
            _shellObject.MoveTo(0, 0);
            UpdatePlane();
            UpdateShell();
        }

        public void Start()
        {
            lock (this)
            {
                if (Status != GameStatus.Ready)
                    throw new InvalidOperationException("Game is not in ready status");
                Status = GameStatus.Running;
                ResetLocation();
                MovePlane();
                OnLoaded();
            }
        }

        private void MovePlane()
        {
            _tasks[0] = Task.Run(() =>
             {
                 double distance = (_args.ShellRadius + _args.PlaneRadius) * (_args.ShellRadius + _args.PlaneRadius);
                 distance -= 2*_args.ShellRadius * _args.PlaneRadius;
                 while (true)
                 {
                     if (Status == GameStatus.Ready)
                         return Task.CompletedTask;
                     if (Status == GameStatus.Running)
                     {
                         _planeObject.Move();
                         UpdatePlane();
                         double rx = _args.PlaneLocation.X + _planeObject.Location.X + _args.PlaneRadius
                                      - (_shellObject.Location.X + _args.ShellRadius + _args.ShellLocation.X);
                         double ry = _args.PlaneLocation.Y + _planeObject.Location.Y + _args.PlaneRadius
                                      - (_shellObject.Location.Y + _args.ShellRadius + _args.ShellLocation.Y);
                         if (distance > (rx * rx + ry * ry))
                         {
                             OnGameOver();
                             return Task.CompletedTask;
                         }
                         if (_planeObject.Location.X + _args.PlaneLocation.X >= _args.BorderWidth)
                         {
                             _planeObject.MoveTo(-2 * _args.PlaneRadius, _planeObject.Location.Y);
                         }
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
                    return;

                lock (_syncFire)
                {
                    if (!CanFire)
                        return;
                    CanFire = false;
                }
                double radians = angle * (Math.PI / 180);
                _shellObject.Dx = _args.BarrelLength * Math.Sin(radians);
                _shellObject.Dy = -_args.BarrelLength * Math.Cos(radians);
                _shellObject.MoveTo(_shellObject.Dx, _shellObject.Dy);
                _shellObject.Dx *= ShellDelta;
                _shellObject.Dy *= ShellDelta;
                UpdateShell();
                Debug.WriteLine($"X: {_shellObject.Location.X} Y:{_shellObject.Location.Y}");
                Debug.WriteLine($"DX: {_shellObject.Dx} DY:{_shellObject.Dy}");
                _tasks[1] = Task.Run(() =>
                  {
                      double negDiameter = -2d * _args.ShellRadius;
                      while (true)
                      {
                          if (Status == GameStatus.Ready)
                              return Task.CompletedTask;
                          if (Status == GameStatus.Running)
                          {
                              _shellObject.Move();
                              UpdateShell();
                              //Debug.WriteLine($"X: {_shellObject.Location.X} Y:{_shellObject.Location.Y}");
                              if (_shellObject.Location.X + _args.ShellLocation.X <= negDiameter || _shellObject.Location.X + _args.ShellLocation.X >= _args.BorderWidth ||
                                        _shellObject.Location.Y + _args.ShellLocation.Y <= negDiameter || _shellObject.Location.Y + _args.ShellLocation.Y >= _args.BorderHeight)
                              {
                                  //Debug.WriteLine("++++++++++++++++++++++");
                                  //Debug.WriteLine($"X: {_shellObject.Location.X } Y:{_shellObject.Location.Y }");
                                  //Debug.WriteLine($"ShellLocation.X: {_args.ShellLocation.X} ShellLocation.Y:{_args.ShellLocation.Y}");
                                  //Debug.WriteLine($"BorderWidth: {_args.BorderWidth} BorderHeight:{_args.BorderHeight}");
                                  //Debug.WriteLine($"DX: {_shellObject.Dx} DY:{_shellObject.Dy}");
                                  _shellObject.MoveTo(0, 0);
                                  UpdateShell();
                                  OnLoaded();
                                  return Task.CompletedTask;
                              }
                          }
                          Thread.Sleep(SleepSpan);
                      }
                  });
            }
        }

        private void OnLoaded()
        {
            lock (_syncFire)
            {
                CanFire = true;
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
                    return;
                Status = GameStatus.Pausing;
            }
        }
        public void Continue()
        {
            lock (this)
            {
                if (Status != GameStatus.Pausing)
                    return;
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
