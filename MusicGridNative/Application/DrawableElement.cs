using SFML.Graphics;
using SFML.System;

namespace MusicGrid
{
    public struct DrawableElement
    {
        public UiElement Element { get; }

        private ActionRenderTask finalTask;

        private readonly RectangleShape background;
        private readonly Text text;

        private readonly UiControllerEntity controller;

        private int depth;
        private uint characterSize;
        private string displayString;
        private Vector2f size;
        private Vector2f position;
        private Color textColor;

        private bool registered;
        private const string ConsoleSourceIdentifier = "DRAWABLE ELEMENT";

        public DrawableElement(UiControllerEntity controller, Vector2f size, Vector2f position, int depth = 0, uint characterSize = 12)
        {
            this.controller = controller;
            this.depth = depth;
            this.characterSize = characterSize;
            this.displayString = null;
            this.size = size;
            this.position = position;
            this.textColor = Style.Foreground;

            this.registered = false;

            Element = new UiElement();
            background = new RectangleShape();
            text = new Text();

            finalTask = new ActionRenderTask(null, depth);
            finalTask.Action = Render;

            Element.Color = Style.Background;
            Element.HoverColor = Style.BackgroundHover;
            Element.ActiveColor = Style.BackgroundActive;
            Element.DisabledColor = Style.BackgroundDisabled;
            Element.SelectedColor = Style.BackgroundHover;

            Position = position;
            Size = size;
            Depth = depth;
            CharacterSize = characterSize;

            Register();
        }

        public void Register()
        {
            if (registered)
            {
                ConsoleEntity.Log("Attempt to register registered object", ConsoleSourceIdentifier);
                return;
            }
            controller.Register(Element);
            registered = true;
        }

        public void Deregister()
        {
            if (!registered)
            {
                ConsoleEntity.Log("Attempt to deregister unregistered object", ConsoleSourceIdentifier);
                return;
            }
            controller.Deregister(Element);
            registered = false;
        }

        private void Render(RenderTarget target)
        {
            background.FillColor = Element.ComputedColor;

            target.Draw(background);
            if (!string.IsNullOrWhiteSpace(displayString))
                target.Draw(text);
        }

        #region Properties
        public int Depth
        {
            get => depth;
            set
            {
                depth = value;
                finalTask.Depth = depth;
                Element.Depth = depth;
            }
        }

        public uint CharacterSize
        {
            get => characterSize;
            set
            {
                characterSize = value;
                text.CharacterSize = characterSize;
            }
        }

        public string DisplayString
        {
            get => displayString;
            set
            {
                displayString = value;
                text.DisplayedString = displayString;
            }
        }

        public UiElement DepthContainer
        {
            get => Element.DepthContainer;
            set => Element.DepthContainer = value;
        }

        public IRenderTask RenderTask => finalTask;

        public Vector2f Position
        {
            get => position;
            set
            {
                position = value;
                Element.Position = position;
                background.Position = position;
                text.Position = position;
            }
        }

        public Vector2f Size
        {
            get => size;
            set
            {
                size = value;
                Element.Size = size;
                background.Size = size;
            }
        }

        public Color TextColor
        {
            get => textColor;
            set
            {
                textColor = value;
                text.FillColor = textColor;
            }
        }
        #endregion
    }
}
