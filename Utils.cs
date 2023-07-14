namespace SpiritEye
{
    public static class Utils
    {
        private static bool _IsProgressing = false;

        public static void Error(string message)
        {
            lock (_lock)
            {
                ClearProgressBar();
                WithColor(ConsoleColor.Red, () => Console.Error.Write($"[error] {message}"));
            }
        }
        public static void Warn(string message)
        {
            lock (_lock)
            {
                ClearProgressBar();
                WithColor(ConsoleColor.Yellow, () => Console.Error.Write($"[Warn] {message}"));
            }
        }

        public static void Info(string message)
        {
            lock (_lock)
            {
                ClearProgressBar();
                WithColor(ConsoleColor.Green, () => Console.Error.Write($"[INFO] {message}"));
            }
        }

        public static void ClearProgressBar()
        {
            lock (_lock)
            {
                if (_IsProgressing)
                {
                    _IsProgressing = false;
                    for (var i = 0; i < Console.WindowWidth; i++)
                    {
                        Console.Error.Write(" ");
                    }
                    Console.Error.Write("\r");
                }
                else
                {
                    Console.Error.WriteLine();
                }
            }
        }

        // Draw a progress bar in the console
        public static void ProgressBar(float percent, string message = "")
        {
            lock (_lock)
            {
                if (!_IsProgressing)
                {
                    _IsProgressing = true;
                    Console.Error.WriteLine();
                }

                WithColor(ConsoleColor.Cyan, () => Console.Write("["));

                var width = Console.WindowWidth - 12 - message.Length;
                var pos = (int)(width * percent / 100);
                for (var i = 0; i < width; i++)
                {
                    if (i < pos)
                    {
                        Console.Error.Write("=");
                    }
                    else if (i == pos)
                    {
                        Console.Error.Write(">");
                    }
                    else
                    {
                        Console.Error.Write(" ");
                    }
                }

                WithColor(ConsoleColor.Cyan, () => Console.Error.Write("] "));
                WithColor(ConsoleColor.Yellow, () => Console.Error.Write($"{percent:0.00}%"));
                Console.Error.Write($" {message}\r");
            }
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

        private static readonly object _lock = new();

        public static void WithColor(ConsoleColor color, Action action)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            action.Invoke();
            Console.ForegroundColor = oldColor;
        }
    }
}
