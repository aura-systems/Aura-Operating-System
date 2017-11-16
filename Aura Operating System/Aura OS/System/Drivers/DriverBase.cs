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
