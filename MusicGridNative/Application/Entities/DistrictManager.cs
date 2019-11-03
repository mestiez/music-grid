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
        public const string ConsoleSourceIdentifier = "DISTRICT MANAGER";
        public IReadOnlyList<District> Districts => districts.AsReadOnly();

        public override void Created()
        {
            World.Lua.LinkFunction<string>("save_grid", this, SaveGrid);
            World.Lua.LinkFunction<string>("load_grid", this, LoadGrid);
            World.Lua.LinkFunction<string>("import_district", this, ImportPlaylist);

            World.Lua.LinkFunction("ask_save_grid", this, AskSaveGrid);
            World.Lua.LinkFunction("ask_load_grid", this, AskLoadGrid);
            World.Lua.LinkFunction("ask_import_district", this, AskImportPlaylist);
        }

        public void AskImportPlaylist()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                DefaultExt = ".m3u8",
                Multiselect = true,
                Title = "Import m3u8",
                Filter = "M3U8 Playlists|*.m3u8|All files|*.*"
            };
            var result = dialog.ShowDialog();
            if (result == false) return;
            foreach (var path in dialog.FileNames)
                ImportPlaylist(path);
        }

        public void AskLoadGrid()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                DefaultExt = ".mgd",
                Multiselect = false,
                Title = "Load music grid",
                Filter = "Music grids|*.mgd|JSON files|*.json|All files|*.*"
            };
            var result = dialog.ShowDialog();
            if (result == false) return;
            LoadGrid(dialog.FileName);
        }

        public void AskSaveGrid()
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                DefaultExt = ".mgd",
                Title = "Save music grid",
                Filter = "Music grids|*.mgd|JSON files|*.json|All files|*.*"
            };
            var result = dialog.ShowDialog();
            if (result == false) return;
            SaveGrid(dialog.FileName);
        }

        public void ImportPlaylist(string path)
        {
            var district = FileModelConverter.LoadM3U(path);
            AddDistrict(district, true);
        }

        public void AddDistrict(District district, bool giveRandomProperties = false)
        {
            if (districts.Contains(district)) throw new ArgumentException($"District {district.Name} already exists");
            districts.Add(district);
            if (giveRandomProperties)
            {
                district.Size = new Vector2f(250 * (float)Math.Ceiling(district.Entries.Count / 20f), 64 * Math.Min(district.Entries.Count, 30));
                district.Position = MusicGridApplication.Main.ScreenToWorld(new Vector2i(15, 15));
                district.Color = new Color(Utilities.RandomByte(), Utilities.RandomByte(), Utilities.RandomByte());
            }
            World.Add(new DistrictEntity(district));
        }

        public void RemoveDistrict(District district)
        {
            var entities = World.GetEntitiesByType<DistrictEntity>();
            foreach (var entity in entities.Where(e => e.District == district))
                World.Destroy(entity);
            if (!districts.Remove(district))
                ConsoleEntity.Log($"Attempt to remove non-existant district {district}", ConsoleSourceIdentifier);
        }

        public void SaveGrid(string targetPath)
        {
            ConsoleEntity.Log($"Saving grid to {targetPath}", ConsoleSourceIdentifier);
            targetPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, targetPath));
            var name = Path.GetFileName(targetPath);
            var fullPath = targetPath.Substring(0, targetPath.Length - name.Length);

            var relativisedDistricts = new List<District>();

            foreach (var district in districts)
            {
                var copy = new District(district.Name, district.Position, district.Size, district.Color, district.Locked);
                copy.Entries = district.Entries.Select((e) =>
                {
                    return new DistrictEntry(e.Name, Utilities.GetRelativePath(e.Path, fullPath));
                }).ToList();
                relativisedDistricts.Add(copy);
            }

            var grid = new Grid(name);
            grid.Districts = relativisedDistricts;

            try
            {
                var json = JsonConvert.SerializeObject(grid);
                File.WriteAllText(targetPath, json);
            }
            catch (Exception e)
            {
                ConsoleEntity.Log(e.Message, ConsoleSourceIdentifier);
                return;
            }
            ConsoleEntity.Log($"Grid succesfully saved", ConsoleSourceIdentifier);
        }

        public void LoadGrid(string path)
        {
            Grid grid = null;
            try
            {
                ConsoleEntity.Log($"Loading grid at {path}", ConsoleSourceIdentifier);
                var json = File.ReadAllText(path);
                grid = JsonConvert.DeserializeObject<Grid>(json);
            }
            catch (Exception e)
            {
                ConsoleEntity.Log(e.Message, ConsoleSourceIdentifier);
                return;
            }

            RemoveAllDistricts();

            path = path.Substring(0, path.Length - Path.GetFileName(path).Length);
            foreach (var district in grid.Districts)
            {
                var copy = new District(district.Name, district.Position, district.Size, district.Color, district.Locked);
                copy.Entries = district.Entries.Select((e) =>
                {
                    return new DistrictEntry(e.Name, Path.GetFullPath(Path.Combine(path, e.Path)));
                }).ToList();
                AddDistrict(copy);
            }
            ConsoleEntity.Log($"Grid succesfully loaded", ConsoleSourceIdentifier);
        }

        public void RemoveAllDistricts()
        {
            var entities = World.GetEntitiesByType<DistrictEntity>();
            foreach (var entity in entities)
                World.Destroy(entity);
            districts.Clear();
        }
    }
}
