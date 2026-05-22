internal sealed class SnakeGameService
{
    public Pos? Update(GameState s)
    {
        var head = s.Snake.First!.Value;
        var delta = s.Dir switch
        {
            Direction.Up => new Pos(0, -1),
            Direction.Down => new Pos(0, 1),
            Direction.Left => new Pos(-1, 0),
            Direction.Right => new Pos(1, 0),
            _ => new Pos(0, 0)
        };

        var next = new Pos(head.X + delta.X, head.Y + delta.Y);

        // Kollision: vägg eller egen kropp
        if (next.X < 0 || next.X >= s.Width || next.Y < 0 || next.Y >= s.Height || s.Occupied.Contains(next))
        {
            s.GameOver = true;
            return null;
        }

        // Flytta huvud
        s.Snake.AddFirst(next);
        s.Occupied.Add(next);

        if (next == s.Food)
        {
            s.Score++;
            s.Food = SpawnFood(s);
            return null; // växer: ingen svans tas bort
        }

        // Ta bort svans
        var tail = s.Snake.Last!.Value;
        s.Snake.RemoveLast();
        s.Occupied.Remove(tail);
        return tail;
    }

    public Pos SpawnFood(GameState s)
    {
        var rnd = Random.Shared;
        while (true)
        {
            var p = new Pos(rnd.Next(0, s.Width), rnd.Next(0, s.Height));
            if (!s.Occupied.Contains(p)) return p;
        }
    }
}
