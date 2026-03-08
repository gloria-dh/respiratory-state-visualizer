using System.Drawing;

namespace Respiratory_State_Visualizer_V0
{
    // Draws avatar image layers in the correct z-order.
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
            Image breath = null,
            Image cheeks = null)
        {
            // Maintain aspect ratio: fit a square centred in the available bounds
            int side = System.Math.Min(bounds.Width, bounds.Height);
            int x = bounds.X + (bounds.Width - side) / 2;
            int y = bounds.Y + (bounds.Height - side) / 2;
            Rectangle dest = new Rectangle(x, y, side, side);

            // Bottom → top draw order
            if (skinTone != null) g.DrawImage(skinTone, bounds);
            if (clothing != null) g.DrawImage(clothing, bounds);
            if (mainOutline != null) g.DrawImage(mainOutline, bounds);
            if (face != null) g.DrawImage(face, bounds);
            if (hair != null) g.DrawImage(hair, bounds);
            if (accessories != null) g.DrawImage(accessories, bounds);
            if (chestLevel != null) g.DrawImage(chestLevel, bounds);
            if (breath != null) g.DrawImage(breath, bounds);
            if (cheeks != null) g.DrawImage(cheeks, bounds);
        }
    }
}
