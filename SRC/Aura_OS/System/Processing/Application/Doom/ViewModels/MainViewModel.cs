using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using Aura_OS;
using Aura_OS.System.Processing.Application.Emulators.GameBoyEmu.Utils;
using DoomSharp.Core;
using DoomSharp.Core.Graphics;
using DoomSharp.Core.Input;

namespace DoomSharp.Windows.ViewModels;

public class MainViewModel : IGraphics
{
    public static readonly MainViewModel Instance = new();

    private MainViewModel()
    {
        _output = new DirectBitmap(640, 400);
    }

    private string _title = "DooM#";

    private DirectBitmap? _output;

    private readonly int _stride;
    private readonly byte[] _screenBuffer;

    private readonly Queue<InputEvent> _events = new();

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
        }
    }

    public DirectBitmap? Output
    {
        get => _output;
        set
        {
            if (value is null)
            {
                return;
            }

            _output = value;
        }
    }

    public void Initialize()
    {
        
    }

    public void UpdatePalette(byte[] palette)
    {
    }

    public void ScreenReady(byte[] output)
    {
        Array.Copy(output, 0, _screenBuffer, 0, output.Length);
        
        Kernel.canvas.DrawImage(_output.Bitmap, 0, 0);
    }

    public void StartTic()
    {
        while (_events.TryDequeue(out var ev)) // has events
        {
            DoomGame.Instance.PostEvent(ev);
        }
    }

    public void AddEvent(InputEvent ev)
    {
        _events.Enqueue(ev);
    }
}