using System;
using System.Collections.Generic;
using System.Text;
using Aura_OS.System.GUI.Graphics;
 namespace Aura_OS.System.GUI
{
    public class CGLF
    {
        public BinaryReader File { get; set; }
         public List<CGLFGlyph> Glyphs { get; set; } = new List<CGLFGlyph>();
         public ushort UnitsPerEm { get; set; }
        public short XMin { get; set; }
        public short YMin { get; set; }
        public short XMax { get; set; }
        public short YMax { get; set; }
         public CGLF(byte[] b)
        {
            File = new BinaryReader(b);
            LoadData();
        }
         public void LoadData()
        {
            //File.Seek(0);
             UnitsPerEm = File.GetUint16();
            XMin = File.GetInt16();
            YMin = File.GetInt16();
            XMax = File.GetInt16();
            YMax = File.GetInt16();
             var c = File.GetInt32();
             for (int i = 0; i < c; i++)
            {
                var g = new CGLFGlyph();
                 g.YMin = File.GetInt16();
                g.YMax = File.GetInt16();
                g.XMax = File.GetInt16();
                g.XMin = File.GetInt16();
                 var triangleCount = File.GetInt32();
                 for (int j = 0; j < triangleCount; j++)
                {
                    var at = new Point(File.GetInt32(), File.GetInt32());
                    var bt = new Point(File.GetInt32(), File.GetInt32());
                    var ct = new Point(File.GetInt32(), File.GetInt32());
                    var trianle = new Triangle(at, bt, ct);
                     g.Triangles.Add(trianle);
                    Glyphs.Add(g);
                }
            }
        }
    }
} 