namespace Respiratory_State_Visualizer_V0
{
    internal class AvatarProfile
    {
        internal HairChoice Hair { get; set; }
        internal ClothingChoice Clothing { get; set; }
        internal SkinToneChoice SkinTone { get; set; }
        internal AccessoryChoice Accessories { get; set; }
    }

    internal enum HairChoice
    {
        None,
        LongBlack, LongBrown, LongBlonde, LongRed,
        MediumBlack, MediumBrown, MediumBlonde, MediumRed,
        ShortBlack, ShortBrown, ShortBlonde, ShortRed
    }

    internal enum ClothingChoice { Clothing1, Clothing2 }
    internal enum SkinToneChoice { Skin1, Skin2, Skin3, Skin4 }
    internal enum AccessoryChoice { None, Headphones }
}
