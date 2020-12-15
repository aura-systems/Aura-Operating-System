/*
* PROJECT:          Aura Operating System Development
* CONTENT:          https://github.com/Haydend/ConsoleDraw
* PROGRAMMERS:      Haydend <haydendunnicliffe@gmail.com>
*/

using ConsoleDraw.Inputs;
using ConsoleDraw.Windows.Base;
using System;

namespace ConsoleDraw.Windows
{
    public class Alert : PopupWindow
    {
        private Button okBtn;
        private static int textLength = 46;


        public Alert(String Message, Window parentWindow)
            : base("Message", 6, (Console.WindowWidth / 2) - 25, 50, 5 + (int)Math.Ceiling(((Double)Message.Length / textLength)), parentWindow)
        {
            Create(Message, parentWindow);
        }

        public Alert(String Message, Window parentWindow, String Title)
            : base(Title, 6, (Console.WindowWidth / 2) - 30, 25, 5 + (int)Math.Ceiling(((Double)Message.Length / textLength)), parentWindow)
        {
            Create(Message, parentWindow);
        }

        public Alert(String Message, Window parentWindow, ConsoleColor backgroundColour)
            : base("Message", 6, (Console.WindowWidth / 2) - 25, 50, 5 + (int)Math.Ceiling(((Double)Message.Length / textLength)), parentWindow)
        {
            BackgroundColour = backgroundColour;

            Create(Message, parentWindow);
        }

        public Alert(String Message, Window parentWindow, ConsoleColor backgroundColour, String Title)
            : base(Title, 6, (Console.WindowWidth / 2) - 25, 50, 5 + (int)Math.Ceiling(((Double)Message.Length / textLength)), parentWindow)
        {
            BackgroundColour = backgroundColour;

            Create(Message, parentWindow);
        }

        private void Create(String Message, Window parentWindow)
        {
            var count = 0;
            while ((count*45) < Message.Length)
            {
                var splitMessage = Message.PadRight(textLength * (count + 1), ' ').Substring((count * textLength), textLength);
                var messageLabel = new Label(splitMessage, PostionX + 2 + count, PostionY + 2, "messageLabel", this);
                Inputs.Add(messageLabel);

                count++;
            }

            /*
            var messageLabel = new Label(Message, PostionX + 2, PostionY + 2, "messageLabel", this);
            messageLabel.BackgroundColour = BackgroundColour;*/

            okBtn = new Button(PostionX + Height - 2, PostionY + 2, "OK", "OkBtn", this);
            okBtn.Action = delegate() { ExitWindow(); };

            Inputs.Add(okBtn);

            CurrentlySelected = okBtn;

            Draw();
            MainLoop();
        }
    }
}
