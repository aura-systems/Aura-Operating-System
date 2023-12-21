using Aura_OS.System.Application.Emulators.GameBoyEmu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Aura_OS.System.Application
{
    public class ApplicationConfig
    {
        public Type Template;
        public int X;
        public int Y;
        public int Weight;
        public int Height;

        public ApplicationConfig(Type template, int x, int y, int weight, int height)
        {
            Template = template;
            X = x;
            Y = y;
            Weight = weight;
            Height = height;
        }
    }

    public class ApplicationManager
    {
        public List<ApplicationConfig> ApplicationTemplates;

        public ApplicationManager()
        {
            ApplicationTemplates = new List<ApplicationConfig>();
        }

        public void RegisterApplication(ApplicationConfig config)
        {
            ApplicationTemplates.Add(config);
        }

        public void RegisterApplication(Type template, int x, int y, int weight, int height)
        {
            ApplicationConfig config = new(template, x, y, weight, height);
            ApplicationTemplates.Add(config);
        }

        public App Instantiate(ApplicationConfig config)
        {
            App app = null;

            if (config.Template == typeof(Terminal))
            {
                app = new Terminal(config.Weight, config.Height, config.X, config.Y);
            }
            else if (config.Template == typeof(Cube))
            {
                app = new Cube(config.Weight, config.Height, config.X, config.Y);
            }
            else if (config.Template == typeof(SystemInfo))
            {
                app = new SystemInfo(config.Weight, config.Height, config.X, config.Y);
            }
            else if (config.Template == typeof(MemoryInfo))
            {
                app = new MemoryInfo(config.Weight, config.Height, config.X, config.Y);
            }
            else if (config.Template == typeof(GameBoyEmu))
            {
                app = new GameBoyEmu(config.Weight, config.Height, config.X, config.Y);
            }
            else
            {
                throw new InvalidOperationException("Type d'application non reconnu.");
            }

            return app;
        }
    }
}
