using System;

namespace Diction
{
    /// <summary>
    /// class for specifying position of station
    /// specified as fraction (x,y) of width/height of container
    /// also performs transformations on coordinates to make map
    /// less cluttered
    /// </summary>
    public class Coordinate
    {
        //Longitude is +ve Eastwards
        //Latitude is +ve Northwards
        public double X { get; set; }
        public double Y { get; private set; }

        private const double XCentre = 0.554; // centre of London is piccadilly circus
        private const double YCentre = 0.644;
        private const double Distortion = 0.65; // lower value of distortion gives higher radial stretch

        //constructor
        public Coordinate(double latitude, double longitude)
        {
            this.Y = latitude;
            this.X = longitude;
        }

        // conversion
        public void ConvertToFraction(Coordinate TopLeft, Coordinate BottomRight)
        {
            double width = BottomRight.X - TopLeft.X;
            double height = TopLeft.Y - BottomRight.Y;

            double left = this.X - TopLeft.X;
            double top = TopLeft.Y - this.Y;

            this.X = left / width;
            this.Y = top / height;

            this.X = Transform(this.X, XCentre);
            this.Y = Transform(this.Y, YCentre);
        }

        private double Transform(double p, double centre)
        {
            double newValue;

            if (p > centre)
            {
                newValue = 0.5 + (p - centre) / (2 * (1 - centre));
                newValue = 0.5 + Math.Pow((2 * (newValue - 0.5)), Distortion) / 2;
            }

            else
            {
                newValue = 0.5 - (centre - p) / (2 * (centre));
                newValue = 0.5 - Math.Pow((2 * (0.5 - newValue)), Distortion) / 2;
            }

            return newValue;
        }
    }
}
