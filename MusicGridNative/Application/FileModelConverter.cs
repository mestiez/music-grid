using M3U.NET;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;

namespace MusicGrid
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
                    ConsoleEntity.Log(e.Message, typeof(FileModelConverter).Name) ;
                }
            }

            District district = new District(info.Name.Substring(0, info.Name.LastIndexOf('.')));
            district.Entries.AddRange(entries);

            ConsoleEntity.Log($"Successfully loaded playlist at {path}", typeof(FileModelConverter).Name);
            return district;
        }
    }
}
