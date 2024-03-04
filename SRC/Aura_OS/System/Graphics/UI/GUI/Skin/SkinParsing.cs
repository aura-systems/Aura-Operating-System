using Aura_OS.System.Parser;
using Aura_OS.System.Utils;
using Cosmos.System.Graphics;
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
                    string bitmapName = node.GetAttribute("name").Value;
                    _skinName = node.GetAttribute("contentPath").Value;

                    if (Kernel.Installed)
                    {
                        Settings config = new Settings(@"0:\System\settings.ini");
                        Kernel.ThemeManager.BmpPath = config.GetValue("themeBmpPath");

                        if (!File.Exists(Kernel.ThemeManager.BmpPath))
                        {
                            Kernel.ThemeManager.BmpPath = Files.IsoVolume + "UI\\Themes\\" + _skinName + ".bmp";
                        }
                    }
                    else
                    {
                        Kernel.ThemeManager.BmpPath = Files.IsoVolume + "UI\\Themes\\" + _skinName + ".bmp";
                    }

                    CustomConsole.WriteLineInfo("Loading bitmap: " + Kernel.ThemeManager.BmpPath);

                    try
                    {
                        Bitmap bitmap = new Bitmap(File.ReadAllBytes(Kernel.ThemeManager.BmpPath));
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
                    string name = node.GetAttribute("name").Value;

                    if (name.StartsWith("window") || name.StartsWith("button") || name.StartsWith("check") || name.StartsWith("input"))
                    {
                        Frame.Region[] regions = RegionListBuilder.Build(node, _bitmaps);
                        Frame.Text[] texts = null;

                        _frames.Add(name, new Frame(regions, texts));

                        CustomConsole.WriteLineOK(name + " added successfully!");
                    }
                }
            }
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
