#if UNIT_VCONTAINER
#nullable enable
namespace UniT.UI.DI
{
    using UniT.DI;
    using UniT.Logging.DI;
    using UniT.ResourceManagement.DI;
    using VContainer;

    public static class UIManagerVContainer
    {
        public static void RegisterUIManager(this IContainerBuilder builder)
        {
            if (builder.Exists(typeof(IUIManager), true)) return;
            builder.RegisterDependencyContainer();
            builder.RegisterLoggerManager();
            builder.RegisterAssetsManager();
            builder.RegisterComponentInNewPrefabResource<RootUICanvas>(nameof(RootUICanvas), Lifetime.Singleton);
            builder.Register<UIManager>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}
#endif