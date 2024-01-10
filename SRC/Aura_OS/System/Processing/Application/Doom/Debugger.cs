using Aura_OS.System.Graphics.UI.GUI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura_OS.System.Processing.Application.Doom
{
    public class Debugger
    {
        public string Text = "";

        public Debugger() { }

        public void Write(string message)
        {
            Text += message;
        }

        public void WriteLine(string message)
        {
            Text += message += "\n";
        }
    }
}
