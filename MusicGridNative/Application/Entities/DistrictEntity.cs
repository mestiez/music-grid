using SFML.Graphics;
using SFML.System;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MusicGrid
{
    public class DistrictEntity : Entity
    {
        public readonly District District;

        private Vertex[] resizeHandleVertices;
        private RectangleShape background;
        private Text title;

        private UiControllerEntity uiController;

        private readonly UiElement backgroundElement = new UiElement();
        private readonly UiElement resizeHandle = new UiElement();
        private readonly Dictionary<DistrictEntry, UiElement> entryElements = new Dictionary<DistrictEntry, UiElement>();
        private readonly Dictionary<DistrictEntry, ActionRenderTask> renderTasks = new Dictionary<DistrictEntry, ActionRenderTask>();

        private ShapeRenderTask backgroundTask;
        private ShapeRenderTask titleTask;
        private PrimitiveRenderTask entryTask;
        private PrimitiveRenderTask handleTask;

        private Vertex[] entryVertices;

        private Text entryText;
        private bool needToRecalculateLayout = true;

        private const uint CharacterSize = 72;
        private static int MinimumDepth = 100;
        private const float HandleSize = 16;
        private const float EntryMargin = 3;

        private static readonly Vector2f[] handleShape = new Vector2f[3]
            {
                new Vector2f(0, 0),
                new Vector2f(-HandleSize, 0),
                new Vector2f(0, -HandleSize)
            };

        public DistrictEntity(District district)
        {
            District = district;
        }

        public override void Created()
        {
            background = new RectangleShape
            {
                Position = new Vector2f(0, 0),
                Size = new Vector2f(256, 256),
                FillColor = new Color(125, 0, 15)
            };

            entryText = new Text("Entry", MusicGridApplication.Assets.DefaultFont)
            {
                FillColor = new Color(255, 255, 255, 255),
                Position = new Vector2f(0, 0),
                CharacterSize = CharacterSize
            };

            title = new Text("District", MusicGridApplication.Assets.DefaultFont)
            {
                FillColor = new Color(255, 255, 255, 125),
                Position = new Vector2f(0, 0),
                CharacterSize = CharacterSize
            };

            resizeHandleVertices = new Vertex[3]
            {
                new Vertex(handleShape[0], title.FillColor),
                new Vertex(handleShape[1], title.FillColor),
                new Vertex(handleShape[2], title.FillColor)
            };

            uiController = World.GetEntityByType<UiControllerEntity>();

            resizeHandle.DepthContainer = backgroundElement;

            resizeHandle.OnMouseDown += (o, e) => BringToFront();
            backgroundElement.OnMouseDown += (o, e) => BringToFront();

            uiController.Register(resizeHandle);
            uiController.Register(backgroundElement);

            backgroundTask = new ShapeRenderTask(background, backgroundElement.Depth);
            titleTask = new ShapeRenderTask(title, backgroundElement.Depth);
            entryTask = new PrimitiveRenderTask(entryVertices, PrimitiveType.Quads, backgroundElement.Depth);
            handleTask = new PrimitiveRenderTask(resizeHandleVertices, PrimitiveType.Triangles, backgroundElement.Depth);

            BringToFront();
        }

        private void BringToFront()
        {
            MinimumDepth--;
            backgroundElement.Depth = MinimumDepth;
        }

        private Color GetFrontColor(byte alpha = 125)
        {
            return Utilities.IsTooBright(District.Color) ? new Color(0, 0, 0, alpha) : new Color(255, 255, 255, alpha);
        }

        public override void Update()
        {
            if (entryElements.Count != District.Entries.Count)
                RegenerateEntryElements();

            HandleDragging();
            HandleResizing();

            SyncElement();
            SyncResizeHandle();

            CalculateLayout();
        }

        private void CalculateLayout()
        {
            if (!needToRecalculateLayout) return;
            needToRecalculateLayout = false;

            float preferredSize = 256;
            float scaledMargin = (float)Math.Min(EntryMargin, District.Size.X / 256);

            int rowItemCount = (int)Math.Floor(District.Size.X / preferredSize);//(int)Math.Ceiling(District.Size.X / (Math.Min(entryElements.Count, 10) * (preferredSize + EntryMargin) + EntryMargin)) ;
            if (rowItemCount > 10) rowItemCount = 10;
            if (rowItemCount > entryElements.Count) rowItemCount = entryElements.Count;
            if (rowItemCount < 1) rowItemCount = 1;
            int rowCount = (int)Math.Ceiling(entryElements.Count / (float)rowItemCount);

            int totalIndex = 0;

            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                int thisRowItemCount = Math.Min(rowItemCount, entryElements.Count - totalIndex);

                float containerWidth = District.Size.X;
                float flexBasis = thisRowItemCount * (preferredSize + scaledMargin) + scaledMargin;
                float remainingSpace = containerWidth - flexBasis;
                float flexSpace = (1f / thisRowItemCount) * remainingSpace;
                float entryHeight = (District.Size.Y - scaledMargin) / rowCount - scaledMargin;
                float previousEndPosition = 0;

                for (int i = 0; i < thisRowItemCount; i++)
                {
                    UiElement element = entryElements.Values.ElementAt(totalIndex);

                    element.Color = GetFrontColor(50);
                    element.HoverColor = GetFrontColor(80);
                    element.ActiveColor = GetFrontColor(50);

                    element.Size = new Vector2f(flexSpace + preferredSize, entryHeight);
                    element.Position = new Vector2f(0, 0) + District.Position + new Vector2f(previousEndPosition + scaledMargin, scaledMargin + (entryHeight + scaledMargin) * rowIndex);

                    int vertexIndex = totalIndex * 4;
                    var computed = element.Color;
                    entryVertices[vertexIndex] = new Vertex(element.Position, computed);
                    entryVertices[vertexIndex + 1] = new Vertex(new Vector2f(element.Position.X + element.Size.X, element.Position.Y), computed);
                    entryVertices[vertexIndex + 2] = new Vertex(element.Position + element.Size, computed);
                    entryVertices[vertexIndex + 3] = new Vertex(new Vector2f(element.Position.X, element.Position.Y + element.Size.Y), computed);

                    previousEndPosition = element.Position.X + element.Size.X - District.Position.X;

                    totalIndex++;
                }
            }
        }

        private void RegenerateEntryElements()
        {
            entryVertices = new Vertex[District.Entries.Count * 4];
            const float textPadding = 20;

            foreach (var entry in entryElements)
                uiController.Deregister(entry.Value);

            entryElements.Clear();
            renderTasks.Clear();

            for (int i = 0; i < District.Entries.Count; i++)
            {
                DistrictEntry entry = District.Entries[i];
                UiElement element = new UiElement
                {
                    DepthContainer = backgroundElement,
                    Position = new Vector2f(i * 5, i * 5),
                    Size = new Vector2f(80, 32)
                };

                element.OnMouseDown += (o, e) =>
                {
                    e.PropagateEvent();
                };

                renderTasks.Add(entry, new ActionRenderTask((target) =>
                {
                    entryText.Position = element.Position;

                    entryText.DisplayedString = entry.Name;

                    var textBounds = entryText.GetLocalBounds();
                    entryText.Origin = new Vector2f(textBounds.Width, textBounds.Height) / 2;

                    float scale = Math.Min(element.Size.X / (textBounds.Width / textBounds.Height), element.Size.Y) / CharacterSize;
                    entryText.Scale = new Vector2f(scale, scale) / (CharacterSize / (72f - textPadding));
                    entryText.Position = element.Position + element.Size / 2;

                    target.Draw(entryText);
                }, backgroundElement.Depth));

                entryElements.Add(entry, element);
            }

            foreach (var entry in entryElements)
                uiController.Register(entry.Value);

            needToRecalculateLayout = true;
        }

        private void HandleDragging()
        {
            if (!backgroundElement.IsBeingHeld) return;

            District.Position += Input.MouseDelta;

            if (Math.Abs(Input.MouseDelta.X) + Math.Abs(Input.MouseDelta.Y) > 0)
                needToRecalculateLayout = true;
        }

        private void HandleResizing()
        {
            if (!resizeHandle.IsBeingHeld) return;

            District.Size += Input.MouseDelta;

            if (District.Size.X < 64)
                District.Size.X = 64;

            if (District.Size.Y < 64)
                District.Size.Y = 64;

            if (Math.Abs(Input.MouseDelta.X) + Math.Abs(Input.MouseDelta.Y) > 0)
                needToRecalculateLayout = true;
        }

        private void SyncElement()
        {
            backgroundElement.Color = District.Color;
            backgroundElement.ActiveColor = District.Color;
            backgroundElement.HoverColor = District.Color;
            backgroundElement.Position = District.Position;
            backgroundElement.Size = District.Size;

            background.Position = backgroundElement.Position;
            background.Size = backgroundElement.Size;
            background.FillColor = backgroundElement.ComputedColor;

            var titleBounds = title.GetLocalBounds();
            title.Origin = new Vector2f(titleBounds.Width, titleBounds.Height) / 2;

            float scale = (float)Math.Min(District.Size.X / (titleBounds.Width / titleBounds.Height), District.Size.Y) / 150f;
            title.Scale = new Vector2f(scale, scale) / (CharacterSize / 72f);
            title.Position = background.Position + background.Size / 2;
            title.FillColor = GetFrontColor();

            if (title.DisplayedString != District.Name)
                title.DisplayedString = District.Name;
        }

        private void SyncResizeHandle()
        {
            Vector2f offset = District.Position + District.Size;

            resizeHandleVertices[0].Position = offset + handleShape[0];
            resizeHandleVertices[1].Position = offset + handleShape[1];
            resizeHandleVertices[2].Position = offset + handleShape[2];

            resizeHandle.Color = GetFrontColor();
            resizeHandle.ActiveColor = Utilities.Lerp(resizeHandle.Color, Color.Black, 0.2f);
            resizeHandle.HoverColor = Utilities.Lerp(resizeHandle.Color, Color.White, 0.2f);
            resizeHandle.Position = offset - new Vector2f(HandleSize, HandleSize);
            resizeHandle.Size = new Vector2f(HandleSize, HandleSize);

            resizeHandleVertices[0].Color = resizeHandle.ComputedColor;
            resizeHandleVertices[1].Color = resizeHandle.ComputedColor;
            resizeHandleVertices[2].Color = resizeHandle.ComputedColor;
        }

        public override IEnumerable<IRenderTask> Render()
        {
            backgroundTask.Depth = backgroundElement.Depth;
            titleTask.Depth = backgroundElement.Depth;
            handleTask.Depth = backgroundElement.Depth;

            for (int i = 0; i < District.Entries.Count; i++)
            {
                var entry = District.Entries[i];
                var element = entryElements[entry];
                int vertexIndex = i * 4;
                var computed = element.ComputedColor;
                entryVertices[vertexIndex] = new Vertex(element.Position, computed);
                entryVertices[vertexIndex + 1] = new Vertex(new Vector2f(element.Position.X + element.Size.X, element.Position.Y), computed);
                entryVertices[vertexIndex + 2] = new Vertex(element.Position + element.Size, computed);
                entryVertices[vertexIndex + 3] = new Vertex(new Vector2f(element.Position.X, element.Position.Y + element.Size.Y), computed);
            }

            entryTask.Depth = backgroundElement.Depth;
            entryTask.Vertices = entryVertices;

            yield return backgroundTask;
            yield return titleTask;

            if (District.Entries.Any())
            {
                yield return entryTask;
                foreach (var entry in District.Entries)
                {
                    var task = renderTasks[entry];
                    task.Depth = backgroundElement.Depth;
                    yield return task;
                }
            }

            yield return handleTask;
        }

        public override void Destroyed()
        {
            uiController.Deregister(backgroundElement);
            uiController.Deregister(resizeHandle);

            foreach (var entry in entryElements)
                uiController.Deregister(entry.Value);

            title.Dispose();
            background.Dispose();
        }
    }
}
