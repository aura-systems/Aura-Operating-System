/*
* PROJECT:          Aura Operating System Development
* CONTENT:          End point
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System;

namespace Aura_OS.System.Network.IPV4
{
    public class EndPoint : IComparable
    {
        internal Address address;
        internal UInt16 port;

        public EndPoint(Address addr, UInt16 port)
        {
            this.address = addr;
            this.port = port;
        }

        public override string ToString()
        {
            return this.address.ToString() + ":" + this.port.ToString();
        }

        public int CompareTo(object obj)
        {
            if (obj is EndPoint)
            {
                EndPoint other = (EndPoint)obj;
                if ((other.address.CompareTo(this.address) != 0) ||
                    (other.port != this.port))
                {
                    return -1;
                }

                return 0;
            }
            else
                throw new ArgumentException("obj is not a IPv4EndPoint", "obj");
        }
    }
}
