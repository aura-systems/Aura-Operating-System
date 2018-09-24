/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - CommandManager
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using Aura_OS.System.CosmosSFS;
using Aura_OS.System.Utils;
using Cosmos.HAL.BlockDevice;
using SimpleFileSystem;
using System;
using System.Collections.Generic;
using System.Text;
using L = Aura_OS.System.Translation;


namespace Aura_OS.System.Shell.cmdIntr
{

    public class CommandManager
    {
        //TO-DO: Do for all commands:
        //       Windows like command, Linux like command, Aura original command (optional for the last one)
        //Example: else if ((cmd.Equals("ipconfig")) || (cmd.Equals("ifconfig")) || (cmd.Equals("netconf"))) {

        /// <summary>
        /// Empty constructor. (Good for debug)
        /// </summary>
        public CommandManager() { }

        public static List<String> CMDs = new List<string>();

        private static void Register(string cmd)
        {
            CMDs.Add(cmd);
        }

        public static void RegisterAllCommands()
        {
            Register("shutdown");
            Register("reboot");
            Register("sha256");
            Register("clear");
            Register("echo");
            Register("help");
            Register("cd");
            Register("cp");
            Register("dir");
            Register("ls");
            Register("mkdir");
            Register("rmdir");
            Register("rmfil");
            Register("mkfil");
            Register("edit");
            Register("vol");
            Register("run");
            Register("logout");
            Register("passwd");
            Register("settings");
            Register("systeminfo");
            Register("version");
            Register("ipconfig");
            Register("ifconfig");
            Register("netconf");
            Register("time");
            Register("date");
            Register("beep");
            Register("snake");
            Register("md5");
            Register("export");
            Register("lspci");
            Register("about");
            Register("debug");
        }

        static SimpleFS Fs;

