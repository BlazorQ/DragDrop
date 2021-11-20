using Microsoft.Extensions.DependencyInjection;

namespace BlazorQ.DragDrop
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlazorDragDrop(this IServiceCollection services)
        {
            return services.AddScoped(typeof(DragDropService));
        }
    }
}
