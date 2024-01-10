using System;

namespace DoomSharp.Core;

public interface IConsole
{
    void Write(string message);

    public void WriteLine(string message)
    {
        Write(message);
        Write(Environment.NewLine);
    }

    void SetTitle(string title);
    void Shutdown();
}