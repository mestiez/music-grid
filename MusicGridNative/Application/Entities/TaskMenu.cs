using Microsoft.Win32;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicGridNative
{
    public class TaskMenu : Entity
    {
        private readonly List<Button> buttons = new List<Button>();
        private float height = 32;
        private RectangleShape background;
        private RectangleShape buttonBackground;
        private Text buttonText;

        private ShapeRenderTask backgroundTask;

        private ActionRenderTask[] textTasks;
        private UiElement[] buttonElements;

        private float buttonPos;
        private static readonly uint CharacterSize = 16;
        private UiControllerEntity uiController;

        public float Height { get => height; set => height = value; }
        public IReadOnlyList<Button> Buttons => buttons.AsReadOnly();

        public override void Created()
        {
            uiController = World.GetEntityByType<UiControllerEntity>();
            buttonText = new Text("invalid!", MusicGridApplication.Assets.DefaultFont)
            {
                FillColor = Color.White,
                Position = default,
                CharacterSize = CharacterSize
            };

            buttons.Add(new Button("import m3u8", () =>
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
                {
                    var district = FileModelConverter.LoadM3U(path);
                    district.Size = new Vector2f(250 * (float)Math.Ceiling( district.Entries.Count / 20f), 64 * Math.Min(district.Entries.Count, 30));
                    district.Position = MusicGridApplication.Main.ScreenToWorld(new Vector2i(15, 15));
                    district.Color = new Color(Utilities.RandomByte(), Utilities.RandomByte(), Utilities.RandomByte());
                    World.Add(new DistrictEntity(district));
                }
            }));

            buttons.Add(new Button("load grid", () => { ConsoleEntity.Show("should load a grid from file"); }));
            buttons.Add(Button.Separator);
            buttons.Add(new Button("save grid", () => { ConsoleEntity.Show("save file dialog time"); }));
            buttons.Add(Button.Separator);
            buttons.Add(new Button("fit view to grid", () => { ConsoleEntity.Show("check everything and set the camera up correctly"); }));
            buttons.Add(new Button("reset camera", () => { Configuration.CurrentConfiguration.Pan = default; Configuration.CurrentConfiguration.Zoom = 1; }));

            RecalculateLayout();

            Input.WindowResized += (obj, e) =>
            {
                RecalculateLayout();
            };

        }

        private void RecalculateLayout()
        {
            if (buttonElements != null)
                foreach (var elem in buttonElements)
                    uiController.Deregister(elem);

            background = new RectangleShape()
            {
                Size = new Vector2f(Input.WindowSize.X, height),
                Position = default,
                FillColor = new Color(0, 0, 0, 200)
            };
            backgroundTask = new ShapeRenderTask(background, 0);

            buttonBackground = new RectangleShape()
            {
                FillColor = new Color(255, 0, 0, 125)
            };

            buttonElements = new UiElement[buttons.Count];
            textTasks = new ActionRenderTask[buttons.Count];
            float halfCharSize = CharacterSize * .5f;
            float margin = 15;
            float mostRight = World.RenderTarget.Size.X;
            for (int i = 0; i < buttons.Count; i++)
            {
                var button = Buttons[i];
                float width = halfCharSize * button.Label.Length;

                var elem = new UiElement
                {
                    Disabled = !button.Interactive,
                    Depth = int.MinValue,
                    IsScreenSpace = true,
                    Color = Color.Transparent,
                    ActiveColor = new Color(255, 255, 255, 10),
                    HoverColor = new Color(255, 255, 255, 25),
                    Position = background.Position + new Vector2f(buttonPos, 0),
                    Size = new Vector2f(width + margin * 2, height)
                };

                elem.OnMouseDown += (o, e) => { button.Action(); };

                uiController.Register(elem);
                buttonElements[i] = elem;

                buttonPos += width + margin * 2;
                textTasks[i] = new ActionRenderTask((target) =>
                {
                    buttonText.DisplayedString = button.Label;
                    buttonText.Position = elem.Position + new Vector2f(margin, -halfCharSize + height / 2);

                    buttonBackground.Position = elem.Position;
                    buttonBackground.Size = elem.Size;
                    buttonBackground.FillColor = elem.ComputedColor;

                    target.Draw(buttonBackground);
                    target.Draw(buttonText);
                }, 0);
            }
        }

        public override IEnumerable<IRenderTask> RenderScreen()
        {
            buttonPos = 0;
            yield return backgroundTask;
            foreach (var task in textTasks)
                yield return task;
        }
    }
}
