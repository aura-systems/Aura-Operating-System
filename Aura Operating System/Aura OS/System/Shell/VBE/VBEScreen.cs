/*
* PROJECT:          Aura Operating System Development
* CONTENT:          VBE Screen
* PROGRAMMERS:      Myvar
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Shell.VBE
{
    public class VbeScreen
    {
        /// <summary>
        /// Driver for Setting vbe modes and ploting/getting pixels
        /// </summary>
        private VbeDriver _vbe = new VbeDriver();

        /// <summary>
        /// The current Width of the screen in pixels
        /// </summary>
        public int ScreenWidth { get; set; }
        /// <summary>
        /// The current Height of the screen in pixels
        /// </summary>
        public int ScreenHeight { get; set; }
        /// <summary>
        /// The current Bytes per pixel
        /// </summary>
        public int ScreenBpp { get; set; }

        #region Display

        /// <summary>
        /// All the avalible screen resolutions
        /// </summary>
        public enum ScreenSize
        {
            Size320X200,
            Size640X400,
            Size640X480,
            Size800X600,
            Size1024X768,
            Size1280X1024

        }
        /// <summary>
        /// All the suported Bytes per pixel
        /// </summary>
        public enum Bpp
        {
            Bpp15,
            Bpp16,
            Bpp24,
            Bpp32
        }

        /// <summary>
        /// Use this to setup the screen, this will disable the console.
        /// </summary>
        /// <param name="aSize">The dezired screen resolution</param>
        /// <param name="aBpp">The dezired Bytes per pixel</param>
        public void SetMode(ScreenSize aSize, Bpp aBpp)
        {
            //Get screen size
            switch (aSize)
            {
                case ScreenSize.Size320X200:
                    ScreenWidth = 320;
                    ScreenHeight = 200;
                    break;
                case ScreenSize.Size640X400:
                    ScreenWidth = 640;
                    ScreenHeight = 400;
                    break;
                case ScreenSize.Size640X480:
                    ScreenWidth = 640;
                    ScreenHeight = 480;
                    break;
                case ScreenSize.Size800X600:
                    ScreenWidth = 800;
                    ScreenHeight = 600;
                    break;
                case ScreenSize.Size1024X768:
                    ScreenWidth = 1024;
                    ScreenHeight = 768;
                    break;
                case ScreenSize.Size1280X1024:
                    ScreenWidth = 1280;
                    ScreenHeight = 1024;
                    break;
            }
            //Get bpp
            switch (aBpp)
            {
                case Bpp.Bpp15:
                    ScreenBpp = 15;
                    break;
                case Bpp.Bpp16:
                    ScreenBpp = 16;
                    break;
                case Bpp.Bpp24:
                    ScreenBpp = 24;
                    break;
                case Bpp.Bpp32:
                    ScreenBpp = 32;
                    break;
            }
            //set the screen
            _vbe.vbe_set((ushort)ScreenWidth, (ushort)ScreenHeight, (ushort)ScreenBpp);
        }

        #endregion

        public void Disable()
        {
            _vbe.vbe_disable();
        }



    }
}