        /// <summary>
        /// Shell Interpreter
        /// </summary>
        /// <param name="cmd">Command</param>
        public static void _CommandManger(string cmd)
        {

            if (Kernel.debugger != null)
            {
                if (Kernel.debugger.enabled)
                {
                    Kernel.debugger.Send("Cmd manager: " + cmd);
                }
            }

            #region Power

            if (cmd.Equals("shutdown") || cmd.Equals("sd"))
            {//NOTE: Why isn't it just the constructor? This leaves more room for <package>.<class>.HelpInfo;
                Power.Shutdown.c_Shutdown();
            }
            else if (cmd.Equals("reboot") || cmd.Equals("rb"))
            {
                Power.Reboot.c_Reboot();
            }

            #endregion Power

            #region Console

            else if ((cmd.Equals("clear")) || (cmd.Equals("cls")))
            {
                c_Console.Clear.c_Clear();
            }
            else if (cmd.StartsWith("echo "))
            {
                c_Console.Echo.c_Echo(cmd);
            }
            else if (cmd.Equals("help"))
            {
                System.Translation.List_Translation._Help();
            }

            #endregion Console

            #region FileSystem

            else if (cmd.Equals("sfs"))
            {
                var blockDevice = BlockDevice.Devices[0];
                var p = new Partition(blockDevice, 0, blockDevice.BlockCount);
                Fs = new SimpleFS(new CosmosBlockDevice(p));

                Console.WriteLine("Sfs initialized.");
            }

            else if (cmd.Equals("sfsload"))
            {
                Fs.Load();
            }

            else if (cmd.Equals("sfsformat"))
            {
                Fs.Format();
            }

            else if (cmd.Equals("sfsls"))
            {
                foreach (var directory in Fs?.GetAllDirectories())
                {
                    Console.WriteLine(directory);
                }

                foreach (var fl in Fs?.GetAllFiles())
                {
                    Console.WriteLine(fl + " " + Fs.ReadAllText(fl));
                }
            }

            else if (cmd.Equals("sfsmkdir"))
            {
                Fs?.CreateDirectory("test");
            }

            else if (cmd.Equals("sfsrmdir"))
            {
                Fs.DeleteDirectory("test");
            }

            else if (cmd.Equals("sfsmkfil"))
            {
                Fs?.WriteAllText("bob.txt", "Content of bob.txt");
            }

            else if (cmd.Equals("sfsrdfil"))
            {
                Console.WriteLine("bob.txt:");
                Console.WriteLine(Fs.ReadAllText("bob.txt"));
            }

            else if (cmd.Equals("sfsrmfil"))
            {
                Console.WriteLine("bob.txt rm");
                Fs.DeleteFile("bob.txt");
            }

            else if (cmd.Equals("sfsfilexist"))
            {
                if (Fs.FileExists("bob.txt"))
                {
                    Console.WriteLine("Exists!!");
                }
                else
                {
                    Console.WriteLine("Not exist!!");
                }
            }

            else if (cmd.StartsWith("cd "))
            {
                FileSystem.CD.c_CD(cmd);
            }
            else if (cmd.Equals("cp"))
            {
                FileSystem.CP.c_CP_only();
            }
            else if (cmd.StartsWith("cp "))
            {
                FileSystem.CP.c_CP(cmd);
            }
            else if ((cmd.Equals("dir")) || (cmd.Equals("ls")))
            {
                FileSystem.Dir.c_Dir();
            }
            else if ((cmd.StartsWith("dir ")) || (cmd.StartsWith("ls ")))
            {
                FileSystem.Dir.c_Dir(cmd);
            }
            else if (cmd.Equals("mkdir"))
            {
                FileSystem.Mkdir.c_Mkdir();
            }
            else if (cmd.StartsWith("mkdir "))
            {
                FileSystem.Mkdir.c_Mkdir(cmd);
            }
            else if (cmd.StartsWith("rmdir "))
            {
                FileSystem.Rmdir.c_Rmdir(cmd);
            }//TODO: orgainize
            else if (cmd.StartsWith("rmfil "))
            {
                FileSystem.Rmfil.c_Rmfil(cmd);
            }
            else if (cmd.Equals("mkfil"))
            {
                FileSystem.Mkfil.c_mkfil();
            }
            else if (cmd.StartsWith("mkfil "))
            {
                FileSystem.Mkfil.c_mkfil(cmd);
            }
            else if (cmd.StartsWith("edit "))
            {
                FileSystem.Edit.c_Edit(cmd);
            }
            else if (cmd.Equals("vol"))
            {
                FileSystem.Vol.c_Vol();
            }
            else if (cmd.StartsWith("run "))
            {
                FileSystem.Run.c_Run(cmd);
            }

            #endregion FileSystem

            #region Settings

            else if (cmd.Equals("logout"))
            {
                Settings.Logout.c_Logout();
            }
            else if (cmd.Equals("settings"))
            {
                Settings.Settings.c_Settings();
            }
            else if (cmd.StartsWith("settings "))
            {
                Settings.Settings.c_Settings(cmd);
            }
            else if (cmd.StartsWith("passwd "))
            {
                Settings.Passwd.c_Passwd(cmd);
            }
            else if (cmd.Equals("passwd"))
            {
                Settings.Passwd.c_Passwd(Kernel.userLogged);
            }

            #endregion Settings

            #region System Infomation

            else if (cmd.Equals("systeminfo"))
            {
                SystemInfomation.SystemInfomation.c_SystemInfomation();
            }
            else if ((cmd.Equals("ver")) || (cmd.Equals("version")))
            {
                SystemInfomation.Version.c_Version();
            }
            else if ((cmd.Equals("ipconfig")) || (cmd.Equals("ifconfig")) || (cmd.Equals("netconf")))
            {
                SystemInfomation.IPConfig.c_IPConfig();
            }
            else if ((cmd.Equals("time")) || (cmd.Equals("date")))
            {
                SystemInfomation.Time.c_Time();
            }

            #endregion System Infomation

            #region Tests

            else if (cmd.Equals("crash"))
            {
                Tests.Crash.c_Crash();
            }

            else if (cmd.Equals("cmd"))
            {
                CMDs.Add("ipconfig");
                CMDs.Add("netconf");
                CMDs.Add("help");
            }

            else if (cmd.Equals("crashcpu"))
            {
                int value = 1;
                value = value - 1;
                int result = 1 / value; //Division by 0
            }

            else if (cmd.Equals("beep"))
            {
                Kernel.speaker.beep();
            }

            else if (cmd.Equals("play"))
            {
                Kernel.speaker.playmusic();
            }

            else if (cmd.Equals("udp"))
            {
                var xClient = new System.Network.IPV4.UDP.UdpClient(4242);
                xClient.Connect(new System.Network.IPV4.Address(192,168,1,12), 4242);
                xClient.Send(Encoding.ASCII.GetBytes("Hello from Aura Operating System!"));
            }

            else if (cmd.Equals("tcp"))
            {
                var xClient = new System.Network.IPV4.TCP.TCPClient(4343);
                xClient.Connect(new System.Network.IPV4.Address(192, 168, 1, 12), 4224);
                xClient.Send(Encoding.ASCII.GetBytes("1"));
                xClient.Send(Encoding.ASCII.GetBytes("2"));
                xClient.Send(Encoding.ASCII.GetBytes("3"));
                xClient.Send(Encoding.ASCII.GetBytes("4"));
                xClient.Send(Encoding.ASCII.GetBytes("5"));

            }

            else if (cmd.Equals("dhcp"))
            {
                byte[] macb = { 0x00, 0x0C, 0x29, 0x7C, 0x85, 0x28 };
                HAL.MACAddress mac = new HAL.MACAddress(macb);
                System.Network.DHCP.DHCPDiscover dhcp_discover = new System.Network.DHCP.DHCPDiscover(mac, System.Network.IPV4.Address.Zero, new System.Network.IPV4.Address(192,168,1,100));
                //System.Network.DHCP.DHCPRequest dhcp_request = new System.Network.DHCP.DHCPRequest(mac, System.Network.IPV4.Address.Zero, new System.Network.IPV4.Address(192, 168, 1, 100), new System.Network.IPV4.Address(192, 168, 1, 254));

                System.Network.IPV4.OutgoingBuffer.AddPacket(dhcp_discover);
                System.Network.NetworkStack.Update();
            }

            else if (cmd.Equals("haship"))
            {
                Console.WriteLine(new HAL.MACAddress(new byte[] { 00, 01, 02, 03, 04, 05 }).Hash);
                Console.WriteLine(new System.Network.IPV4.Address(192, 168, 1, 12).Hash);
            }

            else if (cmd.Equals("dns"))
            {
                System.Network.IPV4.UDP.DNS.DNSClient DNSRequest = new System.Network.IPV4.UDP.DNS.DNSClient(53);
                DNSRequest.Ask("perdu.com");
            }

            //else if (cmd.Equals("discover"))
            //{
            //    //byte[] mac = { 0x00,0x0C, 0x29,0x7C, 0x85,0x28};                
            //    foreach (HAL.Drivers.Network.NetworkDevice device in HAL.Drivers.Network.NetworkDevice.Devices)
            //    {                    
            //        int a = 296 + System.Computer.Info.HostnameLength();
            //        ushort b = (ushort)a;
            //        Console.WriteLine("SRC MAC: " + device.MACAddress.ToString());
            //        System.Network.DHCP.DHCPDiscoverRequest request = new System.Network.DHCP.DHCPDiscoverRequest(device.MACAddress, b);
            //        Console.WriteLine("Sending DHCP Discover packet...");
            //        request.Send(device);
            //        System.Network.NetworkStack.Update();
            //    }              
            //}

            //else if (cmd.StartsWith("xml "))
            //{
            //    Util.xml.CmdXmlParser.c_CmdXmlParser(cmd, 0, 4);
            //}

            #endregion Tests

            #region Tools

            else if (cmd.Equals("snake"))
            {
                Tools.Snake.c_Snake();
            }
            else if (cmd.StartsWith("md5"))
            {
                Tools.MD5.c_MD5(cmd);
            }
            else if (cmd.StartsWith("sha256"))
            {
                Tools.SHA256.c_SHA256(cmd);
            }
            else if (cmd.StartsWith("ping"))
            {
                Network.Ping.c_Ping(cmd);
            }
            else if (cmd.Equals("debug"))
            {
                Tools.Debug.c_Debug();
            }
            else if (cmd.StartsWith("debug "))
            {
                Tools.Debug.c_Debug(cmd);
            }

            #endregion

            #region Util           

            else if (cmd.StartsWith("export"))
            {
                Util.EnvVar.c_Export(cmd);
            }

            else if (cmd.Equals("lspci"))
            {
                Util.Lspci.c_Lspci();
            }

            else if (cmd.Equals("about"))
            {
                Util.About.c_About();
            }

            else
            {
                if (cmd.Length <= 0)
                {
                    Console.WriteLine();
                    return;
                }
                else if (cmd.Length == 2)
                {
                    FileSystem.ChangeVol.c_ChangeVol(cmd);
                }
                else
                {
                    Util.CmdNotFound.c_CmdNotFound();
                }
            }

            CommandsHistory.Add(cmd); //adding last command to the commands history   

            Console.WriteLine();

            #endregion Util

        }

        static void ANETSTACKINIT()
        {
            while(true)
            {
                for (int i=0; i < 50000000; i++)
                {
                }
                Console.WriteLine("thread");
            }
        }

    }
}