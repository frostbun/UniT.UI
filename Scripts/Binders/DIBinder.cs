#if UNIT_DI
#nullable enable
namespace UniT.UI
{
    using UniT.DI;
    using UniT.Logging;
    using UniT.ResourceManagement;

    public static class DIBinder
    {
        public static void AddUIManager(
            this DependencyContainer container,
            RootUICanvas             rootUICanvas
        )
        {
            if (container.Contains<IUIManager>()) return;
            container.AddLoggerManager();
            container.AddResourceManagers();
            container.Add(rootUICanvas);
            container.AddInterfaces<UIManager>();
        }
    }
}
#endif