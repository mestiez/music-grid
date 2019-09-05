using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicGridNative
{
    public static class Input
    {
        private static Window window;

        public static void SetWindow(Window window)
        {
            Input.window = window;
        }
    }
}
