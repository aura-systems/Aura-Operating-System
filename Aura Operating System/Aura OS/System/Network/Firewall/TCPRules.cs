using System.Collections.Generic;
using System.IO;
using System;

/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Firewall - Core - TCP Rules filter
* PROGRAMMERS:      Alexy Da Cruz <dacruzalexy@gmail.com>
*/

namespace Aura_OS.System.Network.Firewall
{
    class TCPRules
    {
        static string[] rules;
        static string[] reset;
        static List<string> rulesfile = new List<string>();

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

        public static void Delete(string ip, string port, string incoming, string outgoing)
        {
            LoadRules();
            DeleteRule(ip + " " + port + " " + incoming + " " + outgoing);
            PushRules();
        }

        private static void DeleteRule(string rule)
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
                else
                {
                    Translation.Text.Display("fw_rule_doesnt_exists");
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

        private static void AddRule(string ip, ushort port, bool incoming, bool outgoing)
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

        //public static void GetRules(string ip)
        //{
        //    Translation.Text.Display("title_firewall_rules");
        //    foreach (string item in rules)
        //    {
        //        string[] rule = item.Split(',');
        //        string[] net = rule[0].Split(':');


        //        if (rule[0].StartsWith(ip))
        //        {
        //            if (rule[1] == "true")
        //            {
        //                rule[1] = "DENY";
        //            }
        //            else
        //            {
        //                rule[1] = "ALLOW";
        //            }

        //            if (rule[2] == "true")
        //            {
        //                rule[2] = "DENY";
        //            }
        //            else
        //            {
        //                rule[2] = "ALLOW";
        //            }

        //            Console.WriteLine(rule[0] + ", " + rule[1] + ", " + rule[2]);
        //        }

        //    }
        //}

        private static void PushRules()
        {
            File.WriteAllLines(@"0:\System\firewall_tcp.conf", rules);
        }

        private static void LoadRules()
        {
            rules = reset;
            rules = File.ReadAllLines(@"0:\System\firewall_tcp.conf");
        }
    }
}
