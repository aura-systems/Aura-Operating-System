using Aura_OS.System.Graphics.UI.GUI.Skin;
using System.Collections.Generic;
using System;
using Aura_OS.System.Parser;
using Cosmos.Kernel.System.Graphics;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System;

public class RegionListBuilder
{

    /// <summary>Initializes a new frame region list builder</summary>
    private RegionListBuilder() { }

    /// <summary>
    ///   Builds a region list from the regions specified in the provided frame XML node
    /// </summary>
    /// <param name="frameElement">
    ///   XML node for the frame whose regions wille be processed
    /// </param>
    /// <param name="bitmaps">
    ///   Bitmap lookup table used to associate a region's bitmap id to the real bitmap
    /// </param>
    /// <returns>
    ///   A list of the regions that have been extracted from the frame XML node
    /// </returns>
    public static Frame.Region[] Build(
      NanoXMLNode frameElement, IDictionary<string, Bitmap> bitmaps
    )
    {
        RegionListBuilder builder = new RegionListBuilder();
        builder.retrieveBorderSizes(frameElement);
        return builder.createAndPlaceRegions(frameElement, bitmaps);
    }

    /// <summary>Value of an optional XML attribute</summary>
    /// <param name="node">XML node holding the attribute</param>
    /// <param name="name">Name of the attribute</param>
    /// <returns>The attribute's value, or null if the node has no such attribute</returns>
    private static string Attr(NanoXMLNode node, string name)
    {
        // GEN3-GAP(null-deref): GetAttribute returns null for a missing attribute; dereferencing it
        // is a fatal #PF on gen3 (gen2 silently read low memory).
        NanoXMLAttribute attribute = node.GetAttribute(name);
        return attribute == null ? null : attribute.Value;
    }

    /// <summary>Retrieves the sizes of the border regions in a frame</summary>
    /// <param name="frameElement">
    ///   XML node for the frame containing the region
    /// </param>
    private void retrieveBorderSizes(NanoXMLNode frameElement)
    {
        foreach (NanoXMLNode element in frameElement.SubNodes)
        {
            // Only regions have a size (<text> nodes have no w/h)
            if (element.Name != "region")
            {
                continue;
            }

            // Left and right border width determination
            string hplacement = Attr(element, "hplacement");
            string w = Attr(element, "w");
            if (hplacement != null && w != null)
            {
                if (hplacement == "left")
                {
                    this.leftBorderWidth = Math.Max(this.leftBorderWidth, int.Parse(w));
                }
                else if (hplacement == "right")
                {
                    this.rightBorderWidth = Math.Max(this.rightBorderWidth, int.Parse(w));
                }
            }

            // Top and bottom border width determination
            string vplacement = Attr(element, "vplacement");
            string h = Attr(element, "h");
            if (vplacement != null && h != null)
            {
                if (vplacement == "top")
                {
                    this.topBorderWidth = Math.Max(this.topBorderWidth, int.Parse(h));
                }
                else if (vplacement == "bottom")
                {
                    this.bottomBorderWidth = Math.Max(this.bottomBorderWidth, int.Parse(h));
                }
            }
        }
    }

    /// <summary>
    ///   Creates and places the regions needed to be drawn to render the frame
    /// </summary>
    /// <param name="frameElement">
    ///   XML node for the frame containing the region
    /// </param>
    /// <param name="bitmaps">
    ///   Bitmap lookup table to associate a region's bitmap id to the real bitmap
    /// </param>
    /// <returns>The regions created for the frame</returns>
    private Frame.Region[] createAndPlaceRegions(NanoXMLNode frameElement, IDictionary<string, Bitmap> bitmaps)
    {
        var regions = new List<Frame.Region>();

        foreach (NanoXMLNode element in frameElement.SubNodes)
        {
            // CustomConsole.WriteLineInfo(element.Name);

            if (element.Name == "region")
            {
                // The region id is optional (the cursor frame's region has none)
                string id = Attr(element, "id");
                string source = Attr(element, "source");
                string hplacement = Attr(element, "hplacement");
                string vplacement = Attr(element, "vplacement");
                string x = Attr(element, "x");
                string y = Attr(element, "y");
                string w = Attr(element, "w");
                string h = Attr(element, "h");

                // Assign the trivial attributes
                var region = new Frame.Region()
                {
                    Id = id,
                    Texture = bitmaps[source],
                    HorizontalPlacement = hplacement,
                    VerticalPlacement = vplacement,
                    SourceRegion = new Rectangle()
                };
                region.SourceRegion.Left = int.Parse(x);
                region.SourceRegion.Top = int.Parse(y);
                region.SourceRegion.Right = int.Parse(x) + int.Parse(w);
                region.SourceRegion.Bottom = int.Parse(y) + int.Parse(h);

                regions.Add(region);

                // .WriteLineInfo("Left=" + region.SourceRegion.Left + " Top=" + region.SourceRegion.Top + " Right=" + region.SourceRegion.Right + " Bottom=" + region.SourceRegion.Bottom);
            }
        }

        return regions.ToArray();
    }

    /// <summary>Width of the frame's left border regions</summary>
    private int leftBorderWidth;
    /// <summary>Width of the frame's top border regions</summary>
    private int topBorderWidth;
    /// <summary>Width of the frame's right border regions</summary>
    private int rightBorderWidth;
    /// <summary>Width of the frame's bottom border regions</summary>
    private int bottomBorderWidth;

}