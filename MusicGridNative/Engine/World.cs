﻿using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicGrid
{
    public class World
    {
        private bool dirtyList;

        private readonly List<Entity> entities = new List<Entity>();

        private readonly Dictionary<Entity, int> createBuffer = new Dictionary<Entity, int>();
        private readonly List<Entity> destroyBuffer = new List<Entity>();

        public ILuaConsole Lua = new LuaConsole();
        public RenderTarget RenderTarget { get; set; }
        public Color ClearColor { get; set; }

        public World(RenderTarget target)
        {
            RenderTarget = target;

            Lua.LinkFunction("get_entities", this, () => entities.AsReadOnly());
            Lua.LinkFunction("delete_all_entities", this, () =>
            {
                foreach (var entity in entities)
                    Destroy(entity);
            });
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

            if (Input.WindowHasFocus)
            {
                foreach (var entity in entities)
                    entity.PreRender();

                SortAndRender();

                foreach (var entity in entities)
                    entity.PostRender();
            }
        }

        private void SortAndRender()
        {
            var tasks = new List<IRenderTask>();
            var screenSpaceTasks = new List<IRenderTask>();
            foreach (Entity entity in entities)
            {
                tasks.AddRange(entity.Render());
                screenSpaceTasks.AddRange(entity.RenderScreen());
            }

            foreach (var task in tasks.OrderByDescending(t => t.Depth))
                task.Render(RenderTarget);

            var oldView = new View(RenderTarget.GetView());
            var floatSize = (Vector2f)RenderTarget.Size;
            RenderTarget.SetView(new View(floatSize / 2, floatSize));

            foreach (var task in screenSpaceTasks.OrderByDescending(t => t.Depth))
                task.Render(RenderTarget);

            RenderTarget.SetView(oldView);
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
            foreach (var entityOrderPair in createBuffer)
                entityOrderPair.Key.Created();

            foreach (var entityOrderPair in createBuffer)
                entities.Insert(entityOrderPair.Value, entityOrderPair.Key);

            foreach (var entityOrderPair in createBuffer)
                entityOrderPair.Key.Initialised();

            createBuffer.Clear();
        }

        public void Destroy(Entity entity)
        {
            dirtyList = true;
            destroyBuffer.Add(entity);
        }

        public void Add(Entity entity, int executionOrder = 0)
        {
            dirtyList = true;
            entity.World = this;
            createBuffer.Add(entity, executionOrder);
        }

        public IReadOnlyList<Entity> GetEntities()
        {
            return entities.AsReadOnly();
        }

        public T GetEntityByType<T>() where T : Entity
        {
            foreach (Entity entity in entities)
                if (entity is T typed) return typed;

            foreach (var entity in createBuffer)
                if (entity.Key is T typed) return typed;

            return null;
        }

        public T[] GetEntitiesByType<T>() where T : Entity => entities.OfType<T>().Concat(createBuffer.OfType<T>()).ToArray();
    }
}
