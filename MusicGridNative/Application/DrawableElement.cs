using OpenTK.Graphics.ES20;
using SFML.Graphics;
using SFML.System;
using Shared;
using Color = SFML.Graphics.Color;

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
        private TextAlignmentMode textAlignment;
        private Vector2f encapsulationMargin = new Vector2f(20, 15);

        private bool registered;
        private float textScale = 1;
        private Vector2f alignmentPadding;

        public DrawableElement(UiControllerEntity controller, Vector2f size = default, Vector2f position = default, int depth = 0, uint characterSize = 16)
        {
            this.controller = controller;

            Element = new UiElement();
            background = new RectangleShape();
            text = new Text("", MusicGridApplication.Assets.DefaultFont);

            finalTask = new ActionRenderTask(null, depth);
            finalTask.Action = Render;

            Element.Color = new Color(Style.Background);
            Element.HoverColor = new Color(Style.BackgroundHover);
            Element.ActiveColor = new Color(Style.BackgroundActive);
            Element.DisabledColor = new Color(Style.BackgroundDisabled);
            Element.SelectedColor = new Color(Style.BackgroundHover);

            Depth = depth;
            CharacterSize = characterSize;
            Size = size;
            Position = position;
            TextColor = new Color(Style.Foreground);

            Register();
        }

        public void Register()
        {
            if (registered)
            {
                ConsoleEntity.Log("Attempt to register registered object", this);
                return;
            }
            controller.Register(Element);
            registered = true;
        }

        public void Deregister()
        {
            if (!registered)
            {
                ConsoleEntity.Log("Attempt to deregister unregistered object", this);
                return;
            }
            controller.Deregister(Element);
            registered = false;
        }

        private void Render(RenderTarget target)
        {
            if (DrawBackground)
            {
                background.FillColor = Element.ComputedColor;
                target.Draw(background);
            }
            if (!string.IsNullOrWhiteSpace(displayString))
            {
                if (HideOverflow)
                {
                    int x, y, w, h;

                    if (Element.IsScreenSpace)
                    {
                        x = (int)Position.X;
                        y = ((int)target.Size.Y - (int)(Position.Y + Size.Y));
                        w = (int)Size.X;
                        h = (int)Size.Y;
                    }
                    else
                    {
                        var sPos = target.MapCoordsToPixel(Position);
                        var sSize = target.MapCoordsToPixel(Position + Size) - sPos;

                        x = (int)sPos.X;
                        y = ((int)target.Size.Y - (int)(sPos.Y + sSize.Y));
                        w = (int)sSize.X;
                        h = (int)sSize.Y;
                    }

                    GL.Scissor(x, y, w, h);
                    GL.Enable(EnableCap.ScissorTest);
                    target.Draw(text);
                    GL.Disable(EnableCap.ScissorTest);
                }
                else
                    target.Draw(text);
            }
        }

        public void ForceRecalculateLayout()
        {
            if (string.IsNullOrWhiteSpace(Text)) return;
            if (!EncapsulateText && !IsCenteringText)
            {
                ConsoleEntity.Log("Recalculation of layout is unwarranted!", this);
                return;
            }
            var localBounds = text.GetLocalBounds();
            if (EncapsulateText)
            {
                var newSize = new Vector2f(EncapsulationPadding.X * 2 + localBounds.Width * TextScale, EncapsulationPadding.Y * 2 + localBounds.Height * TextScale);
                size = newSize;
                Element.Size = size;
                background.Size = size;
            }

            Vector2f pos = Element.Position;

            //should use flag enum haha :) Get your shit together.
            switch (TextAlignment)
            {
                case TextAlignmentMode.Horizontally:
                    pos.X += Element.Size.X / 2 - localBounds.Width * TextScale / 2;
                    break;
                case TextAlignmentMode.Vertically:
                    pos.Y += Element.Size.Y / 2 - localBounds.Height * TextScale / 2;
                    break;
                case TextAlignmentMode.Both:
                    pos += new Vector2f(Element.Size.X / 2 - localBounds.Width * TextScale / 2, Element.Size.Y / 2 - localBounds.Height * TextScale / 2);
                    break;
            }

            text.Position = Utilities.Round(pos) + TextOffset;
        }

        #region Properties
        public bool HideOverflow { get; set; }
        public bool DrawBackground { get; set; } = true;

        public Texture Texture
        {
            get => background.Texture;
            set => background.Texture = value;
        }

        public bool EncapsulateText
        {
            get => encapsulateText;
            set
            {
                encapsulateText = value;
                ForceRecalculateLayout();
            }
        }

        public float TextScale
        {
            get => textScale;
            set
            {
                textScale = value;
                text.Scale = new Vector2f(value, value);
            }
        }

        public Vector2f EncapsulationPadding
        {
            get => encapsulationMargin;
            set
            {
                if (encapsulationMargin == value) return;
                encapsulationMargin = value;
                ForceRecalculateLayout();
            }
        }

        public Vector2f TextOffset
        {
            get => alignmentPadding;
            set
            {
                if (alignmentPadding == value) return;
                alignmentPadding = value;
                ForceRecalculateLayout();
            }
        }

        public TextAlignmentMode TextAlignment
        {
            get => textAlignment;
            set
            {
                if (textAlignment == value) return;
                textAlignment = value;
                ForceRecalculateLayout();
            }
        }

        public bool IsCenteringText => TextAlignment != TextAlignmentMode.None;

        public int Depth
        {
            get => depth;
            set
            {
                if (depth == value) return;
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
                if (characterSize == value) return;
                characterSize = value;
                text.CharacterSize = characterSize;
                if (IsCenteringText || EncapsulateText)
                    ForceRecalculateLayout();
            }
        }

        public string Text
        {
            get => displayString;
            set
            {
                if (displayString == value) return;
                displayString = value ?? "";
                text.DisplayedString = displayString;
                if (IsCenteringText || EncapsulateText)
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
                if (IsCenteringText || EncapsulateText) return;
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
                    ConsoleEntity.Log("Can't set size when it's controlled by the element", this);
                    return;
                }
                size = value;
                Element.Size = size;
                background.Size = size;
                if (IsCenteringText)
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

        public enum TextAlignmentMode
        {
            None,
            Horizontally,
            Vertically,
            Both
        }
    }
}
