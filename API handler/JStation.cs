namespace API
{
    public class JStation // offline json station format
    {
        public string commonName { get; set; }

        public JStation() { }

        public JStation(string name)
        {
            this.commonName = name;
        }
    }
}
