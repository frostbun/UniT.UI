#if UNIT_DI
#nullable enable
namespace UniT.UI
{
    using UniT.DI;
    using UniT.Extensions;
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
            container.Add(Resources.Load<GameObject>(nameof(RootUICanvas)).GetComponentOrThrow<RootUICanvas>());
            container.AddInterfaces<UIManager>();
        }
    }
}
#endif