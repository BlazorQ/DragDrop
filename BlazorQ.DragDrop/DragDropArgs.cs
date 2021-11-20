namespace BlazorQ.DragDrop
{
    public class DragDropArgs<T>
    {
        public DragDropArgs(object source, object target, T dropped)
        {
            Source = source;
            Target = target is T ? (T)target : default;
            Dropped = dropped;
        }

        public object Source { get; set; }

        public T Target { get; set; }

        public T Dropped { get; set; }
    }
}
