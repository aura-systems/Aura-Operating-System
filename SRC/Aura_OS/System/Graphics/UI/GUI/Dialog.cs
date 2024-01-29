using System;
using System.Collections.Generic;
using Aura_OS.System.Graphics.UI.GUI.Components; // Assurez-vous d'avoir une référence aux composants de l'UI

namespace Aura_OS.System.Graphics.UI.GUI
{
    internal class Dialog : Window
    {
        private string _title;
        private string _message;
        private List<Button> _buttons;

        public Dialog(string title, string message) : base(title, 40, 40, 300, 150)
        {
            _title = title;
            _message = message;
            _buttons = new List<Button>();
        }

        public void AddButton(string buttonText, Action onClickAction)
        {
            Button button = new Button(buttonText, 0, 0, 100, 30);
            button.Action = onClickAction;
            _buttons.Add(button);
        }

        public override void Draw()
        {
            base.Draw();

            foreach (var button in _buttons)
            {
                button.Draw();
            }
        }
    }
}
