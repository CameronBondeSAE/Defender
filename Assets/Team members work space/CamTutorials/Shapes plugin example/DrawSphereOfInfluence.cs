using Shapes;
using UnityEngine;

public class DrawSphereOfInfluence : ImmediateModeShapeDrawer
{
	public override void DrawShapes(Camera cam)
	{
		base.DrawShapes(cam);

		using( Draw.Command( cam ) ) // all immediate mode drawing should happen within these using-statements
		// using (Shapes.Draw.Command(Camera.current))
		{
			// Set up draw state
			Draw.ResetAllDrawStates(); // ensure everything is set to their defaults
			Draw.BlendMode = ShapesBlendMode.Additive;
			Draw.Thickness = 4f;
			Draw.LineGeometry = LineGeometry.Volumetric3D; // : LineGeometry.Flat2D;
			Draw.ThicknessSpace = ThicknessSpace.Meters;
			// Draw.Color = new Color(1, 1f, 1, 0.02f);
			Draw.Color = new Color(0, 1f, 0, 0.02f);
			
			Draw.Radius = 5f;

			// Position + rotate drawing to XZ plane
			Matrix4x4 matrix = Matrix4x4.TRS(
				transform.position,                   // move to object's XZ
				Quaternion.Euler(90f, 0f, 0f),        // rotate from XY to XZ
				Vector3.one
			);
			Draw.Matrix = matrix;
			
			Draw.Disc(Vector3.zero);

			// Draw.Sphere(transform.position, 4f);
			Draw.ThicknessSpace = ThicknessSpace.Meters;
		}
	}

	// Draw.Color = Color.green;
	// 	Draw.Sphere(transform.position, 13.2f);
	// 	Draw.Arc(transform.position, 0, 360, ArcEndCap.None);
	// }
}