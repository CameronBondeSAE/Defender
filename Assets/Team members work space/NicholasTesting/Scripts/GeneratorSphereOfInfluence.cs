using Shapes;
using UnityEngine;

[ExecuteInEditMode]
public class GeneratorSphereOfInfluence : ImmediateModeShapeDrawer
{
    public Color activeColour = new Color(0f, 1f, 0f, 0.02f);
    public Color inactiveColour = new Color(1f, 0.5f, 0f, 0.02f);

    public NicholasScripts.Generator generator;

    public override void DrawShapes(Camera cam)
    {
        base.DrawShapes(cam);

        if (generator == null)
            return;

        using (Draw.Command(cam))
        {
            Draw.ResetAllDrawStates();
            Draw.BlendMode = ShapesBlendMode.Additive;
            Draw.Thickness = 4f;
            Draw.LineGeometry = LineGeometry.Volumetric3D;
            Draw.ThicknessSpace = ThicknessSpace.Meters;

            // Set color based on whether the generator has been used
            Draw.Color = generator.IsUsed ? activeColour : inactiveColour;

            // Get radius directly from the generator model
            Draw.Radius = generator.PowerRange;

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