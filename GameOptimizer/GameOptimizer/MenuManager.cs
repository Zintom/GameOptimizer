using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZintomShellHelper
{
    public static class MenuManager
    {

        public static int ESCAPE_KEY = -1;

        /// <summary>
        /// Creates an option menu in text form. Returns the selected option.
        /// </summary>
        /// <returns>Selected item. If 'Escape' pressed will return -1.</returns>
        public static int CreateMenu(string[] options, bool horizontal = false, int positionOffset = 0)
        {
            bool DrawnOnce = false;
            int selectedOption = 0;

            DrawMenu(options, selectedOption, horizontal, ref DrawnOnce, positionOffset);

            while (true)
            {
                while (!Console.KeyAvailable)
                {
                }

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    return selectedOption;
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    return -1;
                }
                else if (keyInfo.Key == ConsoleKey.UpArrow || keyInfo.Key == ConsoleKey.LeftArrow)
                {
                    if (selectedOption > 0)
                        selectedOption -= 1;
                    else
                        selectedOption = options.Length - 1;
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow || keyInfo.Key == ConsoleKey.RightArrow)
                {
                    if (selectedOption < options.Length - 1)
                        selectedOption += 1;
                    else
                        selectedOption = 0;
                }

                DrawMenu(options, selectedOption, horizontal, ref DrawnOnce, positionOffset);
                Thread.Sleep(16);
            }
        }

        /// <summary>
        /// Helper method to create a back menu.
        /// </summary>
        /// <param name="positionOffset">Horizontal Offset of Text.</param>
        public static void CreateBackMenu(int positionOffset = 0)
        {
            CreateMenu(new string[] { "Back" }, false, positionOffset);
        }

        public static void DrawMenu(string[] options, int selected, bool horizontal, ref bool DrawnOnce, int positionOffset)
        {
            if (DrawnOnce && !horizontal)
                Console.CursorTop -= options.Length;
            else if (DrawnOnce && horizontal)
                Console.CursorTop -= 1;

            if (horizontal)
            {
                if (positionOffset > 0)
                    drawOffset(positionOffset);

                for (int o = 0; o < options.Length; o++)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("<");

                    if (o == selected)
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write(options[o]);
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(options[o]);
                    }

                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("> ");
                }
                Console.WriteLine("");
            }
            else
            {
                for (int o = 0; o < options.Length; o++)
                {
                    if (positionOffset > 0)
                        drawOffset(positionOffset);

                    if (o == selected)
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine(options[o]);
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine(options[o]);
                    }
                }
            }

            Console.TreatControlCAsInput = true;
            Console.CursorVisible = false;

            DrawnOnce = true;
        }

        private static void drawOffset(int positionOffset)
        {
            if (positionOffset > 0)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("".PadLeft(positionOffset));
            }
        }

        public static void DrawTitle(string title, string sub_title, string content, bool clear = false)
        {
            if (clear)
                Console.Clear();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("".PadRight(title.Length + 2, '='));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" " + title);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("".PadRight(title.Length + 2, '='));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n" + sub_title);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(content);
        }

        public static void DrawTitle(string title, string sub_title, bool clear = false)
        {
            if (clear)
                Console.Clear();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("".PadRight(title.Length + 2, '='));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" " + title);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("".PadRight(title.Length + 2, '='));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n" + sub_title);

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void DrawTitle(string title, bool clear = false)
        {
            if (clear)
                Console.Clear();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("".PadRight(title.Length + 2, '='));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" " + title);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("".PadRight(title.Length + 2, '='));

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        /// <summary>
        /// For when you want the user to be able to enter text again.
        /// </summary>
        public static void Reset()
        {
            Console.TreatControlCAsInput = false;
            Console.CursorVisible = true;
        }

    }
}