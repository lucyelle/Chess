using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess
{
    class Program
    {
        static void Main(string[] args)
        {
            Board board = new Board();
            board.Draw();

            int round = 0;
            while (true)
            {
                bool canContinue = PlayerTurn(board, round % 2 == 0 ? FigureColor.White : FigureColor.Black);
                if (!canContinue)
                {
                    break;
                }
                Console.Clear();
                board.Draw();
                round++;
            }
            Console.ReadKey();
        }

        static bool PlayerTurn(Board board, FigureColor color)
        {
            StepPossibility stepState = board.GetStepPossibilities(color);
            if (stepState == StepPossibility.CheckMate)
            {
                Console.WriteLine($"Checkmate! {color} lost!");
                return false;
            }
            if (stepState == StepPossibility.StaleMate)
            {
                Console.WriteLine($"Stalemate! It's a draw!");
                return false;
            }

            Console.WriteLine(color);
            while (true)
            {
                (int, int) from = GetValidCoordinates(board, color);
                Figure figure = board.Figures[from.Item1, from.Item2];
                if (figure != null && figure.Color == color)
                {
                    (int, int) to = GetValidCoordinates(board, color);
                    if (board.StepWithoutCheck(from.Item1, from.Item2, to.Item1, to.Item2, color))
                    {
                        board.Figures[from.Item1, from.Item2] = null;
                        board.Figures[to.Item1, to.Item2] = figure;
                        return true;
                    }
                    else
                    {
                        Console.Clear();
                        board.Draw();
                        Console.WriteLine(color);
                        Console.WriteLine("You can't step there!");
                    }
                }
                else
                {
                    Console.Clear();
                    board.Draw();
                    Console.WriteLine(color);
                    Console.WriteLine("Not your figure you naughty boy!");
                }
            }
        }

        static (int, int) GetValidCoordinates(Board board, FigureColor color)
        {
            while (true)
            {
                (int, int)? input = GetCoordinates();
                if (input != null)
                {
                    return input.Value;
                }
                Console.Clear();
                board.Draw();
                Console.WriteLine(color);
                Console.WriteLine("Invalid coordinates!");
            }
        }

        static (int, int)? GetCoordinates()
        {
            string input = Console.ReadLine().Trim();
            if (input.Length != 2)
            {
                return null;
            }
            char column = char.ToUpper(input[0]);
            char row = char.ToUpper(input[1]);
            if (!char.IsLetter(column) || !char.IsDigit(row))
            {
                return null;
            }
            if (row < '1' || row > '8' || column < 'A' || column > 'H')
            {
                return null;
            }
            return (column - 'A', 7 - (row - '1'));
        }
    }

    enum StepPossibility
    {
        CheckMate, StaleMate, Continue
    }

    class Board
    {
        public readonly Figure[,] Figures = new Figure[8, 8];

        public Board()
        {
            for (int i = 0; i < 8; i++)
            {
                Figures[i, 1] = new Pawn(FigureColor.Black);
                Figures[i, 6] = new Pawn(FigureColor.White);
            }
            Figures[1, 0] = new Knight(FigureColor.Black);
            Figures[6, 0] = new Knight(FigureColor.Black);
            Figures[1, 7] = new Knight(FigureColor.White);
            Figures[6, 7] = new Knight(FigureColor.White);
            Figures[0, 0] = new Rook(FigureColor.Black);
            Figures[7, 0] = new Rook(FigureColor.Black);
            Figures[0, 7] = new Rook(FigureColor.White);
            Figures[7, 7] = new Rook(FigureColor.White);
            Figures[2, 0] = new Bishop(FigureColor.Black);
            Figures[5, 0] = new Bishop(FigureColor.Black);
            Figures[2, 7] = new Bishop(FigureColor.White);
            Figures[5, 7] = new Bishop(FigureColor.White);
            Figures[3, 0] = new Queen(FigureColor.Black);
            Figures[3, 7] = new Queen(FigureColor.White);
            Figures[4, 0] = new King(FigureColor.Black);
            Figures[4, 7] = new King(FigureColor.White);
        }

        public void Draw()
        {
            Console.WriteLine("        A     B     C     D     E     F     G     H");
            Console.WriteLine("    --------------------------------------------------");

            for (int j = 0; j < 8; j++)
            {
                for (int height = 0; height < 3; height++)
                {
                    if (height == 1)
                    {
                        Console.ResetColor();
                        Console.Write("  " + (8 - j) + " |");
                    }
                    else
                    {
                        Console.ResetColor();
                        Console.Write("    |");
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        Console.BackgroundColor = (i + (j % 2)) % 2 == 0 ? ConsoleColor.White : ConsoleColor.Black;
                        Figure figure = Figures[i, j];
                        if (figure != null && height == 1)
                        {
                            Console.ForegroundColor = figure.Color == FigureColor.White ? ConsoleColor.DarkYellow : ConsoleColor.Blue;
                            Console.Write("  " + figure.Visuals() + "  ");
                        }
                        else
                        {
                            Console.Write("      ");
                        }
                    }

                    if (height == 1)
                    {
                        Console.ResetColor();
                        Console.Write("| " + (8 - j) + "  ");
                    }
                    else
                    {
                        Console.ResetColor();
                        Console.Write("|    ");
                    }
                        Console.WriteLine();
                }
            }
            Console.WriteLine("    --------------------------------------------------");
            Console.WriteLine("        A     B     C     D     E     F     G     H");
            Console.ResetColor();
        }

        public (int, int) GetKing(FigureColor color)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (Figures[x, y] != null && Figures[x, y].Visuals() == "KG" && Figures[x, y].Color == color)
                    {
                        return (x, y);
                    }
                }
            }
            throw new InvalidOperationException($"No king with color {color} found!");
        }

        public bool IsKingChecked(FigureColor color)
        {
            var (kingX, kingY) = GetKing(color);

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (Figures[x, y] != null && Figures[x, y].Color != color && Figures[x, y].CanStep(x, y, kingX, kingY, this))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public StepMemento StepWithMemento(int xFrom, int yFrom, int xTo, int yTo, FigureColor color)
        {
            if (Figures[xFrom, yFrom] == null)
            {
                return null;
            }
            if (Figures[xFrom, yFrom].Color == color && Figures[xFrom, yFrom].CanStep(xFrom, yFrom, xTo, yTo, this))
            {
                Figure oldAtTo = Figures[xTo, yTo];
                Figures[xTo, yTo] = Figures[xFrom, yFrom];
                Figures[xFrom, yFrom] = null;
                return new StepMemento(xFrom, yFrom, xTo, yTo, oldAtTo);
            }
            return null;
        }

        public void RestoreMemento(StepMemento memento)
        {
            Figures[memento.FromX, memento.FromY] = Figures[memento.ToX, memento.ToY];
            Figures[memento.ToX, memento.ToY] = memento.OldAtTo;
        }

        public StepPossibility GetStepPossibilities(FigureColor color)
        {
            for (int yfrom = 0; yfrom < 8; yfrom++)
            {
                for (int xfrom = 0; xfrom < 8; xfrom++)
                {
                    for (int yto = 0; yto < 8; yto++)
                    {
                        for (int xto = 0; xto < 8; xto++)
                        {
                            StepMemento memento = StepWithMemento(xfrom, yfrom, xto, yto, color);
                            if (memento != null)
                            {
                                if (!IsKingChecked(color))
                                {
                                    RestoreMemento(memento);
                                    return StepPossibility.Continue;
                                }
                                RestoreMemento(memento);
                            }
                        }
                    }
                }
            }
            if (IsKingChecked(color))
            {
                return StepPossibility.CheckMate;
            }
            return StepPossibility.StaleMate;
        }

        public bool StepWithoutCheck(int xFrom, int yFrom, int xTo, int yTo, FigureColor color)
        {
            StepMemento memento = StepWithMemento(xFrom, yFrom, xTo, yTo, color);
            if (memento != null && !IsKingChecked(color))
            {
                return true;
            }
            if (memento != null)
            {
                RestoreMemento(memento);
            }
            return false;
        }
    }

    class StepMemento
    {
        public readonly int FromX;
        public readonly int FromY;
        public readonly int ToX;
        public readonly int ToY;
        public readonly Figure OldAtTo;

        public StepMemento(int fromX, int fromY, int toX, int toY, Figure oldAtTo)
        {
            FromX = fromX;
            FromY = fromY;
            ToX = toX;
            ToY = toY;
            OldAtTo = oldAtTo;
        }
    }

    enum FigureColor
    {
        White, Black
    }

    abstract class Figure
    {
        public readonly FigureColor Color;

        public Figure(FigureColor color)
        {
            Color = color;
        }
        public abstract bool CanStep(int xFrom, int yFrom, int xTo, int yTo, Board board);
        public abstract string Visuals();
    }

    class Pawn : Figure
    {
        public Pawn(FigureColor color) : base(color)
        {
        }

        private int DirectionConstant()
        {
            return Color == FigureColor.White ? -1 : 1;
        }

        private int StartConstant()
        {
            return Color == FigureColor.White ? 6 : 1;
        }

        public override bool CanStep(int xFrom, int yFrom, int xTo, int yTo, Board board)
        {
            /*
             * - y irányban 1-et mozdul (ha nincs előtte bábu)
             * - kezdőhelyről 2-t tud (ha nincs előtte bábu egyik helyen sem)
             * - ütés: x és y is 1-et változik, ha van ott ellenség
             */

            if (xFrom == xTo && yFrom + DirectionConstant() == yTo && board.Figures[xTo, yTo] == null)
            {
                return true;
            }
            if (xFrom == xTo 
                && yFrom == StartConstant()
                && yFrom + 2 * DirectionConstant() == yTo
                && board.Figures[xTo, yTo] == null
                && board.Figures[xTo, yTo - DirectionConstant()] == null)
            {
                return true;
            }
            if (Math.Abs(xFrom - xTo) == 1 && yFrom + DirectionConstant() == yTo && board.Figures[xTo, yTo] != null && Color != board.Figures[xTo, yTo].Color)
            {
                return true;
            }
            return false;
        }

        public override string Visuals()
        {
            return "pw";
        }
    }

    class Knight : Figure
    {
        public Knight(FigureColor color) : base(color)
        {
        }

        public override bool CanStep(int xFrom, int yFrom, int xTo, int yTo, Board board)
        {
            /*
             *  - rövidebb szárral indul: először 1-et lép, aztán 2-t
             *  - hosszabb szárral indul: először 2-t lép, aztán 1-et
             *  - ha nem ellenség van a célon, nem léphet
             */
             if (((Math.Abs(yTo - yFrom) == 1 && Math.Abs(xTo - xFrom) == 2)
                || (Math.Abs(xTo - xFrom) == 1 && Math.Abs(yTo - yFrom) == 2)) 
                && (board.Figures[xTo, yTo] == null || Color != board.Figures[xTo, yTo].Color))
             {
                 return true;
             }
             return false;
        }

        public override string Visuals()
        {
            return "Kt";
        }
    }

    class Rook : Figure
    {
        public Rook(FigureColor color) : base(color)
        {
        }

        public override bool CanStep(int xFrom, int yFrom, int xTo, int yTo, Board board)
        {
            /*
             * - csak x irányban mozog: + vagy -
             * - csak y irányban mozog: + vagy -
             * - útközben nem lehet bábu
             * - cél: vagy null, vagy ellenség
             */
            if (board.Figures[xTo, yTo] != null && board.Figures[xTo, yTo].Color == this.Color)
            {
                return false;
            }
            if (xFrom != xTo && yFrom != yTo)
            {
                return false;
            }
            {

            }
            if (yFrom == yTo)
            {
                int from = xFrom;
                for (int i = 0; i < Math.Abs(xFrom - xTo) - 1; i++)
                {
                    if (xFrom - xTo > 0 && board.Figures[from - 1, yTo] == null)
                    {
                        from--;
                    }
                    else if (xFrom - xTo < 0 && board.Figures[from + 1, yTo] == null)
                    {
                        from++;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            if (xFrom == xTo)
            {
                int from = yFrom;
                for (int i = 0; i < Math.Abs(yFrom - yTo) - 1; i++)
                {
                    if (yFrom - yTo > 0 && board.Figures[xTo, from - 1] == null)
                    {
                        from--;
                    }
                    else if (yFrom - yTo < 0 && board.Figures[xTo, from + 1] == null)
                    {
                        from++;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            return true;
        }
        public override string Visuals()
        {
            return "Rk";
        }
    }

    class Bishop : Figure
    {
        public Bishop(FigureColor color) : base(color)
        {
        }

        public override bool CanStep(int xFrom, int yFrom, int xTo, int yTo, Board board)
        {
            /*
             * - ha xTo - xFrom = yTo - yFrom
             * - ha a cél null vagy ellenség
             * - ha útközben nincs bábu
             */
            if (Math.Abs(xTo - xFrom) == Math.Abs(yTo - yFrom))
            {
                if (board.Figures[xTo, yTo] == null || board.Figures[xTo, yTo].Color != this.Color)
                {
                    int x = xFrom;
                    int y = yFrom;
                    for (int i = 0; i < Math.Abs(xTo - xFrom) - 1; i++)
                    {
                        if (xTo - xFrom > 0 && yTo - yFrom < 0 && board.Figures[x + 1, y - 1] == null)
                        {
                            x++;
                            y--;
                        }
                        else if (xTo - xFrom > 0 && yTo - yFrom > 0 && board.Figures[x + 1, y + 1] == null)
                        {
                            x++;
                            y++;
                        }
                        else if (xTo - xFrom < 0 && yTo - yFrom < 0 && board.Figures[x - 1, y - 1] == null)
                        {
                            x--;
                            y--;
                        }
                        else if (xTo - xFrom < 0 && yTo - yFrom > 0 && board.Figures[x - 1, y + 1] == null)
                        {
                            x--;
                            y++;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }
            return false;
        }

        public override string Visuals()
        {
            return "Bi";
        }
    }

    class Queen : Figure
    {
        private Figure rook;
        private Figure bishop;

        public Queen(FigureColor color) : base(color)
        {
            rook = new Rook(color);
            bishop = new Bishop(color);
        }

        public override bool CanStep(int xFrom, int yFrom, int xTo, int yTo, Board board)
        {
            return rook.CanStep(xFrom, yFrom, xTo, yTo, board) || bishop.CanStep(xFrom, yFrom, xTo, yTo, board);
        }

        public override string Visuals()
        {
            return "Qn";
        }
    }

    class King : Figure
    {
        public King(FigureColor color) : base(color)
        {
        }

        public override bool CanStep(int xFrom, int yFrom, int xTo, int yTo, Board board)
        {
            /*
             * - minden irányba 1-et léphet
             * - célon null vagy ellenség
             */

            if (board.Figures[xTo, yTo] == null || board.Figures[xTo, yTo].Color != this.Color)
            {
                if (Math.Abs(xTo - xFrom) <= 1 && Math.Abs(yTo - yFrom) <= 1)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        public override string Visuals()
        {
            return "KG";
        }
    }
}
