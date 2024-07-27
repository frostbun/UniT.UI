#if UNIT_ZENJECT
#nullable enable
namespace UniT.UI
{
    using UniT.Logging;
    using UniT.ResourceManagement;
    using Zenject;

    public static class ZenjectBinder
    {
        public static void BindUIManager(this DiContainer container)
        {
            if (container.HasBinding<IUIManager>()) return;
            container.BindLoggerManager();
            container.BindAssetsManager();
            container.Bind<RootUICanvas>().FromNewComponentOnNewGameObject().AsSingle();
            container.BindInterfacesTo<UIManager>().AsSingle();
        }
    }
}
#endif