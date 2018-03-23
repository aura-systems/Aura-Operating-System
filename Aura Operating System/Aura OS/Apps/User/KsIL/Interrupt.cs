using System;
using System.Collections.Generic;
using System.Text;

namespace KsIL
{
    public class Interrupt
    {

        public static List<Interrupt> Default
        {

            get
            {

                List<Interrupt> r = new List<Interrupt>
                {
                    new Interrupts.Invoke()
                };

                r.AddRange(DefaultCosmos);

                return r;

            }
        }

        public static List<Interrupt> DefaultCosmos
        {
            get
            {

                List<Interrupt> r = new List<Interrupt>
                {
                    new Interrupts.Console(),
                    new Interrupts.File()
                };

                return r;

            }
        }

        public Int16 Code;
                
        public Interrupt()
        {
        }

        public virtual void Run(byte[] Parameters, Memory mMemory)
        {
        }

    }
}
