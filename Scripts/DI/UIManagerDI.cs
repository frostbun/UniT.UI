#if UNIT_DI
#nullable enable
namespace UniT.UI.DI
{
    using UniT.DI;
    using UniT.Extensions;
    using UniT.Logging.DI;
    using UniT.ResourceManagement.DI;
    using UnityEngine;

    public static class UIManagerDI
    {
        public static void AddUIManager(this DependencyContainer container)
        {
            if (container.Contains<IUIManager>()) return;
            container.AddLoggerManager();
            container.AddAssetsManager();
            container.Add(Resources.Load<GameObject>(nameof(RootUICanvas)).Instantiate().GetComponentOrThrow<RootUICanvas>());
            container.AddInterfaces<UIManager>();
        }
    }
}
#endif