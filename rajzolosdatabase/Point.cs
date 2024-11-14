using System;

public class Point
{
    public int Id { get; set; }
    public int DrawingId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public char Character { get; set; }
    public ConsoleColor Color { get; set; }

    public Drawing Drawing { get; set; }
}
