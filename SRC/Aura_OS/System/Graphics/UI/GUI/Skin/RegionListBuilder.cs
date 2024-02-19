using Aura_OS.System.Graphics.UI.GUI.Skin;
using System.Collections.Generic;
using System;
using Aura_OS.System.Parser;
using Cosmos.System.Graphics;
using Aura_OS.System.Graphics.UI.GUI;
using System.Xml.Linq;
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

    /// <summary>Retrieves the sizes of the border regions in a frame</summary>
    /// <param name="frameElement">
    ///   XML node for the frame containing the region
    /// </param>
    private void retrieveBorderSizes(NanoXMLNode frameElement)
    {
        foreach (NanoXMLNode element in frameElement.SubNodes)
        {
            // Left and right border width determination
            string hplacement = element.GetAttribute("hplacement").Value;
            string w = element.GetAttribute("w").Value;
            if (hplacement == "left")
            {
                this.leftBorderWidth = Math.Max(this.leftBorderWidth, int.Parse(w));
            }
            else if (hplacement == "right")
            {
                this.rightBorderWidth = Math.Max(this.rightBorderWidth, int.Parse(w));
            }

            // Top and bottom border width determination
            string vplacement = element.GetAttribute("vplacement").Value;
            string h = element.GetAttribute("h").Value;
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
            CustomConsole.WriteLineInfo(element.Name);

            if (element.Name == "region")
            {
                string idAttribute = element.GetAttribute("id").Value;
                string id = (idAttribute == null) ? null : idAttribute;
                string source = element.GetAttribute("source").Value;
                string hplacement = element.GetAttribute("hplacement").Value;
                string vplacement = element.GetAttribute("vplacement").Value;
                string x = element.GetAttribute("x").Value;
                string y = element.GetAttribute("y").Value;
                string w = element.GetAttribute("w").Value;
                string h = element.GetAttribute("h").Value;

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

                CustomConsole.WriteLineInfo("Left=" + region.SourceRegion.Left + " Top=" + region.SourceRegion.Top + " Right=" + region.SourceRegion.Right + " Bottom=" + region.SourceRegion.Bottom);
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