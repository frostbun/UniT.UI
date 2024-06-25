#nullable enable
namespace UniT.UI.Utilities.Buttons
{
    using UniT.UI.Activity;
    using UniT.UI.View;
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Button))]
    public abstract class ShowActivityButton<TActivity> : View where TActivity : IActivityWithoutParams
    {
        protected override void OnInitialize()
        {
            this.GetComponent<Button>().onClick.AddListener(() => this.Manager.Show<TActivity>());
        }
    }

    [RequireComponent(typeof(Button))]
    public abstract class ShowActivityButton<TActivity, TParams> : View where TActivity : IActivityWithParams<TParams>
    {
        [SerializeField] private TParams @params = default!;

        protected override void OnInitialize()
        {
            this.GetComponent<Button>().onClick.AddListener(() => this.Manager.Show<TActivity, TParams>(this.@params));
        }
    }
}