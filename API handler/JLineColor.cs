namespace API
{
    public class JLineColor // offline json line colour format
    {
        public string Color { get; set; } // color in hexadecimal RGB format

        public JLineColor() { }

        public JLineColor(string color)
        {
            this.Color = color;
        }
    }
}
