#if UNIT_DI
#nullable enable
namespace UniT.UI.DI
{
    using UniT.DI;
    using UniT.Logging.DI;
    using UniT.ResourceManagement.DI;

    public static class UIManagerDI
    {
        public static void AddUIManager(this DependencyContainer container)
        {
            if (container.Contains<IUIManager>()) return;
            container.AddLoggerManager();
            container.AddAssetsManager();
            container.AddFromComponentInNewPrefabResource<RootUICanvas>(nameof(RootUICanvas));
            container.AddInterfaces<UIManager>();
        }
    }
}
#endif