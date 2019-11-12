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
        private Sprite lockedIcon;
        private Text title;

        private UiControllerEntity uiController;
        private DistrictManager manager;

        private readonly UiElement backgroundElement = new UiElement();
        private readonly UiElement resizeHandle = new UiElement();
        private readonly Dictionary<DistrictEntry, UiElement> entryElements = new Dictionary<DistrictEntry, UiElement>();
        private readonly Dictionary<DistrictEntry, ActionRenderTask> renderTasks = new Dictionary<DistrictEntry, ActionRenderTask>();

        private ShapeRenderTask backgroundTask;
        private ShapeRenderTask titleTask;
        private PrimitiveRenderTask entryTask;
        private PrimitiveRenderTask handleTask;
        private ShapeRenderTask lockedIconTask;

        private Vertex[] entryVertices;

        private Text[] entryTexts;
        private bool needToRecalculateLayout = true;

        private readonly uint CharacterSize = 72;
        private static int MinimumDepth = 999999;
        private const float HandleSize = 16;
        private const float LockedIconSize = 14;
        private const float EntryMargin = 3;
        public const string ConsoleSourceIdentifier = "DISTRICT";

        private Vector2f temporarySize;
        private Vector2f temporaryPosition;

        private static readonly Vector2f[] handleShape = new Vector2f[3]
        {
            new Vector2f(0, 0),
            new Vector2f(-HandleSize, 0),
            new Vector2f(0, -HandleSize)
        };

        public DistrictEntity(District district)
        {
            District = district;
            CharacterSize = (uint)Configuration.CurrentConfiguration.TextClarity;
        }

        public override void Created()
        {
            background = new RectangleShape
            {
                Position = new Vector2f(0, 0),
                Size = new Vector2f(256, 256),
                FillColor = new Color(125, 0, 15)
            };

            entryTexts = new Text[0];

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

            lockedIcon = new Sprite(MusicGridApplication.Assets.LockedIcon)
            {
                Origin = (Vector2f)MusicGridApplication.Assets.LockedIcon.Size,
                Scale = new Vector2f(LockedIconSize, LockedIconSize) / MusicGridApplication.Assets.LockedIcon.Size.X,
                Color = new Color(255, 255, 255, 150),
            };

            uiController = World.GetEntityByType<UiControllerEntity>();
            manager = World.GetEntityByType<DistrictManager>();

            resizeHandle.DepthContainer = backgroundElement;

            resizeHandle.OnMouseDown += (o, e) => BringToFront();
            backgroundElement.OnMouseDown += (o, e) =>
            {
                if (e.Button == SFML.Window.Mouse.Button.Middle)
                    e.PropagateEvent();
                else BringToFront();
            };

            backgroundElement.OnSelect += (o, e) =>
            {
                if (e.Button == SFML.Window.Mouse.Button.Right)
                    OpenContextMenu();
            };

            backgroundElement.Selectable = true;

            uiController.Register(resizeHandle);
            uiController.Register(backgroundElement);

            backgroundTask = new ShapeRenderTask(background, backgroundElement.Depth);
            titleTask = new ShapeRenderTask(title, backgroundElement.Depth);
            entryTask = new PrimitiveRenderTask(entryVertices, PrimitiveType.Quads, backgroundElement.Depth);
            handleTask = new PrimitiveRenderTask(resizeHandleVertices, PrimitiveType.Triangles, backgroundElement.Depth);
            lockedIconTask = new ShapeRenderTask(lockedIcon, backgroundElement.Depth);

            temporarySize = District.Size;
            temporaryPosition = District.Position;

            BringToFront();
        }

        private void OpenContextMenu()
        {
            const int maxDisplayValues = 2;
            var selectedDistricts = World.GetEntitiesByType<DistrictEntity>().Where(d => d.backgroundElement.IsSelected).Select(d => d.District).ToList();
            int count = selectedDistricts.Count();
            string displayNames = string.Join(",", selectedDistricts.Take(maxDisplayValues)) + (count > maxDisplayValues ? $" + {count - maxDisplayValues} more" : "");

            var buttons = new List<Button>(new[] {
                new Button(displayNames, default, false),
                new Button($"delete {(selectedDistricts.Count() == 1 ? "district" : "selected districts")}", () => {
                    OpenDeletionConfirmationDialog(selectedDistricts, displayNames);
                }),
                new Button($"fit view to {(selectedDistricts.Count() == 1 ? "district" : "selected districts")}", () => {
                    World.GetEntityByType<CameraControllerEnity>().FitToView(selectedDistricts);
                }),
            });

            if (count == 1)
            {
                buttons.Add(new Button($"{(District.Muted ? "unmute" : "mute")} district", () => { District.Muted = !District.Muted; }));
                buttons.Add(new Button($"{(District.Locked ? "unlock" : "lock")} district", () => { District.Locked = !District.Locked; }));
            }
            else
            {
                buttons.Add(new Button("lock selected districts", () =>
                {
                    foreach (var district in selectedDistricts)
                        district.Locked = true;
                }));
                buttons.Add(new Button("unlock selected districts", () =>
                {
                    foreach (var district in selectedDistricts)
                        district.Locked = false;
                }));
                buttons.Add(new Button("mute selected districts", () =>
                {
                    foreach (var district in selectedDistricts)
                        district.Muted = true;
                }));
                buttons.Add(new Button("unmute selected districts", () =>
                {
                    foreach (var district in selectedDistricts)
                        district.Muted = false;
                }));
            }

            ContextMenuEntity.Open(buttons, (Vector2f)Input.ScreenMousePosition);
        }

        private void OpenDeletionConfirmationDialog(IEnumerable<District> selectedDistricts, string displayNames)
        {
            World.Add(DialogboxEntity.CreateConfirmationDialog($"Are you sure you want to\ndelete {displayNames}", () =>
            {
                foreach (var district in selectedDistricts)
                    manager.RemoveDistrict(district);
                uiController.ClearSelection();
            }));
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
            HandleDragging();
            HandleResizing();

            SyncElement();
            SyncResizeHandle();

            CalculateLayout();

            if (entryElements.Count != District.Entries.Count)
                RegenerateEntryElements();
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
            entryTexts = new Text[District.Entries.Count];
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
                    Size = new Vector2f(80, 32),
                    Color = GetFrontColor(50),
                    HoverColor = GetFrontColor(80),
                    ActiveColor = GetFrontColor(50),
                };

                element.OnMouseDown += (o, e) =>
                {
                    e.PropagateEvent();
                };

                element.OnDoubleClick += (o, e) =>
                {
                    if (e.Button != SFML.Window.Mouse.Button.Left) return;
                    if (District.Muted) return;
                    var player = World.GetEntityByType<MusicControlsEntity>();
                    player.SetColor(District.Color);
                    player.TrackQueue.SkipToOrEnqueue(entry);
                    player.MusicPlayer.Play();
                };

                entryTexts[i] = new Text("Entry", MusicGridApplication.Assets.DefaultFont)
                {
                    FillColor = Style.Foreground,
                    Position = new Vector2f(0, 0),
                    CharacterSize = CharacterSize
                };

                var myText = entryTexts[i];
                myText.DisplayedString = entry.Name;
                var textBounds = myText.GetLocalBounds();
                myText.Origin = new Vector2f(textBounds.Width, textBounds.Height) / 2;

                renderTasks.Add(entry, new ActionRenderTask((target) =>
                {
                    myText.Position = element.Position + element.Size / 2;
                    float scale = Math.Min(element.Size.X / (textBounds.Width / textBounds.Height), element.Size.Y) / (CharacterSize + textPadding);
                    myText.Scale = new Vector2f(scale, scale);
                    target.Draw(myText);
                }, backgroundElement.Depth));

                entryElements.Add(entry, element);
            }

            foreach (var entry in entryElements)
                uiController.Register(entry.Value);

            needToRecalculateLayout = true;
        }

        private void HandleDragging()
        {
            if (District.Locked) return;
            if (!backgroundElement.IsBeingHeld || Input.HeldButton != SFML.Window.Mouse.Button.Left) return;

            temporaryPosition += Input.MouseDelta;

            float snapSize = Configuration.CurrentConfiguration.SnappingSize;
            var snappedPosition = manager.SnappingEnabled ?
                new Vector2f((float)Math.Round(temporaryPosition.X / snapSize) * snapSize, (float)Math.Round(temporaryPosition.Y / snapSize) * snapSize) :
                temporaryPosition
                ;
            if (Utilities.SquaredMagnitude(snappedPosition - District.Position) > .01f)
            {
                District.Position = snappedPosition;
                needToRecalculateLayout = true;
            }
        }

        private void HandleResizing()
        {
            if (District.Locked) return;
            if (!resizeHandle.IsBeingHeld) return;

            temporarySize += Input.MouseDelta;

            float snapSize = Configuration.CurrentConfiguration.SnappingSize;
            var snappedSize = manager.SnappingEnabled ?
                new Vector2f((float)Math.Round(temporarySize.X / snapSize) * snapSize, (float)Math.Round(temporarySize.Y / snapSize) * snapSize) :
                temporarySize
                ;

            if (snappedSize.X < 64)
                snappedSize.X = 64;

            if (snappedSize.Y < 64)
                snappedSize.Y = 64;

            if (Utilities.SquaredMagnitude(snappedSize - District.Size) > .01f)
            {
                District.Size = snappedSize;
                needToRecalculateLayout = true;
            }
        }

        private float SmoothedSquareWave(float x, float a, float f, float d) => (2 * a / (float)Math.PI) * (float)Math.Atan((float)Math.Sin(2 * (float)Math.PI * x * f) / d);

        private void SyncElement()
        {
            UpdateDistrictLinkedValues();

            if (backgroundElement.IsSelected)
            {
                float intensity = SmoothedSquareWave(-MusicGridApplication.Globals.Time + (District.Position.X + District.Position.Y) * Configuration.CurrentConfiguration.SelectionWaveWave, .5f,
                    Configuration.CurrentConfiguration.SelectionWaveFrequency,
                    Configuration.CurrentConfiguration.SelectionWaveSmoothness) + .5f;
                backgroundElement.SelectedColor = Utilities.Lerp(
                    Utilities.Lerp(District.Color, Color.Black, .3f),
                    Utilities.Lerp(District.Color, Color.White, .3f),
                    intensity);
            }

            backgroundElement.Position = District.Position;
            backgroundElement.Size = District.Size;

            background.Position = backgroundElement.Position;
            background.Size = backgroundElement.Size;
            background.FillColor = backgroundElement.ComputedColor;

            var titleBounds = title.GetLocalBounds();
            title.Origin = new Vector2f(titleBounds.Width, titleBounds.Height) / 2;

            float scale = Math.Min(District.Size.X / (titleBounds.Width / titleBounds.Height), District.Size.Y) / 150f;
            title.Scale = new Vector2f(scale, scale) / (CharacterSize / 72f);
            title.Position = background.Position + background.Size / 2;
            title.FillColor = GetFrontColor();

            if (District.Locked)
                lockedIcon.Position = District.Position - new Vector2f(EntryMargin, EntryMargin) + District.Size;
        }

        private void UpdateDistrictLinkedValues()
        {
            if (!District.Dirty) return;
            District.Dirty = false;

            backgroundElement.Color = District.Color;
            backgroundElement.ActiveColor = District.Color;
            backgroundElement.HoverColor = District.Color;
            backgroundElement.DisabledColor = District.Color;
            title.DisplayedString = District.Name;
        }

        private void SyncResizeHandle()
        {
            if (District.Locked) return;
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
            lockedIconTask.Depth = backgroundElement.Depth;

            if (!District.Muted)
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

            if (!District.Muted)
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

            if (District.Locked)
                yield return lockedIconTask;
            else
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

        public override string ToString() => District + " entity";

        public bool IsSelected => backgroundElement.IsSelected;
    }
}
