#if UNIT_ZENJECT
#nullable enable
namespace UniT.UI
{
    using Zenject;

    public static class ZenjectBinder
    {
        public static void BindUIManager(
            this DiContainer container,
            RootUICanvas     rootUICanvas
        )
        {
            container.BindInstance(rootUICanvas).AsSingle();
            container.BindInterfacesTo<UIManager>().AsSingle();
        }
    }
}
#endif