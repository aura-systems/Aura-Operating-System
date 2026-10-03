/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Layout file reader
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Parser;
using Cosmos.Kernel.System.Graphics;

namespace Aura_OS.System.Graphics.UI.GUI.Layout
{
    /// <summary>
    /// Builds the elements and controls of a layout file (Layout/README.md).
    /// A mistake in the file throws an InvalidDataException naming the file.
    /// </summary>
    internal static class LayoutLoader
    {
        private const int DefaultFieldWidth = 200;
        private const int DefaultControlHeight = 23;

        /// <summary>
        /// The window's elements, in a vertical stack. Dialogs and positioned elements are handed
        /// to the layout apart, out of the flow.
        /// </summary>
        public static StackNode BuildWindow(NanoXMLNode window, AppLayout layout)
        {
            StackNode root = new StackNode();
            root.Padding = ThicknessAttr(window, "padding", layout);
            root.Spacing = IntAttr(window, "spacing", 0, layout);

            foreach (NanoXMLNode element in window.SubNodes)
            {
                if (element.Name == "Dialog")
                {
                    layout.AddDialog(BuildDialog(element, layout));
                    continue;
                }

                LayoutNode node = Build(element, layout);

                if (node.IsPositioned)
                {
                    layout.AddPositioned(node);
                }
                else
                {
                    root.Children.Add(node);
                }
            }

            return root;
        }

        private static LayoutNode Build(NanoXMLNode element, AppLayout layout)
        {
            LayoutNode node;

            switch (element.Name)
            {
                case "Stack":
                    node = BuildStack(element, new StackNode(), layout);
                    break;
                case "Panel":
                    node = BuildPanel(element, layout);
                    break;
                case "Grid":
                    node = BuildGrid(element, layout);
                    break;
                default:
                    node = BuildControl(element, layout);
                    break;
            }

            ReadCommonAttributes(element, node, layout);
            return node;
        }

        private static StackNode BuildStack(NanoXMLNode element, StackNode stack, AppLayout layout)
        {
            stack.Padding = ThicknessAttr(element, "padding", layout);
            stack.Spacing = IntAttr(element, "spacing", 0, layout);

            string orientation = Attr(element, "orientation");

            if (orientation == "horizontal")
            {
                stack.Orientation = Orientation.Horizontal;
            }
            else if (orientation != null && orientation != "vertical")
            {
                throw layout.Error("orientation must be vertical or horizontal, not '" + orientation + "'.");
            }

            foreach (NanoXMLNode child in element.SubNodes)
            {
                stack.Children.Add(BuildInFlow(child, layout));
            }

            return stack;
        }

        /// <summary>
        /// A stack on a panel: the panel is a control of its own, added before the elements on it
        /// so it is drawn under them.
        /// </summary>
        private static PanelNode BuildPanel(NanoXMLNode element, AppLayout layout)
        {
            Panel panel = new Panel(ColorAttr(element, "color", Kernel.Gray, layout), 0, 0, 0, 0);
            panel.Borders = BoolAttr(element, "borders", false, layout);
            layout.AddControl(new ControlNode(panel));

            return (PanelNode)BuildStack(element, new PanelNode(panel), layout);
        }

        private static GridNode BuildGrid(NanoXMLNode element, AppLayout layout)
        {
            GridNode grid = new GridNode();
            grid.Columns = ParseColumns(Attr(element, "columns") ?? "auto", layout);
            grid.Padding = ThicknessAttr(element, "padding", layout);
            grid.RowSpacing = IntAttr(element, "rowSpacing", 0, layout);
            grid.ColumnSpacing = IntAttr(element, "columnSpacing", 0, layout);
            grid.RowHeight = IntAttr(element, "rowHeight", 0, layout);

            foreach (NanoXMLNode rowElement in element.SubNodes)
            {
                if (rowElement.Name != "Row")
                {
                    throw layout.Error("<Grid> holds <Row> elements, not <" + rowElement.Name + ">.");
                }

                RowNode row = new RowNode();
                int columns = 0;

                foreach (NanoXMLNode cell in rowElement.SubNodes)
                {
                    LayoutNode node = BuildInFlow(cell, layout);
                    columns += Math.Max(1, node.ColumnSpan);
                    row.Children.Add(node);
                }

                if (columns > grid.Columns.Count)
                {
                    throw layout.Error("a <Row> has " + columns + " cells, the grid " + grid.Columns.Count + " columns.");
                }

                ReadCommonAttributes(rowElement, row, layout);
                grid.Children.Add(row);
            }

            return grid;
        }

