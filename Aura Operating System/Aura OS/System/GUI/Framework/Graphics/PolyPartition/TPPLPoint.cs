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

using tppl_float = System.Single;

namespace PolyPartition
{
    public sealed class TpplPoint : ICloneable
    {
        public TpplPoint(tppl_float x, tppl_float y, int id)
        {
            X = x; Y = y;
        }

        public TpplPoint(tppl_float x, tppl_float y)
            : this(x, y, 0)
        {
        }

        public TpplPoint(TpplPoint src)
        {
            X = src.X;
            Y = src.Y;
            Id = src.Id;
        }

        public TpplPoint()
            : this(0.0f, 0.0f, 0)
        {
        }

        public tppl_float X
        {
            get;
            set;
        }

        public tppl_float Y
        {
            get;
            set;
        }

        public int Id
        {
            get;
            set;
        }

        public static TpplPoint operator + (TpplPoint lhs, TpplPoint rhs)
        {
            return new TpplPoint(lhs.X + rhs.X, lhs.Y + rhs.Y);
        }

        public static TpplPoint operator + (TpplPoint lhs, tppl_float rhs)
        {
            return new TpplPoint(lhs.X + rhs, lhs.Y + rhs);
        }

        public static TpplPoint operator - (TpplPoint lhs, TpplPoint rhs)
        {
            return new TpplPoint(lhs.X - rhs.X, lhs.Y - rhs.Y);
        }

        public static TpplPoint operator - (TpplPoint lhs, tppl_float rhs)
        {
            return new TpplPoint(lhs.X - rhs, lhs.Y - rhs);
        }

        public static TpplPoint operator * (TpplPoint lhs, TpplPoint rhs)
        {
            return new TpplPoint(lhs.X * rhs.X, lhs.Y * rhs.Y);
        }

        public static TpplPoint operator * (TpplPoint lhs, tppl_float rhs)
        {
            return new TpplPoint(lhs.X * rhs, lhs.Y * rhs);
        }

        public static TpplPoint operator / (TpplPoint lhs, TpplPoint rhs)
        {
            return new TpplPoint(lhs.X / rhs.X, lhs.Y / rhs.Y);
        }

        public static TpplPoint operator / (TpplPoint lhs, tppl_float rhs)
        {
            return new TpplPoint(lhs.X / rhs, lhs.Y / rhs);
        }

        public static bool operator == (TpplPoint lhs, TpplPoint rhs)
        {
            return (lhs.X == rhs.X) && (lhs.Y == rhs.Y);
        }

        public static bool operator != (TpplPoint lhs, TpplPoint rhs)
        {
            return !(lhs == rhs);
        }

        public object Clone()
        {
            return new TpplPoint(X, Y, Id);
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}", X, Y, Id);
        }
    }
}
