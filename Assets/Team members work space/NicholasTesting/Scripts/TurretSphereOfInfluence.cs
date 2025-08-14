using Shapes;
using UnityEngine;

[ExecuteInEditMode]
public class TurretSphereOfInfluence : ImmediateModeShapeDrawer
{
    public Color activeColour = new Color(0f, 1f, 0f, 0.02f);
    public Color inactiveColour = new Color(1f, 0.5f, 0f, 0.02f);
    public float radius = 5f;
    public NicholasScripts.Model_Turret turretModel;

    public override void DrawShapes(Camera cam)
    {
        base.DrawShapes(cam);

        using (Draw.Command(cam))
        {
            Draw.ResetAllDrawStates();
            Draw.BlendMode = ShapesBlendMode.Additive;
            Draw.Thickness = 4f;
            Draw.LineGeometry = LineGeometry.Volumetric3D;
            Draw.ThicknessSpace = ThicknessSpace.Meters;

            // Choose colour based on turret activation
            if (turretModel != null && turretModel.isActivated)
                Draw.Color = activeColour;
            else
                Draw.Color = inactiveColour;

            Draw.Radius = radius;

            Matrix4x4 matrix = Matrix4x4.TRS(
                transform.position,
                Quaternion.Euler(90f, 0f, 0f),
                Vector3.one
            );
            Draw.Matrix = matrix;

            Draw.Disc(Vector3.zero);
        }
    }
}