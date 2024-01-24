/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Terminal redirections
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura_OS.System.Processing.Application.Terminal
{
    public class TerminalTextWriter : TextWriter
    {
        private TerminalApp _terminal;

        public TerminalTextWriter(TerminalApp terminal)
        {
            _terminal = terminal;
        }

        public override void Write(char value)
        {
            _terminal.WriteToConsole(value.ToString());
        }

        public override Encoding Encoding => Encoding.UTF8;
    }

    public class TerminalTextReader : TextReader
    {
        private TerminalApp _terminal;

        public TerminalTextReader(TerminalApp terminal)
        {
            _terminal = terminal;
        }

        public override string ReadLine()
        {
            return _terminal.ReadLineFromConsole();
        }
    }
}
