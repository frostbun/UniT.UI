#nullable enable
namespace UniT.UI.Utilities
{
    public sealed class HideButton : GenericButton
    {
        protected override void OnClick()
        {
            this.Manager.Hide(this.Activity);
        }
    }
}