namespace NicholasScripts
{
    public class MGTurret : BaseTurret
    {
        protected override void Fire()
        {
            view.FireEffect();
            view.Fire();
        }
    }
}