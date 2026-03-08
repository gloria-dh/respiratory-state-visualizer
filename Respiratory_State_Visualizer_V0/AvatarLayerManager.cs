using System.Drawing;
using System.Windows.Forms;

namespace Respiratory_State_Visualizer_V0
{
    // Manages avatar image layers and invalidates the panel when they change.
    internal sealed class AvatarLayerManager
    {
        private readonly Control targetPanel;

        internal Image SkinTone { get; private set; }
        internal Image Clothing { get; private set; }
        internal Image Outline { get; private set; }
        internal Image Face { get; private set; }
        internal Image Hair { get; private set; }
        internal Image Accessories { get; private set; }
        internal Image ChestLevel { get; private set; }
        internal Image Breath { get; private set; }

        internal AvatarLayerManager(Control panel)
        {
            targetPanel = panel;
        }

        internal void SetSkinTone(Image img) { SkinTone = img; targetPanel.Invalidate(); }
        internal void SetClothing(Image img) { Clothing = img; targetPanel.Invalidate(); }
        internal void SetOutline(Image img) { Outline = img; targetPanel.Invalidate(); }
        internal void SetFace(Image img) { Face = img; targetPanel.Invalidate(); }
        internal void SetHair(Image img) { Hair = img; targetPanel.Invalidate(); }
        internal void SetAccessories(Image img) { Accessories = img; targetPanel.Invalidate(); }
        internal void SetChestLevel(Image img) { ChestLevel = img; targetPanel.Invalidate(); }
        internal void SetBreath(Image img) { Breath = img; targetPanel.Invalidate(); }

        internal void PaintLayers(Graphics g, Rectangle bounds)
        {
            AvatarLayerPainter.PaintLayers(g, bounds,
                SkinTone, Clothing, Outline, Face, Hair, Accessories,
                ChestLevel, Breath);
        }
    }
}
