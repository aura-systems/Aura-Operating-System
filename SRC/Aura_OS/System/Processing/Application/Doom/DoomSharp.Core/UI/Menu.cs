using System;
using System.Collections.Generic;

namespace DoomSharp.Core.UI;

public class Menu
{
    public Menu(int numItems, Menu? previousMenu, IList<MenuItem> items, Action drawRoutine, int x, int y, int lastOn)
    {
        NumItems = numItems;
        PreviousMenu = previousMenu;
        Items = items;
        DrawRoutine = drawRoutine;
        X = x;
        Y = y;
        LastOn = lastOn;
    }

    public int NumItems { get; set; }
    public Menu? PreviousMenu { get; set; }
    public IList<MenuItem> Items { get; set; }
    public Action DrawRoutine { get; set; }
    
    public int X { get; set; }
    public int Y { get; set; }
    public int LastOn { get; set; }
}


public class MenuItem
{
    public MenuItem(MenuItemStatus status, string name, Action<int>? choiceRoutine = null, char? hotKey = null)
    {
        Status = status;
        Name = name;
        ChoiceRoutine = choiceRoutine;
        HotKey = hotKey;
    }

    public MenuItemStatus Status { get; set; }
    public string Name { get; set; }
    public Action<int>? ChoiceRoutine { get; set; }
    public char? HotKey { get; set; }
}


public enum MenuItemStatus
{
    Empty = -1,
    NoCursorHere = 0,
    Ok = 1,
    ArrowsOk = 2
}