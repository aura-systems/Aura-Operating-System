//#define COSMOSDEBUG
using Aura_OS.System;
using Aura_OS.System.Graphics.UI.GUI;
using CosmosGL.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Security;
using System.Text;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// Represents a png image.
    /// </summary>
    public class Png : Image
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Png"/> class.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="colorDepth">The color depth.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when either the width or height is lower than 0.</exception>
        public Png(uint width, uint height, ColorDepth colorDepth) : base(width, height, colorDepth)
        {
            if (width < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            RawData = new int[width * height];
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Png"/> class from a byte array
        /// representing the pixels.
        /// </summary>
        /// <param name="width">The width of the png.</param>
        /// <param name="height">The height of the png.</param>
        /// <param name="pixelData">A byte array which includes the values for each pixel.</param>
        /// <param name="colorDepth">The format of the pixel data.</param>
        /// <exception cref="NotImplementedException">Thrown if color depth is not 32.</exception>
        /// <exception cref="OverflowException">Thrown if png size is bigger than Int32.MaxValue.</exception>
        /// <exception cref="ArgumentException">Thrown on fatal error.</exception>
        /// <exception cref="ArgumentNullException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        public Png(uint width, uint height, byte[] pixelData, ColorDepth colorDepth) : base(width, height, colorDepth)
        {
            RawData = new int[width * height];
            if (colorDepth != ColorDepth.ColorDepth32 && colorDepth != ColorDepth.ColorDepth24)
            {
                Global.Debugger.Send("Only color depths 24 and 32 are supported!");
                throw new NotImplementedException("Only color depths 24 and 32 are supported!");
            }

            for (int i = 0; i < RawData.Length; i++)
            {
                if (colorDepth == ColorDepth.ColorDepth32)
                {
                    RawData[i] = BitConverter.ToInt32(new byte[] { pixelData[i * 4], pixelData[i * 4 + 1], pixelData[i * 4 + 2], pixelData[i * 4 + 3] }, 0);
                }
                else
                {
                    RawData[i] = BitConverter.ToInt32(new byte[] { 0, pixelData[i * 3], pixelData[i * 3 + 1], pixelData[i * 3 + 2] }, 0);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Png"/> class, using the specified path to a BMP file.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown if path is invalid.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown if path is null.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="IOException">Thrown on IO error.</exception>
        /// <exception cref="NotSupportedException">
        /// <list type="bullet">
        /// <item>Thrown on fatal error.</item>
        /// <item>The path refers to non-file.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ObjectDisposedException">Thrown if the stream is closed.</exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown if header is not from a BMP.</item>
        /// <item>Info header size has the wrong value.</item>
        /// <item>Number of planes is not 1. Can not read file.</item>
        /// <item>Total Image Size is smaller than pure image size.</item>
        /// </list>
        /// </exception>
        /// <exception cref="NotImplementedException">Thrown if pixelsize is other then 32 / 24 or the file compressed.</exception>
        /// <exception cref="SecurityException">Thrown if the caller does not have permissions to read / write the file.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file cannot be found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the specified path is invalid.</exception>
        /// <exception cref="PathTooLongException">Thrown if the specified path is exceed the system-defined max length.</exception>
        public Png(string path) : this(path, ColorOrder.BGR)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Png"/> class, with a specified path to a BMP file.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="colorOrder">Order of colors in each pixel.</param>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown if path is invalid.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown if path is null.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="IOException">Thrown on IO error.</exception>
        /// <exception cref="NotSupportedException">
        /// <list type="bullet">
        /// <item>Thrown on fatal error.</item>
        /// <item>The path refers to non-file.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ObjectDisposedException">Thrown if the stream is closed.</exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown if header is not from a BMP.</item>
        /// <item>Info header size has the wrong value.</item>
        /// <item>Number of planes is not 1. Can not read file.</item>
        /// <item>Total Image Size is smaller than pure image size.</item>
        /// </list>
        /// </exception>
        /// <exception cref="NotImplementedException">Thrown if pixelsize is other then 32 / 24 or the file compressed.</exception>
        /// <exception cref="SecurityException">Thrown if the caller does not have permissions to read / write the file.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file cannot be found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the specified path is invalid.</exception>
        /// <exception cref="PathTooLongException">Thrown if the specified path is exceed the system-defined max length.</exception>
        public Png(string path, ColorOrder colorOrder = ColorOrder.BGR) : base(0, 0, ColorDepth.ColorDepth32) //Call the image constructor with wrong values
        {
            CreatePng(File.ReadAllBytes(path), colorOrder);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Png"/> class, with the specified image data byte array.
        /// </summary>
        /// <param name="imageData">byte array.</param>
        /// <exception cref="ArgumentNullException">Thrown if imageData is null / memory error.</exception>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="IOException">Thrown on IO error.</exception>
        /// <exception cref="NotSupportedException">Thrown on fatal error.</exception>
        /// <exception cref="ObjectDisposedException">Thrown on fatal error.</exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown if header is not from a BMP.</item>
        /// <item>Info header size has the wrong value.</item>
        /// <item>Number of planes is not 1.</item>
        /// <item>Total Image Size is smaller than pure image size.</item>
        /// </list>
        /// </exception>
        /// <exception cref="NotImplementedException">Thrown if pixelsize is other then 32 / 24 or the file compressed.</exception>
        public Png(byte[] imageData) : this(imageData, ColorOrder.BGR) //Call the image constructor with wrong values
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Png"/> class, with the specified image data byte array.
        /// </summary>
        /// <param name="imageData">byte array.</param>
        /// <param name="colorOrder">Order of colors in each pixel.</param>
        /// <exception cref="ArgumentNullException">Thrown if imageData is null / memory error.</exception>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="IOException">Thrown on IO error.</exception>
        /// <exception cref="NotSupportedException">Thrown on fatal error.</exception>
        /// <exception cref="ObjectDisposedException">Thrown on fatal error.</exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown if header is not from a BMP.</item>
        /// <item>Info header size has the wrong value.</item>
        /// <item>Number of planes is not 1.</item>
        /// <item>Total Image Size is smaller than pure image size.</item>
        /// </list>
        /// </exception>
        /// <exception cref="NotImplementedException">Thrown if pixelsize is other then 32 / 24 or the file compressed.</exception>
        public Png(byte[] imageData, ColorOrder colorOrder = ColorOrder.BGR) : base(0, 0, ColorDepth.ColorDepth32) //Call the image constructor with wrong values
        {
            CreatePng(imageData, colorOrder);
        }


        // For more information about the format: https://docs.microsoft.com/en-us/previous-versions/ms969901(v=msdn.10)?redirectedfrom=MSDN
        /// <summary>
        /// Creates a png from the given I/O stream.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <param name="colorOrder">Order of colors in each pixel.</param>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentNullException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="IOException">Thrown on IO error.</exception>
        /// <exception cref="NotSupportedException">
        /// <list type="bullet">
        /// <item>Thrown on fatal error.</item>
        /// <item>The stream does not support seeking.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ObjectDisposedException">Thrown if the stream is closed.</exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown if header is not from a BMP.</item>
        /// <item>Info header size has the wrong value.</item>
        /// <item>Number of planes is not 1. Can not read file.</item>
        /// <item>Total Image Size is smaller than pure image size.</item>
        /// </list>
        /// </exception>
        /// <exception cref="NotImplementedException">Thrown if pixelsize is other then 32 / 24 or the file compressed.</exception>
        private void CreatePng(byte[] bytes, ColorOrder colorOrder)
        {
            var reader = new CosmosGL.System.BinaryReader(bytes);

            if (reader.GetUint8() != 137) return ;
            if (reader.GetUint8() != 80) return ;
            if (reader.GetUint8() != 78) return ;
            if (reader.GetUint8() != 71) return ;
            if (reader.GetUint8() != 13) return ;
            if (reader.GetUint8() != 10) return ;
            if (reader.GetUint8() != 26) return ;
            if (reader.GetUint8() != 10) return ;


            DirectBitmap re = null;

            while (reader.Tell() < bytes.Length)
            {
                var len = reader.GetUint32();
                var name = reader.GetString(4);

                var pos = reader.Tell();

                bool done = false;

                switch (name)
                {
                    case "IHDR":
                        re = new DirectBitmap((int)reader.GetUint32(), (int)reader.GetUint32());
                        var bitdepth = reader.GetUint8();
                        var colortype = reader.GetUint8();
                        var compression = reader.GetUint8();
                        var filtermethod = reader.GetUint8();
                        var interlace = reader.GetUint8();
                        CustomConsole.WriteLineInfo("IHDR");
                        break;
                    case "PLTE":
                        CustomConsole.WriteLineInfo("PLTE");
                        break;
                    case "IDAT":
                        CustomConsole.WriteLineInfo("IDAT in");
                        var buf = new List<byte>();
                        reader.GetUint8();
                        reader.GetUint8();
                        for (int i = 2; i < len; i++)
                        {
                            buf.Add(reader.GetUint8());
                        }

                        var data = ZlibDecoder.Inflate(buf);

                        var d = new CosmosGL.System.BinaryReader(data.ToArray());

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

                            var line = new CosmosGL.System.BinaryReader(scanline.ToArray());
                            prevScanline.Clear();
                            prevScanline.AddRange(scanline);

                            for (int x = 0; x < re.Width; x++)
                            {
                                var a = line.GetUint8();

                                var r = line.GetUint8();
                                var g = line.GetUint8();
                                var b = line.GetUint8();


                                // if (b == 55 || r == 55 || g == 55) Debugger.Break();

                                var argb = (a << 24) | (r << 16) | (g << 8) | b;

                                re.SetPixel(x, y, argb);
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

                        CustomConsole.WriteLineInfo("IDAT out");
                        break;
                    case "IEND":
                        CustomConsole.WriteLineInfo("IEND");
                        done = true;
                        break;
                }

                if (done) break;

                reader.Seek((int) (pos + len));
                var crc = reader.GetUint32();
            }

            CustomConsole.WriteLineInfo("oe");

            global::System.Console.ReadKey();
        }
    }
}
