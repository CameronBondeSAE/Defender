namespace NicholasScripts
{
    /// <summary>
    /// 
    /// </summary>
    public class MGTurret : BaseTurret
    {
        protected override void Fire()
        {
            view.FireEffect();
            view.Fire();
        }
    }
}