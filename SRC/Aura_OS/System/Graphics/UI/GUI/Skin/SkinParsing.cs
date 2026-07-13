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
                    string bitmapName = node.GetAttribute("name").Value;
                    _skinName = node.GetAttribute("contentPath").Value;

                    // GEN3-GAP(iso9660): gen2 read the theme sheet from the boot ISO
                    // (Files.IsoVolume); gen3 cannot read the ISO, so the sheet ships as an
                    // embedded resource (Resources/UI/Themes/<skin>.bmp) unless a themeBmpPath
                    // from settings.ini points to a file on the mounted volume.
                    string bmpPath = null;

                    if (Kernel.Installed)
                    {
                        Settings config = new Settings(Kernel.RootVolume + "/System/settings.ini");
                        bmpPath = config.GetValue("themeBmpPath");

                        if (bmpPath == null || !File.Exists(bmpPath))
                        {
                            bmpPath = null;
                        }
                    }

                    try
                    {
                        byte[] bmpBytes;

                        if (bmpPath == null)
                        {
                            bmpBytes = Files.GetUiResource("Themes/" + _skinName + ".bmp");
                            bmpPath = "Embedded:Themes/" + _skinName + ".bmp";
                        }
                        else
                        {
                            bmpBytes = File.ReadAllBytes(bmpPath);
                        }

                        Bitmap bitmap = new Bitmap(bmpBytes);
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
                    string name = node.GetAttribute("name").Value;

                    if (name.StartsWith("window") || name.StartsWith("button") ||
                        name.StartsWith("slider") || name.StartsWith("rail") ||
                        name.StartsWith("cursor")  || name.StartsWith("check") || name.StartsWith("input"))
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
