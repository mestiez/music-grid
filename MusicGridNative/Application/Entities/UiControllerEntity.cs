using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicGridNative
{
    public class UiControllerEntity : Entity
    {
        private List<UiElement> elements = new List<UiElement>();
        private bool requiresElementResort = false;

        public UiElement FocusedElement { get; set; }

        public void Register(UiElement element)
        {
            elements.Add(element);
            element.Controller = this;
            element.OnDepthChanged += OnDepthChange;
            requiresElementResort = true;
        }

        public void Deregister(UiElement element)
        {
            elements.Remove(element);

            if (FocusedElement == element)
                FocusedElement = null;

            element.Controller = null;
            element.OnDepthChanged -= OnDepthChange;
            requiresElementResort = true;
        }

        private void OnDepthChange(object obj, EventArgs e)
        {
            requiresElementResort = true;
        }

        private void ReSortElements()
        {
            requiresElementResort = true;
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
