/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Base interface for managers.
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

namespace Aura_OS.System
{
    public interface IManager
    {
        public void Initialize();
        public string GetName();
    }
}
