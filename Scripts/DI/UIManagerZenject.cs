#if UNIT_ZENJECT
#nullable enable
namespace UniT.UI.DI
{
    using UniT.DI;
    using UniT.Logging.DI;
    using UniT.ResourceManagement.DI;
    using Zenject;

    public static class UIManagerZenject
    {
        public static void BindUIManager(this DiContainer container)
        {
            if (container.HasBinding<IUIManager>()) return;
            container.BindDependencyContainer();
            container.BindLoggerManager();
            container.BindAssetsManager();
            container.Bind<RootUICanvas>().FromComponentInNewPrefabResource(nameof(RootUICanvas)).AsSingle();
            container.BindInterfacesTo<UIManager>().AsSingle();
        }
    }
}
#endif