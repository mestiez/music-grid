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
        public static DistrictEntry FileToEntry(string absolutePath, string newRoot = "")
        {
            Uri path = new Uri(absolutePath);
            Uri relative = null;
            FileInfo info = new FileInfo(path.AbsolutePath);

            try
            {
                relative = path.MakeRelativeUri(new Uri(newRoot));
            }
            catch (Exception)
            {
                ConsoleEntity.Show($"Failed to assign relative path to " + path.AbsolutePath);
            }

            return new DistrictEntry(info.Name, relative.ToString());
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
                    entries.Add(FileToEntry(item.Location));
                }
                catch (Exception e)
                {
                    ConsoleEntity.Show(e);
                }
            }

            District district = new District(info.Name, new Vector2f(), new Vector2f(), Color.White);
            district.Entries.AddRange(entries);

            return district;
        }
    }
}
