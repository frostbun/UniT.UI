#if UNIT_DI
#nullable enable
namespace UniT.UI
{
    using UniT.DI;
    using UniT.Logging;
    using UniT.ResourceManagement;
    using UnityEngine;

    public static class DIBinder
    {
        public static void AddUIManager(this DependencyContainer container)
        {
            if (container.Contains<IUIManager>()) return;
            container.AddLoggerManager();
            container.AddAssetsManager();
            container.Add(new GameObject().AddComponent<RootUICanvas>());
            container.AddInterfaces<UIManager>();
        }
    }
}
#endif