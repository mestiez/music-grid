using SFML.Graphics;
using SFML.System;
using System.Collections.Generic;

namespace MusicGrid
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

        private UiElement backgroundElement;
        private UiElement[] buttonElements;

        private float buttonPos;
        private static readonly uint CharacterSize = 16;
        private UiControllerEntity uiController;
        private DistrictManager districtManager;

        public float Height
        {
            get => height;
            set
            {
                height = value;
                RecalculateLayout();
            }
        }
        public IReadOnlyList<Button> Buttons => buttons.AsReadOnly();

        public override void Created()
        {
            Input.WindowResized += (obj, e) => { RecalculateLayout(); };
            uiController = World.GetEntityByType<UiControllerEntity>();
            districtManager = World.GetEntityByType<DistrictManager>();
            backgroundElement = new UiElement();
            uiController.Register(backgroundElement);
            buttonText = new Text("invalid!", MusicGridApplication.Assets.DefaultFont)
            {
                FillColor = Color.White,
                Position = default,
                CharacterSize = CharacterSize
            };
            CreateButtons();
            RecalculateLayout();
        }

        private void CreateButtons()
        {
            buttons.Add(new Button("file", () =>
            {
                ContextMenuEntity.Main.MinimumWidth = buttonElements[0].Size.X;
                ContextMenuEntity.Open(new[] {
                    new Button("import m3u8", districtManager.AskImportPlaylist),
                    new Button("load grid", districtManager.AskLoadGrid),
                    new Button("save grid", districtManager.AskSaveGrid),
                    Button.HorizontalSeparator,
                    new Button("quit", () => { World.Add(DialogboxEntity.CreateConfirmationDialog("Are you sure you want to quit?", () => { World.Lua.Execute("quit()"); })); }),
                },
                    buttonElements[0].Position + new Vector2f(0, Height));
            }));

            buttons.Add(new Button("view", () =>
            {
                ContextMenuEntity.Main.MinimumWidth = buttonElements[1].Size.X;
                ContextMenuEntity.Open(new[] {
                    new Button("fit view to grid", () => { World.GetEntityByType<CameraControllerEnity>().FitToView(districtManager.Districts); }),
                    new Button("reset view", () => { Configuration.CurrentConfiguration.Pan = default; Configuration.CurrentConfiguration.Zoom = 1; }),
                },
                    buttonElements[1].Position + new Vector2f(0, Height));
            }));

            buttons.Add(new Button("help", () =>
            {
                ContextMenuEntity.Main.MinimumWidth = buttonElements[2].Size.X;
                ContextMenuEntity.Open(new[] {
                    new Button("about", default, false),
                    new Button("open console", () => { ConsoleEntity.Main.ConsoleIsOpen = true; }),
                },
                    buttonElements[2].Position + new Vector2f(0, Height));
            }));
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
                FillColor = Style.Background
            };

            backgroundElement.Position = default;
            backgroundElement.Depth = 0;
            backgroundElement.IsScreenSpace = true;
            backgroundElement.Size = background.Size;

            backgroundTask = new ShapeRenderTask(background, 0);

            buttonBackground = new RectangleShape();

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
                    Depth = 0,
                    IsScreenSpace = true,
                    Color = Color.Transparent,
                    ActiveColor = new Color(255, 255, 255, 10),
                    HoverColor = new Color(255, 255, 255, 25),
                    Position = background.Position + new Vector2f(buttonPos, 0),
                    DepthContainer = backgroundElement,
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
                }, elem.Depth);
            }
        }

        public override IEnumerable<IRenderTask> RenderScreen()
        {
            buttonPos = 0;
            yield return backgroundTask;
            foreach (var task in textTasks)
                yield return task;
        }

        public override void Destroyed()
        {
            uiController.Deregister(backgroundElement);
            foreach (var item in buttonElements)
                uiController.Deregister(item);
        }
    }
}
