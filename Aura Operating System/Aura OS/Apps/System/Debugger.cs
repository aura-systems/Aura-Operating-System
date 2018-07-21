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
    class Debugger
    {

        UdpClient xClient;

        public Debugger(Address IP, int Port)
        {
            xClient = new UdpClient(Port);
            xClient.Connect(IP, Port);
            Send("--- Aura Debugger v0.1 ---");
        }

        public void Send(string message)
        {
            xClient.Send(Encoding.ASCII.GetBytes("[" + Aura_OS.System.Time.TimeString(true,true,true) + "] - " + message));
        }
    }
}
