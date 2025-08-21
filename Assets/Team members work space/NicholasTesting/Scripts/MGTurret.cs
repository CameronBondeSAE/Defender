namespace NicholasScripts
{
    /// <summary>
    /// Machine-gun turret: BaseTurret implementation that triggers view effects and bullet spawn.
    /// </summary>
    public class MGTurret : BaseTurret
    {
        protected override void Fire()
        {
            // view.FireEffect();
            if (view != null)
                view.FireServer(); 
        }
    }
}