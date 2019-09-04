using System.Collections.Generic;

namespace MusicGridNative
{
    public class World
    {
        private bool dirtyList;

        private List<Entity> entities = new List<Entity>();

        private List<Entity> createBuffer = new List<Entity>();
        private List<Entity> destroyBuffer = new List<Entity>();

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
        }

        private void HandleCreationBuffer()
        {
            foreach (var entity in createBuffer)
                entity.Created();

            foreach (var entity in createBuffer)
                entities.Add(entity);

            foreach (var entity in createBuffer)
                entity.Initialised();
        }

        public void Destroy(Entity entity)
        {
            destroyBuffer.Add(entity);
            dirtyList = true;
        }

        public void Add(Entity entity)
        {
            entity.World = this;
            createBuffer.Add(entity);
        }
    }
}
