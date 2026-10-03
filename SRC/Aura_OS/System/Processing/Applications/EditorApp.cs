/*
* PROJECT:          Aura Operating System Development
* CONTENT:          File editor application. The window is Resources/UI/Layouts/Editor.xml.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Graphics.UI.GUI.Layout;

namespace Aura_OS.System.Processing.Applications
{
    public class EditorApp : Application
    {
        private TextBox _content;
        private Dialog _dialog;
        private string _filePath;

        public EditorApp(string filePath, int x = 0, int y = 0) : base(AppLayout.Load("Editor"), x, y)
        {
            _content = Find<TextBox>("content");
            _dialog = Find<Dialog>("dialog");
            _filePath = filePath;

            On("save", SaveFile);
            On("closeDialog", CloseDialog);

            if (!string.IsNullOrEmpty(filePath))
            {
                SetTitle("Editor - " + filePath);
            }

            // An empty path (new document from the start menu) or a file that does not exist yet opens empty.
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                _content.Text = File.ReadAllText(filePath);
            }
        }

        private void SaveFile()
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                ShowDialog(DialogState.Error, "This document has no file path, nothing was saved.");
                return;
            }

            try
            {
                File.WriteAllText(_filePath, _content.Text);
            }
            catch (Exception ex)
            {
                ShowDialog(DialogState.Error, "Save failed: " + ex.Message);
                return;
            }

            ShowDialog(DialogState.Information, "Your file has been saved!");
        }

        private void ShowDialog(DialogState state, string message)
        {
            _dialog.SetState(state);
            _dialog.Message = message;
            _dialog.Visible = true;
            MarkDirty();
        }

        private void CloseDialog()
        {
            _dialog.Visible = false;
            MarkDirty();
        }
    }
}
