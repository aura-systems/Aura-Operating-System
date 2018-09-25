using CosmosELFCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Executables
{
	public unsafe class ElfHandler
	{
		private byte[] UnmanagedString(string s)
		{
			var re = new byte[s.Length + 1];

			for (int i = 0; i < s.Length; i++)
			{
				re[i] = (byte)s[i];
			}

			re[s.Length + 1] = 0; //c requires null terminated string
			return re;
		}

		public void BR()
		{
			fixed (byte* ptr = TestFile.test_so)
			{
				var exe = new UnmanagedExecutible(ptr);
				exe.Load();
				exe.Link();

				Console.WriteLine("Executing");

				new ArgumentWriter();
				exe.Invoke("tty_clear");

				new ArgumentWriter()
					.Push(5)  //fg
					.Push(15); //bg
				exe.Invoke("tty_set_color");

				fixed (byte* str = UnmanagedString("Hello World"))
				{
					new ArgumentWriter()
						.Push((uint)str);
					exe.Invoke("tty_puts");
				}


			}
		}
	}
}
