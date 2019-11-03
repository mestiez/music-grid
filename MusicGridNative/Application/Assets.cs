using SFML.Graphics;

namespace MusicGrid
{
    public class Assets
    {
        public Font DefaultFont;
        public Texture LockedIcon;

        public Assets()
        {
            DefaultFont =  new Font(Properties.Resources.kosugiMaru);

            var converter = new System.Drawing.ImageConverter();
            LockedIcon = new Texture(converter.ConvertTo(Properties.Resources.locked, typeof(byte[])) as byte[]);
            LockedIcon.Smooth = true;
        }
    }
}
