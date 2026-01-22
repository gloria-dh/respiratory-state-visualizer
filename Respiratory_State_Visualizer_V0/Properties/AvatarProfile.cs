using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respiratory_State_Visualizer_V0.Properties
{
    internal class AvatarProfile
    {
        internal HairChoice Hair {  get; set; }
        internal ClothingChoice Clothing { get; set; }
        internal SkinToneChoice SkinTone { get; set; }
        internal AccessoryChoice Accessories {  get; set; } 
    }

    internal enum HairChoice { None, long_black, long_brown, long_blonde, long_red, 
                                medium_black, medium_brown, medium_blonde, medium_red,
                                short_black, short_brown, short_blonde, short_red}
    internal enum ClothingChoice { clothing_1, clothing_2 }
    internal enum SkinToneChoice { skin_1, skin_2, skin_3, skin_4 }
    internal enum AccessoryChoice { None, headphones}
}
