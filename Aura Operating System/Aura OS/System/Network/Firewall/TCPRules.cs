using Aura_OS.System.Translation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Aura_OS.System.Network.Firewall
{
    class TCPRules
    {
        public static string[] rules;
        static string[] reset;
        static List<string> rulesfile = new List<string>();

        /// <summary>
        /// Method to create an user.
        /// </summary>
        public static void Create(string ip, string port, string incoming, string outgoing)
        {
            if (!IPV4.Address.IsIPAddress(ip))
            {
                return;
            }
            ushort f_port = ushort.Parse(port);         

            bool f_notbool = false;
            bool f_incoming = bool.TryParse(incoming, out f_notbool);
            bool f_outgoing = bool.TryParse(incoming, out f_notbool);

            LoadRules();
            AddRule(ip, f_port, f_incoming, f_outgoing);
            PushRules();
        }

        public static void Create(string ip, ushort port, bool incoming, bool outgoing)
        {
            LoadRules();
            AddRule(ip, port, incoming, outgoing);
            PushRules();
        }

        public static void DeleteRule(string ip, ushort port, bool incoming, bool outgoing)
        {

            foreach (string line in rules)
            {
                rulesfile.Add(line);
            }

            int counter = -1;
            int index = 0;

            bool exists = false;

            foreach (string element in rulesfile)
            {
                counter = counter + 1;
                if (element.Contains(ip + ":" + port.ToString()))
                {
                    index = counter;
                    exists = true;
                }
            }
            if (exists)
            {
                rulesfile.RemoveAt(index);

                rules = rulesfile.ToArray();

                rulesfile.Clear();

                File.Delete(@"0:\System\firewall_tcp.conf");

                PushRules();
            }
        }


        public static void DeleteRule(string rule)
        {

            foreach (string line in rules)
            {
                rulesfile.Add(line);
            }

            int counter = -1;
            int index = 0;

            bool exists = false;

            foreach (string element in rulesfile)
            {
                counter = counter + 1;
                if (element.Contains(rule))
                {
                    index = counter;
                    exists = true;
                }
            }
            if (exists)
            {
                rulesfile.RemoveAt(index);

                rules = rulesfile.ToArray();

                rulesfile.Clear();

                File.Delete(@"0:\System\firewall_tcp.conf");

                PushRules();
            }
        }

        public static void AddRule(string ip, ushort port, bool incoming, bool outgoing)
        {
            bool contains = false;

            foreach (string line in rules)
            {
                rulesfile.Add(line);
                if (line.StartsWith(ip + ":" + port.ToString() + "," + incoming.ToString() + "," + outgoing.ToString()))
                {
                    contains = true;
                }
            }

            if (!contains)
            {
                rulesfile.Add(ip + ":" + port.ToString() + "," + incoming.ToString() + "," + outgoing.ToString());
            }

            rules = rulesfile.ToArray();

            rulesfile.Clear();
        }

        public static void PushRules()
        {
            File.WriteAllLines(@"0:\System\firewall_tcp.conf", rules);
        }

        public static void LoadRules()
        {
            //reset of users string array in memory if there is "something"
            rules = reset;
            //load
            rules = File.ReadAllLines(@"0:\System\firewall_tcp.conf");
        }
    }
}
