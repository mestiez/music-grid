﻿using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicGridNative
{
    public class World
    {
        private bool dirtyList;

        private readonly List<Entity> entities = new List<Entity>();

        private readonly Dictionary<Entity, int> createBuffer = new Dictionary<Entity, int>();
        private readonly List<Entity> destroyBuffer = new List<Entity>();

        public RenderTarget RenderTarget { get; set; }
        public Color ClearColor { get; set; }

        public World(RenderTarget target)
        {
            RenderTarget = target;
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

        public T GetEntityByType<T>() where T : Entity
        {
            foreach (Entity entity in entities)
                if (entity is T typed) return typed;

            foreach (var entity in createBuffer)
                if (entity.Key is T typed) return typed;

            return null;
        }
    }
}
