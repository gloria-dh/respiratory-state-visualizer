using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Respiratory_State_Visualizer_V0
{
    public partial class AvatarCustomize : UserControl
    {
        // Layer manager handles image fields and panel invalidation
        private AvatarLayerManager layers;

        // Objects
        private AvatarProfile currentProfile = new AvatarProfile();

        // EVENTS
        internal event Action<AvatarProfile> AvatarSaved;

        // Hair lookup: (typeIndex, colourIndex) → (resource accessor, enum value)
        private static readonly Dictionary<(int type, int colour), (Func<Image> GetImage, HairChoice Choice)> HairLookup =
            new Dictionary<(int type, int colour), (Func<Image>, HairChoice)>
            {
                // Black
                { (1, 0), (() => Properties.Resources.hair_long_black,   HairChoice.LongBlack) },
                { (2, 0), (() => Properties.Resources.hair_medium_black, HairChoice.MediumBlack) },
                { (3, 0), (() => Properties.Resources.hair_short_black,  HairChoice.ShortBlack) },
                // Brown
                { (1, 1), (() => Properties.Resources.hair_long_brown,   HairChoice.LongBrown) },
                { (2, 1), (() => Properties.Resources.hair_medium_brown, HairChoice.MediumBrown) },
                { (3, 1), (() => Properties.Resources.hair_short_brown,  HairChoice.ShortBrown) },
                // Blonde
                { (1, 2), (() => Properties.Resources.hair_long_blonde,   HairChoice.LongBlonde) },
                { (2, 2), (() => Properties.Resources.hair_medium_blonde, HairChoice.MediumBlonde) },
                { (3, 2), (() => Properties.Resources.hair_short_blonde,  HairChoice.ShortBlonde) },
                // Red
                { (1, 3), (() => Properties.Resources.hair_long_red,   HairChoice.LongRed) },
                { (2, 3), (() => Properties.Resources.hair_medium_red, HairChoice.MediumRed) },
                { (3, 3), (() => Properties.Resources.hair_short_red,  HairChoice.ShortRed) },
            };

        public AvatarCustomize()
        {
            InitializeComponent();

            this.DoubleBuffered = true;

            layers = new AvatarLayerManager(pnlAvatarCustomization);
            pnlAvatarCustomization.Paint += PnlAvatarCustomization_Paint;

            tableLayoutPanel1.BackColor = AppTheme.SlateGray;
            lblCustomize.ForeColor = AppTheme.Orange;

            CreateStartingLayers();
            SetComboBoxDefaultDisplay();
        }

        private void SetComboBoxDefaultDisplay()
        {
            cmbHairType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbHairColour.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbClothing.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAccessories.DropDownStyle = ComboBoxStyle.DropDownList;

            cmbHairType.SelectedIndex = 1;
            cmbHairColour.SelectedIndex = 0;
            cmbClothing.SelectedIndex = 0;
            cmbAccessories.SelectedIndex = 0;
        }

        private void CreateStartingLayers()
        {
            // Set default avatar
            layers.SetSkinTone(Properties.Resources.skin_2);
            layers.SetClothing(Properties.Resources.clothing_1);
            layers.SetOutline(Properties.Resources.main_outline);
            layers.SetFace(Properties.Resources.face_calm);
            layers.SetHair(Properties.Resources.hair_long_black);

            // Save default avatar choice to start
            currentProfile.SkinTone = SkinToneChoice.Skin2;
            currentProfile.Clothing = ClothingChoice.Clothing1;
            currentProfile.Hair = HairChoice.LongBlack;
            currentProfile.Accessories = AccessoryChoice.None;

            pnlAvatarCustomization.Invalidate(); // force redraw
        }

        // Drawing layers in order via shared painter
        private void PnlAvatarCustomization_Paint(object sender, PaintEventArgs e)
        {
            layers.PaintLayers(e.Graphics, pnlAvatarCustomization.ClientRectangle);
        }

        private void btnSkin1_Click(object sender, EventArgs e)
        {
            currentProfile.SkinTone = SkinToneChoice.Skin1;
            layers.SetSkinTone(Properties.Resources.skin_1);
        }

        private void btnSkin2_Click(object sender, EventArgs e)
        {
            currentProfile.SkinTone = SkinToneChoice.Skin2;
            layers.SetSkinTone(Properties.Resources.skin_2);
        }

        private void btnSkin3_Click(object sender, EventArgs e)
        {
            currentProfile.SkinTone = SkinToneChoice.Skin3;
            layers.SetSkinTone(Properties.Resources.skin_3);
        }

        private void btnSkin4_Click(object sender, EventArgs e)
        {
            currentProfile.SkinTone = SkinToneChoice.Skin4;
            layers.SetSkinTone(Properties.Resources.skin_4);
        }

        private void cmbHairType_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateHair();
        }

        private void cmbHairColour_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateHair();
        }

        private void UpdateHair()
        {
            if (cmbHairType.SelectedIndex == 0)
            {
                layers.SetHair(null);
                currentProfile.Hair = HairChoice.None;
                return;
            }

            var key = (type: cmbHairType.SelectedIndex, colour: cmbHairColour.SelectedIndex);
            if (HairLookup.TryGetValue(key, out var entry))
            {
                layers.SetHair(entry.GetImage());
                currentProfile.Hair = entry.Choice;
            }
        }

        private void cmbClothing_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbClothing.SelectedIndex == 0) // clothing 1
            {
                layers.SetClothing(Properties.Resources.clothing_1);
                currentProfile.Clothing = ClothingChoice.Clothing1;
            }
            else if (cmbClothing.SelectedIndex == 1) // clothing 2
            {
                layers.SetClothing(Properties.Resources.clothing_2);
                currentProfile.Clothing = ClothingChoice.Clothing2;
            }
        }

        private void cmbAccessories_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbAccessories.SelectedIndex == 0) // None
            {
                layers.SetAccessories(null);
                currentProfile.Accessories = AccessoryChoice.None;
            }
            else if (cmbAccessories.SelectedIndex == 1) // Headphones
            {
                layers.SetAccessories(Properties.Resources.Accessories_headphones);
                currentProfile.Accessories = AccessoryChoice.Headphones;
            }
        }

        internal void PullCurrentProfile()
        {
            AvatarSaved?.Invoke(currentProfile);
        }
    }
}
