using Aura_OS.System.Processing.Processes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class Rect
    {
        public int Top;
        public int Left;
        public int Bottom;
        public int Right;

        public Rect(int top, int left, int bottom, int right)
        {
            Top = top;
            Left = left;
            Bottom = bottom;
            Right = right;
        }

        public static List<Rect> RectSplit(Rect subjectRect, Rect cuttingRect)
        {
            List<Rect> outputRects = new List<Rect>();

            // Clone the subject rect
            Rect subjectCopy = new Rect(subjectRect.Top, subjectRect.Left, subjectRect.Bottom, subjectRect.Right);

            // Begin splitting
            // 1 - Split by left edge if that edge is between the subject's left and right edges 
            if (cuttingRect.Left >= subjectCopy.Left && cuttingRect.Left <= subjectCopy.Right)
            {
                var newRect = new Rect(subjectCopy.Top, subjectCopy.Left, subjectCopy.Bottom, cuttingRect.Left - 1);
                outputRects.Add(newRect);
                subjectCopy.Left = cuttingRect.Left;
            }

            // 2 - Split by top edge
            if (cuttingRect.Top >= subjectCopy.Top && cuttingRect.Top <= subjectCopy.Bottom)
            {
                var newRect = new Rect(subjectCopy.Top, subjectCopy.Left, cuttingRect.Top - 1, subjectCopy.Right);
                outputRects.Add(newRect);
                subjectCopy.Top = cuttingRect.Top;
            }

            // 3 - Split by right edge
            if (cuttingRect.Right >= subjectCopy.Left && cuttingRect.Right <= subjectCopy.Right)
            {
                var newRect = new Rect(subjectCopy.Top, cuttingRect.Right + 1, subjectCopy.Bottom, subjectCopy.Right);
                outputRects.Add(newRect);
                subjectCopy.Right = cuttingRect.Right;
            }

            // 4 - Split by bottom edge
            if (cuttingRect.Bottom >= subjectCopy.Top && cuttingRect.Bottom <= subjectCopy.Bottom)
            {
                var newRect = new Rect(cuttingRect.Bottom + 1, subjectCopy.Left, subjectCopy.Bottom, subjectCopy.Right);
                outputRects.Add(newRect);
                subjectCopy.Bottom = cuttingRect.Bottom;
            }

            return outputRects;
        }

        public static void AddClipRect(Rect addedRect)
        {
            int i = 0;
            while (i < Explorer.WindowManager.ClipRects.Count)
            {
                Rect curRect = Explorer.WindowManager.ClipRects[i];

                if (!(curRect.Left <= addedRect.Right &&
                      curRect.Right >= addedRect.Left &&
                      curRect.Top <= addedRect.Bottom &&
                      curRect.Bottom >= addedRect.Top))
                {
                    i++;
                    continue;
                }

                Explorer.WindowManager.ClipRects.RemoveAt(i);
                List<Rect> splitRects = RectSplit(curRect, addedRect);

                foreach (var splitRect in splitRects)
                {
                    Explorer.WindowManager.ClipRects.Add(splitRect);
                }

                i = 0;
            }

            Explorer.WindowManager.ClipRects.Add(addedRect);
        }
    }
}
