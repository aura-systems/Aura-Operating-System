/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Threading - Thread
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/


namespace Aura_OS.System.Threading
{
    public class Thread : ThreadBase
    {
        public Thread(string name, ThreadMethod ThrdInvoke)
        {
            this.name = name;
            ThreadManager.Register(this);
            this.action = ThrdInvoke;
        }
    }
}
