using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MusicGrid
{
    public static class TextureCache
    {
        private static Dictionary<string, Texture> cache = new Dictionary<string, Texture>();

        public static void LoadAsync(string filename, Action<Texture> callback)
        {
            Task.Run(() =>
            {
                callback(Load(filename));
            });
        }

        public static Texture Load(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return null; 

            if (!cache.TryGetValue(filename, out var tex))
            {
                try
                {
                    tex = new Texture(filename);
                    tex.Smooth = true;
                }
                catch (System.Exception)
                {
                    return null;
                }
                cache.Add(filename, tex);
            }
            return tex;
        }

        public static void Clear()
        {
            cache.Clear();
        }
    }
}