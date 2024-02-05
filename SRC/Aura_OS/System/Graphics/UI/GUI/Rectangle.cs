using Aura_OS.System.Processing.Processes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class Rectangle
    {
        public int Top;
        public int Left;
        public int Bottom;
        public int Right;

        public Rectangle(int top, int left, int bottom, int right)
        {
            Top = top;
            Left = left;
            Bottom = bottom;
            Right = right;
        }

        public static List<Rectangle> RectSplit(Rectangle subjectRect, Rectangle cuttingRect)
        {
            List<Rectangle> outputRects = new List<Rectangle>();

            // Clone the subject rect
            Rectangle subjectCopy = new Rectangle(subjectRect.Top, subjectRect.Left, subjectRect.Bottom, subjectRect.Right);

            // Begin splitting
            // 1 - Split by left edge if that edge is between the subject's left and right edges 
            if (cuttingRect.Left >= subjectCopy.Left && cuttingRect.Left <= subjectCopy.Right)
            {
                var newRect = new Rectangle(subjectCopy.Top, subjectCopy.Left, subjectCopy.Bottom, cuttingRect.Left - 1);
                outputRects.Add(newRect);
                subjectCopy.Left = cuttingRect.Left;
            }

            // 2 - Split by top edge
            if (cuttingRect.Top >= subjectCopy.Top && cuttingRect.Top <= subjectCopy.Bottom)
            {
                var newRect = new Rectangle(subjectCopy.Top, subjectCopy.Left, cuttingRect.Top - 1, subjectCopy.Right);
                outputRects.Add(newRect);
                subjectCopy.Top = cuttingRect.Top;
            }

            // 3 - Split by right edge
            if (cuttingRect.Right >= subjectCopy.Left && cuttingRect.Right <= subjectCopy.Right)
            {
                var newRect = new Rectangle(subjectCopy.Top, cuttingRect.Right + 1, subjectCopy.Bottom, subjectCopy.Right);
                outputRects.Add(newRect);
                subjectCopy.Right = cuttingRect.Right;
            }

            // 4 - Split by bottom edge
            if (cuttingRect.Bottom >= subjectCopy.Top && cuttingRect.Bottom <= subjectCopy.Bottom)
            {
                var newRect = new Rectangle(cuttingRect.Bottom + 1, subjectCopy.Left, subjectCopy.Bottom, subjectCopy.Right);
                outputRects.Add(newRect);
                subjectCopy.Bottom = cuttingRect.Bottom;
            }

            return outputRects;
        }

        public static void AddClipRect(Rectangle addedRect)
        {
            int i = 0;
            while (i < Explorer.WindowManager.ClipRects.Count)
            {
                Rectangle curRect = Explorer.WindowManager.ClipRects[i];

                if (!(curRect.Left <= addedRect.Right &&
                      curRect.Right >= addedRect.Left &&
                      curRect.Top <= addedRect.Bottom &&
                      curRect.Bottom >= addedRect.Top))
                {
                    i++;
                    continue;
                }

                Explorer.WindowManager.ClipRects.RemoveAt(i);
                List<Rectangle> splitRects = RectSplit(curRect, addedRect);

                foreach (var splitRect in splitRects)
                {
                    Explorer.WindowManager.ClipRects.Add(splitRect);
                }

                i = 0;
            }

            Explorer.WindowManager.ClipRects.Add(addedRect);
        }

        public bool Intersects(Rectangle other)
        {
            return !(Left > other.Right || Right < other.Left || Top > other.Bottom || Bottom < other.Top);
        }

        public static Rectangle? Intersection(Rectangle rect1, Rectangle rect2)
        {
            if (!rect1.Intersects(rect2)) return null;

            int intersectTop = Math.Max(rect1.Top, rect2.Top);
            int intersectLeft = Math.Max(rect1.Left, rect2.Left);
            int intersectBottom = Math.Min(rect1.Bottom, rect2.Bottom);
            int intersectRight = Math.Min(rect1.Right, rect2.Right);

            return new Rectangle(intersectTop, intersectLeft, intersectBottom, intersectRight);
        }

    }
}
