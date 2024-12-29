// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using MFToolkit.DI.DynamicInjection;

IServiceCollection services = new ServiceCollection();
services.AddMFInjectables(services);