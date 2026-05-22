using System.Collections.Generic;

// Minimal Snake för konsolen (.NET 9)
// Enkel spel-loop, icke-blockerande input, kollisioner och mat.

internal class Program
{
    private static async Task Main()
    {
        Console.CursorVisible = false;
        var width = 40;
        var height = 20;
        TryResizeConsole(width, height);

        var game = new SnakeGameService();
        var state = new GameState(width, height);
        state.Food = game.SpawnFood(state);

        await RunAsync(state, game, TimeSpan.FromMilliseconds(120));

        Console.SetCursorPosition(0, height + 3);
        Console.CursorVisible = true;
        Console.WriteLine($"Game Over! Poäng: {state.Score}");
        Console.WriteLine("Tryck valfri tangent för att avsluta...");
        Console.ReadKey(true);
    }

    // --- Spelloop ---
    private static async Task RunAsync(GameState s, SnakeGameService game, TimeSpan tick)
    {
        Console.Clear();
        DrawHud(s);
        DrawBorder(s);
        DrawFood(s);

        // Rita start-huvud
        DrawCell(s.Snake.First!.Value, 'O', ConsoleColor.Green);

        using var timer = new PeriodicTimer(tick);
        while (!s.GameOver && await timer.WaitForNextTickAsync())
        {
            s.Dir = ReadDirection(s.Dir);
            var removedTail = game.Update(s);

            // Rendering av ändringar
            if (removedTail.HasValue)
                DrawCell(removedTail.Value, ' ');

            // Förra huvudet blir kropp
            var second = s.Snake.First!.Next;
            if (second is not null)
                DrawCell(second.Value, 'o', ConsoleColor.Green);

            // Rita huvud
            DrawCell(s.Snake.First!.Value, 'O', ConsoleColor.Green);

            // Rita mat (kan ha flyttats)
            DrawFood(s);
            DrawHud(s);
        }
    }

    // --- Input ---
    private static Direction ReadDirection(Direction current)
    {
        if (!Console.KeyAvailable) return current;
        var key = Console.ReadKey(intercept: true).Key;
        return key switch
        {
            ConsoleKey.UpArrow => current == Direction.Down ? current : Direction.Up,
            ConsoleKey.DownArrow => current == Direction.Up ? current : Direction.Down,
            ConsoleKey.LeftArrow => current == Direction.Right ? current : Direction.Left,
            ConsoleKey.RightArrow => current == Direction.Left ? current : Direction.Right,
            _ => current
        };
    }

    // --- Rendering ---
    private static void DrawHud(GameState s)
    {
        Console.SetCursorPosition(0, 0);
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write($"Poäng: {s.Score}    ");
        Console.ResetColor();
    }

    private static void DrawFood(GameState s)
    {
        DrawCell(s.Food, 'X', ConsoleColor.White);
    }

    private static void DrawBorder(GameState s)
    {
        const int topRow = 1; // under HUD
        var w = s.Width;
        var h = s.Height;

        Console.ForegroundColor = ConsoleColor.DarkGray;
        try
        {
            Console.SetCursorPosition(0, topRow);
            Console.Write('┌');
            Console.Write(new string('─', w));
            Console.Write('┐');

            for (var y = 0; y < h; y++)
            {
                var row = topRow + 1 + y;
                Console.SetCursorPosition(0, row);
                Console.Write('│');
                Console.SetCursorPosition(w + 1, row);
                Console.Write('│');
            }

            Console.SetCursorPosition(0, topRow + 1 + h);
            Console.Write('└');
            Console.Write(new string('─', w));
            Console.Write('┘');
        }
        catch
        {
            // Ignorera om konsolen har ändrat storlek oväntat
        }

        Console.ResetColor();
    }

    private static void DrawCell(Pos p, char ch, ConsoleColor? color = null)
    {
        var x = Math.Clamp(p.X + 1, 0, Math.Max(0, Console.BufferWidth - 1));
        var y = Math.Clamp(p.Y + 2, 0, Math.Max(0, Console.BufferHeight - 1)); // +1 HUD, +1 övre kant
        try
        {
            Console.SetCursorPosition(x, y);
            if (color.HasValue) Console.ForegroundColor = color.Value;
            Console.Write(ch);
            if (color.HasValue) Console.ResetColor();
        }
        catch
        {
            // Ignorera om konsolen har ändrat storlek oväntat
        }
    }

    // --- Hjälp ---
    private static void TryResizeConsole(int width, int height)
    {
        try
        {
            var winW = Math.Max(Console.WindowWidth, width + 2);
            var winH = Math.Max(Console.WindowHeight, height + 3); // +HUD + kantlinjer
            var bufW = Math.Max(Console.BufferWidth, winW);
            var bufH = Math.Max(Console.BufferHeight, winH);
            Console.SetBufferSize(bufW, bufH);
            Console.SetWindowSize(winW, winH);
        }
        catch
        {
            // Ignorera om miljön inte tillåter storleksändring (t.ex. CI/terminal)
        }
    }
}

// (Inga top-level metoder – all logik finns i Program-klassen ovan.)
