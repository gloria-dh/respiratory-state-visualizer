using Respiratory_State_Visualizer_V0.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Respiratory_State_Visualizer_V0
{
    public partial class AvatarCustomize : UserControl
    {
        // IMAGES
        private Image mainOutline; // Fixed
        private Image face;        // Fixed

        private Image hair;
        private Image skinTone;
        private Image clothing;
        private Image accesories;

        // Objects
        private AvatarProfile currentProfile = new AvatarProfile();

        // EVENTS
        internal event Action<AvatarProfile> AvatarSaved;

        public AvatarCustomize()
        {
            InitializeComponent();

            this.DoubleBuffered = true;

            pnlAvatarCustomization.Paint += pnlAvatarCustomization_Paint;

            tableLayoutPanel1.BackColor = MainForm.SlateGray;
            lblCustomize.ForeColor = MainForm.Orange;

            createStartingLayers();
            setComboBoxDefaultDisplay();
        }

        private void setComboBoxDefaultDisplay()
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

        private void createStartingLayers()
        {
            // Set default avatar
            skinTone = Properties.Resources.skin_2;
            clothing = Properties.Resources.clothing_1;
            mainOutline = Properties.Resources.main_outline;
            face = Properties.Resources.face_calm;
            hair = Properties.Resources.hair_long_black;
            // Add accessory default

            // Save default avatar choice to start
            currentProfile.SkinTone = SkinToneChoice.skin_2;
            currentProfile.Clothing = ClothingChoice.clothing_1;
            currentProfile.Hair = HairChoice.long_black;
            currentProfile.Accessories = AccessoryChoice.None;

            pnlAvatarCustomization.Invalidate(); // force redraw


        }

        // Drawing layers in order in the panel
        private void pnlAvatarCustomization_Paint(object sender, PaintEventArgs e)
        {
            Rectangle r = pnlAvatarCustomization.ClientRectangle;

            // Bottom -> top
            if (skinTone != null) e.Graphics.DrawImage(skinTone, r);
            if (clothing != null) e.Graphics.DrawImage(clothing, r);
            if (mainOutline != null) e.Graphics.DrawImage(mainOutline, r);
            if (face != null) e.Graphics.DrawImage(face, r);
            if (hair != null) e.Graphics.DrawImage(hair, r);
            if (accesories != null) e.Graphics.DrawImage(accesories, r);
        }

        // Setters for customization buttons
        internal void setSkin(Image img) {skinTone = img; pnlAvatarCustomization.Invalidate();}
        internal void setClothing(Image img) {clothing = img; pnlAvatarCustomization.Invalidate();}
        internal void setOutline(Image img) { mainOutline = img; pnlAvatarCustomization.Invalidate(); }
        internal void setFace(Image img) {face = img; pnlAvatarCustomization.Invalidate();}
        internal void setHair(Image img) {hair = img; pnlAvatarCustomization.Invalidate();}
        internal void setAccesories(Image img) {accesories = img; pnlAvatarCustomization.Invalidate();}

     

        private void btnSkin1_Click(object sender, EventArgs e)
        {
            currentProfile.SkinTone = SkinToneChoice.skin_1;
            setSkin(Properties.Resources.skin_1);
        }

        private void btnSkin2_Click(object sender, EventArgs e)
        {
            currentProfile.SkinTone = SkinToneChoice.skin_2;
            setSkin(Properties.Resources.skin_2);
        }

        private void btnSkin3_Click(object sender, EventArgs e)
        {
            currentProfile.SkinTone = SkinToneChoice.skin_3;
            setSkin(Properties.Resources.skin_3);
        }

        private void btnSkin4_Click(object sender, EventArgs e)
        {
            currentProfile.SkinTone = SkinToneChoice.skin_4;
            setSkin(Properties.Resources.skin_4);
        }

        private void cmbHairType_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateHair();
        }

        private void cmbHairColour_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateHair();
        }

        private void updateHair()
        {
            if (cmbHairType.SelectedIndex == 0)
            {
                setHair(null);
                currentProfile.Hair = HairChoice.None;
                return;
            }

            if (cmbHairColour.SelectedIndex == 0) // black
            {
                if (cmbHairType.SelectedIndex == 1) // long
                {
                    setHair(Properties.Resources.hair_long_black);
                    currentProfile.Hair = HairChoice.long_black;
                }
                else if (cmbHairType.SelectedIndex == 2) // medium
                {
                    setHair(Properties.Resources.hair_medium_black);
                    currentProfile.Hair = HairChoice.medium_black;
                }
                else if (cmbHairType.SelectedIndex == 3) // short
                {
                    setHair(Properties.Resources.hair_short_black);
                    currentProfile.Hair = HairChoice.short_black;
                }
            }
            else if (cmbHairColour.SelectedIndex == 1) // brown
            {
                if (cmbHairType.SelectedIndex == 1) // long
                {
                    setHair(Properties.Resources.hair_long_brown);
                    currentProfile.Hair = HairChoice.long_brown;
                }
                else if (cmbHairType.SelectedIndex == 2) // medium
                {
                    setHair(Properties.Resources.hair_medium_brown);
                    currentProfile.Hair = HairChoice.medium_brown;
                }
                else if (cmbHairType.SelectedIndex == 3) // short
                {
                    setHair(Properties.Resources.hair_short_brown);
                    currentProfile.Hair = HairChoice.short_brown;
                }
            }
            else if (cmbHairColour.SelectedIndex == 2) // blonde
            {
                if (cmbHairType.SelectedIndex == 1) // long
                {
                    setHair(Properties.Resources.hair_long_blonde);
                    currentProfile.Hair = HairChoice.long_blonde;
                }
                else if (cmbHairType.SelectedIndex == 2) // medium
                {
                    setHair(Properties.Resources.hair_medium_blonde);
                    currentProfile.Hair = HairChoice.medium_blonde;
                }
                else if (cmbHairType.SelectedIndex == 3) // short
                {
                    setHair(Properties.Resources.hair_short_blonde);
                    currentProfile.Hair = HairChoice.short_blonde;
                }
            }
            else if (cmbHairColour.SelectedIndex == 3) // red
            {
                if (cmbHairType.SelectedIndex == 1) // long
                {
                    setHair(Properties.Resources.hair_long_red);
                    currentProfile.Hair = HairChoice.long_red;
                }
                else if (cmbHairType.SelectedIndex == 2) // medium
                {
                    setHair(Properties.Resources.hair_medium_red);
                    currentProfile.Hair = HairChoice.medium_red;
                }
                else if (cmbHairType.SelectedIndex == 3) // short
                {
                    setHair(Properties.Resources.hair_short_red);
                    currentProfile.Hair = HairChoice.short_red;
                }
            }
        }

        private void cmbClothing_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbClothing.SelectedIndex == 0) // clothing 1
            {
                setClothing(Properties.Resources.clothing_1);
                currentProfile.Clothing = ClothingChoice.clothing_1;
            }
            else if (cmbClothing.SelectedIndex == 1) // clothing 2
            {
                setClothing(Properties.Resources.clothing_2);
                currentProfile.Clothing = ClothingChoice.clothing_2;
            }
        }

        private void cmbAccessories_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbAccessories.SelectedIndex == 0) // None
            {
                setAccesories(null);
            }
            else if (cmbAccessories.SelectedIndex == 1) // Headphones
            {
                setAccesories(Properties.Resources.Accessories_headphones);
                currentProfile.Accessories = AccessoryChoice.headphones;
            }
        }

        
      
        internal void PullCurrentProfile()
        {
            AvatarSaved?.Invoke(currentProfile);
        }
    }
}
