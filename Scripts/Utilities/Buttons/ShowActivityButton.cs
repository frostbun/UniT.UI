#nullable enable
namespace UniT.UI.Utilities.Buttons
{
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Button))]
    public abstract class ShowActivityByPrefabButton<TActivity> : View where TActivity : IActivityWithoutParams
    {
        [SerializeField] private TActivity    prefab = default!;
        [SerializeField] private ActivityType type;

        protected override void OnInitialize()
        {
            this.GetComponent<Button>().onClick.AddListener(() => this.Manager.Show(this.Manager.Get<TActivity>(this.prefab), this.type));
        }
    }

    [RequireComponent(typeof(Button))]
    public abstract class ShowActivityByPrefabButton<TActivity, TParams> : View where TActivity : IActivityWithParams
    {
        [SerializeField]     private TActivity    prefab  = default!;
        [SerializeReference] private TParams      @params = default!;
        [SerializeField]     private ActivityType type;

        protected override void OnInitialize()
        {
            this.GetComponent<Button>().onClick.AddListener(() => this.Manager.Show(this.Manager.Get<TActivity>(this.prefab), this.@params!, this.type));
        }
    }

    [RequireComponent(typeof(Button))]
    public abstract class ShowActivityByNameButton<TActivity> : View where TActivity : IActivityWithoutParams
    {
        [SerializeField] private new string       name = default!;
        [SerializeField] private     ActivityType type;

        protected override void OnInitialize()
        {
            this.GetComponent<Button>().onClick.AddListener(() => this.Manager.Show(this.Manager.Get<TActivity>(this.name), this.type));
        }
    }

    [RequireComponent(typeof(Button))]
    public abstract class ShowActivityByNameButton<TActivity, TParams> : View where TActivity : IActivityWithParams
    {
        [SerializeField]     private new string       name    = default!;
        [SerializeReference] private     TParams      @params = default!;
        [SerializeField]     private     ActivityType type;

        protected override void OnInitialize()
        {
            this.GetComponent<Button>().onClick.AddListener(() => this.Manager.Show(this.Manager.Get<TActivity>(this.name), this.@params!, this.type));
        }
    }

    [RequireComponent(typeof(Button))]
    public abstract class ShowActivityByTypeButton<TActivity> : View where TActivity : IActivityWithoutParams
    {
        [SerializeField] private ActivityType type;

        protected override void OnInitialize()
        {
            this.GetComponent<Button>().onClick.AddListener(() => this.Manager.Show(this.Manager.Get<TActivity>(this.name), this.type));
        }
    }

    [RequireComponent(typeof(Button))]
    public abstract class ShowActivityByTypeButton<TActivity, TParams> : View where TActivity : IActivityWithParams
    {
        [SerializeReference] private TParams      @params = default!;
        [SerializeField]     private ActivityType type;

        protected override void OnInitialize()
        {
            this.GetComponent<Button>().onClick.AddListener(() => this.Manager.Show(this.Manager.Get<TActivity>(this.name), this.@params!, this.type));
        }
    }
}