        /// <summary>
        /// An element of a stack or grid: x and y only place the window's own elements.
        /// </summary>
        private static LayoutNode BuildInFlow(NanoXMLNode element, AppLayout layout)
        {
            LayoutNode node = Build(element, layout);

            if (node.IsPositioned)
            {
                throw layout.Error("x and y are for the window's own elements, not for <" + element.Name + "> in a container.");
            }

            return node;
        }

        private static ControlNode BuildControl(NanoXMLNode element, AppLayout layout)
        {
            string text = Attr(element, "text") ?? "";
            Component component;

            // Fields fill their column (or the window) unless the file gives them a width.
            Alignment alignment = Alignment.Start;

            switch (element.Name)
            {
                case "Label":
                    component = new Label(text, ColorAttr(element, "color", Color.Black, layout), 0, 0);
                    break;

                case "Button":
                    component = BuildButton(element, text, layout);
                    break;

                case "Image":
                    component = new Picture(ImageAttr(element, layout), 0, 0);
                    break;

                case "Console":
                    Components.Console console = new Components.Console(0, 0, ControlSize(element, "width", 400, layout), ControlSize(element, "height", 300, layout));
                    console.CursorVisible = BoolAttr(element, "cursor", false, layout);
                    console.ScrollBar = BoolAttr(element, "scrollBar", false, layout);
                    component = console;
                    break;

                case "TextBox":
                    TextBox textBox = new TextBox(0, 0, ControlSize(element, "width", DefaultFieldWidth, layout), ControlSize(element, "height", DefaultControlHeight, layout), text);
                    textBox.Multiline = BoolAttr(element, "multiline", false, layout);
                    textBox.Password = BoolAttr(element, "password", false, layout);
                    textBox.Enter = layout.Handler(Attr(element, "onEnter"));
                    component = textBox;
                    alignment = Alignment.Stretch;
                    break;

                case "Checkbox":
                    Checkbox checkbox = new Checkbox(text, ColorAttr(element, "color", Color.Black, layout), 0, 0, BoolAttr(element, "checked", false, layout));
                    checkbox.Changed = layout.Handler(Attr(element, "onChange"));
                    component = checkbox;
                    break;

                case "Slider":
                    Slider slider = new Slider(0, 0, ControlSize(element, "width", DefaultFieldWidth, layout), ControlSize(element, "height", DefaultControlHeight, layout));
                    slider.Value = IntAttr(element, "value", 0, layout);
                    slider.Changed = layout.Handler(Attr(element, "onChange"));
                    component = slider;
                    alignment = Alignment.Stretch;
                    break;

                case "DropDown":
                    DropDown dropDown = new DropDown(0, 0, ControlSize(element, "width", DefaultFieldWidth, layout), ControlSize(element, "height", DefaultControlHeight, layout));

                    foreach (NanoXMLNode item in element.SubNodes)
                    {
                        if (item.Name != "Item")
                        {
                            throw layout.Error("<DropDown> holds <Item> elements, not <" + item.Name + ">.");
                        }

                        dropDown.AddItem(item.Value ?? "");
                    }

                    dropDown.SelectedIndex = IntAttr(element, "selectedIndex", -1, layout);
                    dropDown.SelectionChanged = layout.Handler(Attr(element, "onChange"));
                    component = dropDown;
                    alignment = Alignment.Stretch;
                    break;

                default:
                    throw layout.Error("unknown element <" + element.Name + ">.");
            }

            ControlNode node = new ControlNode(component);
            node.HorizontalAlignment = alignment;

            layout.AddControl(node);
            return node;
        }

        private static Button BuildButton(NanoXMLNode element, string text, AppLayout layout)
        {
            string iconName = Attr(element, "icon");
            Bitmap icon = iconName != null ? Kernel.ResourceManager.GetIcon(iconName) : null;

            // Wide enough for the icon and the text, 8 pixels on each side.
            int contentWidth = text.Length * Kernel.font.Width;

            if (icon != null)
            {
                contentWidth += (int)icon.Width + (text.Length > 0 ? 4 : 0);
            }

            int width = ControlSize(element, "width", contentWidth + 16, layout);
            int height = ControlSize(element, "height", DefaultControlHeight, layout);

            Button button;

            if (icon == null)
            {
                button = new Button(text, 0, 0, width, height);
            }
            else if (text.Length > 0)
            {
                button = new Button(icon, text, 0, 0, width, height);
            }
            else
            {
                button = new Button(icon, 0, 0, width, height);
            }

            button.Click = layout.Handler(Attr(element, "onClick"));
            return button;
        }

