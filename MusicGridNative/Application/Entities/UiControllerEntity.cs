﻿using SFML.Window;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicGrid
{
    public class UiControllerEntity : Entity
    {
        private List<UiElement> elements = new List<UiElement>();
        private bool requiresElementResort = false;

        private List<UiElement> registerBuffer = new List<UiElement>();
        private List<UiElement> deregisterBuffer = new List<UiElement>();

        private HashSet<UiElement> selected = new HashSet<UiElement>();
        private int inputCount;

        public IReadOnlyList<UiElement> Selected => selected.ToList().AsReadOnly();
        public IReadOnlyList<UiElement> Elements => elements.Concat(registerBuffer).ToList().AsReadOnly();
        public bool Multiselecting { get; private set; } = false;

        public UiElement FocusedElement { get; set; }
        public bool IsPointerOverElement { get; private set; }
        public bool UserIsInputting => inputCount > 0;

        public override void Created()
        {
            World.Lua.LinkFunction(Functions.SelectAll, this, () => { ToggleSelectAll(); });
            World.Lua.LinkFunction(Functions.EnableMultiselect, this, () => { Multiselecting = true; });
            World.Lua.LinkFunction(Functions.DisableMultiselect, this, () => { Multiselecting = false; });
        }

        public void Register(UiElement element)
        {
            registerBuffer.Add(element);
            requiresElementResort = true;
        }

        public void Deregister(UiElement element)
        {
            deregisterBuffer.Add(element);
            requiresElementResort = true;
        }

        public void NotifyInputStart()
        {
            inputCount++;
            ConsoleEntity.Log($"Added input block #{inputCount}", this);
        }

        public void NotifyInputEnd()
        {
            ConsoleEntity.Log($"Removed input block #{inputCount}", this);
            inputCount--;
        }

        private void HandleBuffers()
        {
            foreach (var elem in deregisterBuffer)
            {
                if (elem == null)
                {
                    ConsoleEntity.Log($"Attempt to deregister null UI element", this);
                    return;
                }
                elements.Remove(elem);
                registerBuffer.Remove(elem);
                if (FocusedElement == elem)
                    FocusedElement = null;
                elem.Controller = null;
                elem.OnDepthChanged -= OnDepthChange;
            }

            foreach (var elem in registerBuffer)
            {
                elements.Add(elem);
                elem.Controller = this;
                elem.OnDepthChanged += OnDepthChange;
                FocusedElement = null;
                elem.EvaluateInteraction(default);
            }

            deregisterBuffer.Clear();
            registerBuffer.Clear();
        }

        private void OnDepthChange(object obj, EventArgs e)
        {
            requiresElementResort = true;
        }

        private void ReSortElements()
        {
            requiresElementResort = false;
            HandleBuffers();
            var ordered = new List<UiElement>();
            foreach (var element in elements.Where(elem => elem.DepthContainer == null).OrderBy(elem => elem.Depth))
            {
                foreach (var child in elements.Where(elem => elem.DepthContainer == element).OrderBy(elem => elem.Depth))
                    ordered.Add(child);
                ordered.Add(element);
            }
            elements = ordered;
        }

        public void HandleSelection(UiElement elem)
        {
            if (Multiselecting)
            {
                if (elem.IsSelected)
                    Deselect(elem);
                else
                    Select(elem, true);
            }
            else
                Select(elem, false);
        }

        public void Select(UiElement element, bool multiselect = false)
        {
            if (!element.Selectable || !element.Interactable) return;
            if (!multiselect)
                ClearSelection();
            element.IsSelected = true;
            selected.Add(element);
            element.ComputeColors();
        }

        public void Deselect(UiElement element)
        {
            if (!element.Selectable || !element.Interactable) return;
            if (!selected.Contains(element)) return;
            element.IsSelected = false;
            selected.Remove(element);
            element.ComputeColors();
        }

        public void ClearSelection()
        {
            foreach (var elem in selected)
            {
                elem.IsSelected = false;
                elem.ComputeColors();
            }
            selected.Clear();
        }

        public override void Update()
        {
            var focus = Input.WindowHasFocus;
            if (ConsoleEntity.Main.ConsoleIsOpen) return;

            if (requiresElementResort)
                ReSortElements();

            var info = new InteractionInfo
            {
                Pressed = Input.PressedButton,
                Held = Input.HeldButton,
                Released = Input.ReleasedButton
            };

            if (FocusedElement != null)
                info.FirstServed = FocusedElement.EvaluateInteraction(info);
            else
            {
                IsPointerOverElement = false;
                foreach (UiElement element in elements)
                {
                    if (!focus)
                        element.ComputeColors();
                    else if (element.EvaluateInteraction(info))
                    {
                        IsPointerOverElement = true;
                        info.FirstServed = true;
                    }
                }
            }

            if (!info.FirstServed && (
                info.Held.HasValue && info.Held.Value != Mouse.Button.Middle ||
                info.Pressed.HasValue && info.Pressed.Value != Mouse.Button.Middle ||
                info.Released.HasValue && info.Released.Value != Mouse.Button.Middle
                ))
                ClearSelection();
        }

        public void SelectAll()
        {
            ClearSelection();
            foreach (var elem in elements)
                if (elem.SelectInSelectAll)
                    Select(elem, true);
        }

        public void ToggleSelectAll()
        {
            if (selected.Count == 0)
                SelectAll();
            else
                ClearSelection();
        }
    }
}
