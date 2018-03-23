using System;
using System.Collections.Generic;
using System.Text;

namespace KsIL.Interrupts
{

    public class File : Interrupt
    {

        public File()
        {
            Code = 2;
        }


        public override void Run(byte[] Parameters, Memory mMemory)
        {

            string Path = Encoding.UTF8.GetString(mMemory.GetData(BitConverter.ToInt32(Parameters, 1)));

            if (Parameters[0] == 0x00)
            {

                System.IO.File.Delete(Path);

            }
            else if (Parameters[0] == 0x01)
            {

                string Content = Encoding.UTF8.GetString(mMemory.GetData(BitConverter.ToInt32(Parameters, 5)));

                System.IO.File.WriteAllText(Path, Content);

            }
            else if (Parameters[0] == 0x02)
            {

                byte[] Content = mMemory.GetData(BitConverter.ToInt32(Parameters, 5));

                System.IO.File.WriteAllBytes(Path, Content);

            }
            else if (Parameters[0] == 0x03)
            {

                string Content = Encoding.UTF8.GetString(mMemory.GetData(BitConverter.ToInt32(Parameters, 5)));

                System.IO.File.AppendAllText(Path, Content);

            }
            else if (Parameters[0] == 0x04)
            {

                System.IO.Directory.Delete(Path);

            }
            else if (Parameters[0] == 0x05)
            {

                System.IO.Directory.CreateDirectory(Path);

            }
            else if (Parameters[0] == 0x06)
            {

                if (System.IO.Directory.Exists(Path))
                {
                    mMemory.Set(Memory.CONDITIONAL_RESULT, 0x01);
                }
                else
                {
                    mMemory.Set(Memory.CONDITIONAL_RESULT, 0x00);
                }

            }
            else if (Parameters[0] == 0x07)
            {

                if (System.IO.File.Exists(Path))
                {
                    mMemory.Set(Memory.CONDITIONAL_RESULT, 0x01);
                }
                else
                {
                    mMemory.Set(Memory.CONDITIONAL_RESULT, 0x00);
                }

            }



        }

    }
}