        /// <summary>
        /// The bitmap of an Image: src, a file of the app's package or an embedded image
        /// ("UI/Images/AuraLogo.bmp"), or icon, an icon key ("32-folder.bmp").
        /// </summary>
        private static Bitmap ImageAttr(NanoXMLNode element, AppLayout layout)
        {
            string src = Attr(element, "src");
            string icon = Attr(element, "icon");

            if ((src == null) == (icon == null))
            {
                throw layout.Error("an <Image> has either a src or an icon attribute.");
            }

            if (icon != null)
            {
                return Kernel.ResourceManager.GetIcon(icon);
            }

            try
            {
                return layout.GetImage(src);
            }
            catch (Exception ex)
            {
                throw layout.Error("cannot load the image '" + src + "': " + ex.Message);
            }
        }

        /// <summary>
        /// A dialog over the window, centered unless x or y place it. Its &lt;Button&gt; elements
        /// are its buttons.
        /// </summary>
        private static ControlNode BuildDialog(NanoXMLNode element, AppLayout layout)
        {
            Dialog dialog = new Dialog(Attr(element, "title") ?? "", Attr(element, "message") ?? "", 0, 0);

            string state = Attr(element, "state");

            if (state == "error")
            {
                dialog.SetState(DialogState.Error);
            }
            else if (state != null && state != "information")
            {
                throw layout.Error("a dialog state is information or error, not '" + state + "'.");
            }

            foreach (NanoXMLNode child in element.SubNodes)
            {
                if (child.Name != "Button")
                {
                    throw layout.Error("<Dialog> holds <Button> elements, not <" + child.Name + ">.");
                }

                dialog.AddButton(Attr(child, "text") ?? "", layout.Handler(Attr(child, "onClick")));
            }

            ControlNode node = new ControlNode(dialog);
            ReadCommonAttributes(element, node, layout);
            return node;
        }

        /// <summary>
        /// The attributes every element takes: id, visible, margin, width, height, align, valign,
        /// columnSpan, x and y.
        /// </summary>
        private static void ReadCommonAttributes(NanoXMLNode element, LayoutNode node, AppLayout layout)
        {
            node.Id = Attr(element, "id");

            if (node.Id != null)
            {
                layout.Register(node);
            }

            node.Margin = ThicknessAttr(element, "margin", layout);
            node.Width = SizeAttr(element, "width", layout);
            node.Height = SizeAttr(element, "height", layout);
            node.ColumnSpan = IntAttr(element, "columnSpan", 1, layout);

            string align = Attr(element, "align");

            if (align != null)
            {
                node.HorizontalAlignment = ParseAlignment(align, "left", "right", layout);
            }

            string valign = Attr(element, "valign");

            if (valign != null)
            {
                node.VerticalAlignment = ParseAlignment(valign, "top", "bottom", layout);
            }

            if (Attr(element, "x") != null)
            {
                node.X = IntAttr(element, "x", 0, layout);
            }

            if (Attr(element, "y") != null)
            {
                node.Y = IntAttr(element, "y", 0, layout);
            }

            // Last: hiding a container hides the elements it holds.
            node.Visible = BoolAttr(element, "visible", true, layout);
        }

        #region Attributes

        /// <summary>
        /// The attribute's value, null when missing.
        /// </summary>
        public static string Attr(NanoXMLNode element, string name)
        {
            NanoXMLAttribute attribute = element.GetAttribute(name);
            return attribute != null ? attribute.Value : null;
        }

