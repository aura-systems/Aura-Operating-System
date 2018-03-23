using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace KsIL.Interrupts
{
    public class Invoke : Interrupt
    {

        public Invoke()
        {
            Code = 0;
        }

        public override void Run(byte[] Parameters, Memory mMemory)
        {

            if (Parameters[0] == 0x03)
            {

                Assembly.LoadFile(Encoding.UTF8.GetString(Utill.GetData(1, Parameters)));
                return;

            }


            int offset = 1;

            int ClassLength = BitConverter.ToInt32(Parameters, offset);
            string ClassPath = Encoding.UTF8.GetString(Utill.GetData(offset, Parameters));

            offset += 4;

            string[] ConstructorArgs = Utill.GetStringArray(Utill.GetData(offset, Parameters), mMemory);

            offset += BitConverter.ToInt32(Parameters, offset) + 4;

            string Method = Encoding.UTF8.GetString(Utill.GetData(offset, Parameters));
            
            offset += BitConverter.ToInt32(Parameters, offset) + 4;

            string[] MethodArgs = Utill.GetStringArray(Utill.GetData(offset, Parameters), mMemory);

            offset += BitConverter.ToInt32(Parameters, offset) + 4;

            int Output = 13;


            if (Parameters.Length > offset)
            {

                Output = BitConverter.ToInt32(Parameters, offset);

            }

            if (Parameters[0] == 0x00)
            {

                CreateAndInvoke(ClassPath, ConstructorArgs, Method, MethodArgs);

            }
            else if (Parameters[0] == 0x01)
            {

                mMemory.SetData(Output, (byte[])CreateAndInvoke(ClassPath, ConstructorArgs, Method, MethodArgs));

            }
            else if (Parameters[0] == 0x02)
            {

                mMemory.SetData(Output, Encoding.UTF8.GetBytes((string)CreateAndInvoke(ClassPath, ConstructorArgs, Method, MethodArgs)));

            }

        }
        
        public static object CreateAndInvoke(string typeName, string[] constructorArgs, string methodName, string[] methodArgs)
        {

            Type type = Type.GetType(typeName);
            object instance = Activator.CreateInstance(type, constructorArgs);

            MethodInfo method = type.GetMethod(methodName);
            return method.Invoke(instance, methodArgs);

        }

    }
}
