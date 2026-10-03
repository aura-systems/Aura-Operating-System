/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Grid layout
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;

namespace Aura_OS.System.Graphics.UI.GUI.Layout
{
    /// <summary>
    /// A row of a grid: its elements fill the grid's columns from the left, ColumnSpan columns each.
    /// The grid places them; hiding the row hides the whole line.
    /// </summary>
    public class RowNode : ContainerNode
    {
        public override void Measure()
        {
            // GridNode measures the cells, column by column.
        }

        public override void Arrange(int x, int y, int width, int height)
        {
            // GridNode places the cells, column by column.
        }
    }

    /// <summary>
    /// Places the elements of its rows in aligned columns, like a form: labels in the first column,
    /// fields in the second. A column is a width in pixels, Auto (its widest element) or Star (the
    /// width the other columns leave, shared between several). A row is as tall as its tallest
    /// element, and at least RowHeight.
    /// </summary>
    public class GridNode : ContainerNode
    {
        public List<int> Columns = new List<int>();
        public int RowSpacing;
        public int ColumnSpacing;
        public int RowHeight;

        private int[] _columnWidths = new int[0];
        private int[] _rowHeights = new int[0];

        public override void Measure()
        {
            int columnCount = Columns.Count;

            _columnWidths = new int[columnCount];
            _rowHeights = new int[Children.Count];

            for (int column = 0; column < columnCount; column++)
            {
                _columnWidths[column] = Math.Max(0, Columns[column]);
            }

            int height = 0;
            int rows = 0;

            for (int row = 0; row < Children.Count; row++)
            {
                RowNode rowNode = (RowNode)Children[row];

                if (!rowNode.Visible)
                {
                    continue;
                }

                int rowHeight = RowHeight;
                int column = 0;

                foreach (LayoutNode cell in rowNode.Children)
                {
                    int span = Span(cell, column);

                    if (cell.Visible)
                    {
                        cell.Measure();

                        // A spanning cell does not widen its columns.
                        if (span == 1 && Columns[column] < 0)
                        {
                            _columnWidths[column] = Math.Max(_columnWidths[column], cell.DesiredWidth);
                        }

                        rowHeight = Math.Max(rowHeight, cell.DesiredHeight);
                    }

                    column += span;
                }

                _rowHeights[row] = rowHeight;
                height += rowHeight;
                rows++;
            }

            if (rows > 1)
            {
                height += RowSpacing * (rows - 1);
            }

            int width = 0;

            foreach (int columnWidth in _columnWidths)
            {
                width += columnWidth;
            }

            if (columnCount > 1)
            {
                width += ColumnSpacing * (columnCount - 1);
            }

            DesiredWidth = (Width >= 0 ? Width : width + Padding.Horizontal) + Margin.Horizontal;
            DesiredHeight = (Height >= 0 ? Height : height + Padding.Vertical) + Margin.Vertical;
        }

        public override void Arrange(int x, int y, int width, int height)
        {
            x += Margin.Left + Padding.Left;
            y += Margin.Top + Padding.Top;
            width = Math.Max(0, width - Margin.Horizontal - Padding.Horizontal);

            int columnCount = Columns.Count;
            int[] widths = new int[columnCount];
            int used = ColumnSpacing * Math.Max(0, columnCount - 1);
            int stars = 0;

            for (int column = 0; column < columnCount; column++)
            {
                if (Columns[column] == Star)
                {
                    stars++;
                }
                else
                {
                    widths[column] = _columnWidths[column];
                    used += widths[column];
                }
            }

            // The "*" columns share what the others leave; the last one takes the division's rest.
            int free = Math.Max(0, width - used);
            int star = 0;

            for (int column = 0; column < columnCount; column++)
            {
                if (Columns[column] == Star)
                {
                    star++;
                    widths[column] = star == stars ? free - (free / stars) * (stars - 1) : free / stars;
                }
            }

            int top = y;

            for (int row = 0; row < Children.Count; row++)
            {
                RowNode rowNode = (RowNode)Children[row];

                if (!rowNode.Visible)
                {
                    continue;
                }

                int left = x;
                int column = 0;

                foreach (LayoutNode cell in rowNode.Children)
                {
                    int span = Span(cell, column);
                    int cellWidth = ColumnSpacing * (span - 1);

                    for (int i = column; i < column + span; i++)
                    {
                        cellWidth += widths[i];
                    }

                    if (cell.Visible)
                    {
                        cell.Arrange(left, top, cellWidth, _rowHeights[row]);
                    }

                    left += cellWidth + ColumnSpacing;
                    column += span;
                }

                top += _rowHeights[row] + RowSpacing;
            }
        }

        /// <summary>
        /// Columns the cell takes from that column, cut at the last column (the loader rejects
        /// rows wider than the grid).
        /// </summary>
        private int Span(LayoutNode cell, int column)
        {
            return Math.Max(1, Math.Min(cell.ColumnSpan, Columns.Count - column));
        }
    }
}
