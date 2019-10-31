using System;
using System.Collections.Generic;

namespace MusicGrid
{
    public abstract class Entity
    {
        private bool isWorldSet;
        private World world;
        private string name;
        private bool nameHasBeenSet;

        public World World
        {
            get => world;

            set
            {
                if (isWorldSet) throw new InvalidOperationException("This entity already has a World assigned to it");
                isWorldSet = true;
                world = value;
            }
        }

        public string Name
        {
            get { return nameHasBeenSet ? name : GetType().Name; }
            set
            {
                nameHasBeenSet = true;
                name = value;
            }
        }
        public bool Visible { get; set; } = true;
        public bool Active { get; set; } = true;

        public virtual void Created() { }
        public virtual void Initialised() { }
        public virtual void Destroyed() { }

        public virtual void PreUpdate() { }
        public virtual void Update() { }
        public virtual void PostUpdate() { }

        public virtual void PreRender() { }
        public virtual IEnumerable<IRenderTask> Render() { yield break; }
        public virtual IEnumerable<IRenderTask> RenderScreen() { yield break; }
        public virtual void PostRender() { }

        public override string ToString() => Name;
    }
}
