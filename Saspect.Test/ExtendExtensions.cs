using Autofac;
using Saspect.Autofac;
using Sinjector;

namespace Saspect.Test;

public static class ExtendExtensions
{
    public static void AddServiceWithAspects<T>(this IExtend extend) =>
        extend.AddSpecific(builder =>
        {
            builder
                .RegisterType<T>()
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance()
                .ApplyAspects();
        });
}