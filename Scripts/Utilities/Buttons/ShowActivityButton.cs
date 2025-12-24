#nullable enable
namespace UniT.UI.Utilities
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
    public abstract class ShowActivityByPrefabButton<TActivity, TParams> : View where TActivity : IActivityWithParams<TParams> where TParams : notnull
    {
        [SerializeField]     private TActivity    prefab  = default!;
        [SerializeReference] private TParams      @params = default!;
        [SerializeField]     private ActivityType type;

        protected override void OnInitialize()
        {
            this.GetComponent<Button>().onClick.AddListener(() => this.Manager.Show(this.Manager.Get<TActivity>(this.prefab), this.@params, this.type));
        }
    }

    [RequireComponent(typeof(Button))]
    public abstract class ShowActivityByKeyButton<TActivity> : View where TActivity : IActivityWithoutParams
    {
        [SerializeField] private string       key = string.Empty;
        [SerializeField] private ActivityType type;

        protected override void OnInitialize()
        {
            this.GetComponent<Button>().onClick.AddListener(() => this.Manager.Show(this.Manager.Get<TActivity>(this.key), this.type));
        }
    }

    [RequireComponent(typeof(Button))]
    public abstract class ShowActivityByKeyButton<TActivity, TParams> : View where TActivity : IActivityWithParams<TParams> where TParams : notnull
    {
        [SerializeField]     private string       key     = string.Empty;
        [SerializeReference] private TParams      @params = default!;
        [SerializeField]     private ActivityType type;

        protected override void OnInitialize()
        {
            this.GetComponent<Button>().onClick.AddListener(() => this.Manager.Show(this.Manager.Get<TActivity>(this.key), this.@params, this.type));
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
    public abstract class ShowActivityByTypeButton<TActivity, TParams> : View where TActivity : IActivityWithParams<TParams> where TParams : notnull
    {
        [SerializeReference] private TParams      @params = default!;
        [SerializeField]     private ActivityType type;

        protected override void OnInitialize()
        {
            this.GetComponent<Button>().onClick.AddListener(() => this.Manager.Show(this.Manager.Get<TActivity>(this.name), this.@params, this.type));
        }
    }
}