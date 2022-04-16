namespace Lifeboat.RoleModifiers;

public abstract class BaseModifier
{
    public PlayerControl Owner { get; set; }

    public abstract string GetImportantTaskText();
    public abstract string GetGameSummaryDescription();

    public virtual void Awake() { }
    public virtual void Start() { }
    public virtual void Update() { }
    public virtual void OnDestroy() { }
}