/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Slider class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Cosmos.Kernel.System.Input;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    /// <summary>
    /// Horizontal slider, Value from 0 to 255.
    /// </summary>
    public class Slider : Component
    {
        /// <summary>
        /// Called when dragging the slide changes Value (setting Value does not call it).
        /// </summary>
        public Action Changed;

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
                // Taken by a press on the slide, not by the button held from elsewhere crossing it.
                if (!_sliderPressed && Kernel.MouseManager.IsLeftPressOn(_slide))
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

                // GEN3-GAP(null-deref): an integer /0 halts the kernel, so guard the rail length.
                int range = Width - _slide.Width;
                if (range > 0)
                {
                    int value = (currentX * 255) / range;

                    if (value != _value)
                    {
                        Value = value;

                        // GEN3-GAP(null-deref): invoking a null delegate halts the kernel.
                        if (Changed != null)
                        {
                            Changed();
                        }
                    }
                }
            }

            _slide.Update();

            if (_slide.IsDirty())
            {
                MarkDirty();
            }
        }

        /// <summary>
        /// Puts the slide back at Value on the new rail.
        /// </summary>
        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Value = _value;
        }

        public override void Draw()
        {
            base.Draw();

            _slide.Draw(this);
        }
    }
}