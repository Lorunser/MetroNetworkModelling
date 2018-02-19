using System.Collections.Generic;

namespace Diction
{
    /// <summary>
    /// Specific station dictionary
    /// to translate between
    /// 
    /// name
    /// id
    /// position
    /// 
    /// where position is given  as
    /// a fraction (x,y) of display size
    /// </summary>
    public class StationDictionary : BaseDictionary, IStationDictionary
    {
        private Coordinate[] Coords { get; set; }
        private Coordinate TopLeft { get; set; }
        private Coordinate BottomRight { get; set; }

        private bool Converted = false; // ensures only able to access positions once they have been converted into percentages

        public StationDictionary(List<string> vals) : base(vals)
        {
            Coords = new Coordinate[base.N];
        }

        public int GetKey(string value, double latitude, double longitude) // overloaded method for normal GetKey which also updates coordinates
        {
            int key = base.GetKey(value);
            if (ValidKey(key))
            {
                Coords[key] = new Coordinate(latitude, longitude);
            }
            return key;
        }

        // accesor methods

        public Coordinate GetPosition(int key)
        {
            if (!Converted)
            {
                ConvertIntoPercents();
                Converted = true;
            }

            return Coords[key];
        }

        // converts between absolute longitude and latitude to relative percents

        private void ConvertIntoPercents()
        {
            AssignTopLeft();
            AssignBottomRight();

            for (int i = 0; i < N; i++)
            {
                if (Coords[i] != null)
                {
                    Coords[i].ConvertToFraction(TopLeft, BottomRight);
                }
            }
        }

        private void AssignTopLeft()
        {
            double maxLat, minLong;
            maxLat = Coords[0].Y;
            minLong = Coords[0].X;

            foreach (var c in Coords)
            {
                if (c != null)
                {
                    if (c.Y > maxLat)
                    {
                        maxLat = c.Y;
                    }

                    if (c.X < minLong)
                    {
                        minLong = c.X;
                    }
                }
            }

            TopLeft = new Coordinate(maxLat, minLong);
        }

        private void AssignBottomRight()
        {
            double minLat, maxLong;
            minLat = Coords[0].Y;
            maxLong = Coords[0].X;

            foreach (var c in Coords)
            {
                if (c != null)
                {
                    if (c.Y < minLat)
                    {
                        minLat = c.Y;
                    }

                    if (c.X > maxLong)
                    {
                        maxLong = c.X;
                    }
                }
            }

            BottomRight = new Coordinate(minLat, maxLong);
        }
    }
}
