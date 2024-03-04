/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Slider class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System;
using UniLua;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    public class Slider : Component
    {
        public int Value
        {
            get { return _value; }
            set
            {
                _value = value; 
                int newPositionX = (_value * (Width - _slide.Width)) / 255;
                if (newPositionX < 0)
                {
                    newPositionX = 0;
                }
                else if (newPositionX > Width - _slide.Width)
                {
                    newPositionX = Width - _slide.Width;
                }

                _slide.X = newPositionX;
                _slide.MarkDirty();
                MarkDirty();
            }
        }

        private Button _slide;
        private bool _sliderPressed = false;
        private int _firstX;
        private int _value = 0;

        public Slider(int x, int y, int width, int height) : base(x, y, width, height)
        {
            SetNormalFrame(Kernel.ThemeManager.GetFrame("rail.horizontal"));
            _slide = new Button(10, 3, Height, Height - 6);
            _slide.SetNormalFrame(Kernel.ThemeManager.GetFrame("slider.horizontal.normal"));
            _slide.SetHighlightedFrame(Kernel.ThemeManager.GetFrame("slider.horizontal.highlighted"));

            AddChild(_slide);
        }

        public override void Update()
        {
            int clickX = (int)MouseManager.X;
            int clickY = (int)MouseManager.Y;

            if (Kernel.MouseManager.IsLeftButtonDown)
            {
                if (_slide.IsInside(clickX, clickY) && !_sliderPressed)
                {
                    _firstX = clickX - _slide.X;
                    _sliderPressed = true;
                }
            }
            else
            {
                _sliderPressed = false;
            }

            if (_sliderPressed)
            {
                int currentX = clickX - _firstX;

                if (currentX < 0)
                {
                    currentX = 0;
                }
                else if (currentX > Width - _slide.Width)
                {
                    currentX = Width - _slide.Width;
                }

                _slide.X = currentX;
                _slide.MarkDirty();
                MarkDirty();

                Value = (currentX * 255) / (Width - _slide.Width);
            }

            _slide.Update();

            if (_slide.IsDirty())
            {
                MarkDirty();
            }
        }

        public override void Draw()
        {
            base.Draw();

            _slide.Draw(this);
        }
    }
}