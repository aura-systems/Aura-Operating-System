using System;
using System.Collections.Generic;
using System.Text;

namespace WMCommandFramework
{
    public class CommandVersion
    {
        private int[] versionArr;
        private string tag;

        public CommandVersion(int major, int minor, int revision, int build, string tag = "")
        {
            if (major <= -1)
                major = 0;
            if (minor <= -1)
                minor = 0;
            if (revision <= -1)
                revision = 0;
            if (build <= -1)
                build = 0;
            int[] x = { major, minor, revision, build };
            versionArr = x;
            this.tag = tag;
        }

        public CommandVersion(int minor, int revision, int build, string tag = "")
        {
            if (minor <= -1)
                minor = 0;
            if (revision <= -1)
                revision = 0;
            if (build <= -1)
                build = 0;
            int[] x = { minor, revision, build };
            versionArr = x;
            this.tag = tag;
        }

        public CommandVersion(int revision, int build, string tag = "")
        {
            if (revision <= -1)
                revision = 0;
            if (build <= -1)
                build = 0;
            int[] x = { revision, build };
            versionArr = x;
            this.tag = tag;
        }

        public int Major
        {
            get
            {
                if (versionArr.Length == 4)
                    return versionArr[0];
                else return 0;
            }
        }

        public int Minor
        {
            get
            {
                if (versionArr.Length == 4)
                    return versionArr[1];
                else if (versionArr.Length == 3)
                    return versionArr[0];
                else return 0;
            }
        }

        public int Revision
        {
            get
            {
                if (versionArr.Length == 4)
                    return versionArr[2];
                else if (versionArr.Length == 3)
                    return versionArr[1];
                else if (versionArr.Length == 2)
                    return versionArr[0];
                else return 0;
            }
        }

        public int Build
        {
            get
            {
                if (versionArr.Length == 4)
                    return versionArr[3];
                else if (versionArr.Length == 3)
                    return versionArr[2];
                else if (versionArr.Length == 2)
                    return versionArr[1];
                else return 0;
            }
        }

        public string Tag
        {
            get
            {
                if (tag == null || tag == "") return "";
                return tag;
            }
        }

        public override string ToString()
        {
            if (tag == null || tag == "")
            {
                //Don't use tag.
                if (versionArr.Length == 4)
                    return $"{Major}.{Minor}.{Revision}.{Build}";
                else if (versionArr.Length == 3)
                    return $"{Minor}.{Revision}.{Build}";
                else if (versionArr.Length == 2)
                    return $"{Revision}.{Build}";
                else return "";
            }
            else
            {
                //Use tag.
                if (versionArr.Length == 4)
                    return $"{Major}.{Minor}.{Revision}.{Build}-{Tag}";
                else if (versionArr.Length == 3)
                    return $"{Minor}.{Revision}.{Build}-{Tag}";
                else if (versionArr.Length == 2)
                    return $"{Revision}.{Build}-{Tag}";
                else return "";
            }
        }

        public static CommandVersion Parse(string data)
        {
            if (data.Contains("-"))
            {
                var dat = data.Split('-');
                var tag = dat[1];
                string x = dat[0];
                var spl = x.Split('.');
                if (spl.Length == 4)
                    return new CommandVersion(int.Parse(spl[0]), int.Parse(spl[1]), int.Parse(spl[2]), int.Parse(spl[3]), tag);
                else if (spl.Length == 3)
                    return new CommandVersion(int.Parse(spl[0]), int.Parse(spl[1]), int.Parse(spl[2]), tag);
                else if (spl.Length == 2)
                    return new CommandVersion(int.Parse(spl[0]), int.Parse(spl[1]), tag);
                else return new CommandVersion(0,0,0,0);
            }
            else
            {
                var spl = data.Split('.');
                if (spl.Length == 4)
                    return new CommandVersion(int.Parse(spl[0]), int.Parse(spl[1]), int.Parse(spl[2]), int.Parse(spl[3]));
                else if (spl.Length == 3)
                    return new CommandVersion(int.Parse(spl[0]), int.Parse(spl[1]), int.Parse(spl[2]));
                else if (spl.Length == 2)
                    return new CommandVersion(int.Parse(spl[0]), int.Parse(spl[1]));
                else return new CommandVersion(0, 0, 0, 0);
            }
        }
    }
}
