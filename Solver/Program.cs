using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Transactions;

namespace Solver
{



    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Geben sie die Seitenlänge des gewünschten Sudoku feldes ein, Die Quadratwurzel der Zahl muss eine glatte Zahl sein.");
            Console.WriteLine("(Empfohlene größen: 9,16,25,36,49,64)");
            int size = int.Parse(Console.ReadLine());



            Field[,] board = IntializeBoard(size, size);
            List<Field> unsolved = new List<Field>();
            foreach (var item in board)
            {
                if (!item.Value.HasValue)
                {
                    unsolved.Add(item);
                }
            }

            PrintGrid(board);

            List<Field> lowestEntropyFields = new List<Field>();
            List<Field> solvedFields = new List<Field>();
            int lowestEntropy = 250;
            int loopCount = 0;



            while (lowestEntropy > 0)
            {
                lowestEntropy = 250;
                foreach (var item in unsolved)
                {
                    CalculateEntropy(item, board);
                }

                foreach (var item in unsolved)
                {
                    if (item.Superpositions.Count == lowestEntropy)
                    {
                        lowestEntropyFields.Add(item);
                    }
                    if (item.Superpositions.Count < lowestEntropy && item.Superpositions.Count > 1)
                    {
                        lowestEntropyFields.Clear();
                        lowestEntropyFields.Add(item);
                        lowestEntropy = item.Superpositions.Count;
                    }
                    if (item.Superpositions.Count == 0)
                    {
                        solvedFields.Add(item);
                    }
                }


                Random random = new Random();

                if (lowestEntropyFields.Count > 0)
                {
                    Field randomField = lowestEntropyFields[random.Next(0, lowestEntropyFields.Count)];
                    randomField.Value = randomField.Superpositions[random.Next(0, randomField.Superpositions.Count)];
                    unsolved.Remove(randomField);
                }
                if (lowestEntropyFields.Count == 0)
                {
                    break;
                }

                if (unsolved.Count == 0)
                {
                    Console.WriteLine("Finished after " + loopCount + " runs");
                    break;
                }


                lowestEntropyFields.Clear();

                Console.WriteLine(solvedFields.Count + " Fields Solved");

            }
            PrintGrid(board);

            Field[][] outputBoard = ConvertToJagged(board);

            string json = JsonSerializer.Serialize(outputBoard);
            Console.WriteLine(json);
        }

        static Field[,] IntializeBoard(int x_dims, int y_dims, int?[,] set_positions = null)
        {

            Field[,] board = new Field[x_dims, y_dims];

            for (int x = 0; x < x_dims; x++)
            {
                for (int y = 0; y < y_dims; y++)
                {
                    board[x, y] = new Field();
                    if (set_positions != null && set_positions[x, y].HasValue)
                    {
                        board[x, y].Value = set_positions[x, y].Value;
                    }
                    board[x, y].x = x;
                    board[x, y].y = y;
                }
            }
            return board;
        }

        static void PrintGrid(Field[,] grid)
        {
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);
            String Out = " ";

            for (int x = 0; x < rows; x++)
            {
                for (int y = 0; y < cols; y++)
                {
                    if (grid[x, y].Value.HasValue)
                    {
                        Out = grid[x, y].Value.ToString() + " ";
                    }
                    else
                    {
                        Out = "N ";
                    }
                    Console.Write(Out);
                }
                Console.WriteLine();
            }
        }

        static void CalculateEntropy(Field field, Field[,] grid)
        {
            List<int> inRow = new List<int>();
            List<int> inCol = new List<int>();
            List<int> inSub = new List<int>();
            List<int> Possible = new List<int>();

            for (int x = 0; x < grid.GetLength(0); x++)
            {
                if (grid[x, field.y].Value.HasValue)
                {
                    inCol.Add(grid[x, field.y].Value.Value);
                }
            }
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                if (grid[field.x, y].Value.HasValue)
                {
                    inRow.Add(grid[field.x, y].Value.Value);
                }
            }

            int offsetX = findSubgridPoint(field.x, grid.GetLength(0));
            int offsetY = findSubgridPoint(field.y, grid.GetLength(1));
            for (int x = offsetX; x <= offsetX + 2; x++)
            {
                for (int y = offsetY; y <= offsetY + 2; y++)
                {
                    if (grid[x, y].Value.HasValue)
                    {
                        inSub.Add(grid[x, y].Value.Value);
                    }
                }
            }

            for (int i = 1; i <= grid.GetLength(0); i++)
            {
                if (!inRow.Contains(i) && !inCol.Contains(i) && !inSub.Contains(i))
                {
                    Possible.Add(i);
                }
            }
            field.Superpositions = Possible;
        }

        static int findSubgridPoint(int number, int gridsize)
        {
            double gridsizeDouble = gridsize;
            double remainderDouble = number % Math.Sqrt(gridsizeDouble);
            int remainder = (int)remainderDouble;

            if (remainder == 0)
            {
                return number;
            }

            return number - remainder;
        }

        static Field[][] ConvertToJagged(Field[,] board)
        {
            int rows = board.GetLength(0);
            int cols = board.GetLength(1);

            Field[][] output = new Field[rows][];

            for (int x = 0; x < rows; x++)
            {
                output[x] = new Field[cols];
                for (int y = 0; y < cols; y++)
                {
                    output[x][y] = board[x, y];
                }
            }

            return output;
        }

    }
    class Field
    {
        public int x { get; set; }
        public int y { get; set; }
        public int? Value { get; set; }
        public List<int> Superpositions { get; set; }
    }
}
