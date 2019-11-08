using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MusicGrid
{
    public class World
    {
        private bool dirtyList;

        private readonly List<Entity> entities = new List<Entity>();

        private readonly Dictionary<Entity, int> createBuffer = new Dictionary<Entity, int>();
        private readonly List<Entity> destroyBuffer = new List<Entity>();
        private bool startingEntitiesInitialised = false;

        public ILuaConsole Lua = new LuaConsole();
        public RenderTarget RenderTarget { get; set; }
        public Color ClearColor { get; set; }

        public World(RenderTarget target)
        {
            RenderTarget = target;

            Lua.LinkFunction("get_entities", this, () => entities.AsReadOnly());
            Lua.LinkFunction("get_entity", this, new Func<string, Entity>((s) => { return GetEntityByName(s); }).Method);
            Lua.LinkFunction("destroy", this, (Entity e) => Destroy(e));
            Lua.LinkFunction("destroy_all_entities", this, () =>
            {
                foreach (var entity in entities)
                    Destroy(entity);
            });
        }

        private void ExecuteAutoLuaFile()
        {
            var dir = Properties.Resources.ScriptsFolder;
            if (!Directory.Exists(dir))
            {
                ConsoleEntity.Log($"Can't find scripts folder at {dir}", "WORLD");
                return;
            }
            var files = Directory.GetFiles(dir, "*.lua");
            ConsoleEntity.Log($"Found {files.Length} {(files.Length == 1 ? "script" : "scripts")}", "WORLD");
            foreach (var path in files)
            {
                try
                {
                    Lua.Execute(File.ReadAllText(path));
                }
                catch (Exception e)
                {
                    ConsoleEntity.Log($"Error executing {path}: {e.Message}", "WORLD");
                }
            }
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
                if (entity.Active)
                    entity.PreUpdate();

            foreach (var entity in entities)
                if (entity.Active)
                    entity.Update();

            foreach (var entity in entities)
                if (entity.Active)
                    entity.PostUpdate();

            if (Input.WindowHasFocus)
            {
                foreach (var entity in entities)
                    if (entity.Visible)
                        entity.PreRender();

                SortAndRender();

                foreach (var entity in entities)
                    if (entity.Visible)
                        entity.PostRender();
            }
        }

        private void SortAndRender()
        {
            var tasks = new List<IRenderTask>();
            var screenSpaceTasks = new List<IRenderTask>();
            foreach (Entity entity in entities)
            {
                if (!entity.Visible) continue;
                tasks.AddRange(entity.Render());
                screenSpaceTasks.AddRange(entity.RenderScreen());
            }

            foreach (var task in tasks.OrderByDescending(t =>
            {
                if (t == null)
                {
                    ConsoleEntity.Log("Attempt to draw null task", "WORLD");
                    return 0;
                }
                return t.Depth;
            }))
                task?.Render(RenderTarget);

            var oldView = new View(RenderTarget.GetView());
            var floatSize = (Vector2f)RenderTarget.Size;
            RenderTarget.SetView(new View(floatSize / 2, floatSize));

            foreach (var task in screenSpaceTasks.OrderByDescending(t =>
            {
                if (t == null)
                {
                    ConsoleEntity.Log("Attempt to draw null screen task", "WORLD");
                    return 0;
                }
                return t.Depth;
            }))
                task?.Render(RenderTarget);

            RenderTarget.SetView(oldView);
        }

        private void HandleDestructionBuffer()
        {
            foreach (var entity in destroyBuffer)
                entity?.Destroyed();

            foreach (var entity in destroyBuffer)
            {
                if (entity == null) continue;
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
                entities.Insert(Utilities.Clamp(entityOrderPair.Value, 0, entities.Count-1), entityOrderPair.Key);

            foreach (var entityOrderPair in createBuffer)
                entityOrderPair.Key.Initialised();

            createBuffer.Clear();

            if (!startingEntitiesInitialised)
            {
                ExecuteAutoLuaFile();
                startingEntitiesInitialised = true;
            }
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

        public Entity GetEntityByName(string name)
        {
            foreach (Entity entity in entities)
                if (entity.Name == name) return entity;

            foreach (Entity entity in createBuffer.Keys)
                if (entity.Name == name) return entity;

            return null;
        }

        public T[] GetEntitiesByType<T>() where T : Entity => entities.OfType<T>().Concat(createBuffer.OfType<T>()).ToArray();
    }
}
