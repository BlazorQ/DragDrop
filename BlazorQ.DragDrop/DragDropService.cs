using System;
using System.Collections;

namespace BlazorQ.DragDrop
{
    internal class DragDropService
    {
        public DragDropEffects DraggedItemEffect { get; set; }

        public object Source { get; set; }

        public object Target { get; set; }

        /// <summary>
        /// Items of the source dropzone in which the drag operation started
        /// </summary>
        public IList SourceCollection { get; set; }

        /// <summary>
        /// Items of the target dropzone in which the drag operation ended
        /// </summary>
        public IList TargetCollection { get; set; }

        

        /// <summary>
        /// Resets the service to initial state
        /// </summary>
        public void Reset()
        {
            ShouldRender = true;
            Source = null;
            SourceCollection = null;
            TargetCollection = null;
            Target = null;

            StateHasChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool ShouldRender { get; set; } = true;

        // Notify subscribers that there is a need for rerender
        public EventHandler StateHasChanged { get; set; }
    }
}
