using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BlazorQ.DragDrop
{
    public enum DragDropEffects
    {
        Move,
        Copy
    }
    public partial class DropZone<TItem>
    {
        /// <summary>
        /// Allows to pass a delegate which executes if something is dragging over this DropZone.
        /// If the source and target items are accepted, you should return an item of type TItem.
        /// This item will be used as item to drop.
        /// Source item
        /// Target item
        /// </summary>
        [Parameter]
        public Func<object, object, TItem> Accepts { get; set; }

        /// <summary>
        /// Allows to pass a delegate which decides if the item is draggable
        /// </summary>
        [Parameter]
        public Func<TItem, bool> AllowsDrag { get; set; }

        [Parameter]
        public bool CanReorder { get; set; } = true;

        /// <summary>
        /// Allows to pass a delegate which executes if a drag operation ends
        /// </summary>
        [Parameter]
        public Action<TItem> DragEnd { get; set; }

        /// <summary>
        /// Raises a callback with the dropped item as parameter in case the item can not be dropped due to the given Accept Delegate
        /// </summary>
        [Parameter]
        public EventCallback<TItem> OnItemDropRejected { get; set; }

        /// <summary>
        /// Raises a callback with the dropped item as parameter
        /// </summary>
        [Parameter]
        public EventCallback<DragDropArgs<TItem>> OnItemDrop { get; set; }

        /// <summary>
        /// Raises a callback with the replaced item as parameter
        /// </summary>
        [Parameter]
        public EventCallback<TItem> OnReplacedItemDrop { get; set; }

        /// <summary>
        /// List of items for the dropzone
        /// </summary>
        [Parameter]
        public IList<TItem> Items { get; set; }

        /// <summary>
        /// Maximum Number of items which can be dropped in this dropzone. Defaults to null which means unlimited.
        /// </summary>
        [Parameter]
        public int? MaxItems { get; set; }

        /// <summary>
        /// Raises a callback with the dropped item as parameter in case the item can not be dropped due to item limit.
        /// </summary>
        [Parameter]
        public EventCallback<TItem> OnItemDropRejectedByMaxItemLimit { get; set; }

        [Parameter]
        public RenderFragment<TItem> ChildContent { get; set; }

        /// <summary>
        /// Specifies one or more classnames for the DropZone element.
        /// </summary>
        [Parameter]
        public string Class { get; set; }

        /// <summary>
        /// Specifies the id for the DropZone element.
        /// </summary>
        [Parameter]
        public string Id { get; set; }

        /// <summary>
        /// Allows to pass a delegate which specifies one or more classnames for the draggable div that wraps your elements.
        /// </summary>
        [Parameter]
        public Func<TItem, string> ItemWrapperClass { get; set; }

        /// <summary>
        /// Allows to pass a delegate which specifies style for the draggable div that wraps your elements.
        /// </summary>
        [Parameter]
        public Func<TItem, string> ItemWrapperStyle { get; set; }

        [Parameter]
        public DragDropEffects DragDropEffect { get; set; }

        [Parameter]
        public TItem SelectedItem { get; set; }

        [Parameter]
        public EventCallback<TItem> SelectedItemChanged { get; set; }

        /// <summary>
        /// Index of the Active Spacing div
        /// </summary>
        /// 
        private int? activeSpacerIndex;
        private int? ActiveSpacerIndex
        {
            get => activeSpacerIndex;
            set
            {
                activeSpacerIndex = value;
                DragDropService.ShouldRender = true;
                StateHasChanged();
                DragDropService.ShouldRender = false;
            }
        }

        private bool IsMaxItemLimitReached(TItem acceptedItem)
        {
            return !Items.Contains(acceptedItem) && MaxItems.HasValue && MaxItems == Items.Count();
        }

        private string IsItemDragable(TItem item)
        {
            if (AllowsDrag == null)
                return "true";
            if (item == null)
                return "false";
            return AllowsDrag(item).ToString();
        }

        private TItem IsItemAccepted(object dragTargetItem)
        {
            if (Accepts == null)
                return DragDropService.Source is TItem ? (TItem)DragDropService.Source : default;
            else
                return Accepts(DragDropService.Source, dragTargetItem);
        }

        protected override bool ShouldRender()
        {
            return DragDropService.ShouldRender;
        }

        private void ForceRender(object sender, EventArgs e)
        {
            StateHasChanged();
        }

        protected override void OnInitialized()
        {
            DragDropService.StateHasChanged += ForceRender;

            base.OnInitialized();
        }

        public void OnDragEnd(TItem item)
        {
            Debug.WriteLine($"OnDragEnd: {DragDropService.Target}");

            var acceptedItem = IsItemAccepted(DragDropService.Target);

            if (DragEnd != null && acceptedItem != null)
                DragEnd(acceptedItem);

            Reset();
        }

        public void OnDragEnter(TItem item)
        {
            Debug.WriteLine($"OnDragEnter: {item}");

            DragDropService.TargetCollection = (IList)Items;

            DragDropService.Target = item;

            DragDropService.ShouldRender = true;
            StateHasChanged();
            DragDropService.ShouldRender = false;
        }

        public void OnDragOver(TItem item)
        {
            Debug.WriteLine($"OnDragOver: {item}");

            if (DragDropService.TargetCollection != (IList)Items)
            {
                DragDropService.TargetCollection = (IList)Items;

                DragDropService.ShouldRender = true;
                StateHasChanged();
                DragDropService.ShouldRender = false;
            }
        }

        public void OnDragLeave(TItem item)
        {
            Debug.WriteLine($"OnDragLeave: {item}");

            if (item == null && DragDropService.Target == null && ActiveSpacerIndex == null)
                DragDropService.TargetCollection = null;

            if (item != null)
                DragDropService.Target = null;

            DragDropService.ShouldRender = true;
            StateHasChanged();
            DragDropService.ShouldRender = false;
        }

        public void OnDragStart(DragEventArgs e, TItem item)
        {
            Debug.WriteLine($"OnDragStart: {item}");

            DragDropService.ShouldRender = true;
            DragDropService.DraggedItemEffect = DragDropEffect;
            DragDropService.Source = item;
            DragDropService.SourceCollection = (IList)Items;
            SelectedItem = item;
            SelectedItemChanged.InvokeAsync(item);

            //not working. https://github.com/dotnet/aspnetcore/issues/18754
            switch (DragDropEffect)
            {
                case DragDropEffects.Move:
                    e.DataTransfer.DropEffect = "move";
                    e.DataTransfer.EffectAllowed = "move";
                    break;
                case DragDropEffects.Copy:
                    e.DataTransfer.DropEffect = "copy";
                    e.DataTransfer.EffectAllowed = "copy";
                    break;
            }

            StateHasChanged();
            DragDropService.ShouldRender = false;
        }

        public void OnClick(TItem item)
        {
            SelectedItem = item;
            SelectedItemChanged.InvokeAsync(item);

            StateHasChanged();
        }

        private string GetClassesForDraggable(TItem item)
        {
            var classes = new List<string>();

            classes.Add("blazorq-dropzone-item");

            if (ItemWrapperClass != null)
                classes.Add(ItemWrapperClass(item));

            var accepted = IsItemAccepted(DragDropService.Target) != null;

            if (item.Equals(DragDropService.Source))
            {
                if (DragDropService.DraggedItemEffect == DragDropEffects.Move)
                    classes.Add("blazorq-dropzone-item-in-transit-move"); //hide item, until drop decision
                else
                    classes.Add("blazorq-dropzone-item-in-transit-copy");
            }

            if (!item.Equals(DragDropService.Source) && item.Equals(DragDropService.Target))
                if (accepted || IsReordering())
                    classes.Add("blazorq-dropzone-item-dragged-over");
                else
                    classes.Add("blazorq-dropzone-item-dragged-over-denied");

            if (DragDropService.Source != null)
                classes.Add("blazorq-dropzone-item-inprogess");
            else if (item.Equals(SelectedItem))
                classes.Add("blazorq-dropzone-item-selected");

            if (AllowsDrag != null && item != null && !AllowsDrag(item))
                classes.Add("blazorq-dropzone-item-noselect");

            return string.Join(' ', classes);
        }

        private string GetClassesForDropZone()
        {
            Debug.WriteLine($"GetClassesForDropZone: IsDropAllowed = {IsDropAllowed()}");

            var classes = new List<string>();

            classes.Add("blazorq-dropzone");

            if (!string.IsNullOrEmpty(Class))
                classes.Add(Class);

            if (IsDropAllowed())
                classes.Add("blazorq-dropzone-dragged-over");

            return string.Join(' ', classes);
        }

        private string GetClassesForSpacing(int spacerId)
        {
            //Debug.WriteLine($"GetClassesForSpacing: {spacerId}");

            var acceptedItem = IsReordering() ? (TItem)DragDropService.Source : IsItemAccepted(DragDropService.Target);

            var classes = new List<string>();

            classes.Add("blazorq-dropzone-spacing");

            if (DragDropService.Target == null && ActiveSpacerIndex == spacerId)
            {
                if (Items.IndexOf(acceptedItem) == -1 || (spacerId - Items.IndexOf(acceptedItem)) < 0 || (spacerId - Items.IndexOf(acceptedItem)) > 1)
                    classes.Add("blazorq-dropzone-spacing-dragged-over");
            }

            return string.Join(' ', classes);
        }

        private bool IsDropAllowed()
        {
            if (DragDropService.TargetCollection == (IList)Items)
            {
                var acceptedItem = IsItemAccepted(DragDropService.Target);

                if (acceptedItem != null)
                {
                    if (IsMaxItemLimitReached(acceptedItem))
                    {
                        OnItemDropRejectedByMaxItemLimit.InvokeAsync(acceptedItem);
                        return false;
                    }
                }
                else
                {
                    OnItemDropRejected.InvokeAsync(acceptedItem);
                    return false;
                }

                return true;
            }
            else
                return false;
        }

        private bool IsReordering()
        {
            return DragDropService.Source is TItem && Items == DragDropService.TargetCollection && CanReorder;
        }

        private void OnDropItemOnSpacing(int newIndex)
        {
            Debug.WriteLine($"OnDropItemOnSpacing: {newIndex}");

            var isReordering = IsReordering();

            if (!IsDropAllowed() && !isReordering)
            {
                Reset();

                return;
            }

            var acceptedItem = isReordering ? (TItem)DragDropService.Source : IsItemAccepted(DragDropService.Target);
            var oldIndex = Items.IndexOf(acceptedItem);

            if (oldIndex == -1)
            {
                if (DragDropService.DraggedItemEffect == DragDropEffects.Move)
                    DragDropService.SourceCollection.Remove(acceptedItem);
            }
            else
            {
                Items.RemoveAt(oldIndex);

                if (newIndex > oldIndex)
                    newIndex--;
            }

            Items.Insert(newIndex, acceptedItem);

            OnItemDrop.InvokeAsync(new DragDropArgs<TItem>(DragDropService.Source, DragDropService.Target, acceptedItem));

            Reset();
        }

        private void OnDrop()
        {
            DragDropService.ShouldRender = true;

            var isReordering = IsReordering();

            if (!IsDropAllowed() && !isReordering)
            {
                Reset();

                return;
            }

            var acceptedItem = isReordering ? (TItem)DragDropService.Source : IsItemAccepted(DragDropService.Target);

            if (DragDropService.Target is not TItem && !Items.Contains(acceptedItem))
            {
                if (DragDropService.DraggedItemEffect == DragDropEffects.Move)
                    DragDropService.SourceCollection.Remove(acceptedItem);

                Items.Insert(Items.Count, acceptedItem);
            }
            else if (DragDropService.Target is TItem && Items.Contains(acceptedItem))
            {
                Swap((TItem)DragDropService.Target, acceptedItem);
            }

            OnItemDrop.InvokeAsync(new DragDropArgs<TItem>(DragDropService.Source, DragDropService.Target, acceptedItem));

            Reset();

            StateHasChanged();
        }

        private void Swap(TItem draggedOverItem, TItem activeItem)
        {
            var indexDraggedOverItem = Items.IndexOf(draggedOverItem);
            var indexActiveItem = Items.IndexOf(activeItem);

            if (indexActiveItem != -1)
            {
                if (indexDraggedOverItem == indexActiveItem)
                    return;
                TItem tmp = Items[indexDraggedOverItem];
                Items[indexDraggedOverItem] = Items[indexActiveItem];
                Items[indexActiveItem] = tmp;
                OnReplacedItemDrop.InvokeAsync(Items[indexActiveItem]);
            }
        }

        private void Reset()
        {
            activeSpacerIndex = null;
            DragDropService.Reset();
        }

        public void Dispose()
        {
            DragDropService.StateHasChanged -= ForceRender;
        }
    }
}