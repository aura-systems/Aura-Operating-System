/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Version utils class
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using JZero;
using System.Linq;

namespace Aura_OS.System.Network
{
    public static class Version
    {
        public static (string, string, string) GetLastVersionInfo()
        {
            string json = Http.DownloadFile("http://aura.valentin.bzh/os.json");

            JsonReader rdr = new(json);
            rdr.ReadObjectStart();

            string latestVersion = null;
            string latestRevision = null;
            string latestReleaseUrl = null;

            while (rdr.NextProperty())
            {
                var charSegment = rdr.ReadPropertyName();
                string propertyName = new string(charSegment.Array, charSegment.Offset, charSegment.Count);

                if (propertyName.Equals("last-version"))
                {
                    var latestVersionCharSegment = rdr.ReadString();
                    latestVersion = new string(latestVersionCharSegment.Array, latestVersionCharSegment.Offset, latestVersionCharSegment.Count);
                }
                else if (propertyName.Equals("last-revision"))
                {
                    var latestRevisionCharSegment = rdr.ReadString();
                    latestRevision = new string(latestRevisionCharSegment.Array, latestRevisionCharSegment.Offset, latestRevisionCharSegment.Count);
                }
                else if (propertyName.Equals("last-release-url"))
                {
                    var latestReleaseUrlCharSegment = rdr.ReadString();
                    latestReleaseUrl = new string(latestReleaseUrlCharSegment.Array, latestReleaseUrlCharSegment.Offset, latestReleaseUrlCharSegment.Count);
                }
            }

            rdr.ReadEof();

            return (latestVersion, latestRevision, latestReleaseUrl);
        }

        public static int CompareVersions(string version1, string version2)
        {
            var version1Parts = version1.Split('.').Select(v => int.TryParse(v, out int val) ? val : 0).ToArray();
            var version2Parts = version2.Split('.').Select(v => int.TryParse(v, out int val) ? val : 0).ToArray();

            for (int i = 0; i < Math.Min(version1Parts.Length, version2Parts.Length); i++)
            {
                if (version1Parts[i] > version2Parts[i])
                    return 1;
                if (version1Parts[i] < version2Parts[i])
                    return -1;
            }

            return version1Parts.Length.CompareTo(version2Parts.Length);
        }

        public static int CompareRevisions(string rev1, string rev2)
        {
            bool successRev1 = long.TryParse(rev1, out long revision1);
            bool successRev2 = long.TryParse(rev2, out long revision2);

            if (!successRev1 || !successRev2)
            {
                return 0;
            }

            return revision1.CompareTo(revision2);
        }
    }
}
