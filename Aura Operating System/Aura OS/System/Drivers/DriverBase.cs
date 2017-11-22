/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Class to add drivers.
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

namespace Aura_OS.HAL
{
    public abstract class Driver
    {
        public string Name;

        public Driver()
        {
            Kernel.Drivers.Add(this);
        }

        public abstract bool Init();
    }
}
