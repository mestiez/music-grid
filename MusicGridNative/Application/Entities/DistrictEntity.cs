﻿using SFML.Graphics;
using SFML.System;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MusicGridNative
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

        private static int MinimumDepth = 100;

        private const float HandleSize = 16;

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

            title = new Text("District", MusicGridApplication.Assets.DefaultFont)
            {
                FillColor = new Color(255, 255, 255, 125),
                Position = new Vector2f(0, 0),
                CharacterSize = 48
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

            BringToFront();
        }

        private void BringToFront()
        {
            MinimumDepth--;
            backgroundElement.Depth = MinimumDepth;
        }

        public override void Update()
        {
            SyncElement();
            SyncResizeHandle();

            HandleDragging();
            HandleResizing();
        }

        private void HandleDragging()
        {
            if (backgroundElement.IsBeingHeld)
                District.Position += Input.MouseDelta;
        }

        private void HandleResizing()
        {
            if (resizeHandle.IsBeingHeld)
            {
                District.Size += Input.MouseDelta;

                if (District.Size.X < 64)
                    District.Size.X = 64;

                if (District.Size.Y < 64)
                    District.Size.Y = 64;
            }
        }

        private void SyncElement()
        {
            backgroundElement.Color = District.Color;
            backgroundElement.ActiveColor = Utilities.Lerp(District.Color, Color.Black, 0.2f);
            backgroundElement.HoverColor = Utilities.Lerp(District.Color, Color.White, 0.2f);
            backgroundElement.Position = District.Position;
            backgroundElement.Size = District.Size;

            background.Position = backgroundElement.Position;
            background.Size = backgroundElement.Size;
            background.FillColor = backgroundElement.ComputedColor;

            var titleBounds = title.GetGlobalBounds();

            float scale = (float)Math.Min(District.Size.X, District.Size.Y) / 200f;
            title.Scale = new Vector2f(scale, scale);
            title.Position = background.Position + background.Size / 2 - new Vector2f(titleBounds.Width / 2, titleBounds.Height / 2 + scale * 2.5f);
        }

        private void SyncResizeHandle()
        {
            Vector2f offset = District.Position + District.Size;

            resizeHandleVertices[0].Position = offset + handleShape[0];
            resizeHandleVertices[1].Position = offset + handleShape[1];
            resizeHandleVertices[2].Position = offset + handleShape[2];

            resizeHandle.Color = title.FillColor;
            resizeHandle.ActiveColor = Utilities.Lerp(title.FillColor, Color.Black, 0.2f);
            resizeHandle.HoverColor = Utilities.Lerp(title.FillColor, Color.White, 0.2f);
            resizeHandle.Position = offset - new Vector2f(HandleSize, HandleSize);
            resizeHandle.Size = new Vector2f(HandleSize, HandleSize);

            resizeHandleVertices[0].Color = resizeHandle.ComputedColor;
            resizeHandleVertices[1].Color = resizeHandle.ComputedColor;
            resizeHandleVertices[2].Color = resizeHandle.ComputedColor;
        }

        public override IEnumerable<IRenderTask> Render()
        {
            yield return new ShapeRenderTask(background, backgroundElement.Depth);
            yield return new ShapeRenderTask(title, backgroundElement.Depth);
            yield return new PrimitiveRenderTask(resizeHandleVertices, PrimitiveType.Triangles, backgroundElement.Depth);
        }

        public override void Destroyed()
        {
            uiController.Deregister(backgroundElement);
            uiController.Deregister(resizeHandle);
            title.Dispose();
            background.Dispose();
        }
    }
}
