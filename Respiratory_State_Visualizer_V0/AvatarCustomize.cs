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

        // Currently selected hair type: 0 = None, 1 = Long, 2 = Medium, 3 = Short
        private int selectedHairType = 1;

        // Currently selected hair colour: 0 = Black, 1 = Brown, 2 = Blonde, 3 = Red
        private int selectedHairColour = 0;

        // Currently selected skin tone: 0 = Skin1, 1 = Skin2, 2 = Skin3, 3 = Skin4
        private int selectedSkinTone = 1;

        // Currently selected clothing: 0 = Clothing1, 1 = Clothing2
        private int selectedClothing = 0;

        // Currently selected accessory: 0 = None, 1 = Headphones
        private int selectedAccessory = 0;

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

            // Wire up hair preview bar clicks
            pbHairNone.Click += pbHairNone_Click;
            pbHairLong.Click += pbHairLong_Click;
            pbHairMedium.Click += pbHairMedium_Click;
            pbHairShort.Click += pbHairShort_Click;

            // Wire up hair colour bar clicks
            pbHairBlack.Click += pbHairBlack_Click;
            pbHairBrown.Click += pbHairBrown_Click;
            pbHairBlond.Click += pbHairBlond_Click;
            pbHairRed.Click += pbHairRed_Click;

            // Wire up skin tone bar clicks
            pbSkin1.Click += pbSkin1_Click;
            pbSkin2.Click += pbSkin2_Click;
            pbSkin3.Click += pbSkin3_Click;
            pbSkin4.Click += pbSkin4_Click;

            // Load hair colour preview images
            pbHairBlack.Image = Properties.Resources.hairBlack;
            pbHairBrown.Image = Properties.Resources.hairBrown;
            pbHairBlond.Image = Properties.Resources.hairBlond;
            pbHairRed.Image = Properties.Resources.hairRed;

            // Load skin tone preview images (zoomed-in versions)
            pbSkin1.Image = Properties.Resources.skin_1_click;
            pbSkin2.Image = Properties.Resources.skin_2_click;
            pbSkin3.Image = Properties.Resources.skin_3_click;
            pbSkin4.Image = Properties.Resources.skin_4_click;

            // Load clothing preview images
            pbClothing1.Image = Properties.Resources.clothing_1;
            pbClothing2.Image = Properties.Resources.clothing_2;

            // Wire up clothing bar clicks
            pbClothing1.Click += pbClothing1_Click;
            pbClothing2.Click += pbClothing2_Click;

            // Wire up accessories bar clicks
            pbAccNone.Click += pbAccNone_Click;
            pbAccHeadphones.Click += pbAccHeadphones_Click;

            // Load accessories preview image
            pbAccHeadphones.Image = Properties.Resources.Accessories_headphones;

            // Remove default border and use custom Paint for selection outline
            foreach (var pb in new[] { pbHairNone, pbHairLong, pbHairMedium, pbHairShort,
                                       pbHairBlack, pbHairBrown, pbHairBlond, pbHairRed,
                                       pbSkin1, pbSkin2, pbSkin3, pbSkin4,
                                       pbClothing1, pbClothing2,
                                       pbAccNone, pbAccHeadphones })
            {
                pb.BorderStyle = System.Windows.Forms.BorderStyle.None;
                pb.Paint += HairPB_Paint;
            }

            CreateStartingLayers();
            RefreshHairPreviews();
            HighlightSelectedHairPB();
            HighlightSelectedColourPB();
            HighlightSelectedSkinPB();
            HighlightSelectedClothingPB();
            HighlightSelectedAccessoryPB();
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

        private void pbSkin1_Click(object sender, EventArgs e)
        {
            selectedSkinTone = 0;
            HighlightSelectedSkinPB();
            currentProfile.SkinTone = SkinToneChoice.Skin1;
            layers.SetSkinTone(Properties.Resources.skin_1);
        }

        private void pbSkin2_Click(object sender, EventArgs e)
        {
            selectedSkinTone = 1;
            HighlightSelectedSkinPB();
            currentProfile.SkinTone = SkinToneChoice.Skin2;
            layers.SetSkinTone(Properties.Resources.skin_2);
        }

        private void pbSkin3_Click(object sender, EventArgs e)
        {
            selectedSkinTone = 2;
            HighlightSelectedSkinPB();
            currentProfile.SkinTone = SkinToneChoice.Skin3;
            layers.SetSkinTone(Properties.Resources.skin_3);
        }

        private void pbSkin4_Click(object sender, EventArgs e)
        {
            selectedSkinTone = 3;
            HighlightSelectedSkinPB();
            currentProfile.SkinTone = SkinToneChoice.Skin4;
            layers.SetSkinTone(Properties.Resources.skin_4);
        }

        // Hair preview bar click handlers

        private void pbHairNone_Click(object sender, EventArgs e)
        {
            selectedHairType = 0;
            HighlightSelectedHairPB();
            UpdateHair();
        }

        private void pbHairLong_Click(object sender, EventArgs e)
        {
            selectedHairType = 1;
            HighlightSelectedHairPB();
            UpdateHair();
        }

        private void pbHairMedium_Click(object sender, EventArgs e)
        {
            selectedHairType = 2;
            HighlightSelectedHairPB();
            UpdateHair();
        }

        private void pbHairShort_Click(object sender, EventArgs e)
        {
            selectedHairType = 3;
            HighlightSelectedHairPB();
            UpdateHair();
        }

        // Hair colour bar click handlers

        private void pbHairBlack_Click(object sender, EventArgs e)
        {
            selectedHairColour = 0;
            HighlightSelectedColourPB();
            RefreshHairPreviews();
            UpdateHair();
        }

        private void pbHairBrown_Click(object sender, EventArgs e)
        {
            selectedHairColour = 1;
            HighlightSelectedColourPB();
            RefreshHairPreviews();
            UpdateHair();
        }

        private void pbHairBlond_Click(object sender, EventArgs e)
        {
            selectedHairColour = 2;
            HighlightSelectedColourPB();
            RefreshHairPreviews();
            UpdateHair();
        }

        private void pbHairRed_Click(object sender, EventArgs e)
        {
            selectedHairColour = 3;
            HighlightSelectedColourPB();
            RefreshHairPreviews();
            UpdateHair();
        }


        private void RefreshHairPreviews()
        {
            int colourIndex = selectedHairColour;

            pbHairNone.Image = null; // "None" is always blank

            if (HairLookup.TryGetValue((1, colourIndex), out var longEntry))
                pbHairLong.Image = longEntry.GetImage();

            if (HairLookup.TryGetValue((2, colourIndex), out var medEntry))
                pbHairMedium.Image = medEntry.GetImage();

            if (HairLookup.TryGetValue((3, colourIndex), out var shortEntry))
                pbHairShort.Image = shortEntry.GetImage();
        }


        private void HighlightSelectedHairPB()
        {
            Color normalBg = Color.FromArgb(80, 80, 80);
            Color selectedBg = AppTheme.Orange;

            pbHairNone.BackColor = (selectedHairType == 0) ? selectedBg : normalBg;
            pbHairLong.BackColor = (selectedHairType == 1) ? selectedBg : normalBg;
            pbHairMedium.BackColor = (selectedHairType == 2) ? selectedBg : normalBg;
            pbHairShort.BackColor = (selectedHairType == 3) ? selectedBg : normalBg;
        }


        private void HighlightSelectedColourPB()
        {
            Color normalBg = Color.FromArgb(80, 80, 80);
            Color selectedBg = AppTheme.Orange;

            pbHairBlack.BackColor = (selectedHairColour == 0) ? selectedBg : normalBg;
            pbHairBrown.BackColor = (selectedHairColour == 1) ? selectedBg : normalBg;
            pbHairBlond.BackColor = (selectedHairColour == 2) ? selectedBg : normalBg;
            pbHairRed.BackColor = (selectedHairColour == 3) ? selectedBg : normalBg;
        }


        private void HighlightSelectedSkinPB()
        {
            Color normalBg = Color.FromArgb(80, 80, 80);
            Color selectedBg = AppTheme.Orange;

            pbSkin1.BackColor = (selectedSkinTone == 0) ? selectedBg : normalBg;
            pbSkin2.BackColor = (selectedSkinTone == 1) ? selectedBg : normalBg;
            pbSkin3.BackColor = (selectedSkinTone == 2) ? selectedBg : normalBg;
            pbSkin4.BackColor = (selectedSkinTone == 3) ? selectedBg : normalBg;
        }


        private void HighlightSelectedClothingPB()
        {
            Color normalBg = Color.FromArgb(80, 80, 80);
            Color selectedBg = AppTheme.Orange;

            pbClothing1.BackColor = (selectedClothing == 0) ? selectedBg : normalBg;
            pbClothing2.BackColor = (selectedClothing == 1) ? selectedBg : normalBg;
        }

        // Draws a selection border on PictureBoxes
        private void HairPB_Paint(object sender, PaintEventArgs e)
        {
            PictureBox pb = (PictureBox)sender;
            using (Pen pen = new Pen(pb.BackColor, 3))
            {
                Rectangle rect = new Rectangle(1, 1, pb.Width - 2, pb.Height - 2);
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        private void UpdateHair()
        {
            if (selectedHairType == 0)
            {
                layers.SetHair(null);
                currentProfile.Hair = HairChoice.None;
                return;
            }

            int colourIndex = selectedHairColour;

            var key = (type: selectedHairType, colour: colourIndex);
            if (HairLookup.TryGetValue(key, out var entry))
            {
                layers.SetHair(entry.GetImage());
                currentProfile.Hair = entry.Choice;
            }
        }

        private void pbClothing1_Click(object sender, EventArgs e)
        {
            selectedClothing = 0;
            HighlightSelectedClothingPB();
            layers.SetClothing(Properties.Resources.clothing_1);
            currentProfile.Clothing = ClothingChoice.Clothing1;
        }

        private void pbClothing2_Click(object sender, EventArgs e)
        {
            selectedClothing = 1;
            HighlightSelectedClothingPB();
            layers.SetClothing(Properties.Resources.clothing_2);
            currentProfile.Clothing = ClothingChoice.Clothing2;
        }

        private void pbAccNone_Click(object sender, EventArgs e)
        {
            selectedAccessory = 0;
            HighlightSelectedAccessoryPB();
            layers.SetAccessories(null);
            currentProfile.Accessories = AccessoryChoice.None;
        }

        private void pbAccHeadphones_Click(object sender, EventArgs e)
        {
            selectedAccessory = 1;
            HighlightSelectedAccessoryPB();
            layers.SetAccessories(Properties.Resources.Accessories_headphones);
            currentProfile.Accessories = AccessoryChoice.Headphones;
        }


        private void HighlightSelectedAccessoryPB()
        {
            Color normalBg = Color.FromArgb(80, 80, 80);
            Color selectedBg = AppTheme.Orange;

            pbAccNone.BackColor = (selectedAccessory == 0) ? selectedBg : normalBg;
            pbAccHeadphones.BackColor = (selectedAccessory == 1) ? selectedBg : normalBg;
        }

        internal void PullCurrentProfile()
        {
            AvatarSaved?.Invoke(currentProfile);
        }
    }
}
