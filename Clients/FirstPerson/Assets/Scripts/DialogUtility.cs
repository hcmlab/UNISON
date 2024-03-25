using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Backend;
using Backend.ResponseDataCapsules;
using UI.Menu;
using UnityEngine;

public static class DialogUtility
{
    public static readonly List<Scale> PetitionScales = new List<Scale>
    {
        Scale.IncomeTaxLevel, Scale.PropertyTaxLevel, Scale.HealthInsurance, Scale.SchoolCostFree,
        Scale.Lockdown, Scale.MinimumWage, Scale.Disappropriation, Scale.SocialSafety,
        Scale.StockIncomeTaxExempt, Scale.TaxRevenueForVaccine, Scale.HappierPlayers, Scale.TownHallAllowance,
    };

    private static readonly Dictionary<string, Dictionary<Scale, int>> numberTabsByLanguageScale =
        new Dictionary<string, Dictionary<Scale, int>>
        {
            {
                MainMenu.GermanLanguage, new Dictionary<Scale, int>
                {
                    { Scale.IncomeTaxLevel, 4 },
                    { Scale.PropertyTaxLevel, 5 },
                    { Scale.HealthInsurance, 1 },
                    { Scale.SchoolCostFree, 4 },
                    { Scale.Lockdown, 6 },
                    { Scale.MinimumWage, 5 },
                    { Scale.Disappropriation, 5 },
                    { Scale.SocialSafety, 3 },
                    { Scale.StockIncomeTaxExempt, 3 },
                    { Scale.TaxRevenueForVaccine, 2 },
                    { Scale.HappierPlayers, 4 },
                    { Scale.TownHallAllowance, 4 },
                }
            },
            {
                MainMenu.EnglishLanguage, new Dictionary<Scale, int>
                {
                    { Scale.IncomeTaxLevel, 6 },
                    { Scale.PropertyTaxLevel, 5 },
                    { Scale.HealthInsurance, 2 },
                    { Scale.SchoolCostFree, 4 },
                    { Scale.Lockdown, 6 },
                    { Scale.MinimumWage, 5 },
                    { Scale.Disappropriation, 4 },
                    { Scale.SocialSafety, 3 },
                    { Scale.StockIncomeTaxExempt, 4 },
                    { Scale.TaxRevenueForVaccine, 3 },
                    { Scale.HappierPlayers, 5 },
                    { Scale.TownHallAllowance, 3 },
                }
            }
        };

    public static string CreatePetitionEffectsText(Dictionary<Scale, int> scaleValues, List<Petition> petitions)
    {
        HashSet<Scale> scales = GetScalesWithOpenPetitions(petitions);

        StringBuilder stringBuilder = new StringBuilder();
        foreach (KeyValuePair<Scale, int> scaleValue in scaleValues)
        {
            bool hasOpenPetition = scales.Contains(scaleValue.Key);
            if (hasOpenPetition)
            {
                stringBuilder.Append("<u>");
            }

            switch (scaleValue.Key)
            {
                case Scale.IncomeTaxLevel:
                    stringBuilder.Append(LocalizationUtility.GetLocalizedString("petitionEffectIncomeTaxLevel"));
                    if (hasOpenPetition)
                    {
                        stringBuilder.Append("</u>");
                    }

                    stringBuilder.Append(CreateTabs(Scale.IncomeTaxLevel)).Append(scaleValue.Value).Append(" %");
                    break;

                case Scale.PropertyTaxLevel:
                    stringBuilder.Append(LocalizationUtility.GetLocalizedString("petitionEffectPropertyTaxLevel"));
                    if (hasOpenPetition)
                    {
                        stringBuilder.Append("</u>");
                    }

                    stringBuilder.Append(CreateTabs(Scale.PropertyTaxLevel)).Append(scaleValue.Value).Append(" %");
                    break;

                case Scale.HealthInsurance:
                    stringBuilder.Append(LocalizationUtility.GetLocalizedString("petitionEffectHealthInsurance"));
                    if (hasOpenPetition)
                    {
                        stringBuilder.Append("</u>");
                    }

                    stringBuilder.Append(CreateTabs(Scale.HealthInsurance)).Append(IntToBoolAsString(scaleValue.Value));
                    break;

                case Scale.SchoolCostFree:
                    stringBuilder.Append(LocalizationUtility.GetLocalizedString("petitionEffectSchoolCostFree"));
                    if (hasOpenPetition)
                    {
                        stringBuilder.Append("</u>");
                    }

                    stringBuilder.Append(CreateTabs(Scale.SchoolCostFree)).Append(IntToBoolAsString(scaleValue.Value));
                    break;

                case Scale.Lockdown:
                    stringBuilder.Append(LocalizationUtility.GetLocalizedString("petitionEffectLockdown"));
                    if (hasOpenPetition)
                    {
                        stringBuilder.Append("</u>");
                    }

                    stringBuilder.Append(CreateTabs(Scale.Lockdown)).Append(IntToBoolAsString(scaleValue.Value));
                    break;

                case Scale.MinimumWage:
                    stringBuilder.Append(LocalizationUtility.GetLocalizedString("petitionEffectMinimumWage"));
                    if (hasOpenPetition)
                    {
                        stringBuilder.Append("</u>");
                    }

                    stringBuilder.Append(CreateTabs(Scale.MinimumWage)).Append(scaleValue.Value).Append(" MU");
                    break;

                case Scale.Disappropriation:
                    stringBuilder.Append(LocalizationUtility.GetLocalizedString("petitionEffectDisappropriation"));
                    if (hasOpenPetition)
                    {
                        stringBuilder.Append("</u>");
                    }

                    stringBuilder.Append(CreateTabs(Scale.Disappropriation))
                        .Append(IntToBoolAsString(scaleValue.Value));
                    break;

                case Scale.SocialSafety:
                    stringBuilder.Append(LocalizationUtility.GetLocalizedString("petitionEffectSocialSafety"));
                    if (hasOpenPetition)
                    {
                        stringBuilder.Append("</u>");
                    }

                    stringBuilder.Append(CreateTabs(Scale.SocialSafety)).Append(IntToBoolAsString(scaleValue.Value));
                    break;

                case Scale.StockIncomeTaxExempt:
                    stringBuilder.Append(LocalizationUtility.GetLocalizedString("petitionEffectStockIncomeTaxExempt"));
                    if (hasOpenPetition)
                    {
                        stringBuilder.Append("</u>");
                    }

                    stringBuilder.Append(CreateTabs(Scale.StockIncomeTaxExempt)).Append(IntToBoolAsString(scaleValue.Value));
                    break;

                case Scale.TaxRevenueForVaccine:
                    stringBuilder.Append(LocalizationUtility.GetLocalizedString("petitionEffectTaxRevenueForVaccine"));
                    if (hasOpenPetition)
                    {
                        stringBuilder.Append("</u>");
                    }

                    stringBuilder.Append(CreateTabs(Scale.TaxRevenueForVaccine)).Append(scaleValue.Value).Append(" %");
                    break;

                case Scale.HappierPlayers:
                    stringBuilder.Append(LocalizationUtility.GetLocalizedString("petitionEffectHappierPlayers"));
                    if (hasOpenPetition)
                    {
                        stringBuilder.Append("</u>");
                    }

                    stringBuilder.Append(CreateTabs(Scale.HappierPlayers)).Append(IntToBoolAsString(scaleValue.Value));
                    break;

                case Scale.TownHallAllowance:
                    stringBuilder.Append(LocalizationUtility.GetLocalizedString("petitionEffectTownHallAllowance"));
                    if (hasOpenPetition)
                    {
                        stringBuilder.Append("</u>");
                    }

                    stringBuilder.Append(CreateTabs(Scale.TownHallAllowance)).Append(scaleValue.Value).Append(" MU");
                    break;

                default:
                    throw new UnityException($"Unknown scale ID {scaleValue.Key}!");
            }

            stringBuilder.Append("\n");
        }

        return stringBuilder
            .Append("\n")
            .Append(LocalizationUtility.GetLocalizedString("effectsOpenPetitionsHighlighted"))
            .ToString();
    }

