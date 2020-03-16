using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using Shared;
using Color = SFML.Graphics.Color;

namespace MusicGrid
{
    public class ContextMenuEntity : Entity
    {
        public static ContextMenuEntity Main { get; private set; }

        public float ButtonHeight { get; set; } = 24;
        public float ButtonPadding { get; set; } = 8;
        public float MinimumWidth { get; set; } = 150;
        public int ButtonHash { get; private set; }

        private UiElement[] relevantElements = { };

        private readonly List<Button> buttons = new List<Button>();
        private UiElement[] elements;
        private ActionRenderTask[] renderTasks;
        private float computedMaxWidth;
        private Vector2f position;
        private static readonly uint CharacterSize = 16;

        private Text text;
        private RectangleShape background;

        private UiControllerEntity uiController;

        public static void Open(IEnumerable<Button> buttons, Vector2f position, params UiElement[] parents)
        {
            Main.buttons.Clear();
            Main.buttons.AddRange(buttons);

            Main.GenerateButtons();
            position = ClampToScreen(position);
            Main.position = position;

            Main.GenerateButtons(); //second time for post-clamping :)
            Main.Active = true;
            Main.Visible = true;
            Main.ButtonHash = GenerateHash(buttons);

            Main.relevantElements = Main.elements.Concat(parents ?? new UiElement[0]).ToArray();
        }

        public static void ToggleOpen(IEnumerable<Button> buttons, Vector2f position, params UiElement[] parents)
        {
            if (Main.Visible && Main.ButtonHash == GenerateHash(buttons))
                Close();
            else
                Open(buttons, position, parents);
        }

        private static int GenerateHash(IEnumerable<Button> buttons) => string.Join("", buttons.Select(c => c.GetHashCode())).GetHashCode();

        private static Vector2f ClampToScreen(Vector2f position)
        {
            var maxPos = new Vector2f(Input.WindowSize.X - Main.computedMaxWidth, Input.WindowSize.Y - Main.ButtonHeight * Main.buttons.Count());
            position.X = Math.Min(maxPos.X, position.X);
            position.Y = Math.Min(maxPos.Y, position.Y);
            return position;
        }

        public static void Close()
        {
            foreach (var element in Main.elements)
                Main.uiController.Deregister(element);

            Main.Active = false;
            Main.Visible = false;
            Main.ButtonHash = 0;
        }

        public override void Created()
        {
            Active = false;
            Visible = false;

            elements = new UiElement[0];
            renderTasks = new ActionRenderTask[0];

            text = new Text("...", MusicGridApplication.Assets.DefaultFont)
            {
                FillColor = Color.White,
                Position = default,
                CharacterSize = CharacterSize
            };

            uiController = World.GetEntityByType<UiControllerEntity>();
            Main = this;
        }

        public override void Update()
        {
            if (Input.IsAnyButtonPressed)
                if (!relevantElements.Any(e => e.IsUnderMouse))
                    Close();
        }

        public override IEnumerable<IRenderTask> RenderScreen()
        {
            foreach (var task in renderTasks)
                yield return task;
        }

        private void GenerateButtons()
        {
            foreach (var element in elements)
                uiController.Deregister(element);

            elements = new UiElement[buttons.Count];
            renderTasks = new ActionRenderTask[buttons.Count];

            background = new RectangleShape();

            float halfCharSize = CharacterSize * .5f;
            computedMaxWidth = MinimumWidth;

            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];

                var elem = new UiElement()
                {
                    Color = Style.Background.ToSFML(),
                    ActiveColor = Style.BackgroundActive.ToSFML(),
                    HoverColor = Style.BackgroundHover.ToSFML(),
                    DisabledColor = Style.BackgroundDisabled.ToSFML(),
                    Disabled = !button.Interactive,
                    Depth = -1,
                    IsScreenSpace = true,
                    Position = position + new Vector2f(0, i * ButtonHeight),
                };

                elem.OnMouseDown += (o, e) => { Close(); button.Action(); };

                renderTasks[i] = new ActionRenderTask((target) =>
                {
                    elem.Size = new Vector2f(computedMaxWidth, ButtonHeight);

                    text.DisplayedString = button.Label;
                    text.Position = elem.Position + new Vector2f(ButtonPadding, 0);
                    text.FillColor = button.Interactive ? Style.Foreground.ToSFML() : Style.ForegroundDisabled.ToSFML();

                    background.Position = elem.Position;
                    background.Size = elem.Size;
                    background.FillColor = elem.ComputedColor;

                    target.Draw(background);
                    target.Draw(text);
                }, elem.Depth);

                computedMaxWidth = Math.Max(ButtonPadding * 2 + button.Label.Length * halfCharSize, computedMaxWidth);
                uiController.Register(elem);
                elements[i] = elem;
            }
        }

        public override void Destroyed()
        {
            foreach (var item in elements)
                uiController.Deregister(item);
        }
    }
}
