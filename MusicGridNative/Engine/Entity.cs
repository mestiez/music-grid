using SFML.Graphics;
using System.Collections;
using System.Collections.Generic;

namespace MusicGrid
{
    public abstract class Entity
    {
        private bool isManagerSet;
        private World world;

        public World World
        {
            get => world;

            set
            {
                if (isManagerSet) return;
                isManagerSet = true;
                world = value;
            }
        }

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
    }
}
