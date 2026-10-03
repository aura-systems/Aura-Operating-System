/*
* PROJECT:          Aura Operating System Development
* CONTENT:          A package of the online repository
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Network;

namespace Aura_OS.System.Processing
{
    /// <summary>
    /// An entry of the repository's package list (repository.json): what pkg /list shows, and where
    /// pkg /add downloads the .pkg from.
    /// </summary>
    public class RepositoryPackage
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Link { get; set; }
        public string Version { get; set; }
        public bool Installed { get; set; }

        /// <summary>
        /// Downloads and reads the .pkg file.
        /// </summary>
        public Package Download()
        {
            return new Package(Http.DownloadRawFile(Link));
        }
    }
}
