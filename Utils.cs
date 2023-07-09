namespace SpiritEye
{
    public static class Utils
    {
        public static void Error(string message) => WithColor(ConsoleColor.Red, () => Console.Error.WriteLine($"[error] {message}"));
        public static void Warn(string message) => WithColor(ConsoleColor.Yellow, () => Console.Error.WriteLine($"[Warn] {message}"));
        public static void Info(string message) => WithColor(ConsoleColor.Green, () => Console.Error.WriteLine($"[INFO] {message}"));

        // Draw a progress bar in the console
        public static void ProgressBar(float percent, string message = "")
        {
            WithColor(ConsoleColor.Cyan, () => Console.Write("["));

            var width = Console.WindowWidth - 12 - message.Length;
            var pos = (int)(width * percent / 100);
            for (var i = 0; i < width; i++)
            {
                if (i < pos)
                {
                    Console.Write("=");
                }
                else if (i == pos)
                {
                    Console.Write(">");
                }
                else
                {
                    Console.Write(" ");
                }
            }

            WithColor(ConsoleColor.Cyan, () => Console.Write("] "));
            WithColor(ConsoleColor.Yellow, () => Console.Write($"{percent:0.00}%"));
            Console.Write($" {message}\r");
        }

        public static void WithCursorHidden(Action action)
        {
            ConsoleCancelEventHandler recover = (_, _) => Console.CursorVisible = true;
            Console.CancelKeyPress += recover;
            Console.CursorVisible = false;
            action.Invoke();
            Console.CursorVisible = true;
            Console.CancelKeyPress -= recover;
        }

        public static void WithColor(ConsoleColor color, Action action)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            action.Invoke();
            Console.ForegroundColor = oldColor;
        }
    }
}
