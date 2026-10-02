using Aura_OS.System.Filesystem;
using Aura_OS.System.Parser;
using Aura_OS.System.Utils;
using Cosmos.Kernel.System.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aura_OS.System.Graphics.UI.GUI.Skin
{
    public class SkinParsing
    {
        private Dictionary<string, Bitmap> _bitmaps = new Dictionary<string, Bitmap>();
        private Dictionary<string, Frame> _frames = new Dictionary<string, Frame>();
        private string _skinName;

        public void loadSkin(string skinXmlContent)
        {
            NanoXMLDocument xml = new NanoXMLDocument(skinXmlContent);
            NanoXMLNode skin = xml.RootNode;

            CustomConsole.WriteLineOK("xml skin loaded");

            foreach (NanoXMLNode node in skin.SubNodes)
            {
                if (node.Name.Equals("resources"))
                {
                    loadResources(node);
                }
            }

            CustomConsole.WriteLineOK("resources loaded");

            foreach (NanoXMLNode node in skin.SubNodes)
            {
                if (node.Name.Equals("frames"))
                {
                    loadFrames(node);
                }
            }

            CustomConsole.WriteLineOK("frames loaded");
        }

        private void loadResources(NanoXMLNode resourcesNode)
        {
            foreach (NanoXMLNode node in resourcesNode.SubNodes)
            {
                if (node.Name.Equals("bitmap"))
                {
                    string bitmapName = Attr(node, "name");
                    _skinName = Attr(node, "contentPath");

                    if (bitmapName == null)
                    {
                        CustomConsole.WriteLineError("Skipping skin bitmap without a name.");
                        continue;
                    }

                    // Installed: the theme bitmap on disk (FromLegacy converts paths stored by gen2 installs).
                    string bmpPath = null;

                    if (Kernel.Installed)
                    {
                        Settings config = new Settings(AuraPaths.SettingsIni);
                        string themeBmpPath = AuraPath.FromLegacy(config.GetValue("themeBmpPath"));

                        if (!string.IsNullOrEmpty(themeBmpPath) && File.Exists(themeBmpPath))
                        {
                            bmpPath = themeBmpPath;
                        }
                    }

                    try
                    {
                        byte[] bmpData;

                        if (bmpPath != null)
                        {
                            bmpData = File.ReadAllBytes(bmpPath);
                        }
                        else
                        {
                            // GEN3-GAP(iso-files): no ISO volume; the default skin bitmap is an embedded resource.
                            bmpPath = Files.EmbeddedScheme + "UI/Themes/" + _skinName + ".bmp";
                            bmpData = Files.Get("UI/Themes/" + _skinName + ".bmp");
                        }

                        Bitmap bitmap = new Bitmap(bmpData);
                        Kernel.ThemeManager.BmpPath = bmpPath;
                        _bitmaps.Add(bitmapName, bitmap);
                        CustomConsole.WriteLineOK("Bitmap '" + bitmapName + "' added successfully!");
                    }
                    catch (Exception e)
                    {
                        CustomConsole.WriteLineError("Failed to load bitmap '" + bitmapName + "': " + e.Message);
                    }
                }
            }
        }

        private void loadFrames(NanoXMLNode framesNode)
        {
            foreach (NanoXMLNode node in framesNode.SubNodes)
            {
                if (node.Name.Equals("frame"))
                {
                    string name = Attr(node, "name");

                    if (name != null && (name.StartsWith("window") || name.StartsWith("button") ||
                        name.StartsWith("slider") || name.StartsWith("rail") ||
                        name.StartsWith("cursor")  || name.StartsWith("check") || name.StartsWith("input")))
                    {
                        Frame.Region[] regions = RegionListBuilder.Build(node, _bitmaps);
                        Frame.Text[] texts = null;

                        _frames.Add(name, new Frame(regions, texts));

                        CustomConsole.WriteLineOK(name + " added successfully!");
                    }
                }
            }
        }

        /// <summary>Value of an optional XML attribute, or null if the node has no such attribute.</summary>
        private static string Attr(NanoXMLNode node, string name)
        {
            // GEN3-GAP(null-deref): GetAttribute returns null for a missing attribute; dereferencing it
            // is a fatal #PF on gen3.
            NanoXMLAttribute attribute = node.GetAttribute(name);
            return attribute == null ? null : attribute.Value;
        }

        public Frame GetFrame(string name)
        {
            return _frames[name];
        }

        public string GetSkinName()
        {
            return _skinName;
        }
    }
}
