#if UNIT_ZENJECT
#nullable enable
namespace UniT.UI
{
    using UniT.Logging;
    using UniT.ResourceManagement;
    using Zenject;

    public static class ZenjectBinder
    {
        public static void BindUIManager(
            this DiContainer container,
            RootUICanvas     rootUICanvas
        )
        {
            if (container.HasBinding<IUIManager>()) return;
            container.BindLoggerManager();
            container.BindResourceManagers();
            container.BindInstance(rootUICanvas).AsSingle();
            container.BindInterfacesTo<UIManager>().AsSingle();
        }
    }
}
#endif