using SFML.Graphics;

namespace MusicGrid
{
    public struct Style
    {
        public static Color Background => new Color(0, 0, 0, 200);
        public static Color BackgroundDisabled => new Color(0, 0, 0, 200);
        public static Color BackgroundHover => new Color(60, 60, 60, 200);
        public static Color BackgroundActive => new Color(30, 30, 30, 200);

        public static Color Foreground => Color.White;
        public static Color ForegroundDisabled => new Color(125, 125, 125);
    }
}
