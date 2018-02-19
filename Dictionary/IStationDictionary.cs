namespace Diction
{
    // interface for Station Dictionary
    public interface IStationDictionary
    {
        Coordinate GetPosition(int key); // returns x, y coordinate of station as a ratio
    }
}