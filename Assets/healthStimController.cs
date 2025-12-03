using UnityEngine;

public class healthStimController : UsableItem_Base
{
    public HealingStim HealthStimModel;
    public HealthStimView View;

    public KeyCode Space;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(Space))
        {
         HealthStimModel.Use_Rpc();   
         View.StimSound_RPC();
        }     
    }
}
