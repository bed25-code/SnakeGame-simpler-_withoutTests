sealed class GameState
{
    public int Width { get; }
    public int Height { get; }
    public LinkedList<Pos> Snake { get; } = new();
    public HashSet<Pos> Occupied { get; } = new();
    public Direction Dir { get; set; } = Direction.Right;
    public Pos Food { get; set; }
    public int Score { get; set; }
    public bool GameOver { get; set; }

    public GameState(int width, int height)
    {
        Width = width;
        Height = height;
        var start = new Pos(width / 2, height / 2);
        Snake.AddFirst(start);
        Occupied.Add(start);
    }
}
