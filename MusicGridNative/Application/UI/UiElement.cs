﻿using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Linq;
using Shared;
using Color = SFML.Graphics.Color;

namespace MusicGrid
{
    public class UiElement : IElement
    {
        private int depth;
        private float lastClickedTime = -1;
        private UiElement depthContainer;
        private bool isSelected;

        public Vector2f Position { get; set; }
        public Vector2f Size { get; set; }

        public Color Color { get; set; }
        public Color HoverColor { get; set; }
        public Color ActiveColor { get; set; }
        public Color DisabledColor { get; set; }
        public Color SelectedColor { get; set; }

        public bool IsScreenSpace { get; set; }

        public UiControllerEntity Controller { get; set; }
        public UiElement DepthContainer
        {
            get => depthContainer;

            set
            {
                depthContainer = value;
                OnDepthChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int Depth
        {
            get => depth;

            set
            {
                depth = value;
                OnDepthChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsUnderMouse { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsBeingHeld { get; private set; }
        public Color ComputedColor { get; private set; }
        public bool IsSelected
        {
            get => isSelected; 
            set
            {
                isSelected = value;
                if (value)
                    OnSelect?.Invoke(this, new SelectionEventArgs((int)Input.HeldButton.GetValueOrDefault(), Input.ScreenMousePosition.ToNumerics()));
                else
                    OnDeselect?.Invoke(this, new SelectionEventArgs((int)Input.HeldButton.GetValueOrDefault(), Input.ScreenMousePosition.ToNumerics()));
            }
        }

        public bool Selectable { get; set; }
        public bool Disabled { get; set; }
        public float DoubleClickMaxDuration { get; set; } = 0.25f;
        public bool Interactable { get; set; } = true;
        public bool SelectInSelectAll { get; set; } = true;

        public event EventHandler<MouseEventArgs> OnMouseDown;
        public event EventHandler<MouseEventArgs> OnMouseUp;
        public event EventHandler<MouseEventArgs> OnDoubleClick;
        public event EventHandler<SelectionEventArgs> OnSelect;
        public event EventHandler<SelectionEventArgs> OnDeselect;
        public event EventHandler OnDepthChanged;

        public FloatRect GetLocalBounds() => new FloatRect(Position, Size);

        public bool MouseOverlap => Utilities.IsInside(RelevantMousePosition, Position, Size);

        public bool EvaluateInteraction(InteractionInfo info)
        {
            ComputedColor = Color;
            Vector2f mousePos = RelevantMousePosition;

            if (info.FirstServed || !Interactable)
            {
                IsUnderMouse = false;
                IsActive = false;
            }
            else
            {
                IsUnderMouse = Utilities.IsInside(mousePos, Position, Size);
                IsActive = !Disabled && IsUnderMouse && info.Held.HasValue;

                if (IsUnderMouse && info.Pressed.HasValue && !Disabled)
                {
                    var args = new MouseEventArgs((int)info.Pressed.Value, mousePos.ToNumerics());
                    OnMouseDown?.Invoke(this, args);
                    if (lastClickedTime > 0 && MusicGridApplication.Globals.Time - lastClickedTime < DoubleClickMaxDuration)
                    {
                        OnDoubleClick?.Invoke(this, args);
                        lastClickedTime = -1;
                    }
                    else
                        lastClickedTime = MusicGridApplication.Globals.Time;

                    if (!args.IsPermeable)
                    {
                        if (Selectable)
                        {
                            if (info.Pressed.Value == Mouse.Button.Right)
                            {
                                if (Controller.Selected.Contains(this))
                                    Controller.Select(this, true);
                                else
                                    Controller.Select(this, Controller.Multiselecting);
                            }
                            else
                                Controller.HandleSelection(this);
                        }
                        else Controller.ClearSelection();
                        Controller.FocusedElement = this;
                        IsBeingHeld = true;
                    }
                    else return false;
                }
            }

            if (!Disabled && IsBeingHeld && info.Released.HasValue && Interactable)
            {
                if (Controller.FocusedElement == this)
                    Controller.FocusedElement = null;
                IsBeingHeld = false;
                OnMouseUp?.Invoke(this, new MouseEventArgs((int)info.Released.Value, mousePos.ToNumerics()));
            }
            ComputeColors();
            return IsUnderMouse || IsBeingHeld;
        }

        private Vector2f RelevantMousePosition => IsScreenSpace ? (Vector2f)Input.ScreenMousePosition : Input.MousePosition;

        public void ComputeColors()
        {
            if (Disabled)
                ComputedColor = DisabledColor;
            else if (IsSelected)
                ComputedColor = SelectedColor;
            else if (IsActive || IsBeingHeld)
                ComputedColor = ActiveColor;
            else if (IsUnderMouse)
                ComputedColor = HoverColor;
            else
                ComputedColor = Color;
        }
    }
}
