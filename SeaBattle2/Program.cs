using System;
using System.Collections.Generic;
using System.Linq;

namespace SeaBattle
{
    class Program
    {
        static void Main(string[] args)
        {
            GameSession gameSession = new GameSession();
            gameSession.Start();
        }
    }

    class BattleArea
    {
        public int Dimension;
        public char[,] Layout;

        public BattleArea(int dimension)
        {
            Dimension = dimension;
            Layout = new char[Dimension, Dimension];
            Initialize();
        }

        public void Initialize()
        {
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    Layout[i, j] = '~';
                }
            }
        }

        public void Display(bool hideUnits = true)
        {
            Console.Write("   ");
            for (int i = 0; i < Dimension; i++)
            {
                Console.Write($"{i} ");
            }
            Console.WriteLine();

            for (int i = 0; i < Dimension; i++)
            {
                Console.Write($"{i}  ");
                for (int j = 0; j < Dimension; j++)
                {
                    if (hideUnits && Layout[i, j] == 'S')
                    {
                        Console.Write("~ ");
                    }
                    else
                    {
                        Console.Write($"{Layout[i, j]} ");
                    }
                }
                Console.WriteLine();
            }
        }

        public bool IsValidDeployment(int row, int col)
        {
            return row >= 0 && row < Dimension && col >= 0 && col < Dimension && Layout[row, col] == '~';
        }

        public bool TryPlaceUnit(int row, int col, int size, bool horizontal)
        {
            if (horizontal)
            {
                if (col + size > Dimension) return false;
                for (int i = 0; i < size; i++)
                {
                    if (!IsValidDeployment(row, col + i) || !IsAdjacentToShip(row, col + i)) return false;
                }

                for (int i = 0; i < size; i++)
                {
                    Layout[row, col + i] = 'S';
                }
            }
            else
            {
                if (row + size > Dimension) return false;
                for (int i = 0; i < size; i++)
                {
                    if (!IsValidDeployment(row + i, col) || !IsAdjacentToShip(row + i, col)) return false;
                }
                for (int i = 0; i < size; i++)
                {
                    Layout[row + i, col] = 'S';
                }
            }
            return true;
        }

        private bool IsAdjacentToShip(int row, int col)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int newRow = row + i;
                    int newCol = col + j;

                    if (newRow >= 0 && newRow < Dimension && newCol >= 0 && newCol < Dimension && Layout[newRow, newCol] == 'S')
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool HandleShot(int row, int col)
        {
            if (row < 0 || row >= Dimension || col < 0 || col >= Dimension)
            {
                return false;
            }

            if (Layout[row, col] == 'X' || Layout[row, col] == 'O')
            {
                return false;
            }

            if (Layout[row, col] == 'S')
            {
                Layout[row, col] = 'X';
                if (IsShipDestroyed(row, col))
                {
                    MarkAdjacentCellsAsMissed(row, col);
                }
                return true;
            }
            else if (Layout[row, col] == '~')
            {
                Layout[row, col] = 'O';
                return false;
            }
            else
            {
                return false;
            }
        }

        public bool HasShipsRemaining()
        {
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    if (Layout[i, j] == 'S')
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void MarkAdjacentCellsAsMissed(int row, int col)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int newRow = row + i;
                    int newCol = col + j;

                    if (newRow >= 0 && newRow < Dimension && newCol >= 0 && newCol < Dimension && Layout[newRow, newCol] == '~')
                    {
                        Layout[newRow, newCol] = 'O';
                    }
                }
            }
        }

        public bool IsShipDestroyed(int row, int col)
        {
            int horizontalCount = 1;
            int k = 1;
            while (col - k >= 0 && Layout[row, col - k] == 'X')
            {
                horizontalCount++;
                k++;
            }

            k = 1;
            while (col + k < Dimension && Layout[row, col + k] == 'X')
            {
                horizontalCount++;
                k++;
            }

            if (horizontalCount > 1 && (col - k < 0 || col + k >= Dimension || (Layout[row, col - k] != 'S' && Layout[row, col + k] != 'S')))
                return true;


            int verticalCount = 1;
            k = 1;
            while (row - k >= 0 && Layout[row - k, col] == 'X')
            {
                verticalCount++;
                k++;
            }

            k = 1;
            while (row + k < Dimension && Layout[row + k, col] == 'X')
            {
                verticalCount++;
                k++;
            }

            if (verticalCount > 1 && (row - k < 0 || row + k >= Dimension || (Layout[row - k, col] != 'S' && Layout[row + k, col] != 'S')))
                return true;

            return false;
        }

    }

    class Participant
    {
        public BattleArea Area;
        public string Title;

        public Participant(string title, int areaDimension)
        {
            Title = title;
            Area = new BattleArea(areaDimension);
        }

        public (int row, int col) GetTargetCoordinates()
        {
            int row, col;
            while (true)
            {
                Console.Write($"{Title}, введите координаты цели (строка столбец, например, 1 2): ");
                string input = Console.ReadLine();
                string[] parts = input.Split(' ');

                if (parts.Length == 2 && int.TryParse(parts[0], out row) && int.TryParse(parts[1], out col))
                {
                    if (row >= 0 && row < Area.Dimension && col >= 0 && col < Area.Dimension)
                    {
                        return (row, col);
                    }
                    else
                    {
                        Console.WriteLine("Неверные координаты. Попробуйте снова.");
                    }
                }
                else
                {
                    Console.WriteLine("Неверный формат ввода. Попробуйте снова.");
                }
            }
        }
    }

    class Computer : Participant
    {
        private Random generator = new Random();

        HashSet<(int, int)> firedPositions = new HashSet<(int, int)>();

        public Computer(string title, int areaDimension) : base(title, areaDimension) { }

        public void AutoDeployUnits(int[] unitSizes)
        {
            foreach (int size in unitSizes)
            {
                bool deployed = false;
                while (!deployed)
                {
                    int row = generator.Next(0, Area.Dimension);
                    int col = generator.Next(0, Area.Dimension);
                    bool isHorizontal = generator.Next(0, 2) == 0;

                    deployed = Area.TryPlaceUnit(row, col, size, isHorizontal);
                }
            }
        }

        public (int row, int col) GetRandomShotCoordinates()
        {
            int row; int col;
            do
            {
                row = generator.Next(0, Area.Dimension);
                col = generator.Next(0, Area.Dimension);
            } while (firedPositions.Contains((row, col)));

            firedPositions.Add((row, col));
            return (row, col);
        }
    }

    class GameSession
    {
        private Participant player;
        private Computer bot;
        private int fieldSize = 10;
        private int[] unitSizes = { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };

        public GameSession()
        {
            player = new Participant("Игрок", fieldSize);
            bot = new Computer("Бот", fieldSize);
        }

        public void Start()
        {
            Console.WriteLine("Добро пожаловать в игру Морской бой!");

            SetupPlayerUnits();

            bot.AutoDeployUnits(unitSizes);

            PlayGame();

            ShowResults();
        }

        private void SetupPlayerUnits()
        {
            Console.WriteLine("Разместите свои корабли на поле:");
            player.Area.Display(false);
            foreach (int size in unitSizes)
            {
                bool placed = false;
                while (!placed)
                {
                    Console.WriteLine($"Разместите корабль длиной {size}.");
                    Console.Write("Введите координаты начала корабля (строка столбец, например, 1 2): ");
                    string input = Console.ReadLine();
                    string[] parts = input.Split(' ');

                    if (parts.Length == 2 && int.TryParse(parts[0], out int row) && int.TryParse(parts[1], out int col))
                    {
                        Console.Write("Горизонтально? (y/n): ");
                        bool isHorizontal = Console.ReadLine().ToLower() == "y";

                        placed = player.Area.TryPlaceUnit(row, col, size, isHorizontal);
                        if (!placed)
                        {
                            Console.WriteLine("Невозможно разместить корабль здесь. Попробуйте снова.");
                        }
                        else
                        {
                            player.Area.Display(false);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Неверный формат ввода. Попробуйте снова.");
                    }
                }
            }

            Console.WriteLine("Корабли расставлены!");
        }

        private void PlayGame()
        {
            while (player.Area.HasShipsRemaining() && bot.Area.HasShipsRemaining())
            {
                PlayerTurn();
                if (!bot.Area.HasShipsRemaining()) break;

                BotTurn();
                if (!player.Area.HasShipsRemaining()) break;
            }
        }

        private void PlayerTurn()
        {
            Console.WriteLine("\nВаш ход:");
            bot.Area.Display();

            (int row, int col) = player.GetTargetCoordinates();
            bool hit = bot.Area.HandleShot(row, col);

            if (hit)
            {
                Console.WriteLine("Попадание!");

                if (bot.Area.IsShipDestroyed(row, col))
                {
                    Console.WriteLine("Корабль уничтожен!");
                    bot.Area.MarkAdjacentCellsAsMissed(row, col);
                }
            }
            else
            {
                Console.WriteLine("Промах.");
            }
        }

        private void BotTurn()
        {
            Console.WriteLine("\nХод бота:");
            (int row, int col) = bot.GetRandomShotCoordinates();
            bool hit = player.Area.HandleShot(row, col);

            Console.WriteLine($"Бот стреляет в {row} {col}.");

            if (hit)
            {
                Console.WriteLine("Бот попал!");
                if (player.Area.IsShipDestroyed(row, col))
                {
                    Console.WriteLine("Ваш корабль уничтожен!");
                    player.Area.MarkAdjacentCellsAsMissed(row, col);
                }
            }
            else
            {
                Console.WriteLine("Бот промахнулся.");
            }
            player.Area.Display(false);
            Console.ReadKey();
        }

        private void ShowResults()
        {
            Console.WriteLine("\nИгра окончена!");

            if (!bot.Area.HasShipsRemaining())
            {
                Console.WriteLine("Поздравляем! Вы победили!");
            }
            else
            {
                Console.WriteLine("Бот победил.");
            }
        }
    }
}