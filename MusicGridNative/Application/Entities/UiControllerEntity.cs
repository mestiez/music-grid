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
                elem.EvaluateInteraction(false);
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

        public override void Update()
        {
            if (!Input.WindowHasFocus) return;
            if (requiresElementResort)
                ReSortElements();

            bool firstServed = false;
            if (FocusedElement != null)
                FocusedElement.EvaluateInteraction(false);
            else
                foreach (UiElement element in elements)
                {
                    if (element.EvaluateInteraction(firstServed))
                        firstServed = true;
                }
        }
    }
}
