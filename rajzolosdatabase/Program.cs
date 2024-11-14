using System;

using System.Collections.Generic;
using System.Linq;

using Microsoft.EntityFrameworkCore;

class Program

{

    static int selectedOption = 0;

    static List<string> menuOptions = new List<string> { "Új Fájl", "Fájl szerkesztése", "Fájl törlése", "Kilépés" };

    static DrawingDbContext dbContext = new DrawingDbContext();

    static List<(int x, int y, char character, ConsoleColor color)> currentDrawing = new List<(int x, int y, char character, ConsoleColor color)>();

    static ConsoleColor currentColor = ConsoleColor.White;

    static char currentCharacter = '█';

    static void Main()

    {

        Console.SetWindowSize(80, 25); // Konzol méretének beállítása

        dbContext.Database.EnsureCreated(); // Adatbázis létrehozása, ha nem létezik

        bool exitMenu = false;

        while (!exitMenu)

        {

            ShowMenu(); // Menü megjelenítése

            bool exit = false;

            while (!exit)

            {

                if (Console.KeyAvailable)

                {

                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                    switch (keyInfo.Key)

                    {

                        case ConsoleKey.UpArrow:

                            selectedOption = (selectedOption > 0) ? selectedOption - 1 : menuOptions.Count - 1;

                            break;

                        case ConsoleKey.DownArrow:

                            selectedOption = (selectedOption < menuOptions.Count - 1) ? selectedOption + 1 : 0;

                            break;

                        case ConsoleKey.Enter:

                            HandleMenuSelection(ref exit, ref exitMenu);

                            break;

                        case ConsoleKey.Escape:

                            exit = true;

                            break;

                    }

                    ShowMenu();

                }

            }

        }

    }

    static void ShowMenu()

    {

        Console.Clear();

        for (int i = 0; i < menuOptions.Count; i++)

        {

            Console.WriteLine(i == selectedOption ? $"> {menuOptions[i]}" : $"  {menuOptions[i]}");

        }

    }

    static void HandleMenuSelection(ref bool exit, ref bool exitMenu)

    {

        switch (selectedOption)

        {

            case 0:

                CreateNewFile();

                break;

            case 1:

                EditFile();

                break;

            case 2:

                DeleteFile();

                break;

            case 3:

                exit = true;

                exitMenu = true;

                break;

        }

    }

    static void CreateNewFile()

    {

        Console.Clear();

        Console.WriteLine("Írd be az új fájl nevét:");

        string fileName = Console.ReadLine();

        if (!string.IsNullOrEmpty(fileName))

        {

            var newDrawing = new Drawing { Name = fileName };

            currentDrawing.Clear();  // Új rajz kezdése

            RunDrawingProgram(newDrawing);

            dbContext.Drawings.Add(newDrawing); // Rajz mentése

            dbContext.SaveChanges(); // Adatbázisba mentés

            Console.WriteLine($"A rajz '{fileName}' néven elmentve.");

            Console.ReadKey();

        }

        else

        {

            Console.WriteLine("Érvénytelen fájlnév. A fájl létrehozása megszakadt.");

            Console.ReadKey();

        }

    }

    static void EditFile()

    {

        Console.Clear();

        Console.WriteLine("Írd be a szerkeszteni kívánt fájl nevét:");

        string fileName = Console.ReadLine();

        var drawing = dbContext.Drawings.Include(d => d.Points).FirstOrDefault(d => d.Name == fileName);

        if (drawing != null)

        {

            currentDrawing = drawing.Points.Select(p => (p.X, p.Y, p.Character, p.Color)).ToList();

            RestoreConsoleState(currentDrawing);

            RunDrawingProgram(drawing);

            dbContext.SaveChanges(); // Módosítások mentése

            Console.WriteLine($"A rajz '{fileName}' frissítve.");

        }

        else

        {

            Console.WriteLine("A fájl nem található.");

        }

        Console.ReadKey();

    }

    static void DeleteFile()

