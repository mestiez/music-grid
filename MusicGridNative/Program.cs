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
            _ = new MusicGridApplication(500, 500, 3000, "Music Grid");
        }
    }
}
