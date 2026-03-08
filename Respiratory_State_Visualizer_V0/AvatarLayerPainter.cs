using System.Drawing;

namespace Respiratory_State_Visualizer_V0
{
    /// <summary>
    /// Shared helper that draws avatar image layers in the correct z-order.
    /// Used by both the Customize and Run views.
    /// </summary>
    internal static class AvatarLayerPainter
    {
        internal static void PaintLayers(
            Graphics g,
            Rectangle bounds,
            Image skinTone,
            Image clothing,
            Image mainOutline,
            Image face,
            Image hair,
            Image accessories,
            Image chestLevel = null,
            Image breath = null)
        {
            // Maintain aspect ratio: fit a square centred in the available bounds
            int side = System.Math.Min(bounds.Width, bounds.Height);
            int x = bounds.X + (bounds.Width - side) / 2;
            int y = bounds.Y + (bounds.Height - side) / 2;
            Rectangle dest = new Rectangle(x, y, side, side);

            // Bottom → top draw order
            if (skinTone != null) g.DrawImage(skinTone, dest);
            if (clothing != null) g.DrawImage(clothing, dest);
            if (mainOutline != null) g.DrawImage(mainOutline, dest);
            if (face != null) g.DrawImage(face, dest);
            if (hair != null) g.DrawImage(hair, dest);
            if (accessories != null) g.DrawImage(accessories, dest);
            if (chestLevel != null) g.DrawImage(chestLevel, dest);
            if (breath != null) g.DrawImage(breath, dest);
        }
    }
}
