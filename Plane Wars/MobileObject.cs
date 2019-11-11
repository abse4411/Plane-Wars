using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plane_Wars
{
    public struct Location
    {
        public double X;
        public double Y;
    }
    public struct MobileObject
    {
        public Location Location;
        public double Dx;
        public double Dy;

        public void MoveTo(Location location)
        {
            Location.X = location.X;
            Location.Y = location.Y;
        }
        public void MoveTo(double x, double y)
        {
            Location.X = x;
            Location.Y = y;
        }
        public void Move()
        {
            Location.X += Dx;
            Location.Y += Dy;
        }
    }
}
