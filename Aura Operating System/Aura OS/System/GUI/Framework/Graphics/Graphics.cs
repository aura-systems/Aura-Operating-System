using System;
using System.Collections.Generic;
using Aura_OS.System.GUI.Framework.Graphics;
using Aura_OS.System.GUI.Graphics.Rasterizing;
using Aura_OS.System.GUI.Imaging;
using Aura_OS.System.GUI.UI.Fonts;
using Cosmos.Core.Memory.Old;
using PolyPartition;
 namespace Aura_OS.System.GUI.Graphics
{
    public class Graphics
    {
        private ICanvas _canvas;
        private Rectangle Container { get; set; }
        private Rectangle ClipRectangle { get; set; }
        private bool ContainerFlag { get; set; }
        private bool ClipingFlag { get; set; }
        private bool ClipingIsInclude { get; set; }
        private PointF Scale { get; set; } = new PointF(1, 1);
        private Point Transform { get; set; } = new Point(0, 0);
         public Point Offset { get; set; } = new Point(0, 0);
         public int Height { get; set; }
        public int Width { get; set; }
         public uint GetArea(int x0, int y0, int w, int h)
        {
            return _canvas.Blit(x0, y0, w, h);
        }
         public void DrawArea(uint area, int posx, int posy)
        {
            Heap.MemAlloc((uint)(Width * Height * 4));
        }
         public void DrawImage(Framework.Graphics.Image img, int x, int y, Color TransparencyKey = null)
        {
            int z = 0;
            for (int p = y; p < y + img.Height; p++)
            {
                for (int i = x; i < x + img.Width; i++)
                {
                    if (TransparencyKey != null)
                    {
                        if (img.Map[z] != TransparencyKey.ToHex())
                        {
                            SetPixel(i, p, new Color((int)img.Map[z]));
                        }
                    }
                    else
                    {
                        SetPixel(i, p, new Color((int)img.Map[z]));
                    }
                     z++;
                }
            }
        }
         public void DrawString(string c, int x, int y, Color color, Font f)
        {
            int totalwidth = 0;
            for (int i = 0; i < c.Length; i++)
            {
                 var ch = c[i];
                if (ch == ' ')
                {
                    totalwidth += f.Width[0];
                }
                else if (ch == '\t')
                {
                    totalwidth += (f.Width[0] * 4);
                }
                else
                {
                    totalwidth += DrawChar(ch, x + totalwidth, y, color, f);
                }
             }
        }
         public int DrawChar(char c, int x, int y, Color color, Font f)
        {
            var index = 0;
            for (int i = 0; i < f.Char.Count; i++)
            {
                if (c == f.Char[i])
                {
                    index = i;
                    break;
                }
            }
             var width = f.Width[index];
             int z = 0;
            for (int p = y; p < y + f.Height[index]; p++)
            {
                for (int i = x; i < x + width; i++)
                {
                    if (f.Data[index][z] == 1)
                    {
                        SetPixel(i, p, color);
                    }
                     z++;
                }
            }
             return width;
        }
         public Graphics(ICanvas canvas)
        {
            _canvas = canvas;
            Height = canvas.Height;
            Width = canvas.Height;
             Container = new Rectangle(0, 0, canvas.Width, canvas.Height);
            ContainerFlag = false;
        }
         public void GetFillRectangle(int x, int y, int sizex, int sizey)
        {
            
            x = (int)((float)x * Scale.X);
            y = (int)((float)y * Scale.Y);
             x += Transform.X;
            y += Transform.Y;
             for (int height = y; height < y + sizey; height++)
            {
                //_canvas.SetScanLine(Offset.X + x + ((Offset.Y + height) * _canvas.Width), sizex, (uint)c.ToHex());
                /*  for (int width = x; width < w; width++)
                    {
                            SetPixel(width, height, c);
                    }*/
            }
            
        }
         #region Internals
         private uint Blend(uint color1, uint color2, byte alpha)
        {
            uint rb = (color1 & 0xFF00FF) + (alpha * (color2 & 0xFF00FF)) >> 8;
            uint g = (color1 & 0x00FF00) + (alpha * (color2 & 0x00FF00)) >> 8;
            return (rb & 0xFF00FF) + (g & 0x00FF00);
        }
         private byte BlendChanel(byte r, float amount)
        {
            return (byte) ((r * amount) + r * (1 - amount));
        }
         private Color Blend(Color color, Color backColor, float amount)
        {
            byte r = (byte) ((color.R * amount) + backColor.R * (1 - amount));
            byte g = (byte) ((color.G * amount) + backColor.G * (1 - amount));
            byte b = (byte) ((color.B * amount) + backColor.B * (1 - amount));
            return new Color(r, g, b);
        }
         private void SetPixel(int x, int y, Color c)
        {
            x = (int) ((float) x * Scale.X);
            y = (int) ((float) y * Scale.Y);
             x += Transform.X;
            y += Transform.Y;
             if (c.A != 255)
            {
                //transparency needed
                var cn = new Color((int) Blend((uint) _canvas.GetPixel(x, y).ToHex(), (uint) c.ToHex(), (byte) c.A));
                 if (ContainerFlag)
                {
                    if (Container.Intersects(x, y))
                    {
                        _canvas.SetPixel(Offset.X + x, Offset.Y + y, cn);
                    }
                }
                else
                {
                    if (ClipingFlag)
                    {
                        if (ClipingIsInclude)
                        {
                            if (ClipRectangle.Intersects(x, y))
                            {
                                _canvas.SetPixel(Offset.X + x, Offset.Y + y, cn);
                            }
                        }
                        else
                        {
                            if (!ClipRectangle.Intersects(x, y))
                            {
                                _canvas.SetPixel(Offset.X + x, Offset.Y + y, cn);
                            }
                        }
                    }
                    else
                    {
                        _canvas.SetPixel(Offset.X + x, Offset.Y + y, cn);
                    }
                }
            }
            else
            {
                if (ContainerFlag)
                {
                    if (Container.Intersects(x, y))
                    {
                        _canvas.SetPixel(Offset.X + x, Offset.Y + y, c);
                    }
                }
                else
                {
                    if (ClipingFlag)
                    {
                        if (ClipingIsInclude)
                        {
                            if (ClipRectangle.Intersects(x, y))
                            {
                                _canvas.SetPixel(Offset.X + x, Offset.Y + y, c);
                            }
                        }
                        else
                        {
                            if (!ClipRectangle.Intersects(x, y))
                            {
                                _canvas.SetPixel(Offset.X + x, Offset.Y + y, c);
                            }
                        }
                    }
                    else
                    {
                        _canvas.SetPixel(Offset.X + x, Offset.Y + y, c);
                    }
                }
            }
        }
         #endregion
         #region Containers
         public void SetContainer(int x, int y, int w, int h)
        {
            ContainerFlag = true;
            Container = new Rectangle(x, y, w, h);
        }
         #endregion
         #region Misc
         public void Clear(Color c)
        {
            _canvas.Clear((uint) c.ToHex());
        }
         public void Flush()
        {
            _canvas.WriteToScreen();
        }
         #endregion
         #region Draw
         public void DrawImage(int x, int y, Imaging.Image img)
        {
            for (int x1 = 0; x1 < img.Width; x1++)
            {
                for (int y1 = 0; y1 < img.Height; y1++)
                {
                    SetPixel(Offset.X + x + x1, Offset.Y + y + y1, img.GetPixel(x1, y1));
                }
            }
        }
         #endregion
         #region Strings
         private float Clamp(float valueToClamp, float min, float max)
        {
            if (valueToClamp < min)
            {
                return min;
            }
             if (valueToClamp > max)
            {
                return max;
            }
             return valueToClamp;
        }
         private float Smoothstep(float edge0, float edge1, float x)
        {
            var t = Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
            return t * t * (3.0f - 2.0f * t);
        }
         private byte Cftb(float f) => (byte) Math.Floor(f >= 1.0f ? 255f : f * 256.0f);
         public void DrawString(int x, int y, string str, float size, SdfFont font, Color txtcolor)
        {
            float width = 0.5f;
            float edge = 0.2f;
             float borderWidth = 0.0f;
            float borderEdge = 0.05f;
             // ScaleTransform(sz, sz);
             var atlas = font.AtlasImage;
            float sz = 1.0f / ((float) font.FontSize / (float) size);
             // if (size != font.FontSize)
            {
                atlas = atlas.ResizeImage((int) ((float) atlas.Width * sz), (int) ((float) atlas.Height * sz));
            }
             foreach (var c in str)
            {
                var chr = font.GetChar(c).Clone();
                 chr.Width = (int) ((float) chr.Width * sz);
                chr.Height = (int) ((float) chr.Height * sz);
                chr.X = (int) ((float) chr.X * sz);
                chr.Y = (int) ((float) chr.Y * sz);
                chr.Xadvance = (int) ((float) chr.Xadvance * sz);
                chr.Xoffset = (int) ((float) chr.Xoffset * sz);
                chr.Yoffset = (int) ((float) chr.Yoffset * sz);
                 for (int x1 = chr.X; x1 < chr.X + chr.Width; x1++)
                {
                    for (int y1 = chr.Y; y1 < chr.Y + chr.Height; y1++)
                    {
                        float distance = 1.0f - (atlas.GetPixel(x1, y1).R / 255f);
                        float alpha = 1.0f - Smoothstep(width, width + edge, distance);
                         float distance2 = 1.0f - (atlas.GetPixel(x1, y1).R / 255f);
                        float outlinealpha = 1.0f - Smoothstep(borderWidth, borderWidth + borderEdge, distance2);
                         float overallAlpha = alpha + (1.0f - alpha) * outlinealpha;
                         var color = txtcolor;
                         if (overallAlpha != 1.0f)
                        {
                            var gpx = _canvas.GetPixel(x + x1 - chr.X + chr.Xoffset, y + y1 - chr.Y + chr.Yoffset);
                            color = Blend(txtcolor, gpx, overallAlpha);
                        }
                         color.A = 255;
                         SetPixel(Offset.X + x + x1 - chr.X + chr.Xoffset, Offset.Y + y + y1 - chr.Y + chr.Yoffset,
                            color);
                    }
                }
                 x += chr.Xadvance;
            }
        }
         public Size MeasureString(string str, float size, SdfFont font)
        {
            float sz = 1.0f / ((float) font.FontSize / (float) size);
             var p = new Size(0, 0);
            foreach (var c in str)
            {
                var chr = font.GetChar(c).Clone();
                 chr.Width = (int) ((float) chr.Width * sz);
                chr.Height = (int) ((float) chr.Height * sz);
                chr.X = (int) ((float) chr.X * sz);
                chr.Y = (int) ((float) chr.Y * sz);
                chr.Xadvance = (int) ((float) chr.Xadvance * sz);
                chr.Xoffset = (int) ((float) chr.Xoffset * sz);
                chr.Yoffset = (int) ((float) chr.Yoffset * sz);
                 if (chr.Height > p.Height) p.Height = chr.Height;
                p.Width += chr.Xadvance;
            }
             return p;
        }
         #endregion
         #region DrawShapes
         public void DrawEllipse(int xc, int yc, int width, int height, Color c)
        {
            int a2 = width * width;
            int b2 = height * height;
            int fa2 = 4 * a2, fb2 = 4 * b2;
            int x, y, sigma;
             /* first half */
            for (x = 0, y = height, sigma = 2 * b2 + a2 * (1 - 2 * height); b2 * x <= a2 * y; x++)
            {
                SetPixel(Offset.X + xc + x, Offset.Y + yc + y, c);
                SetPixel(Offset.X + xc - x, Offset.Y + yc + y, c);
                SetPixel(Offset.X + xc + x, Offset.Y + yc - y, c);
                SetPixel(Offset.X + xc - x, Offset.Y + yc - y, c);
                if (sigma >= 0)
                {
                    sigma += fa2 * (1 - y);
                    y--;
                }
                sigma += b2 * ((4 * x) + 6);
            }
             /* second half */
            for (x = width, y = 0, sigma = 2 * a2 + b2 * (1 - 2 * width); a2 * y <= b2 * x; y++)
            {
                SetPixel(Offset.X + xc + x, Offset.Y + yc + y, c);
                SetPixel(Offset.X + xc - x, Offset.Y + yc + y, c);
                SetPixel(Offset.X + xc + x, Offset.Y + yc - y, c);
                SetPixel(Offset.X + xc - x, Offset.Y + yc - y, c);
                if (sigma >= 0)
                {
                    sigma += fb2 * (1 - x);
                    x--;
                }
                sigma += a2 * ((4 * y) + 6);
            }
        }
         public void DrawLine(int x, int y, int x2, int y2, Color c)
        {
            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1;
            else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1;
            else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1;
            else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1;
                else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                SetPixel(Offset.X + x, Offset.Y + y, c);
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
        }
         public void DrawPath(Point[] points, Color c)
        {
            for (int i = 0; i < points.Length - 1; i++)
            {
                var a = points[i];
                var b = points[i + 1];
                 DrawLine(a.X, a.Y, b.X, b.Y, c);
            }
        }
         public void DrawPolygon(Point[] points, Color c)
        {
            for (int i = 0; i < points.Length - 1; i++)
            {
                var a = points[i];
                var b = points[i + 1];
                 DrawLine(a.X, a.Y, b.X, b.Y, c);
            }
             var f = points[0];
            var t = points[points.Length - 1];
             DrawLine(f.X, f.Y, t.X, t.Y, c);
        }
         public void DrawRectangle(int x, int y, int w, int h, Color c)
        {
            DrawLine(x, y, x + w, y, c);
            DrawLine(x, y + h, x + w, y + h, c);
             DrawLine(x, y, x, y + h, c);
            DrawLine(x + w, y, x + w, y + h, c);
        }
         #endregion
         #region Fill
         public void FillEllipse(int xc, int yc, int width, int height, Color c)
        {
            int a2 = width * width;
            int b2 = height * height;
            int fa2 = 4 * a2, fb2 = 4 * b2;
            int x, y, sigma;
             /* first half */
            for (x = 0, y = height, sigma = 2 * b2 + a2 * (1 - 2 * height); b2 * x <= a2 * y; x++)
            {
                DrawLine(xc + x, yc + y, xc - x, yc + y, c);
                DrawLine(xc + x, yc - y, xc - x, yc - y, c);
                 if (sigma >= 0)
                {
                    sigma += fa2 * (1 - y);
                    y--;
                }
                sigma += b2 * ((4 * x) + 6);
            }
             /* second half */
            for (x = width, y = 0, sigma = 2 * a2 + b2 * (1 - 2 * width); a2 * y <= b2 * x; y++)
            {
                DrawLine(xc + x, yc + y, xc - x, yc + y, c);
                DrawLine(xc + x, yc - y, xc - x, yc - y, c);
                if (sigma >= 0)
                {
                    sigma += fb2 * (1 - x);
                    x--;
                }
                sigma += a2 * ((4 * y) + 6);
            }
        }
         private void ScanConvertTriangle(List<MinMaxPair> scanBuffer, Point minYVert, Point midYVert,
            Point maxYVert, int handedness)
        {
            ScanConvertLine(scanBuffer, minYVert, maxYVert, 0 + handedness);
            ScanConvertLine(scanBuffer, minYVert, midYVert, 1 - handedness);
            ScanConvertLine(scanBuffer, midYVert, maxYVert, 1 - handedness);
        }
         private void ScanConvertLine(List<MinMaxPair> scanBuffer, Point minYVert, Point maxYVert, int whichSide)
        {
            int yStart = (int) minYVert.Y;
            int yEnd = (int) maxYVert.Y;
            int xStart = (int) minYVert.X;
            int xEnd = (int) maxYVert.X;
             int yDist = yEnd - yStart;
            int xDist = xEnd - xStart;
             if (yDist <= 0)
            {
                return;
            }
             float xStep = (float) xDist / (float) yDist;
            float curX = (float) xStart;
             for (int j = yStart; j < yEnd; j++)
            {
                if (j >= 0 && j < Height)
                {
                    if (whichSide == 0)
                    {
                        scanBuffer[j].Min = (int) curX;
                    }
                    else
                    {
                        scanBuffer[j].Max = (int) curX;
                    }
                }
                 curX += xStep;
            }
        }
         public void FillTriangle(int x, int y, Point v0, Point v1, Point v2, Color c)
        {
            x = (int) ((float) x * Scale.X);
            y = (int) ((float) y * Scale.Y);
             x += Transform.X;
            y += Transform.Y;
             //
            v0.X = (int) ((float) v0.X * Scale.X);
            v0.Y = (int) ((float) v0.Y * Scale.Y);
             v0.X += Transform.X;
            v0.Y += Transform.Y;
             //
            v1.X = (int) ((float) v1.X * Scale.X);
            v1.Y = (int) ((float) v1.Y * Scale.Y);
             v1.X += Transform.X;
            v1.Y += Transform.Y;
             //
            v2.X = (int) ((float) v2.X * Scale.X);
            v2.Y = (int) ((float) v2.Y * Scale.Y);
             v2.X += Transform.X;
            v2.Y += Transform.Y;
             var scanBuffer = new List<MinMaxPair>();
             for (int i = 0; i < _canvas.Height; i++)
            {
                scanBuffer.Add(new MinMaxPair(0, 0));
            }
             Point minYVert = v0;
            Point midYVert = v1;
            Point maxYVert = v2;
             if (maxYVert.Y < midYVert.Y)
            {
                Point temp = maxYVert;
                maxYVert = midYVert;
                midYVert = temp;
            }
             if (midYVert.Y < minYVert.Y)
            {
                Point temp = midYVert;
                midYVert = minYVert;
                minYVert = temp;
            }
             if (maxYVert.Y < midYVert.Y)
            {
                Point temp = maxYVert;
                maxYVert = midYVert;
                midYVert = temp;
            }
             float area = minYVert.TriangleArea(maxYVert, midYVert);
            int handedness = area >= 0 ? 1 : 0;
             ScanConvertTriangle(scanBuffer, minYVert, midYVert, maxYVert, handedness);
             for (var j = 0; j < scanBuffer.Count; j++)
            {
                var minMaxPair = scanBuffer[j];
                /* for (int i = minMaxPair.Min; i < minMaxPair.Max; i++)
                 {
                     SetPixel(x + i, y + j, c);
                 }*/
                _canvas.SetScanLine(((j + y + Offset.Y ) * _canvas.Width) + (minMaxPair.Min + x + Offset.X), minMaxPair.Max - minMaxPair.Min,
                    (uint) c.ToHex());
            }
        }
         public void FillPath(Point[] points, Color c)
        {
            var polly = new TpplPoly(points.Length);
             for (int i = 0; i < points.Length; i++)
            {
                polly[i].X = points[i].X;
                polly[i].Y = points[i].Y;
            }
             var outlist = new List<TpplPoly>();
             var p = new TpplPartition();
            p.Triangulate_EC(polly, outlist);
             foreach (var tpplPoly in outlist)
            {
                var a = tpplPoly.Points[0];
                var b = tpplPoly.Points[1];
                var v = tpplPoly.Points[2];
                 FillTriangle(0, 0, new Point((int) a.X, (int) a.Y), new Point((int) b.X, (int) b.Y),
                    new Point((int) v.X, (int) v.Y), c);
            }
        }
         public void FillPolygon(Point[] points, Color c)
        {
            var polly = new TpplPoly(points.Length + 1);
             for (int i = 0; i < points.Length; i++)
            {
                polly[i].X = points[i].X;
                polly[i].Y = points[i].Y;
            }
             polly[polly.Count - 1].X = points[0].X;
            polly[polly.Count - 1].Y = points[0].Y;
             var outlist = new List<TpplPoly>();
             var p = new TpplPartition();
            p.Triangulate_EC(polly, outlist);
             //for debugging
            var colors = new Color[]
            {
                Colors.Red,
                Colors.Blue
            };
             for (var index = 0; index < outlist.Count; index++)
            {
                var tpplPoly = outlist[index];
                var a = tpplPoly.Points[0];
                var b = tpplPoly.Points[1];
                var v = tpplPoly.Points[2];
                 FillTriangle(0, 0, new Point((int) a.X, (int) a.Y), new Point((int) b.X, (int) b.Y),
                    new Point((int) v.X, (int) v.Y),
                    c);
            }
        }
         public void FillRectangle(int x, int y, int w, int h, Color c)
        {
            x = (int) ((float) x * Scale.X);
            y = (int) ((float) y * Scale.Y);
             x += Transform.X;
            y += Transform.Y;
             for (int height = y; height < y + h; height++)
            {
                _canvas.SetScanLine(Offset.X + x + ((Offset.Y + height) * _canvas.Width), w, (uint) c.ToHex());
                /*  for (int width = x; width < w; width++)
                 {
                          SetPixel(width, height, c);
                 }*/
            }
        }
         #endregion
         #region Clipping
         public void ResetClip()
        {
            ClipingFlag = false;
        }
         public void ExcludeClip(int x, int y, int w, int h)
        {
            ClipingFlag = true;
            ClipingIsInclude = false;
            ClipRectangle = new Rectangle(x, y, w, h);
        }
         public void IncludeClip(int x, int y, int w, int h)
        {
            ClipingFlag = true;
            ClipingIsInclude = true;
            ClipRectangle = new Rectangle(x, y, w, h);
        }
         #endregion
         #region Translation
         public void ResetTransform()
        {
            Transform = new Point(0, 0);
            Scale = new PointF(1, 1);
        }
         public void ScaleTransform(float xSize, float ySize)
        {
            Scale = new PointF(xSize, ySize);
        }
         public void TranslateTransform(int x, int y)
        {
            Transform = new Point(x, y);
        }
         #endregion
    }
} 