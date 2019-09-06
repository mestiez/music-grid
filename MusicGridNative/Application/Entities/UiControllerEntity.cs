using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicGridNative
{
    public class UiControllerEntity : Entity
    {
        public List<UiElement> elements = new List<UiElement>();

        public UiElement FocusedElement { get; set; }

        public void Register(UiElement element)
        {
            elements.Add(element);

            element.Controller = this;
            element.OnDepthChanged += ReSortElements;
        }

        public void Deregister(UiElement element)
        {
            elements.Remove(element);
            element.Controller = null;

            element.OnDepthChanged -= ReSortElements;
        }

        private void ReSortElements(object sender, EventArgs e)
        {
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
