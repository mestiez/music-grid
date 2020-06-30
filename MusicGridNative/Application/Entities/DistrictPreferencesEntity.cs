using SFML.Graphics;
using SFML.System;
using Shared;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Color = SFML.Graphics.Color;

namespace MusicGrid
{
    public class DistrictPreferencesEntity : Entity
    {
        private const int Width = 258;
        private const int Height = 120;
        private UiControllerEntity uiController;
        private Vector2 Size => new Vector2(Width, Height);

        private DrawableElement background;
        private DraggableController draggable;

        public readonly DistrictEntity DistrictEntity;

        public DistrictPreferencesEntity(DistrictEntity districtEntity)
        {
            DistrictEntity = districtEntity;
            Name = $"{DistrictEntity.District.Name} editor";
        }

        public override void Created()
        {
            uiController = World.GetEntityByType<UiControllerEntity>();

            SetupLayout();
        }

        public override void Update()
        {
            draggable.Update();
            if (draggable.HasMoved)
                RecalculateLayout();
        }

        private void SetupLayout()
        {
            const float offset = 25;
            var others = World.GetEntitiesByType<DistrictPreferencesEntity>();
            int count = others.Length - 1;
            var pos = (Vector2f)World.RenderTarget.Size / 2 - Size.ToSFML() / 2;

            background = new DrawableElement(uiController, Size.ToSFML(), pos + (new Vector2f(offset, offset) * count), -count);
            background.Element.IsScreenSpace = true;
            background.DrawBackground = true;

            draggable = new DraggableController(background.Element);
            draggable.OnDragStart += (o, e) => BringToFront();

            RecalculateLayout();
        }

        private SFML.Graphics.Color GetTextColour()
        {
            return Utilities.IsTooBright(background.Element.Color) ? Style.Background.ToSFML() : Style.Foreground.ToSFML();
        }

        private void RecalculateLayout()
        {
            background.Position = background.Element.Position;
            background.Element.Color = DistrictEntity.District.Color.ToSFML();
            background.Element.HoverColor = background.Element.Color;
            background.Element.ActiveColor = background.Element.Color;
        }

        public void BringToFront()
        {
            background.Depth = uiController.Elements.Where(e => e.DepthContainer == null).Min(e => e.Depth) - 1;
        }

        public override void PreRender()
        {
            if (!draggable.HasMoved) return;
            RecalculateLayout();
        }

        public override IEnumerable<IRenderTask> RenderScreen()
        {
            yield return background.RenderTask;
        }
    }
}