    {

        Console.Clear();

        Console.WriteLine("Írd be a törölni kívánt fájl nevét:");

        string fileName = Console.ReadLine();

        var drawing = dbContext.Drawings.FirstOrDefault(d => d.Name == fileName);

        if (drawing != null)

        {

            dbContext.Drawings.Remove(drawing);

            dbContext.SaveChanges();

            Console.WriteLine($"A fájl '{fileName}' törölve.");

        }

        else

        {

            Console.WriteLine("A fájl nem található.");

        }

        Console.ReadKey();

    }

    static void RunDrawingProgram(Drawing drawing)

    {

        Console.Clear();

        int width = Console.WindowWidth;

        int height = Console.WindowHeight;

        int x = width / 2;

        int y = height / 2;

        Console.CursorVisible = false;

        DrawFrame(width, height); // Keret megrajzolása

        if (currentDrawing.Count > 0)

        {

            // Korábbi rajz visszaállítása

            RestoreConsoleState(currentDrawing);

        }

        DrawCursor(x, y);

        DisplayCurrentSelection();

        bool exit = false;

        while (!exit)

        {

            if (Console.KeyAvailable)

            {

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                if (!IsBlockAtPosition(x, y))

                {

                    ClearCursor(x, y);

                }

                switch (keyInfo.Key)

                {

                    case ConsoleKey.W:

                        if (y > 2) y--;

                        break;

                    case ConsoleKey.S:

                        if (y < height - 2) y++;

                        break;

                    case ConsoleKey.A:

                        if (x > 1) x--;

                        break;

                    case ConsoleKey.D:

                        if (x < width - 2) x++;

                        break;

                    case ConsoleKey.Spacebar:

                        PlaceBlock(drawing, x, y);

                        break;

                    case ConsoleKey.Escape:

                        exit = true;

                        break;

                }

                DisplayCurrentSelection();

            }
            ShowMenu();
        }

    }

    static void PlaceBlock(Drawing drawing, int x, int y)

    {

        Console.SetCursorPosition(x, y);

        Console.ForegroundColor = currentColor;

        Console.Write(currentCharacter);

        Console.ResetColor();

        // Blokk hozzáadása az aktuális rajzhoz

        currentDrawing.Add((x, y, currentCharacter, currentColor));

        // Pont adatbázisba mentése

        var point = new Point

        {

            X = x,

            Y = y,

            Character = currentCharacter,

            Color = currentColor,

            DrawingId = drawing.Id,

            Drawing = drawing

        };

        drawing.Points.Add(point);

        dbContext.Points.Add(point);

        dbContext.SaveChanges();

    }

    static void DrawFrame(int width, int height)

    {

        Console.SetCursorPosition(0, 1);

        Console.Write("╔" + new string('═', width - 2) + "╗");

        for (int i = 2; i < height - 1; i++)

        {

            Console.SetCursorPosition(0, i);

            Console.Write("║");

            Console.SetCursorPosition(width - 1, i);

            Console.Write("║");

        }

        Console.SetCursorPosition(0, height - 1);

        Console.Write("╚" + new string('═', width - 2) + "╝");

    }

    static void RestoreConsoleState(List<(int x, int y, char character, ConsoleColor color)> savedDrawing)

    {

        foreach (var (x, y, character, color) in savedDrawing)

        {

            Console.SetCursorPosition(x, y);

            Console.ForegroundColor = color;

            Console.Write(character);

            Console.ResetColor();

        }

    }

    static void DisplayCurrentSelection()

    {

        Console.SetCursorPosition(2, 0);

        Console.Write("Karakter: ");

        Console.Write(currentCharacter);

        Console.Write(" Szín: ");

        Console.ForegroundColor = currentColor;

        Console.Write("█");

        Console.ResetColor();

    }

    static void DrawCursor(int x, int y)

    {

        Console.SetCursorPosition(x, y);

        Console.ForegroundColor = currentColor;

        Console.Write(currentCharacter);

        Console.ResetColor();

    }

    static void ClearCursor(int x, int y)

    {

        Console.SetCursorPosition(x, y);

        Console.Write(" ");

    }

    static bool IsBlockAtPosition(int x, int y)

    {

        return currentDrawing.Exists(block => block.x == x && block.y == y);

    }

}