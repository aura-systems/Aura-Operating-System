using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Aura_OS.System.GUI.Graphics;

namespace Aura_OS.System.GUI.Imaging.Formats
{
    public class Png : IImage
    {
        public Image Read(byte[] bytes)
        {
            var reader = new BinaryReader(bytes);

            if (reader.GetUint8() != 137) return null;
            if (reader.GetUint8() != 80) return null;
            if (reader.GetUint8() != 78) return null;
            if (reader.GetUint8() != 71) return null;
            if (reader.GetUint8() != 13) return null;
            if (reader.GetUint8() != 10) return null;
            if (reader.GetUint8() != 26) return null;
            if (reader.GetUint8() != 10) return null;


            Image re = null;

            while (reader.Tell() < bytes.Length)
            {
                var len = reader.GetUint32();
                var name = reader.GetString(4);

                var pos = reader.Tell();

                bool done = false;

                switch (name)
                {
                    case "IHDR":
                        re = new Image((int) reader.GetUint32(), (int) reader.GetUint32());
                        var bitdepth = reader.GetUint8();
                        var colortype = reader.GetUint8();
                        var compression = reader.GetUint8();
                        var filtermethod = reader.GetUint8();
                        var interlace = reader.GetUint8();
                        break;
                    case "PLTE":
                        break;
                    case "IDAT":
                        var buf = new List<byte>();
                        reader.GetUint8();
                        reader.GetUint8();
                        for (int i = 2; i < len; i++)
                        {
                            buf.Add(reader.GetUint8());
                        }

                        var data = ZlibDecoder.Inflate(buf);

                        var d = new BinaryReader(data.ToArray());

                        var totalScanlines = data.Count / (re.Width + 1) / 4;

                        var prevScanline = new List<byte>();

                        for (int y = 0; y < totalScanlines; y++)
                        {
                            var filter = d.GetUint8();

                            var dat = new List<byte>();

                            for (int x = 0; x < re.Width * 4; x++)
                            {
                                dat.Add(d.GetUint8());
                            }
                            var scanline = new List<byte>();

                            if (filter == 1)
                            {
                                scanline.Add(dat[0]);
                                for (var index = 1; index < dat.Count; index++)
                                {
                                    scanline.Add((byte) ((scanline[index - 4 > 0 ? index - 4 : 0] + dat[index - 1]) % 256));
                                    //scanline.Add((byte) (255));
                                }
                            }
                            else if (filter == 2)
                            {
                                for (var index = 0; index < dat.Count; index++)
                                {
                                     scanline.Add((byte) ((prevScanline[index] + dat[index]) % 256));
                                     //scanline.Add((byte) (255));
                                }
                            }
                            else
                            {
                            }

                            var line = new BinaryReader(scanline.ToArray());
                            prevScanline.Clear();
                            prevScanline.AddRange(scanline);

                            for (int x = 0; x < re.Width; x++)
                            {
                                var a = line.GetUint8();

                                var r = line.GetUint8();
                                var g = line.GetUint8();
                                var b = line.GetUint8();


                                // if (b == 55 || r == 55 || g == 55) Debugger.Break();

                                var c = new Color(r, g, b, 255);

                                re.SetPixel(x, y, c);
                            }

                            /* //re.SetPixel(x, y, d.GetUint32());

                                var b = UnSub(d, x, decoded);
                                var g = UnSub(d, x, decoded);
                                var r = UnSub(d, x, decoded);
                                var a = UnSub(d, x, decoded);

                                var c = new Color(r, g, b);

                                if (c.ToHex() == 0) c = new Color(prev);

                                prev = c.ToHex();


                                //if(b == 55 || r == 55 || g == 55) Debugger.Break();

                                re.SetPixel(x, y, c);*/
                        }


                        break;
                    case "IEND":
                        done = true;
                        break;
                }

                if (done) break;

                reader.Seek((int) (pos + len));
                var crc = reader.GetUint32();
            }


            return re;
        }
    }
}