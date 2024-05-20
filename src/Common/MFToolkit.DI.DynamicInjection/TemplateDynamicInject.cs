using System;
using MFToolkit.DI.DynamicInject;
using MFToolkit.DI.DynamicInjection;
using Microsoft.Extensions.DependencyInjection;

namespace MFToolkit.DI.DynamicInject
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    [System.Diagnostics.Conditional("DynamicInjectGenerator_DEBUG")]
    public sealed partial class DynamicInjectAttribute : Attribute
    {
        public static IServiceCollection DefaultServices = new ServiceCollection();
        public Dependencies Dependencies { get; set; }
        public Type ServiceType { get; set; }
        public Type ImplementationType { get; set; }
        public DynamicInjectAttribute()
        {
            if (Dependencies == Dependencies.Transient)
            {
                if (ServiceType != null && ImplementationType != null)
                {
                    DefaultServices.AddTransient(ServiceType, ServiceType);
                }
                else if (ServiceType != null)
                {
                    DefaultServices.AddTransient(ServiceType);
                }
            }
            else if (Dependencies == Dependencies.Scoped)
            {
                if (ServiceType != null && ImplementationType != null)
                {
                    DefaultServices.AddScoped(ServiceType, ServiceType);
                }
                else if (ServiceType != null)
                {
                    DefaultServices.AddScoped(ServiceType);
                }
            }
            else if (Dependencies == Dependencies.Singleton)
            {
                if (ServiceType != null && ImplementationType != null)
                {
                    DefaultServices.AddSingleton(ServiceType, ServiceType);
                }
                else if (ServiceType != null)
                {
                    DefaultServices.AddSingleton(ServiceType);
                }
            }
        }
    }
}