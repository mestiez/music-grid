using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicGrid
{
    public interface IRenderTask
    {
        int Depth { get; set; }

        void Render(RenderTarget target);
    }
}
