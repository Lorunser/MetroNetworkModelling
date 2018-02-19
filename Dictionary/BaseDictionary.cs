using System.Collections.Generic;
using System.Linq;

namespace Diction
{    
    /// <summary>
    /// code for base dictionary class
    /// maps keys to values and vice-versa
    /// </summary>
    public abstract class BaseDictionary : IDiction
    {
        // list of values held in dictionary, index of item is its key
        public List<string> Values { get; private set; }    
        // standard dictionary used to map string values to integer key  
        private Dictionary<string,int> Lookup { get; set; }

        // field to access size of dictionary
        public int N
        {
            get
            {
                return Lookup.Count();
            }
            private set { }
        }

        // Constructor
        public BaseDictionary(List<string> vals)
        {
            /*
            foreach (var name in vals)
            {
                Values.Add(name.ToLower()); // all lower case for sake of simplicity
            }
            Values.Sort(); // sort alphabetically to allow binary split

            // remove any possible duplicates

            string old = Values[0];
            for (int i = 1; i < Values.Count(); i++)
            {
                if (old == Values[i])
                {
                    Values.RemoveAt(i - 1);
                    i--;
                }
                old = Values[i];
            }
            */
            Values = new List<string>();
            Lookup = new Dictionary<string, int>();

            vals.Sort();
            string old = "";
            for (int i = 0; i < vals.Count(); i++)
            {
                vals[i] = vals[i].ToLower();
                if (old == vals[i])
                {
                    vals.RemoveAt(i - 1);
                    i--;
                }
                old = vals[i];
            }

            for (int i = 0; i < vals.Count(); i++)
            {
                Lookup.Add(vals[i], i);
                Values.Add(vals[i]);
            }
        }

        // Methods
        public string GetValue(int key)
        {
            if (ValidKey(key))
            {
                return Values[key];
            }

            else
            {
                return null;
                // throw custom exception
            }
        }

        public int GetKey(string value) // find key corresponding to given value
        {
            int key;
            value = Prune(value);
            if (Lookup.TryGetValue(value, out key))
            {
                return key;
            }
            return -1;
        }

        private string Prune(string s) // necessary to marry inconsistent naming conventions
        {
            int braceStart;

            s = s.ToLower();
            s = s.Replace(" underground station", "");
            braceStart = s.IndexOf('(');

            if (braceStart != -1)
            {
                s = s.Remove(braceStart);
            }

            s = s.Trim();
            return s;
        }

        protected bool ValidKey(int k) // determines if key is valid
        {
            if (k < N)
            {
                if (k >= 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
