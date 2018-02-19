namespace Diction
{
    // interface for base dictionary
    interface IDiction
    {
        string GetValue(int key); // returns value corresponding to given key
        int GetKey(string value); // returns key corresponding to given value
    }
}
