using M3U.NET;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicGridNative
{
    static class FileModelConverter
    {
        public static DistrictEntry FileToEntry(string absolutePath)
        {
            FileInfo info = new FileInfo(absolutePath);
            return new DistrictEntry(info.Name, absolutePath);
        }


        public static District LoadM3U(string path)
        {
            FileInfo info = new FileInfo(path);
            M3UFile v = new M3UFile(info);
            List<DistrictEntry> entries = new List<DistrictEntry>();

            foreach (MediaItem item in v.Files)
            {
                try
                {
                    entries.Add(FileToEntry(Path.Combine(info.Directory.FullName, item.Location)));
                }
                catch (Exception e)
                {
                    ConsoleEntity.Show(e);
                }
            }

            District district = new District(info.Name);
            district.Entries.AddRange(entries);

            return district;
        }
    }
}