    private static HashSet<Scale> GetScalesWithOpenPetitions(List<Petition> petitions)
    {
        HashSet<Scale> scales = new HashSet<Scale>();
        foreach (Petition petition in petitions)
        {
            if (petition.Status == "open")
            {
                scales.Add(petition.Scale);
            }
        }

        return scales;
    }

    private static string CreateTabs(Scale scale)
    {
        return string.Concat(Enumerable.Repeat("\t", numberTabsByLanguageScale[MainMenu.CurrentLanguage][scale]));
    }

    private static string IntToBoolAsString(int value)
    {
        return value switch
        {
            0 => LocalizationUtility.GetLocalizedString("no"),
            1 => LocalizationUtility.GetLocalizedString("yes"),
            _ => throw new UnityException($"Cannot parse {value} to bool!")
        };
    }

    public static string CreatePetitionText(Template template, int value)
    {
        return template.ValueType switch
        {
            "bool" => template.CurrentValue switch
            {
                0 => template.PositiveText,
                1 => template.NegativeText,
                _ => throw new UnityException($"Unknown bool value \"{template.CurrentValue}\"!")
            },

            "int" => template.PositiveText
                .Replace("{{x}}", template.CurrentValue.ToString(CultureInfo.InvariantCulture))
                .Replace("{{y}}", value.ToString(CultureInfo.InvariantCulture)),

            _ => throw new UnityException($"Unknown value type \"{template.ValueType}\"!")
        };
    }

    public static string CreatePetitionText(Template template)
    {
        return template.ValueType switch
        {
            "bool" => template.CurrentValue switch
            {
                0 => template.PositiveText,
                1 => template.NegativeText,
                _ => throw new UnityException($"Unknown bool value \"{template.CurrentValue}\"!")
            },

            "int" => template.PositiveText
                .Replace("{{x}}", template.CurrentValue.ToString(CultureInfo.InvariantCulture))
                .Replace("{{y}}", "x"),

            _ => throw new UnityException($"Unknown value type \"{template.ValueType}\"!")
        };
    }

    public static string CreatePetitionText(Petition petition)
    {
        return petition.ValueType switch
        {
            "bool" => petition.ProposedValue switch
            {
                0 => petition.NegativeText,
                1 => petition.PositiveText,
                _ => throw new UnityException($"Unknown bool value \"{petition.ProposedValue}\"!")
            },

            "int" => petition.PositiveText
                .Replace("{{x}}", petition.CurrentValue.ToString(CultureInfo.InvariantCulture))
                .Replace("{{y}}", petition.ProposedValue.ToString(CultureInfo.InvariantCulture)),

            _ => throw new UnityException($"Unknown value type \"{petition.ValueType}\"!")
        };
    }
}
