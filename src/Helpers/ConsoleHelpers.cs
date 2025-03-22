using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

class ConsoleHelpers
{
    public static void Configure(bool debug, bool verbose, bool quiet)
    {
        Console.OutputEncoding = Encoding.UTF8;

        _debug = debug;
        _verbose = verbose;
        _quiet = quiet;

        WriteDebugLine($"Debug: {_debug}");
        WriteDebugLine($"Verbose: {_verbose}");
        WriteDebugLine($"Quiet: {_quiet}");
    }

    public static bool IsQuiet()
    {
        return _quiet;
    }

    public static bool IsVerbose()
    {
        return _verbose;
    }

    public static bool IsDebug()
    {
        return _debug;
    }

    public static void DisplayStatus(string status)
    {
        if (!_debug && !_verbose) return;
        if (Console.IsOutputRedirected) return;

        lock (_printLock)
        {
            DisplayStatusErase();
            Console.Write("\r" + status);
            _cchLastStatus = status.Length;
            if (_debug) Thread.Sleep(1);
        }
    }

    public static void DisplayStatusErase()
    {
        if (!_debug && !_verbose) return;
        if (_cchLastStatus <= 0) return;

        lock (_printLock)
        {
            var eraseLastStatus = "\r" + new string(' ', _cchLastStatus) + "\r";
            Console.Write(eraseLastStatus);
            _cchLastStatus = 0;
        }
    }

    public static void Write(string message, ConsoleColor? foregroundColor = null, bool overrideQuiet = false)
    {
        Write(message, foregroundColor, null, overrideQuiet);
    }

    public static void Write(string message, ConsoleColor? foregroundColor, ConsoleColor? backgroundColor, bool overrideQuiet = false)
    {
        if (_quiet && !overrideQuiet) return;

        lock (_printLock)
        {
            var prevForegroundColor = Console.ForegroundColor;
            if (foregroundColor != null) Console.ForegroundColor = (ConsoleColor)foregroundColor;

            var prevBackgroundColor = Console.BackgroundColor;
            if (backgroundColor != null) Console.BackgroundColor = (ConsoleColor)backgroundColor;

            Console.Write(message);
            if (foregroundColor != null) Console.ForegroundColor = prevForegroundColor;
            if (backgroundColor != null) Console.BackgroundColor = prevBackgroundColor;
        }
    }

    public static void WriteLine(string message = "", ConsoleColor? color = null, bool overrideQuiet = false)
    {
        WriteLine(message, color, null, overrideQuiet);
    }

    public static void WriteLine(string message, ConsoleColor? foregroundColor, ConsoleColor? backgroundColor, bool overrideQuiet = false)
    {
        if (_quiet && !overrideQuiet) return;

        lock (_printLock)
        {
            DisplayStatusErase();

            var prevForegroundColor = Console.ForegroundColor;
            if (foregroundColor != null) Console.ForegroundColor = (ConsoleColor)foregroundColor;

            var prevBackgroundColor = Console.BackgroundColor;
            if (backgroundColor != null) Console.BackgroundColor = (ConsoleColor)backgroundColor;

            Console.WriteLine(message);
            if (foregroundColor != null) Console.ForegroundColor = prevForegroundColor;
            if (backgroundColor != null) Console.BackgroundColor = prevBackgroundColor;
        }
    }

    public static void WriteWarning(string message)
    {
        Write(message, ConsoleColor.Black, ConsoleColor.Yellow, overrideQuiet: true);
    }

    public static void WriteWarningLine(string message)
    {
        WriteLine(message, ConsoleColor.Black, ConsoleColor.Yellow, overrideQuiet: true);
    }
    
    public static void WriteError(string message)
    {
        Write(message, ConsoleColor.White, ConsoleColor.Red, overrideQuiet: true);
    }

    public static void WriteErrorLine(string message)
    {
        WriteLine(message, ConsoleColor.White, ConsoleColor.Red, overrideQuiet: true);
    }

    public static void WriteDebug(string message)
    {
        if (!_debug) return;
        Write(message, ConsoleColor.Cyan);
    }

    public static void WriteDebugLine(string message)
    {
        if (!_debug) return;
        WriteLine(message, ConsoleColor.Cyan);
    }

    public static IEnumerable<string> GetAllLinesFromStdin()
    {
        return _allLinesFromStdin == null
            ? ReadAllLinesFromStdin()
            : _allLinesFromStdin;
    }

    private static List<string> ReadAllLinesFromStdin()
    {
        _allLinesFromStdin = new();
        while (true)
        {
            var line = Console.ReadLine();
            if (line == null) break;

            _allLinesFromStdin.Add(line);
        }

        return _allLinesFromStdin;
    }
    
    private static bool _debug = false;
    private static bool _verbose = false;
    private static bool _quiet = false;

    private static object _printLock = new();
    private static int _cchLastStatus = 0;

    private static List<string>? _allLinesFromStdin;
}