/*
* PROJECT:          Aura Operating System Development
* CONTENT:          https://github.com/Haydend/ConsoleDraw
* PROGRAMMERS:      Haydend <haydendunnicliffe@gmail.com>
*/

using ConsoleDraw.Windows.Base;
using System;
using System.Collections.Generic;

namespace ConsoleDraw.Inputs
{
    public class MenuDropdown : FullWindow
    {
        private List<MenuItem> MenuItems;
        
        public MenuDropdown(int Xpostion, int Ypostion, List<MenuItem> menuItems, Window parentWindow)
            : base(Xpostion, Ypostion, 20, menuItems.Count + 2, parentWindow)
        {

            for (var i = 0; i < menuItems.Count; i++)
            {
                menuItems[i].ParentWindow = this;
                menuItems[i].Width = this.Width - 2;
                menuItems[i].Xpostion = Xpostion + i + 1;
                menuItems[i].Ypostion = this.PostionY + 1;
            }

            MenuItems = menuItems;


            Inputs.AddRange(MenuItems);
            
            //CurrentlySelected = MenuItems.FirstOrDefault();
            if (MenuItems.Count != 0)
            {
                CurrentlySelected = MenuItems[0];
            }

            BackgroundColour = ConsoleColor.DarkGray;
            Draw();
            MainLoop();
        }

        
    }
}
