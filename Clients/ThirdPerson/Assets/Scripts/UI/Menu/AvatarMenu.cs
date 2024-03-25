using System;
using System.Collections.Generic;
using System.Linq;
using Settings;
using UI.Auxiliary;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace UI.Menu
{
    public class AvatarMenu : MonoBehaviour
    {
        [SerializeField] private AvatarBodySwitch bodyItemSwitch;
        [SerializeField] private AvatarColorSwitch hairColorItemSwitch;
        [SerializeField] private AvatarColorSwitch skinColorItemSwitch;
        [SerializeField] private AvatarColorSwitch maskColorItemSwitch;
        [SerializeField] private AvatarColorSwitch shirtColorItemSwitch;
        [SerializeField] private AvatarColorSwitch trouserColorItemSwitch;
        [SerializeField] private AvatarColorSwitch shoeColorItemSwitch;
        [SerializeField] private Button randomCustomizationButton;

        private Dictionary<string, AvatarBody> bodies;
        private Dictionary<string, AvatarColor> hairColorByNames;
        private Dictionary<string, AvatarColor> maskColorByNames;
        private Dictionary<string, AvatarColor> shirtColorByNames;
        private Dictionary<string, AvatarColor> shoeColorByNames;
        private Dictionary<string, AvatarColor> skinColorByNames;
        private Dictionary<string, AvatarColor> trouserColorByNames;

        private void Awake()
        {
            randomCustomizationButton.onClick.AddListener(RandomCustomization);
        }

        private void OnEnable()
        {
            InitLists();
            InitItemSwitches();
        }

        private void OnDisable()
        {
            LoadSave.SaveAvatarCustomizationToFile(AvatarSettings.AvatarCustomization);
        }

        private void InitItemSwitches()
        {
            bodyItemSwitch.SetupItemSwitch(bodies,
                AvatarSettings.AvatarCustomization.Body,
                body => AvatarSettings.AvatarCustomization.Body = body);

            hairColorItemSwitch.SetupItemSwitch(hairColorByNames,
                AvatarSettings.AvatarCustomization.HairColor,
                color => AvatarSettings.AvatarCustomization.HairColor = color);

            skinColorItemSwitch.SetupItemSwitch(skinColorByNames,
                AvatarSettings.AvatarCustomization.SkinColor,
                color => AvatarSettings.AvatarCustomization.SkinColor = color);

            maskColorItemSwitch.SetupItemSwitch(maskColorByNames,
                AvatarSettings.AvatarCustomization.MaskColor,
                color => AvatarSettings.AvatarCustomization.MaskColor = color);

            shirtColorItemSwitch.SetupItemSwitch(shirtColorByNames,
                AvatarSettings.AvatarCustomization.ShirtColor,
                color => AvatarSettings.AvatarCustomization.ShirtColor = color);

            trouserColorItemSwitch.SetupItemSwitch(trouserColorByNames,
                AvatarSettings.AvatarCustomization.TrouserColor,
                color => AvatarSettings.AvatarCustomization.TrouserColor = color);

            shoeColorItemSwitch.SetupItemSwitch(shoeColorByNames,
                AvatarSettings.AvatarCustomization.ShoeColor,
                color => AvatarSettings.AvatarCustomization.ShoeColor = color);
        }

        private void InitLists()
        {
            bodies = new Dictionary<string, AvatarBody>();
            foreach (AvatarBody avatarBody in Enum.GetValues(typeof(AvatarBody)))
            {
                bodies[avatarBody.GetDisplayName()] = avatarBody;
            }

            hairColorByNames = new Dictionary<string, AvatarColor>();
            foreach (var avatarColor in AvatarColorExtensions.HairColors)
            {
                hairColorByNames[avatarColor.GetDisplayName()] = avatarColor;
            }

            skinColorByNames = new Dictionary<string, AvatarColor>();
            foreach (var avatarColor in AvatarColorExtensions.SkinColors)
            {
                skinColorByNames[avatarColor.GetDisplayName()] = avatarColor;
            }

            maskColorByNames = new Dictionary<string, AvatarColor>();
            foreach (var avatarColor in AvatarColorExtensions.MaskColors)
            {
                maskColorByNames[avatarColor.GetDisplayName()] = avatarColor;
            }

            shirtColorByNames = new Dictionary<string, AvatarColor>();
            foreach (var avatarColor in AvatarColorExtensions.ShirtColors)
            {
                shirtColorByNames[avatarColor.GetDisplayName()] = avatarColor;
            }

            trouserColorByNames = new Dictionary<string, AvatarColor>();
            foreach (var avatarColor in AvatarColorExtensions.TrouserColors)
            {
                trouserColorByNames[avatarColor.GetDisplayName()] = avatarColor;
            }

            shoeColorByNames = new Dictionary<string, AvatarColor>();
            foreach (var avatarColor in AvatarColorExtensions.ShoeColors)
            {
                shoeColorByNames[avatarColor.GetDisplayName()] = avatarColor;
            }
        }

        private void RandomCustomization()
        {
            var rand = new Random();
            var avatarBodies = Enum.GetValues(typeof(AvatarBody)).Cast<AvatarBody>().ToList();

            AvatarSettings.AvatarCustomization.Body = avatarBodies[rand.Next(avatarBodies.Count)];

            AvatarSettings.AvatarCustomization.HairColor =
                AvatarColorExtensions.HairColors[rand.Next(AvatarColorExtensions.HairColors.Count)];

            AvatarSettings.AvatarCustomization.SkinColor =
                AvatarColorExtensions.SkinColors[rand.Next(AvatarColorExtensions.SkinColors.Count)];

            AvatarSettings.AvatarCustomization.MaskColor =
                AvatarColorExtensions.MaskColors[rand.Next(AvatarColorExtensions.MaskColors.Count)];

            AvatarSettings.AvatarCustomization.ShirtColor =
                AvatarColorExtensions.ShirtColors[rand.Next(AvatarColorExtensions.ShirtColors.Count)];

            AvatarSettings.AvatarCustomization.TrouserColor =
                AvatarColorExtensions.TrouserColors[rand.Next(AvatarColorExtensions.TrouserColors.Count)];

            AvatarSettings.AvatarCustomization.ShoeColor =
                AvatarColorExtensions.ShoeColors[rand.Next(AvatarColorExtensions.ShoeColors.Count)];

            InitItemSwitches();
            LoadSave.SaveAvatarCustomizationToFile(AvatarSettings.AvatarCustomization);
        }
    }
}