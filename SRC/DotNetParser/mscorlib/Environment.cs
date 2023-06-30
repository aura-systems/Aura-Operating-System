namespace System
{
    public static class Environment
    {
        public static int get_SystemPageSize()
        {
            //idk weather this will be breaking some app, but this will be the way to deterim if running under dotnetparser
            return 1;
        }
        public static string GetEnvironmentVariable(string var)
        {
            Console.WriteLine("read env variable: " + var);
            return null;
        }
    }
}
