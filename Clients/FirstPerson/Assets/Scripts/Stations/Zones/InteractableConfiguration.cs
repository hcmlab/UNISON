using System.Collections.Generic;
using UnityEngine;

namespace Stations.Zones
{
    public static class InteractableConfiguration
    {
        public static float GlobalLastInteractionTimestamp = float.MinValue;

        public static readonly Dictionary<ZoneType, float> LastInteractionTimestampByZones =
            new Dictionary<ZoneType, float>();

        public static readonly Dictionary<ZoneType, float> CooldownInSecondsByZones = new Dictionary<ZoneType, float>();

        private const float DefaultCooldownInSeconds = 1f;

        public static (float, float) CalculateRemainingCooldown(ZoneType zoneType)
        {
            if (!LastInteractionTimestampByZones.TryGetValue(zoneType, out float lastInteractionTimestamp))
            {
                lastInteractionTimestamp = float.MinValue;
            }

            float cooldownInSeconds = GetCooldownInSecondsForZone(zoneType);

            float remainingCooldown = lastInteractionTimestamp + cooldownInSeconds - Time.time;
            float remainingGlobalCooldown = GlobalLastInteractionTimestamp + DefaultCooldownInSeconds - Time.time;

            return remainingCooldown >= remainingGlobalCooldown
                ? (Mathf.Max(remainingCooldown, 0), cooldownInSeconds)
                : (Mathf.Max(remainingGlobalCooldown, 0), DefaultCooldownInSeconds);
        }

        public static void ResetCooldowns()
        {
            GlobalLastInteractionTimestamp = float.MinValue;
            LastInteractionTimestampByZones.Clear();
        }

        public static float GetCooldownInSecondsForZone(ZoneType zoneType)
        {
            return CooldownInSecondsByZones.TryGetValue(zoneType, out float cooldownInSeconds)
                ? cooldownInSeconds
                : DefaultCooldownInSeconds;
        }
    }
}