        public static int IntAttr(NanoXMLNode element, string name, int defaultValue, AppLayout layout)
        {
            string value = Attr(element, name);

            if (value == null)
            {
                return defaultValue;
            }

            int result;

            if (!int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            {
                throw layout.Error(name + " must be a number, not '" + value + "'.");
            }

            return result;
        }

        private static bool BoolAttr(NanoXMLNode element, string name, bool defaultValue, AppLayout layout)
        {
            string value = Attr(element, name);

            switch (value)
            {
                case null:
                    return defaultValue;
                case "true":
                    return true;
                case "false":
                    return false;
                default:
                    throw layout.Error(name + " must be true or false, not '" + value + "'.");
            }
        }

        /// <summary>
        /// The size in pixels a control is created with: the attribute's when it is a number, else defaultValue.
        /// </summary>
        private static int ControlSize(NanoXMLNode element, string name, int defaultValue, AppLayout layout)
        {
            int size = SizeAttr(element, name, layout);
            return size >= 0 ? size : defaultValue;
        }

        /// <summary>
        /// A width or height: pixels, "auto" (or no attribute) for LayoutNode.Auto, "*" for LayoutNode.Star.
        /// </summary>
        private static int SizeAttr(NanoXMLNode element, string name, AppLayout layout)
        {
            string value = Attr(element, name);

            if (value == null || value == "auto")
            {
                return LayoutNode.Auto;
            }

            if (value == "*")
            {
                return LayoutNode.Star;
            }

            int size = IntAttr(element, name, 0, layout);

            if (size < 0)
            {
                throw layout.Error(name + " is a size in pixels, auto or *, not '" + value + "'.");
            }

            return size;
        }

        /// <summary>
        /// "4" (all sides), "4,2" (left and right, top and bottom) or "4,2,4,2" (left, top, right, bottom).
        /// </summary>
        private static Thickness ThicknessAttr(NanoXMLNode element, string name, AppLayout layout)
        {
            string value = Attr(element, name);

            if (value == null)
            {
                return new Thickness();
            }

            string[] parts = value.Split(',');
            int[] numbers = new int[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                if (!int.TryParse(parts[i].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out numbers[i]))
                {
                    throw layout.Error(name + " must be numbers separated by commas, not '" + value + "'.");
                }
            }

            switch (numbers.Length)
            {
                case 1:
                    return new Thickness(numbers[0], numbers[0], numbers[0], numbers[0]);
                case 2:
                    return new Thickness(numbers[0], numbers[1], numbers[0], numbers[1]);
                case 4:
                    return new Thickness(numbers[0], numbers[1], numbers[2], numbers[3]);
                default:
                    throw layout.Error(name + " takes 1, 2 or 4 numbers, not '" + value + "'.");
            }
        }

        /// <summary>
        /// "#RRGGBB", "#AARRGGBB" or a name (black, white, gray, darkgray, lightgray, red, green, blue, transparent).
        /// </summary>
        private static Color ColorAttr(NanoXMLNode element, string name, Color defaultValue, AppLayout layout)
        {
            string value = Attr(element, name);
            Color color;

            if (value == null)
            {
                return defaultValue;
            }

            if (TryParseColor(value, out color))
            {
                return color;
            }

            throw layout.Error(name + " must be #RRGGBB, #AARRGGBB or a color name, not '" + value + "'.");
        }

        /// <summary>
        /// "#RRGGBB", "#AARRGGBB" or a name (black, white, gray, darkgray, lightgray, red, green, blue, transparent).
        /// </summary>
        public static bool TryParseColor(string value, out Color color)
        {
            color = Color.Black;

            if (value == null)
            {
                return false;
            }

            if (value.Length > 0 && value[0] == '#')
            {
                uint argb;

                if ((value.Length == 7 || value.Length == 9) && uint.TryParse(value.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out argb))
                {
                    if (value.Length == 7)
                    {
                        argb |= 0xFF000000;
                    }

                    color = Color.FromArgb(unchecked((int)argb));
                    return true;
                }

                return false;
            }

            switch (value)
            {
                case "black":
                    color = Color.Black;
                    return true;
                case "white":
                    color = Color.White;
                    return true;
                case "gray":
                    color = Color.Gray;
                    return true;
                case "darkgray":
                    color = Color.DarkGray;
                    return true;
                case "lightgray":
                    color = Color.LightGray;
                    return true;
                case "red":
                    color = Color.Red;
                    return true;
                case "green":
                    color = Color.Green;
                    return true;
                case "blue":
                    color = Color.Blue;
                    return true;
                case "transparent":
                    color = Color.Transparent;
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Grid columns: "auto,*,120".
        /// </summary>
        private static List<int> ParseColumns(string value, AppLayout layout)
        {
            List<int> columns = new List<int>();

            foreach (string part in value.Split(','))
            {
                string column = part.Trim();
                int width;

                if (column == "auto")
                {
                    columns.Add(LayoutNode.Auto);
                }
                else if (column == "*")
                {
                    columns.Add(LayoutNode.Star);
                }
                else if (int.TryParse(column, NumberStyles.Integer, CultureInfo.InvariantCulture, out width) && width >= 0)
                {
                    columns.Add(width);
                }
                else
                {
                    throw layout.Error("a grid column is auto, * or a width in pixels, not '" + column + "'.");
                }
            }

            return columns;
        }

        private static Alignment ParseAlignment(string value, string start, string end, AppLayout layout)
        {
            if (value == start)
            {
                return Alignment.Start;
            }

            if (value == end)
            {
                return Alignment.End;
            }

            switch (value)
            {
                case "center":
                    return Alignment.Center;
                case "stretch":
                    return Alignment.Stretch;
                default:
                    throw layout.Error("alignment must be " + start + ", center, " + end + " or stretch, not '" + value + "'.");
            }
        }

        #endregion
    }
}
