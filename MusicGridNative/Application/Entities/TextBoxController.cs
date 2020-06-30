using Shared;
using System;

namespace MusicGrid
{
    public class TextBoxController
    {
        public DrawableElement DrawableElement { get; }
        public event EventHandler<string> OnInput;
        public string Value
        {
            get => DrawableElement.Text;
            set => DrawableElement.Text = value;
        }

        public TextBoxController(DrawableElement element)
        {
            DrawableElement = element;
            DrawableElement.Element.Selectable = true;
            DrawableElement.Element.SelectInSelectAll = false;
            DrawableElement.Element.OnSelect += Element_OnSelect;
            DrawableElement.Element.OnDeselect += Element_OnDeselect;
        }

        private void Element_OnDeselect(object sender, SelectionEventArgs e)
        {
            DrawableElement.Element.Controller.NotifyInputEnd();
        }

        private void Element_OnSelect(object sender, SelectionEventArgs e)
        {
            DrawableElement.Element.Controller.NotifyInputStart();
        }

        public void Update()
        {
            if (!DrawableElement.Element.IsSelected) return;

            if (Input.IsKeyReleased(OpenTK.Input.Key.Enter))
                DrawableElement.Element.Controller.Deselect(DrawableElement.Element);
            else if (!Input.IsKeyPressed(OpenTK.Input.Key.Escape))
            {
                string previous = Value;
                foreach (var c in Input.TextEntered)
                {
                    switch (c)
                    {
                        case '\b': //backspace
                            Value = Value.Substring(0, Math.Max(0, Value.Length - 1));
                            break;
                        case (char)127: //ctrl backspace
                            Value = Value.Substring(0, Math.Max(0, Value.LastIndexOf(' ')));
                            break;
                        default:
                            Value += c.ToString();
                            break;
                    }
                }
                if (previous != Value)
                    OnInput?.Invoke(this, Value);
            }
        }
    }
}
