// ============================================================================================================
// PolyPartSharp: library for polygon partition and triangulation based on the PolyPartition C++ library 
// https://github.com/JamesK89/PolyPartSharp
// Original project: https://github.com/ivanfratric/polypartition
// ============================================================================================================
// Original work Copyright (C) 2011 by Ivan Fratric
// Derivative work Copyright (C) 2016 by James John Kelly Jr.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
 using System;
using System.Linq;
using System.Collections.Generic;
 using tppl_float = System.Single;
 namespace PolyPartition
{
     public enum TpplOrder : int
    {
        Unknown = 0,
        Cw = -1,
        Ccw = 1
    }
     public enum TpplVertexType : int
    {
        Regular = 0,
        Start,
        End,
        Split,
        Merge
    }
     public sealed class TpplPoly
    {
        public TpplPoly(int numPoints)
        {
            Clear();
             Points = new List<TpplPoint>();
             for (int i = 0; i < numPoints; i++)
            {
                Points.Add(new TpplPoint());
            }
        }
         public TpplPoly(TpplPoly src)
        {
            Clear();
             Hole = src.Hole;
             if (src.Points != null)
            {
                Points = new List<TpplPoint>(src.Points.Count);
                 for (int i = 0; i < Points.Count; i++)
                {
                    Points[i] = new TpplPoint(src.Points[i]);
                }
            }
        }
         public TpplPoly(TpplPoint a, TpplPoint b, TpplPoint c)
        {
            Triangle(a, b, c);
        }
         public TpplPoly()
        {
            Clear();
            Points = new List<TpplPoint>();
        }
         public List<TpplPoint> Points
        {
            get;
            set;
        }
         public bool Hole
        {
            get;
            set;
        }
         public int Count
        {
            get
            {
                return (Points != null ? Points.Count : 0);
            }
        }
         public TpplPoint this[int i]
        {
            get
            {
                return Points[i];
            }
            set
            {
                Points[i] = value;
            }
        }
         public TpplOrder Orientation
        {
            get
            {
                return GetOrientation();
            }
            set
            {
                SetOrientation(value);
            }
        }
         public void Clear()
        {
            if (Points != null) Points.Clear();
            Hole = false;
        }
         public void Triangle(TpplPoint a, TpplPoint b, TpplPoint c)
        {
            Clear();
            Points = new List<TpplPoint>(3) { a, b, c };
        }
         public TpplOrder GetOrientation()
        {
            TpplOrder ret = TpplOrder.Unknown;
             if (Points != null)
            {
                int i1, i2;
                tppl_float area = 0;
                 for (i1 = 0; i1 < Points.Count; i1++)
                {
                    i2 = i1 + 1;
                     if (i2 == Points.Count) i2 = 0;
                     area += Points[i1].X * Points[i2].Y - Points[i1].Y * Points[i2].X;
                }
                 if (area > 0)
                {
                    return TpplOrder.Ccw;
                }
                else if (area < 0)
                {
                    return TpplOrder.Cw;
                }
            }
             return ret;
        }
         public void SetOrientation(TpplOrder orientation)
        {
            if (GetOrientation() != orientation)
            {
                Invert();
            }
        }
         public void Invert()
        {
            if (Points != null)
            {
                Points.Reverse();
            }
        }
         public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            if (Points != null)
            {
                bool f = true;
                 foreach (TpplPoint p in Points)
                {
                    if (!f)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(p.ToString());
                    f = false;
                }
            }
             return sb.ToString();
        }
    }
}