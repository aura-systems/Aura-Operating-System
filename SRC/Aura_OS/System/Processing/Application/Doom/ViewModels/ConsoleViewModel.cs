using System.Windows;
using DoomSharp.Core;

namespace DoomSharp.Windows.ViewModels;

public class ConsoleViewModel : IConsole
{
    public static readonly ConsoleViewModel Instance = new();

    private ConsoleViewModel() {}

    private string _consoleOutput = "";
    private string _title = "DooM# - Console output";

    public string ConsoleOutput
    {
        get => _consoleOutput;
        set
        {
            _consoleOutput = value;
        }
    }

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
        }
    }

    public void Write(string message)
    {
        ConsoleOutput += message;
    }

    public void SetTitle(string title)
    {
        Title = $"{title} - Console output";
        MainViewModel.Instance.Title = title;
    }

    public void Shutdown()
    {

    }
}