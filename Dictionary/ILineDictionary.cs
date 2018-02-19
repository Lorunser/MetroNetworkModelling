using System.Windows.Media;

namespace Diction
{
    // interface for Line Dictionary
    public interface ILineDictionary
    {
        Color GetLineColor(int key); // returns color of given line
    }
}