#if UNIT_DI
#nullable enable
namespace UniT.UI
{
    using UniT.DI;

    public static class DIBinder
    {
        public static void AddUIManager(
            this DependencyContainer container,
            RootUICanvas             rootUICanvas
        )
        {
            container.Add(rootUICanvas);
            container.AddInterfaces<UIManager>();
        }
    }
}
#endif