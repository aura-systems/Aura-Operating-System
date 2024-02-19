using Aura_OS.System.Parser;
using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

namespace Aura_OS.System.Graphics.UI.GUI.Skin
{
    public class SkinParsing
    {
        private Dictionary<string, Bitmap> bitmaps = new Dictionary<string, Bitmap>();
        private Dictionary<string, Frame> frames = new Dictionary<string, Frame>();

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
                    string contentPath = "1:\\UI\\Themes\\" + node.GetAttribute("contentPath").Value + ".bmp";

                    CustomConsole.WriteLineInfo("Loading bitmap: " + contentPath);

                    try
                    {
                        Bitmap bitmap = new Bitmap(File.ReadAllBytes(contentPath));
                        bitmaps.Add(bitmapName, bitmap);
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

                    if (name.StartsWith("window"))
                    {
                        Frame.Region[] regions = RegionListBuilder.Build(node, bitmaps);
                        Frame.Text[] texts = null;

                        frames.Add(name, new Frame(regions, texts));

                        CustomConsole.WriteLineOK(name + " added successfully!");
                    }
                }
            }
        }

        public Frame GetFrame(string name)
        {
            return frames[name];
        }
    }
}
