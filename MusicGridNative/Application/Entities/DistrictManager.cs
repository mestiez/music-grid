using Microsoft.Win32;
using Newtonsoft.Json;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicGrid
{
    public class DistrictManager : Entity
    {
        private readonly List<District> districts = new List<District>();

        public IReadOnlyList<District> Districts => districts.AsReadOnly();

        public override void Created()
        {
            World.Lua.LinkFunction<string>("save_grid", this, SaveGrid);
            World.Lua.LinkFunction<string>("load_grid", this, LoadGrid);
            World.Lua.LinkFunction<string>("import_district", this, ImportPlaylist);
        }

        public void AskImportPlaylist()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                DefaultExt = ".m3u8",
                Multiselect = true,
                Title = "Import m3u8",
                Filter = "M3U8 Playlists|*.m3u8"
            };
            var result = dialog.ShowDialog();
            if (result == false) return;
            ConsoleEntity.Show(string.Join(", ", dialog.FileNames));
            foreach (var path in dialog.FileNames)
                ImportPlaylist(path);
        }

        public void ImportPlaylist(string path)
        {
            var district = FileModelConverter.LoadM3U(path);
            AddDistrict(district, true);
        }

        public void AddDistrict(District district, bool giveRandomProperties = false)
        {
            districts.Add(district);
            if (giveRandomProperties)
            {
                district.Size = new Vector2f(250 * (float)Math.Ceiling(district.Entries.Count / 20f), 64 * Math.Min(district.Entries.Count, 30));
                district.Position = MusicGridApplication.Main.ScreenToWorld(new Vector2i(15, 15));
                district.Color = new Color(Utilities.RandomByte(), Utilities.RandomByte(), Utilities.RandomByte());
            }
            World.Add(new DistrictEntity(district));
        }

        public void SaveGrid(string targetPath)
        {
            var name = Path.GetFileName(targetPath);
            var saveUri = new Uri(targetPath);

            var relativisedDistricts = new List<District>();

            foreach (var district in districts)
            {
                var copy = new District(district.Name, district.Position, district.Size, district.Color);
                copy.Entries = district.Entries.Select((e) =>
                {
                    Uri uri = new Uri(e.Path);
                    uri.MakeRelativeUri(saveUri);
                    return new DistrictEntry(e.Name, uri.OriginalString);
                }).ToList();
                relativisedDistricts.Add(copy);
            }

            var grid = new Grid(name);
            grid.Districts = districts;

            var json = JsonConvert.SerializeObject(grid);
            File.WriteAllText(targetPath, json);
        }

        public void LoadGrid(string path)
        {
            var json = File.ReadAllText(path);
            var grid = JsonConvert.DeserializeObject<Grid>(json);

            var loadUri = new Uri(path);

            districts.Clear();
            foreach (var district in grid.Districts)
            {
                var copy = new District(district.Name, district.Position, district.Size, district.Color);
                copy.Entries = district.Entries.Select((e) =>
                {
                    Uri uri = new Uri(e.Path);
                    uri.MakeRelativeUri(loadUri);
                    return new DistrictEntry(e.Name, uri.OriginalString);
                }).ToList();
                districts.Add(copy);
            }
        }
    }
}
