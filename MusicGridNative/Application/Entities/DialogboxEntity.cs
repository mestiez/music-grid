using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicGrid
{

    public class DialogboxEntity : Entity
    {
        public readonly string Content;
        public readonly bool CloseOnButtonPress;

        protected readonly Vector2f size;
        protected readonly Vector2f position;
        protected readonly List<Button> buttons;

        protected RectangleShape background;
        protected Text contentText;
        protected RectangleShape buttonBackground;
        protected Text buttonText;

        protected ShapeRenderTask backgroundTask;
        protected ShapeRenderTask contentTask;
        protected ActionRenderTask[] buttonTasks;

        protected UiElement backgroundElement;
        protected UiElement[] buttonElements;

        protected UiControllerEntity uiController;
        protected DraggableController draggable;
        protected float buttonPosition = 0;
        protected float buttonSpacing = 0;

        public const uint CharacterSize = 16;
        public const float ButtonMargin = 16;
        public const float HorizontalButtonPadding = 20;
        public const float ButtonHeight = CharacterSize * 2;

        protected bool requiresRecalculation = true;

        public IReadOnlyList<Button> Buttons => buttons.AsReadOnly();

        public DialogboxEntity(string content, Vector2f size = default, Vector2f position = default, bool closeOnButtonPress = true, IEnumerable<Button> buttons = null)
        {
            CloseOnButtonPress = closeOnButtonPress;
            Content = content;
            this.size = size;
            this.position = position;
            Name = content + " dialog";

            if (buttons == null)
                buttons = new[] {
                    new Button("OK", ()=>{ World.Destroy(this); })
                };
            this.buttons = new List<Button>(buttons);
        }

        public override void Created()
        {
            uiController = World.GetEntityByType<UiControllerEntity>();

            background = new RectangleShape()
            {
                OutlineColor = Style.BackgroundHover.ToSFML(),
                OutlineThickness = 1
            };
            contentText = new Text(Content, MusicGridApplication.Assets.DefaultFont)
            {
                FillColor = Style.Foreground.ToSFML(),
                CharacterSize = CharacterSize,
            };
            buttonBackground = new RectangleShape();
            buttonText = new Text("invalid!", MusicGridApplication.Assets.DefaultFont)
            {
                FillColor = Style.Foreground.ToSFML(),
                Position = default,
                CharacterSize = CharacterSize
            };
            backgroundElement = new UiElement()
            {
                Selectable = false,
                Color = Style.Background.ToSFML(),
                ActiveColor = Style.Background.ToSFML(),
                HoverColor = Style.Background.ToSFML(),
                DisabledColor = Style.Background.ToSFML(),
                IsScreenSpace = true,
                Depth = 0,
                Position = position,
                Size = size,
            };

            draggable = new DraggableController(backgroundElement);

            backgroundTask = new ShapeRenderTask(background, backgroundElement.Depth);
            contentTask = new ShapeRenderTask(contentText, backgroundElement.Depth);

            uiController.Register(backgroundElement);
            GenerateButtons();
        }

        protected void GenerateButtons()
        {
            buttonTasks = new ActionRenderTask[buttons.Count];
            buttonElements = new UiElement[buttons.Count];

            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];
                var elem = new UiElement()
                {
                    Selectable = false,
                    Color = Style.Background.ToSFML(),
                    ActiveColor = Style.BackgroundActive.ToSFML(),
                    HoverColor = Style.BackgroundHover.ToSFML(),
                    DisabledColor = Style.BackgroundDisabled.ToSFML(),
                    Disabled = !button.Interactive,
                    IsScreenSpace = true,
                    DepthContainer = backgroundElement
                };

                elem.OnMouseDown += (o, e) =>
                {
                    if (e.Button != (int)Mouse.Button.Left) return;
                    button.Action?.Invoke();
                    if (CloseOnButtonPress)
                        World.Destroy(this);
                };

                buttonTasks[i] = new ActionRenderTask((target) =>
                {
                    SetupButton(button, elem);
                    target.Draw(buttonBackground);
                    target.Draw(buttonText);

                }, backgroundElement.Depth);

                SetupButton(button, elem);

                buttonElements[i] = elem;
                uiController.Register(elem);
            }
        }

        protected void SetupButton(Button button, UiElement elem)
        {
            buttonText.DisplayedString = button.Label;
            buttonText.FillColor = elem.Disabled ? Style.BackgroundDisabled.ToSFML() : Style.Foreground.ToSFML();

            var localSize = buttonText.GetLocalBounds();
            if (buttons.Count < 2)
            {
                elem.Size = new Vector2f(backgroundElement.Size.X - HorizontalButtonPadding * 2, ButtonHeight);
                elem.Position = backgroundElement.Position + new Vector2f(ButtonMargin, backgroundElement.Size.Y - ButtonMargin - ButtonHeight);
            }
            else
            {
                elem.Size = new Vector2f(localSize.Width + HorizontalButtonPadding, ButtonHeight);
                elem.Position = backgroundElement.Position + new Vector2f(buttonPosition, backgroundElement.Size.Y - ButtonMargin - ButtonHeight);
                buttonPosition += elem.Size.X + buttonSpacing;
            }

            buttonText.Position = elem.Position + (elem.Size / 2) - new Vector2f(localSize.Width / 2, localSize.Height / 1.5f);
            buttonText.Position = new Vector2f((int)buttonText.Position.X, (int)buttonText.Position.Y);
            buttonBackground.Position = elem.Position;
            buttonBackground.Size = elem.Size;
            buttonBackground.FillColor = elem.ComputedColor;
        }

        public override void Update()
        {
            draggable.Update();
            requiresRecalculation |= draggable.HasMoved;
        }

        protected void RecalculateLayout()
        {
            if (!requiresRecalculation) return;
            requiresRecalculation = false;

            backgroundTask.Depth = backgroundElement.Depth;
            backgroundTask.Drawable = background;

            contentTask.Depth = backgroundElement.Depth;
            contentText.Position = backgroundElement.Position;

            background.Position = backgroundElement.Position;
            background.Size = backgroundElement.Size;
            background.FillColor = backgroundElement.ComputedColor;

            var contentTextBounds = contentText.GetLocalBounds();

            contentText.Position = backgroundElement.Position + backgroundElement.Size / 2f - new Vector2f(contentTextBounds.Width / 2, contentTextBounds.Height / 2 + (ButtonMargin / 2 + ButtonHeight / 2));
            contentText.Position = new Vector2f((int)contentText.Position.X, (int)contentText.Position.Y);
            if (buttons.Count > 1)
                buttonSpacing = Utilities.CalculateEvenSpaceGap(backgroundElement.Size.X, buttonElements.Sum(b => b.Size.X), buttonElements.Length, ButtonMargin);
        }

        public override void PreRender()
        {
            RecalculateLayout();
        }

        public override IEnumerable<IRenderTask> RenderScreen()
        {
            buttonPosition = ButtonMargin;
            yield return backgroundTask;
            yield return contentTask;
            foreach (var buttonTask in buttonTasks)
                yield return buttonTask;
        }

        public override void Destroyed()
        {
            foreach (var buttonElement in buttonElements)
                uiController.Deregister(buttonElement);
            uiController.Deregister(backgroundElement);
        }

        public static DialogboxEntity CreateConfirmationDialog(string title, Action action)
        {
            return new DialogboxEntity(title,
                size: new Vector2f(320, 150),
                position: (Vector2f)Input.WindowSize / 2 - new Vector2f(320, 150) / 2,
                buttons: new[] {
                new Button("Yes",action),
                new Button("No", () => {}),
                });
        }
    }
}
