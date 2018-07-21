/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Debugger using UDP!
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Network.IPV4;
using Aura_OS.System.Network.IPV4.UDP;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.Apps.System
{
    public class Debugger
    {

        UdpClient xClient;

        public bool enabled = false;

        int port;
        public Address ip;

        public Debugger(Address IP, int Port)
        {
            ip = IP;
            port = Port;
        }

        public void Start()
        {
            xClient = new UdpClient(port);
            xClient.Connect(ip, port);
            Send("--- Aura Debugger v0.1 ---");
        }

        public void Send(string message)
        {
            if (enabled)
            {
                xClient.Send(Encoding.ASCII.GetBytes("[" + Aura_OS.System.Time.TimeString(true, true, true) + "] - " + message));
            }
        }
    }
}
