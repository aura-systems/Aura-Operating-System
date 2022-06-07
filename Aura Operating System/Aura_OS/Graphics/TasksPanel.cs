using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveOS.GUI
{
    public class TasksPanel : WavePanel
    {
        public override void Update()
        {
            base.Update();
        }

        public override void Draw()
        {
            base.Draw();
        }

        public void AlignTaskbarButtons()
        {
            for (int i = 0; i < children.Count; i++)
            {
                WaveElement item = children[i];
                item.X = (160 * i) + 2;
            }
        }
    }
}
