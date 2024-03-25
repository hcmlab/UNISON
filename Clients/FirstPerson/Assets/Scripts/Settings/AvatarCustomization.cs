using System;

namespace Settings
{
    [Serializable]
    public class AvatarCustomization
    {
        public AvatarBody Body { get; set; }
        public AvatarColor HairColor { get; set; }
        public AvatarColor SkinColor { get; set; }
        public AvatarColor MaskColor { get; set; }
        public AvatarColor ShirtColor { get; set; }
        public AvatarColor TrouserColor { get; set; }
        public AvatarColor ShoeColor { get; set; }

        public AvatarCustomization()
        {
        }

        public AvatarCustomization(AvatarBody body, AvatarColor hairColor, AvatarColor skinColor, AvatarColor maskColor,
            AvatarColor shirtColor, AvatarColor trouserColor, AvatarColor shoeColor)
        {
            Body = body;
            HairColor = hairColor;
            SkinColor = skinColor;
            MaskColor = maskColor;
            ShirtColor = shirtColor;
            TrouserColor = trouserColor;
            ShoeColor = shoeColor;
        }

        public static AvatarCustomization Deserialize(object[] serialized)
        {
            return new AvatarCustomization
            {
                Body = (AvatarBody)serialized[0],
                HairColor = (AvatarColor)serialized[1],
                SkinColor = (AvatarColor)serialized[2],
                MaskColor = (AvatarColor)serialized[3],
                ShirtColor = (AvatarColor)serialized[4],
                TrouserColor = (AvatarColor)serialized[5],
                ShoeColor = (AvatarColor)serialized[6],
            };
        }

        public object[] Serialize()
        {
            return new object[]
            {
                Body, HairColor, SkinColor, MaskColor, ShirtColor, TrouserColor, ShoeColor
            };
        }
    }
}