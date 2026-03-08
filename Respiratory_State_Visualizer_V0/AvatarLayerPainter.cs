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
            Image breath = null,
            Image cheeks = null)
        {
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
