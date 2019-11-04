using System.Collections.Generic;
using SFML.System;

namespace MusicGrid
{
    public class MusicControlsEntity : Entity
    {
        public MusicPlayer MusicPlayer { get; } = new MusicPlayer();

        private UiControllerEntity uiController;
        private DrawableElement background;

        public MusicControlsEntity()
        {
            MusicPlayer.OnFailure += (o, e) =>
            {
                World.Add(new DialogboxEntity(e.Message, new SFML.System.Vector2f(e.Message.Length * DialogboxEntity.CharacterSize/2 + 50, 150)));
            };
        }

        public override void Created()
        {
            uiController = World.GetEntityByType<UiControllerEntity>();
            background = new DrawableElement(uiController, new Vector2f(300,150), new Vector2f(50,50));
            background.Position = new Vector2f(0, Input.WindowSize.Y - background.Size.Y);

            background.Element.IsScreenSpace = true;
            background.Element.ActiveColor = background.Element.Color;
            background.Element.HoverColor = background.Element.Color;

            Input.WindowResized += OnWindowResized;

            World.Lua.LinkFunction("pause", this, () => { MusicPlayer.Pause(); });
            World.Lua.LinkFunction("play", this, () => { MusicPlayer.Play(); });
            World.Lua.LinkFunction("stop", this, () => { MusicPlayer.Stop(); });
            World.Lua.LinkFunction("set_track", this, (string track) => { MusicPlayer.SetTrack(track); });
        }

        private void OnWindowResized(object sender, SFML.Window.SizeEventArgs e)
        {
            background.Position = new Vector2f(0, e.Height - background.Size.Y);
        }

        public override IEnumerable<IRenderTask> RenderScreen()
        {
            yield return background.RenderTask;
        }
    }
}
