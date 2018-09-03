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
using System.Collections.Generic;
 using tppl_float = System.Single;
 namespace PolyPartition
{
    public class TpplPartition
    {
        protected class PartitionVertex
        {
            public bool IsActive;
            public bool IsConvex;
            public bool IsEar;
             public TpplPoint P;
            public tppl_float Angle;
             public PartitionVertex Previous;
            public PartitionVertex Next;
        }
         protected struct MonotoneVertex
        {
            public TpplPoint P;
            public int Previous;
            public int Next;
        }
         protected class VertexSorter : IComparer<MonotoneVertex>
        {
            public List<MonotoneVertex> Vertices;
             public int Compare(MonotoneVertex x, MonotoneVertex y)
            {
                throw new NotImplementedException();
            }
        }
         protected struct Diagonal
        {
            public int Index1;
            public int Index2;
        }
         protected struct DpState
        {
            public bool Visible;
            public tppl_float Weight;
            public int Bestvertex;
        }
         protected struct ScanLineEdge
        {
            public int Index;
             public TpplPoint P1;
            public TpplPoint P2;
             public static bool operator < (ScanLineEdge lhs, ScanLineEdge rhs)
            {
                if (rhs.P1.Y == rhs.P2.Y)
                {
                    if (lhs.P1.Y == lhs.P2.Y)
                    {
                        if (lhs.P1.Y < rhs.P1.Y) return true;
                        else return false;
                    }
                    if (IsConvex(lhs.P1, lhs.P2, rhs.P1)) return true;
                    else return false;
                }
                else if (lhs.P1.Y == lhs.P2.Y)
                {
                    if (IsConvex(rhs.P1, rhs.P2, lhs.P1)) return false;
                    else return true;
                }
                else if (lhs.P1.Y < rhs.P1.Y)
                {
                    if (IsConvex(rhs.P1, rhs.P2, lhs.P1)) return false;
                    else return true;
                }
                else
                {
                    if (IsConvex(lhs.P1, lhs.P2, rhs.P1)) return true;
                    else return false;
                }
            }
            
