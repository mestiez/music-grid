using Microsoft.Win32;
using Newtonsoft.Json;
using SFML.Graphics;
using SFML.System;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Color = Shared.Color;

namespace MusicGrid
{
    public class DistrictManager : Entity
    {
        private readonly List<District> districts = new List<District>();
        public const string ConsoleSourceIdentifier = "DISTRICT MANAGER";
        public IReadOnlyList<District> Districts => districts.AsReadOnly();

        public bool SnappingEnabled { get; private set; } = true;

        public override void Created()
        {
            World.Lua.LinkFunction<string>(Functions.SaveGrid, this, SaveGrid);
            World.Lua.LinkFunction<string>(Functions.LoadGrid, this, (string a) => LoadGrid(a));
            World.Lua.LinkFunction<string>(Functions.ImportDistrict, this, (string a) => ImportPlaylist(a));

            World.Lua.LinkFunction(Functions.AskSaveGrid, this, AskSaveGrid);
            World.Lua.LinkFunction(Functions.AskLoadGrid, this, AskLoadGrid);
            World.Lua.LinkFunction(Functions.AskImportDistrict, this, AskImportPlaylist);

            World.Lua.LinkFunction(Functions.EnableSnap, this, () => { SnappingEnabled = true; });
            World.Lua.LinkFunction(Functions.DisableSnap, this, () => { SnappingEnabled = false; });

            Input.FileDrop += OnFileDrop;
        }

        private void OnFileDrop(object sender, string filename)
        {
            FileInfo file = new FileInfo(filename);
            ConsoleEntity.Log($"Attempt to import {filename}", ConsoleSourceIdentifier);
            switch (file.Extension.ToLower())
            {
                case ".m3u8":
                    var p = ImportPlaylist(filename);
                    p.Position = Input.MousePosition.ToNumerics();
                    break;
                case ".mgd":
                case ".json":
                    LoadGrid(filename);
                    break;
                default:
                    var d = ImportAudioAsPlaylist(filename);
                    d.Position = Input.MousePosition.ToNumerics();
                    break;
            }
        }

        public District ImportAudioAsPlaylist(string filename)
        {
            FileInfo file = new FileInfo(filename);
            District district = new District(file.Name);
            district.Entries.Add(new DistrictEntry(filename, filename));
            AddDistrict(district, true);
            return district;
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

        public District ImportPlaylist(string path)
        {
            var district = FileModelConverter.LoadM3U(path);
            AddDistrict(district, true);
            return district;
        }

        public void AddDistrict(District district, bool giveRandomProperties = false)
        {
            if (districts.Contains(district)) throw new ArgumentException($"District {district.Name} already exists");
            districts.Add(district);
            if (giveRandomProperties)
            {
                district.Size = new Vector2f(250 * (float)Math.Ceiling(district.Entries.Count / 20f), 64 * Math.Min(district.Entries.Count, 30)).ToNumerics();
                district.Position = MusicGridApplication.Main.ScreenToWorld(new Vector2i(15, 15)).ToNumerics();
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
                var copy = new District(district.Name, district.Position, district.Size, district.Color, district.Locked, district.Muted);
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

        public Grid LoadGrid(string path)
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
                return null;
            }

            RemoveAllDistricts();

            path = path.Substring(0, path.Length - Path.GetFileName(path).Length);
            foreach (var district in grid.Districts)
            {
                var copy = new District(district.Name, district.Position, district.Size, district.Color, district.Locked, district.Muted);
                copy.Entries = district.Entries.Select((e) =>
                {
                    return new DistrictEntry(e.Name, Path.GetFullPath(Path.Combine(path, e.Path)));
                }).ToList();
                AddDistrict(copy);
            }
            ConsoleEntity.Log($"Grid succesfully loaded", ConsoleSourceIdentifier);

            return grid;
        }

        public void RemoveAllDistricts()
        {
            var entities = World.GetEntitiesByType<DistrictEntity>();
            foreach (var entity in entities)
                World.Destroy(entity);
            districts.Clear();
        }

        public District GetDistrictFromEntry(DistrictEntry entry)
        {
            return districts.FirstOrDefault(d => d.Entries.Contains(entry));
        }
    }
}
