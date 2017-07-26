using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleShipAlone
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;
            Console.SetWindowSize(42, 22);
            Console.SetBufferSize(42, 22);
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.Clear();
            Map a = new Map(true);
            a = new Map(true);
            a = new Map(true);
            a = new Map(true);
            a = new Map(true);
            a.Print(true);
            while (true)
            {
                DoTurn(a);
                a.Print(true);
            }

        }

        static void DoTurn(Map map)
        {
            ConsoleKey k = 0;
            while (k != ConsoleKey.Enter)
            {
                k = Console.ReadKey(true).Key;
                switch (k)
                {
                    case ConsoleKey.UpArrow: if (map.SelectionY > 0) map.SelectionY = (byte)(map.SelectionY - 1); map.Print(true); break;
                    case ConsoleKey.DownArrow: if (map.SelectionY < 9) map.SelectionY = (byte)(map.SelectionY + 1); map.Print(true); break;
                    case ConsoleKey.LeftArrow: if (map.SelectionX > 0) map.SelectionX = (byte)(map.SelectionX - 1); map.Print(true); break;
                    case ConsoleKey.RightArrow: if (map.SelectionX < 9) map.SelectionX = (byte)(map.SelectionX + 1); map.Print(true); break;
                }

            }
            map.Hit();
        }

    }

    class Point
    {
        public byte X { get; set; }
        public byte Y { get; set; }

        public Point(byte x, byte y)
        {
            X = x;
            Y = y;
        }

        public static bool operator ==(Point p1, Point p2)
        {
            return p1.X == p2.X && p1.Y == p2.Y;
        }

        public static bool operator !=(Point p1, Point p2)
        {
            return p1.X != p2.X || p1.Y != p2.Y;
        }

        public override bool Equals(object obj)
        {
            Point instance = (Point)obj;
            return X == instance.X && Y == instance.Y;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }

    class Ship
    {
        public Map Map { get; set; }
        public byte Length { get; set; }
        public Point[] Points { get; set; }
        public bool IsVertical { get; set; }

        List<Point> zonePoints;
        bool placed;
        byte health;

        public Ship(Map map, byte length, byte x, byte y, bool isVertical)
        {
            Map = map;
            Length = length;
            health = Length;
            Points = new Point[4];
            zonePoints = new List<Point>();
            IsVertical = isVertical;
            for (int i = 0; i < length; i++)
            {
                if (IsVertical)
                {
                    Points[i] = new Point(x, (byte)(y + i));
                }
                else
                {
                    Points[i] = new Point((byte)(x + i), y);
                }
            }

            for (byte i = 0; i < 3; i++)
            {
                if (isVertical)
                {
                    zonePoints.Add(new Point((byte)(Points[0].X - 1 + i), (byte)(Points[0].Y - 1)));
                    zonePoints.Add(new Point((byte)(Points[0].X - 1 + i), (byte)(Points[0].Y + length)));
                }
                else
                {
                    zonePoints.Add(new Point((byte)(Points[0].X - 1), (byte)(Points[0].Y - 1 + i)));
                    zonePoints.Add(new Point((byte)(Points[0].X + length), (byte)(Points[0].Y - 1 + i)));
                }
            }

            for (int i = 0; i < length; i++)
            {
                if (isVertical)
                {
                    zonePoints.Add(new Point((byte)(Points[0].X - 1), (byte)(Points[0].Y + i)));
                    zonePoints.Add(new Point((byte)(Points[0].X + 1), (byte)(Points[0].Y + i)));
                }
                else
                {
                    zonePoints.Add(new Point((byte)(Points[0].X + i), (byte)(Points[0].Y - 1)));
                    zonePoints.Add(new Point((byte)(Points[0].X + i), (byte)(Points[0].Y + 1)));
                }
            }
        }

        public bool Place()
        {
            placed = Set(1);
            if (placed)
            {
                SetZone(2);
                Map.PlaceShip(this);
            }
            return placed;
        }

        public void PlaceMark()
        {
            Set(3);
        }

        public void Clear()
        {
            Set(0);
        }

        bool Set(byte point)
        {
            int can = 0;
            bool place = false;
            for (int i = 0; i < Length; i++)
            {
                byte x = Points[i].X;
                byte y = Points[i].Y;

                if (!place)
                    can = (Map[x, y] != 1 && Map[x, y] != 2 && (x < 10) && (y < 10)) ? can + 1 : can;
                else
                    Map[x, y] = point;
                if (!place && can == Length)
                {
                    place = true;
                    i = -1;
                }
            }
            return place;
        }

        void SetZone(byte point)
        {

            foreach (Point zonePoint in zonePoints)
            {
                byte x = zonePoint.X;
                byte y = zonePoint.Y;
                Map[x, y] = point;
            }
        }

        public void Hit(byte x, byte y)
        {
            if (health > 0)
            {
                Map.LastMessage = "|         Пробил!      |";
                Map[x, y] = 5;
                health--;
                if (health == 0)
                {
                    OnHealthZero();
                }
            }
            else
            {
                OnHealthZero();
            }
        }

        private void OnHealthZero()
        {
            Map.LastMessage = "|        Потопил!      |";
            Set(6);
            SetZone(4);
        }
    }

    class Map
    {
        const char mapEmpty = '.', mapMiss = 'o', mapHit = '*', mapKill = 'x', mapPoint = '●', mapSect = '■', mapZona = '○';
        const int MAPSIZE = 10;

        protected int X = 0, Y = 0;

        public byte this[int x, int y]
        {
            get { if (x < 10 && y < 10) return b[x, y]; else return 255; }
            set { if (x < 10 && y < 10) b[x, y] = value; }
        }

        public Ship[] Ships { get { return ships; } }

        public string LastMessage { get; set; }
        public byte SelectionX = 0, SelectionY = 0;

        static readonly byte[] shipRule = { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };
        int rule = 0;

        byte[,] b;

        int shipIndex = 0;
        Ship[] ships = new Ship[10];

        public Map(bool randomize = false)
        {
            b = new byte[10, 10];
            Initialize(randomize);
        }

        public void Print(bool hide = false)
        {
            Console.SetCursorPosition(X, Y);
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLine("+----------------------+-");
            WriteLine(LastMessage);
            WriteLine("+----------------------+-");
            Console.ForegroundColor = ConsoleColor.Green;
            WriteLine(string.Format("Игрок  :                 "));
            WriteLine("   | А Б В Г Д Е Ё Ж З К ");
            WriteLine(" --+-------------------- ");
            for (int i = 0; i < 10; i++)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                if (i < 9) Console.Write((i + 1) + "  ¦ ");
                else Console.Write((i + 1) + " ¦ ");
                for (int j = 0; j < 10; j++)
                {
                    char p = '\0';

                    switch (b[j, i])
                    {
                        case 0: Console.ForegroundColor = ConsoleColor.White; Console.BackgroundColor = ConsoleColor.Blue; p = mapEmpty; break;
                        case 1: Console.ForegroundColor = ConsoleColor.White; Console.BackgroundColor = ConsoleColor.Blue; p = mapPoint; break;
                        case 2: Console.ForegroundColor = ConsoleColor.White; Console.BackgroundColor = ConsoleColor.Blue; p = mapZona; break;
                        case 3: Console.ForegroundColor = ConsoleColor.White; Console.BackgroundColor = ConsoleColor.Blue; p = mapSect; break;
                        case 4: Console.ForegroundColor = ConsoleColor.DarkCyan; Console.BackgroundColor = ConsoleColor.Blue; p = mapMiss; break;
                        case 5: Console.ForegroundColor = ConsoleColor.Red; Console.BackgroundColor = ConsoleColor.Blue; p = mapHit; break;
                        case 6: Console.ForegroundColor = ConsoleColor.Red; Console.BackgroundColor = ConsoleColor.Blue; p = mapKill; break;
                    }
                    if (hide && j == SelectionX && i == SelectionY)
                        Console.BackgroundColor = ConsoleColor.Green;

                    if (hide)
                    {
                        if (b[j, i] == 1 || b[j, i] == 2) Console.Write(". ");
                        else Console.Write(p + " ");
                    }
                    else
                    {
                        Console.Write(p + " ");
                    }
                    Console.BackgroundColor = ConsoleColor.Blue;
                }
                Console.SetCursorPosition(0, Console.CursorTop + 1);
            }
        }

        public void PlaceShip(Ship ship)
        {
            ships[shipIndex++] = ship;
            rule++;
        }

        void WriteLine(string line)
        {
            Console.WriteLine(line);
            Console.SetCursorPosition(X, Console.CursorTop);
        }

        void Initialize(bool randomize)
        {
            if (!randomize)
            {
                byte selectionX = 0;
                byte selectionY = 0;

                bool isVertical = false;

                Ship ship = new Ship(this, 4, selectionX, selectionY, isVertical);

                while (rule < 10)
                {
                    byte length = shipRule[rule];
                    ship = new Ship(this, length, selectionX, selectionY, isVertical);
                    ship.PlaceMark();
                    Print();

                    ConsoleKey key = Console.ReadKey(true).Key;

                    ship.Clear();

                    switch (key)
                    {
                        case ConsoleKey.UpArrow: if (selectionY > 0) selectionY--; break;
                        case ConsoleKey.DownArrow:
                            selectionY = SetSelection(selectionY, isVertical, length);
                            break;
                        case ConsoleKey.LeftArrow: if (selectionX > 0) selectionX--; break;
                        case ConsoleKey.RightArrow:
                            selectionX = SetSelection(selectionX, isVertical, length);
                            break;
                        case ConsoleKey.V: isVertical = !isVertical; break;
                        case ConsoleKey.Enter: ship = new Ship(this, length, selectionX, selectionY, isVertical); if (ship.Place()) ship = null; continue;
                    }

                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(selectionX + " " + selectionY);


                }

            }
            else
            {
                Random rnd = new Random();

                while (rule < 10)
                {
                    byte x = (byte)rnd.Next(0, 9);
                    byte y = (byte)rnd.Next(0, 9);
                    byte length = shipRule[rule];
                    bool isVertical = rnd.Next(100) < 40;

                    new Ship(this, length, x, y, isVertical).Place();
                }


            }

            for (int i = 0; i < 10; ++i)
            {
                for (int j = 0; j < 10; ++j)
                {
                    if (b[i, j] == 2) b[i, j] = 0;
                }
            }
            Console.Clear();
            Print();
            Console.ReadLine();
        }

        byte SetSelection(byte selection, bool isVertical, byte length)
        {
            if (selection < MAPSIZE - 1)
                selection++;
            return selection;
        }

        public bool Hit()
        {
            byte x = SelectionX;
            byte y = SelectionY;
            if (b[x, y] == 1)
            {
                foreach (Ship ship in ships)
                {
                    if (ship.Points.Contains(new Point(x, y)))
                    {
                        ship.Hit(x, y);
                        return true;
                    }
                }
            }

            if (b[x, y] == 0)
            {
                LastMessage = "|         Мимо!        |";
                b[x, y] = 4;
                return false;
            }
            LastMessage = "|Вы уже атаковали здесь|";
            return true;
        }
    }
}
