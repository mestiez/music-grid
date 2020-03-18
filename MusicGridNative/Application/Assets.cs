using SFML.Graphics;

namespace MusicGrid
{
    public class Assets
    {
        public readonly Font DefaultFont;

        public readonly Texture LockedIcon;
        public readonly Texture NextButton;
        public readonly Texture PlayButton;
        public readonly Texture PreviousButton;
        public readonly Texture PauseButton;
        public readonly Texture RepeatButton;
        public readonly Texture RepeatDisabledButton;
        public readonly Texture ShuffleButton;
        public readonly Texture ShuffleDisabledButton;
        public readonly Texture ResizeHandle;

        private System.Drawing.ImageConverter converter = new System.Drawing.ImageConverter();

        public Assets()
        {
            DefaultFont = new Font(Properties.Resources.kosugiMaru);

            LockedIcon = BitmapToTexture(Properties.Resources.locked);
            LockedIcon.Smooth = true;

            NextButton = BitmapToTexture(Properties.Resources.next);
            PlayButton = BitmapToTexture(Properties.Resources.play);
            PreviousButton = BitmapToTexture(Properties.Resources.previous);
            PauseButton = BitmapToTexture(Properties.Resources.pause);
            RepeatButton = BitmapToTexture(Properties.Resources.repeat);
            RepeatDisabledButton = BitmapToTexture(Properties.Resources.repeatDisabled);
            ShuffleButton = BitmapToTexture(Properties.Resources.shuffle);
            ShuffleDisabledButton = BitmapToTexture(Properties.Resources.shuffleDisabled);
            ResizeHandle = BitmapToTexture(Properties.Resources.resize);
        }

        private Texture BitmapToTexture(System.Drawing.Bitmap bitmap) => new Texture(converter.ConvertTo(bitmap, typeof(byte[])) as byte[]);
    }
}
