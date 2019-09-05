using SFML.Graphics;
using System.Collections.Generic;

namespace MusicGridNative
{
    public class World
    { 
        private bool dirtyList;

        private List<Entity> entities = new List<Entity>();

        private List<Entity> createBuffer = new List<Entity>();
        private List<Entity> destroyBuffer = new List<Entity>();

        public RenderTarget RenderTarget;

        public World(RenderTarget target)
        {
            this.RenderTarget = target;
        }

        public void Step()
        {
            if (dirtyList)
            {
                dirtyList = false;
                HandleCreationBuffer();
                HandleDestructionBuffer();
            }

            foreach (var entity in entities)
                entity.PreUpdate();

            foreach (var entity in entities)
                entity.Update();

            foreach (var entity in entities)
                entity.PostUpdate();


            foreach (var entity in entities)
                entity.PreRender();

            foreach (var entity in entities)
                entity.Render();

            foreach (var entity in entities)
                entity.PostRender();
        }

        private void HandleDestructionBuffer()
        {
            foreach (var entity in destroyBuffer)
                entity.Destroyed();

            foreach (var entity in destroyBuffer)
            {
                createBuffer.Remove(entity);
                entities.Remove(entity);
            }

            destroyBuffer.Clear();
        }

        private void HandleCreationBuffer()
        {
            foreach (var entity in createBuffer)
                entity.Created();

            foreach (var entity in createBuffer)
                entities.Add(entity);

            foreach (var entity in createBuffer)
                entity.Initialised();

            createBuffer.Clear();
        }

        public void Destroy(Entity entity)
        {
            dirtyList = true;
            destroyBuffer.Add(entity);
        }

        public void Add(Entity entity)
        {
            dirtyList = true;
            entity.World = this;
            createBuffer.Add(entity);
        }
    }
}
