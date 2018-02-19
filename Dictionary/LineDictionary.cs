using System;
using System.Collections.Generic;

// addiitional imports
using System.Windows.Media;

namespace Diction
{
    /// <summary>
    /// specific line dictionary to translate between:
    /// name
    /// id
    /// color
    /// ^^^^^ damn you Microsoft for spelling it wrong!!!
    /// </summary>
    public class LineDictionary : BaseDictionary, ILineDictionary
    {
        private List<Color> lineColors { get; set; } // contains colours of line

        // constructor
        public LineDictionary(List<string> vals, List<string> colors) : base(vals)
        {
            lineColors = new List<Color>();

            foreach (var colorString in colors)
            {
                lineColors.Add(ConvertToColor(colorString));
            }
        }

        // additional method
        public Color GetLineColor(int key)
        {
            if (ValidKey(key))
            {
                return lineColors[key];
            }
            else
            {
                return new Color(); // void colour
            }
        }

        private Color ConvertToColor(string rgb)
        {
            Color col = new Color();

            rgb = rgb.Remove(0, 1); // removes hash
            byte r = Convert.ToByte(rgb.Substring(0, 2), 16);
            byte g = Convert.ToByte(rgb.Substring(2, 2), 16);
            byte b = Convert.ToByte(rgb.Substring(4, 2), 16);

            col.A = 255;
            col.R = r;
            col.G = g;
            col.B = b;

            return col;
        }
    }

}
