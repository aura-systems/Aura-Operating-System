/*
* PROJECT:          Aura Operating System Development
* CONTENT:          App window built from a layout file
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Parser;
using Cosmos.Kernel.System.Graphics;

namespace Aura_OS.System.Graphics.UI.GUI.Layout
{
    /// <summary>
    /// The window and controls of an app, described by its layout file Resources/UI/Layouts/&lt;name&gt;.xml
    /// or a package's (Layout/README.md lists the elements). The app finds its controls by id (Find) and
    /// gives the code of the file's event names (On); Application updates, places and draws the controls.
    /// </summary>
    public class AppLayout
    {
        /// <summary>
        /// Layout name, the file name without .xml.
        /// </summary>
        public readonly string Name;

        public readonly string Title;
        public readonly int Width;
        public readonly int Height;

        /// <summary>
        /// Window icon (ResourceManager.GetIcon key), null for the default one.
        /// </summary>
        public readonly string Icon;

        private readonly NanoXMLNode _window;

        // A package app's images (Package.GetImage), null for a kernel app.
        private readonly Func<string, Bitmap> _images;

        private Application _owner;
        private StackNode _root;

        // In file order: the update and draw order.
        private readonly List<ControlNode> _controls = new List<ControlNode>();

        // Elements with an x or y attribute, out of the flow.
        private readonly List<LayoutNode> _positioned = new List<LayoutNode>();

        // Dialogs: drawn above the other controls, and modal while visible.
        private readonly List<ControlNode> _dialogs = new List<ControlNode>();

        private readonly Dictionary<string, LayoutNode> _ids = new Dictionary<string, LayoutNode>();
        private readonly Dictionary<string, Action> _handlers = new Dictionary<string, Action>();

        private AppLayout(string name, NanoXMLNode window, Func<string, Bitmap> images)
        {
            Name = name;
            _window = window;
            _images = images;

            Title = LayoutLoader.Attr(window, "title") ?? name;
            Width = LayoutLoader.IntAttr(window, "width", 400, this);
            Height = LayoutLoader.IntAttr(window, "height", 300, this);
            Icon = LayoutLoader.Attr(window, "icon");
        }

        /// <summary>
        /// Reads the layout file Resources/UI/Layouts/&lt;name&gt;.xml. Its controls are created by
        /// the Application constructor that takes this layout.
        /// </summary>
        /// <exception cref="InvalidDataException">The file is not a valid layout.</exception>
        public static AppLayout Load(string name)
        {
            return Parse(name, Files.GetText("UI/Layouts/" + name + ".xml"), null);
        }

        /// <summary>
        /// Reads a layout file's text: a package app's (Package.LoadLayout). The src of an Image is
        /// looked up with images first, then in the kernel's embedded images.
        /// </summary>
        /// <param name="name">Layout name, for the error messages (name.xml).</param>
        /// <param name="images">The package's image at a path, null when it has none; null for no package.</param>
        /// <exception cref="InvalidDataException">The file is not a valid layout.</exception>
        public static AppLayout Parse(string name, string xml, Func<string, Bitmap> images)
        {
            NanoXMLNode root;

            try
            {
                root = new NanoXMLDocument(xml).RootNode;
            }
            catch (XMLParsingException ex)
            {
                throw new InvalidDataException(name + ".xml: " + ex.Message);
            }

            if (root == null || root.Name != "Window")
            {
                throw new InvalidDataException(name + ".xml: the root element must be <Window>.");
            }

            return new AppLayout(name, root, images);
        }

        /// <summary>
        /// Creates the controls as children of the owner's window and places them.
        /// </summary>
        internal void Build(Application owner)
        {
            _owner = owner;
            _root = LayoutLoader.BuildWindow(_window, this);

            // Last, so they are above the other controls for the mouse too (higher zIndex).
            foreach (ControlNode dialog in _dialogs)
            {
                _owner.Window.AddChild(dialog.Component);
            }

            Arrange();
        }

        /// <summary>
        /// The control with that id.
        /// </summary>
        /// <exception cref="KeyNotFoundException">No element has that id.</exception>
        /// <exception cref="InvalidCastException">The element is not a T.</exception>
        public T Find<T>(string id) where T : Component
        {
            LayoutNode node = FindNode(id);
            ControlNode control = node as ControlNode;
            T component = control != null ? control.Component as T : null;

            if (component == null)
            {
                throw new InvalidCastException(Name + ".xml: '" + id + "' is not a " + typeof(T).Name + ".");
            }

            return component;
        }

        /// <summary>
        /// Runs handler on the file's event of that name (onClick="name", onChange="name"...).
        /// A later call replaces it.
        /// </summary>
        public void On(string name, Action handler)
        {
            _handlers[name] = handler;
        }

        /// <summary>
        /// Shows or hides the element with that id (a container with all its elements). A hidden
        /// element takes no space: the elements after it move up.
        /// </summary>
        public void SetVisible(string id, bool visible)
        {
            FindNode(id).Visible = visible;

            Arrange();
            _owner.MarkDirty();
        }

        /// <summary>
        /// Places the elements in the window's content area, under the title bar and inside the borders.
        /// </summary>
        public void Arrange()
        {
            Window window = _owner.Window;
            int left = 3;
            int top = window.TopBar.Y + window.TopBar.Height;
            int width = window.Width - 2 * left;
            int height = window.Height - 3 - top;

            _root.Measure();
            _root.Arrange(left, top, width, height);

            foreach (LayoutNode node in _positioned)
            {
                ArrangePositioned(node, left, top, width, height);
            }

            foreach (ControlNode dialog in _dialogs)
            {
                ArrangePositioned(dialog, left, top, width, height);
                dialog.MarkPlaced();
            }

            foreach (ControlNode control in _controls)
            {
                control.MarkPlaced();
            }
        }

        /// <summary>
        /// Updates the visible controls; while a dialog is visible, only the dialog (it is modal).
        /// </summary>
        public void Update()
        {
            // The app's code showed, hid or relabelled a control: redraw the window, which places
            // the elements again.
            if (ChangedSincePlaced())
            {
                _owner.MarkDirty();
            }

            for (int i = _dialogs.Count - 1; i >= 0; i--)
            {
                Component dialog = _dialogs[i].Component;

                if (dialog.Visible)
                {
                    dialog.Update();
                    return;
                }
            }

            for (int i = 0; i < _controls.Count; i++)
            {
                Component control = _controls[i].Component;

                if (control.Visible)
                {
                    control.Update();
                }
            }
        }

        /// <summary>
        /// Draws the visible controls into the window, the dialogs last.
        /// </summary>
        public void Draw()
        {
            for (int i = 0; i < _controls.Count; i++)
            {
                DrawControl(_controls[i].Component);
            }

            for (int i = 0; i < _dialogs.Count; i++)
            {
                DrawControl(_dialogs[i].Component);
            }
        }

        private bool ChangedSincePlaced()
        {
            for (int i = 0; i < _controls.Count; i++)
            {
                if (_controls[i].ChangedSincePlaced())
                {
                    return true;
                }
            }

            for (int i = 0; i < _dialogs.Count; i++)
            {
                if (_dialogs[i].ChangedSincePlaced())
                {
                    return true;
                }
            }

            return false;
        }

        private static void DrawControl(Component control)
        {
            if (control.Visible)
            {
                control.Draw();
                control.DrawInParent();

                // Up to date in the window: the window manager would otherwise redraw it next frame,
                // over the controls drawn after it (the buttons of a panel).
                control.MarkCleaned();
            }
        }

        private static void ArrangePositioned(LayoutNode node, int x, int y, int width, int height)
        {
            if (!node.Visible)
            {
                return;
            }

            node.Measure();

            int left = node.X.HasValue ? x + node.X.Value : x + (width - node.DesiredWidth) / 2;
            int top = node.Y.HasValue ? y + node.Y.Value : y + (height - node.DesiredHeight) / 2;

            node.Arrange(left, top, node.DesiredWidth, node.DesiredHeight);
        }

        private LayoutNode FindNode(string id)
        {
            LayoutNode node;

            if (id == null || !_ids.TryGetValue(id, out node))
            {
                throw new KeyNotFoundException(Name + ".xml: no element has the id '" + id + "'.");
            }

            return node;
        }

        #region Building (LayoutLoader)

        internal void Register(LayoutNode node)
        {
            if (_ids.ContainsKey(node.Id))
            {
                throw Error("two elements have the id '" + node.Id + "'.");
            }

            _ids.Add(node.Id, node);
        }

        internal void AddControl(ControlNode control)
        {
            _controls.Add(control);
            _owner.Window.AddChild(control.Component);
        }

        internal void AddDialog(ControlNode dialog)
        {
            _dialogs.Add(dialog);
        }

        internal void AddPositioned(LayoutNode node)
        {
            _positioned.Add(node);
        }

        /// <summary>
        /// The action of an event attribute: runs the handler given to On under that name, or
        /// null when the attribute is missing.
        /// </summary>
        internal Action Handler(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            return new Action(() => Raise(name));
        }

        /// <summary>
        /// The bitmap of an Image's src: the package's file first, else an embedded image ("UI/Images/AuraLogo.bmp").
        /// </summary>
        /// <exception cref="FileNotFoundException">Neither has that image.</exception>
        internal Bitmap GetImage(string src)
        {
            if (_images != null)
            {
                Bitmap image = _images(src);

                if (image != null)
                {
                    return image;
                }
            }

            return Files.GetImage(src);
        }

        internal InvalidDataException Error(string message)
        {
            return new InvalidDataException(Name + ".xml: " + message);
        }

        private void Raise(string name)
        {
            Action handler;

            // GEN3-GAP(null-deref): invoking a null delegate halts the kernel.
            if (_handlers.TryGetValue(name, out handler) && handler != null)
            {
                handler();
            }
            else
            {
                Logs.DoOSLog("[Error] " + Name + ".xml: no handler for the event '" + name + "'.");
            }
        }

        #endregion
    }
}
