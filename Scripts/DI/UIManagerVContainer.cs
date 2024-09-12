#if UNIT_VCONTAINER
#nullable enable
namespace UniT.UI.DI
{
    using UniT.Logging.DI;
    using UniT.ResourceManagement.DI;
    using UnityEngine;
    using VContainer;
    using VContainer.Unity;

    public static class UIManagerVContainer
    {
        public static void RegisterUIManager(this IContainerBuilder builder)
        {
            if (builder.Exists(typeof(IUIManager), true)) return;
            builder.RegisterLoggerManager();
            builder.RegisterAssetsManager();
            builder.RegisterComponentInNewPrefab(_ => Resources.Load<RootUICanvas>(nameof(RootUICanvas)), Lifetime.Singleton);
            builder.Register<UIManager>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}
#endif