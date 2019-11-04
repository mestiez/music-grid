using SFML.Graphics;
using SFML.System;

namespace MusicGrid
{
    public class DrawableElement
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
        private bool encapsulateText;
        private bool centerText;
        private Vector2f encapsulationMargin = new Vector2f(20,15);

        private bool registered;
        private const string ConsoleSourceIdentifier = "DRAWABLE ELEMENT";

        public DrawableElement(UiControllerEntity controller, Vector2f size, Vector2f position, int depth = 0, uint characterSize = 16)
        {
            this.controller = controller;

            Element = new UiElement();
            background = new RectangleShape();
            text = new Text("", MusicGridApplication.Assets.DefaultFont);

            finalTask = new ActionRenderTask(null, depth);
            finalTask.Action = Render;

            Element.Color = Style.Background;
            Element.HoverColor = Style.BackgroundHover;
            Element.ActiveColor = Style.BackgroundActive;
            Element.DisabledColor = Style.BackgroundDisabled;
            Element.SelectedColor = Style.BackgroundHover;

            Depth = depth;
            CharacterSize = characterSize;
            Size = size;
            Position = position;
            TextColor = Style.Foreground;

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

        public void ForceRecalculateLayout()
        {
            if (!EncapsulateText && !CenterText)
            {
                ConsoleEntity.Log("Recalculation of layout is unwarranted!", ConsoleSourceIdentifier);
                return;
            }

            var localBounds = text.GetLocalBounds();
            if (EncapsulateText)
            {
                var newSize = new Vector2f(EncapsulationMargin.X * 2 + localBounds.Width, EncapsulationMargin.Y * 2 + localBounds.Height);
                size = newSize;
                Element.Size = size;
                background.Size = size;
            }
            var pos = Element.Position + new Vector2f(Element.Size.X / 2 - localBounds.Width / 2, Element.Size.Y / 2 - localBounds.Height / 2);
            text.Position = Utilities.Round(pos);
        }

        #region Properties
        public bool EncapsulateText
        {
            get => encapsulateText;
            set
            {
                encapsulateText = value;
                ForceRecalculateLayout();
            }
        }

        public Vector2f EncapsulationMargin
        {
            get => encapsulationMargin;
            set
            {
                encapsulationMargin = value;
                ForceRecalculateLayout();
            }
        }

        public bool CenterText
        {
            get => centerText;
            set
            {
                centerText = value;
                ForceRecalculateLayout();
            }
        }

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
                if (CenterText || EncapsulateText)
                    ForceRecalculateLayout();
            }
        }

        public string DisplayString
        {
            get => displayString;
            set
            {
                displayString = value ?? "";
                text.DisplayedString = displayString;
                if (CenterText || EncapsulateText)
                    ForceRecalculateLayout();
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
                if (CenterText || EncapsulateText) return;
                text.Position = position;
            }
        }

        public Vector2f Size
        {
            get => size;
            set
            {
                if (EncapsulateText)
                {
                    ConsoleEntity.Log("Can't set size when it's controlled by the element", ConsoleSourceIdentifier);
                    return;
                }
                size = value;
                Element.Size = size;
                background.Size = size;
                if (CenterText)
                    ForceRecalculateLayout();
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
