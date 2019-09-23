using SFML.System;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicGridNative
{
    public class Program
    {
        static void Main(string[] args)
        {
            Configuration.LoadConfiguration();

            _ = new MusicGridApplication(
                Configuration.CurrentConfiguration.WindowWidth, 
                Configuration.CurrentConfiguration.WindowHeight, 
                Configuration.CurrentConfiguration.FramerateCap, "Music Grid");

            Configuration.SaveConfiguration();
        }
    }
}
