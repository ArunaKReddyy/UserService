using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Services;

namespace UserService.Application.RegisterServices;

public static class ApplicationDIServices
{
    public static IServiceCollection AddApplicationServices(
            this IServiceCollection services)
    {
        return services.AddScoped<IUserService, Services.UserService>();
    }
}
