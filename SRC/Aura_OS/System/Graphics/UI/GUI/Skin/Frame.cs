using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura_OS.System.Graphics.UI.GUI.Skin
{
    /// <summary>Frame that can be drawn by the GUI painter</summary>
    public class Frame
    {

        /// <summary>Modes in which text can be horizontally aligned</summary>
        public enum HorizontalTextAlignment
        {
            /// <summary>The text's base offset is placed at the left of the frame</summary>
            /// <remarks>
            ///   The base offset is normally identical to the text's leftmost pixel.
            ///   However, a glyph may have some eccentrics like an arc that extends to
            ///   the left over the letter's actual starting position.
            /// </remarks>
            Left,
            /// <summary>
            ///   The text's ending offset is placed at the right of the frame
            /// </summary>
            /// <remarks>
            ///   The ending offset is normally identical to the text's rightmost pixel.
            ///   However, a glyph may have some eccentrics like an arc that extends to
            ///   the right over the last letter's actual ending position.
            /// </remarks>
            Right,
            /// <summary>The text is centered horizontally in the frame</summary>
            Center

        }

        /// <summary>Modes in which text can be vertically aligned</summary>
        public enum VerticalTextAlignment
        {
            /// <summary>The text's baseline is placed at the top of the frame</summary>
            Top,
            /// <summary>The text's baseline is placed at the bottom of the frame</summary>
            Bottom,
            /// <summary>The text's baseline is centered vertically in the frame</summary>
            Center
        }

        /// <summary>Defines a picture region drawn into a frame</summary>
        public struct Region
        {
            /// <summary>Identification string for the region</summary>
            /// <remarks>
            ///   Used to associate regions with specific behavior
            /// </remarks>
            public string Id;
            /// <summary>Texture the picture region is taken from</summary>
            public Cosmos.System.Graphics.Bitmap Texture;
            /// <summary>Area within the texture containing the picture region</summary>
            public Rectangle SourceRegion;

            public string HorizontalPlacement;
            public string VerticalPlacement;
        }

        /// <summary>Describes where within the frame text should be drawn</summary>
        public struct Text
        {
            /// <summary>Font to use for drawing the text</summary>
            public Cosmos.System.Graphics.Fonts.Font Font;
            /// <summary>Offset of the text relative to its specified placement</summary>
            public Point Offset;
            /// <summary>Horizontal placement of the text within the frame</summary>
            public HorizontalTextAlignment HorizontalPlacement;
            /// <summary>Vertical placement of the text within the frame</summary>
            public VerticalTextAlignment VerticalPlacement;
            /// <summary>Color the text will have</summary>
            public Color Color;
        }

        /// <summary>Initializes a new frame</summary>
        /// <param name="regions">Regions needed to be drawn to render the frame</param>
        /// <param name="texts">Location in the frame where text can be drawn</param>
        public Frame(Region[] regions, Text[] texts)
        {
            this.Regions = regions;
            this.Texts = texts;
        }

        /// <summary>Regions that need to be drawn to render the frame</summary>
        public Region[] Regions;

        /// <summary>Locations where text can be drawn into the frame</summary>
        public Text[] Texts;

    }
}
