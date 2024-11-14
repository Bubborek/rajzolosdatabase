using System.Collections.Generic;

public class Drawing
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Point> Points { get; set; } = new List<Point>();
}
