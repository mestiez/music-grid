using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicGrid
{
    public class UiControllerEntity : Entity
    {
        private List<UiElement> elements = new List<UiElement>();
        private bool requiresElementResort = false;

        private List<UiElement> registerBuffer = new List<UiElement>();
        private List<UiElement> deregisterBuffer = new List<UiElement>();

        private HashSet<UiElement> selected = new HashSet<UiElement>();
        public IReadOnlyList<UiElement> Selected => selected.ToList().AsReadOnly();
        public bool Multiselecting { get; private set; } = false;

        public UiElement FocusedElement { get; set; }

        public void Register(UiElement element)
        {
            registerBuffer.Add(element);
            requiresElementResort = true;
        }

        public void Deregister(UiElement element)
        {
            deregisterBuffer.Add(element);
            requiresElementResort = true;
        }

        private void HandleBuffers()
        {
            foreach (var elem in deregisterBuffer)
            {
                elements.Remove(elem);
                registerBuffer.Remove(elem);
                if (FocusedElement == elem)
                    FocusedElement = null;
                elem.Controller = null;
                elem.OnDepthChanged -= OnDepthChange;
            }

            foreach (var elem in registerBuffer)
            {
                elements.Add(elem);
                elem.Controller = this;
                elem.OnDepthChanged += OnDepthChange;
                FocusedElement = null;
                elem.EvaluateInteraction(default);
            }

            deregisterBuffer.Clear();
            registerBuffer.Clear();
        }

        private void OnDepthChange(object obj, EventArgs e)
        {
            requiresElementResort = true;
        }

        private void ReSortElements()
        {
            requiresElementResort = false;
            HandleBuffers();
            var ordered = new List<UiElement>();
            foreach (var element in elements.Where(elem => elem.DepthContainer == null).OrderBy(elem => elem.Depth))
            {
                foreach (var child in elements.Where(elem => elem.DepthContainer == element).OrderBy(elem => elem.Depth))
                    ordered.Add(child);
                ordered.Add(element);
            }
            elements = ordered;
        }

        public void HandleSelection(UiElement elem)
        {
            if (Multiselecting)
            {
                if (elem.IsSelected)
                    Deselect(elem);
                else
                    Select(elem, true);
            }
            else
                Select(elem, false);
        }

        public void Select(UiElement element, bool multiselect = false)
        {
            if (!element.Selectable) return;
            if (!multiselect)
                ClearSelection();
            element.IsSelected = true;
            selected.Add(element);
            element.ComputeColors();
        }

        public void Deselect(UiElement element)
        {
            if (!element.Selectable) return;
            if (!selected.Contains(element)) return;
            element.IsSelected = false;
            selected.Remove(element);
            element.ComputeColors();
        }

        public void ClearSelection()
        {
            foreach (var elem in selected)
            {
                elem.IsSelected = false;
                elem.ComputeColors();
            }
            selected.Clear();
        }

        public override void Update()
        {
            if (!Input.WindowHasFocus) return;
            if (ConsoleEntity.Main.ConsoleIsOpen) return;

            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.A))
            {
                if (selected.Count == 0)
                {
                    ClearSelection();
                    foreach (var item in elements)
                        Select(item, true);
                }
                else
                    ClearSelection();
            }

            Multiselecting = Input.IsKeyHeld(SFML.Window.Keyboard.Key.LShift) || Input.IsKeyHeld(SFML.Window.Keyboard.Key.LControl);
            if (requiresElementResort)
                ReSortElements();

            var info = new InteractionInfo
            {
                Pressed = Input.PressedButton,
                Held = Input.HeldButton,
                Released = Input.ReleasedButton
            };

            if (FocusedElement != null)
                info.FirstServed = FocusedElement.EvaluateInteraction(info);
            else
                foreach (UiElement element in elements)
                    if (element.EvaluateInteraction(info))
                        info.FirstServed = true;

            if (!info.FirstServed && (
                info.Held.HasValue && info.Held.Value != Mouse.Button.Middle ||
                info.Pressed.HasValue && info.Pressed.Value != Mouse.Button.Middle ||
                info.Released.HasValue && info.Released.Value != Mouse.Button.Middle
                ))
                ClearSelection();
        }
    }
}