            public static bool operator > (ScanLineEdge lhs, ScanLineEdge rhs)
            {
                throw new NotImplementedException();
            }
             public static bool IsConvex(TpplPoint p1, TpplPoint p2, TpplPoint p3)
            {
                tppl_float tmp;
                tmp = (p3.Y - p1.Y) * (p2.X - p1.X) - (p3.X - p1.X) * (p2.Y - p1.Y);
                 if (tmp > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
         protected bool IsConvex(TpplPoint p1, TpplPoint p2, TpplPoint p3)
        {
            tppl_float tmp;
            tmp = (p3.Y - p1.Y) * (p2.X - p1.X) - (p3.X - p1.X) * (p2.Y - p1.Y);
             if (tmp > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
         protected bool IsReflex(TpplPoint p1, TpplPoint p2, TpplPoint p3)
        {
            tppl_float tmp;
            tmp = (p3.Y - p1.Y) * (p2.X - p1.X) - (p3.X - p1.X) * (p2.Y - p1.Y);
             if (tmp < 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
         protected bool IsInside(TpplPoint p1, TpplPoint p2, TpplPoint p3, TpplPoint p)
        {
            if (IsConvex(p1, p, p2)) return false;
            if (IsConvex(p2, p, p3)) return false;
            if (IsConvex(p3, p, p1)) return false;
            return true;
        }
         protected bool InCone(TpplPoint p1, TpplPoint p2, TpplPoint p3, TpplPoint p)
        {
            bool convex;
             convex = IsConvex(p1, p2, p3);
             if (convex)
            {
                if (!IsConvex(p1, p2, p)) return false;
                if (!IsConvex(p2, p3, p)) return false;
                return true;
            }
            else
            {
                if (IsConvex(p1, p2, p)) return true;
                if (IsConvex(p2, p3, p)) return true;
                return false;
            }
        }
         protected bool InCone(PartitionVertex v, TpplPoint p)
        {
            TpplPoint p1, p2, p3;
             p1 = v.Previous.P;
            p2 = v.P;
            p3 = v.Next.P;
             return InCone(p1, p2, p3, p);
        }
         protected int Intersects(TpplPoint p11, TpplPoint p12, TpplPoint p21, TpplPoint p22)
        {
            if ((p11.X == p21.X) && (p11.Y == p21.Y)) return 0;
            if ((p11.X == p22.X) && (p11.Y == p22.Y)) return 0;
            if ((p12.X == p21.X) && (p12.Y == p21.Y)) return 0;
            if ((p12.X == p22.X) && (p12.Y == p22.Y)) return 0;
             TpplPoint v1Ort = new TpplPoint(), 
                      v2Ort = new TpplPoint(),
                      v = new TpplPoint();
             tppl_float dot11, dot12, dot21, dot22;
             v1Ort.X = p12.Y - p11.Y;
            v1Ort.Y = p11.X - p12.X;
             v2Ort.X = p22.Y - p21.Y;
            v2Ort.Y = p21.X - p22.X;
             v = p21 - p11;
            dot21 = v.X * v1Ort.X + v.Y * v1Ort.Y;
            v = p22 - p11;
            dot22 = v.X * v1Ort.X + v.Y * v1Ort.Y;
             v = p11 - p21;
            dot11 = v.X * v2Ort.X + v.Y * v2Ort.Y;
            v = p12 - p21;
            dot12 = v.X * v2Ort.X + v.Y * v2Ort.Y;
             if (dot11 * dot12 > 0) return 0;
            if (dot21 * dot22 > 0) return 0;
             return 1;
        }
         TpplPoint Normalize(TpplPoint p)
        {
            TpplPoint r = new TpplPoint();
            tppl_float n = (tppl_float)Math.Sqrt(p.X * p.X + p.Y * p.Y);
            
            if (n != (tppl_float)0)
            {
                r = p / n;
            }
            else
            {
                r.X = 0;
                r.Y = 0;
            }
             return r;
        }
         protected tppl_float Distance(TpplPoint p1, TpplPoint p2)
        {
            tppl_float dx, dy;
             dx = p2.X - p1.X;
            dy = p2.Y - p1.Y;
             return (tppl_float)Math.Sqrt(dx * dx + dy * dy);
        }
         protected void UpdateVertexReflexity(PartitionVertex v)
        {
            PartitionVertex v1 = null, v3 = null;
             v1 = v.Previous;
            v3 = v.Next;
             v.IsConvex = !IsReflex(v1.P, v.P, v3.P);
        }
         protected void UpdateVertex(PartitionVertex v, List<PartitionVertex> vertices)
        {
            int i;
            PartitionVertex v1 = null, v3 = null;
            TpplPoint vec1, vec3;
             v1 = v.Previous;
            v3 = v.Next;
             v.IsConvex = IsConvex(v1.P, v.P, v3.P);
             vec1 = Normalize(v1.P - v.P);
            vec3 = Normalize(v3.P - v.P);
             v.Angle = vec1.X * vec3.X + vec1.Y * vec3.Y;
             if (v.IsConvex)
            {
                v.IsEar = true;
                 int numvertices = vertices.Count;
                for (i = 0; i < numvertices; i++)
                {
                    if ((vertices[i].P.X == v.P.X) && (vertices[i].P.Y == v.P.Y)) continue;
                    if ((vertices[i].P.X == v1.P.X) && (vertices[i].P.Y == v1.P.Y)) continue;
                    if ((vertices[i].P.X == v3.P.X) && (vertices[i].P.Y == v3.P.Y)) continue;
                    if (IsInside(v1.P, v.P, v3.P, vertices[i].P))
                    {
                        v.IsEar = false;
                        break;
                    }
                }
            }
            else
            {
                v.IsEar = false;
            }
        }
         public int Triangulate_EC(TpplPoly poly, List<TpplPoly> triangles)
        {
            int numvertices;
            List<PartitionVertex> vertices = null;
            PartitionVertex ear = null;
             TpplPoly triangle = new TpplPoly();
             int i, j;
            bool earfound;
             if (poly.Count < 3) return 0;
            if (poly.Count == 3)
            {
                triangles.Add(poly);
                return 1;
            }
             numvertices = poly.Count;
             vertices = new List<PartitionVertex>(numvertices);
            
            for (i = 0; i < numvertices; i++)
            {
                vertices.Add(new PartitionVertex());
            }
             for (i = 0; i < numvertices; i++)
            {
                vertices[i].IsActive = true;
                vertices[i].P = poly[i];
                if (i == (numvertices - 1)) vertices[i].Next = vertices[0];
                else vertices[i].Next = vertices[i + 1];
                if (i == 0) vertices[i].Previous = vertices[numvertices - 1];
                else vertices[i].Previous = vertices[i - 1];
            }
             for (i = 0; i < numvertices; i++)
            {
                UpdateVertex(vertices[i], vertices);
            }
             for (i = 0; i < numvertices - 3; i++)
            {
                earfound = false;
                //find the most extruded ear
                for (j = 0; j < numvertices; j++)
                {
                    if (!vertices[j].IsActive) continue;
                    if (!vertices[j].IsEar) continue;
                    if (!earfound)
                    {
                        earfound = true;
                        ear = vertices[j];
                    }
                    else
                    {
                        if (vertices[j].Angle > ear.Angle)
                        {
                            ear = vertices[j];
                        }
                    }
                }
                if (!earfound)
                {
                    vertices.Clear();
                    return 0;
                }
                 triangle = new TpplPoly(ear.Previous.P, ear.P, ear.Next.P);
                triangles.Add(triangle);
                 ear.IsActive = false;
                ear.Previous.Next = ear.Next;
                ear.Next.Previous = ear.Previous;
                 if (i == numvertices - 4) break;
                 UpdateVertex(ear.Previous, vertices);
                UpdateVertex(ear.Next, vertices);
            }
             for (i = 0; i < numvertices; i++)
            {
                if (vertices[i].IsActive)
                {
                    triangle = new TpplPoly(vertices[i].Previous.P, vertices[i].P, vertices[i].Next.P);
                    triangles.Add(triangle);
                    break;
                }
            }
            
            vertices.Clear();
             return 1;
        }
    }
}