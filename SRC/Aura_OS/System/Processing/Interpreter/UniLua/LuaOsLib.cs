 
namespace UniLua
{
	using System.Diagnostics;

	internal class LuaOSLib
	{
		public const string LIB_NAME = "os";

		public static int OpenLib( ILuaState lua )
		{
			NameFuncPair[] define = new NameFuncPair[]
			{
#if !UNITY_WEBPLAYER
				new NameFuncPair("clock", 	OS_Clock),
#endif
			};

			lua.L_NewLib( define );
			return 1;
		}

#if !UNITY_WEBPLAYER
		private static int OS_Clock( ILuaState lua )
		{
			// Seconds since boot; Stopwatch is TSC-backed on Cosmos gen3 (StopwatchPlug).
			lua.PushNumber( Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency );
			return 1;
		}
#endif
	}
}

