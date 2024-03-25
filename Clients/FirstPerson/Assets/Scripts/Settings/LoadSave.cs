using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Settings
{
    /// <summary>
    /// This class is used to load and save settings.
    /// </summary>
    public static class LoadSave
    {
        private static readonly string AudioPath = Application.persistentDataPath + "/audio_data.json";
        private static readonly string ControlPath = Application.persistentDataPath + "/control_data.json";
        private static readonly string PlayerSaveGamePath = Application.persistentDataPath + "/player_data.json";
        private static readonly string MasterSaveGamePath = Application.persistentDataPath + "/master_data.json";

        private static readonly string AvatarCustomizationPath =
            Application.persistentDataPath + "/avatar_customization.json";

        public static void SaveAudioDataToFile(AudioData audioData)
        {
            File.WriteAllText(AudioPath, JsonConvert.SerializeObject(audioData, Formatting.Indented));
        }

        public static AudioData LoadAudioDataFromFile()
        {
            try
            {
                return File.Exists(AudioPath)
                    ? JsonConvert.DeserializeObject<AudioData>(File.ReadAllText(AudioPath))
                    : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static void SaveControlDataToFile(ControlData controlData)
        {
            File.WriteAllText(ControlPath, JsonConvert.SerializeObject(controlData, Formatting.Indented));
        }

        public static ControlData LoadControlDataFromFile()
        {
            try
            {
                return File.Exists(ControlPath)
                    ? JsonConvert.DeserializeObject<ControlData>(File.ReadAllText(ControlPath))
                    : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static void SaveAvatarCustomizationToFile(AvatarCustomization avatarCustomization)
        {
            File.WriteAllText(AvatarCustomizationPath,
                JsonConvert.SerializeObject(avatarCustomization, Formatting.Indented));
        }

        public static AvatarCustomization LoadAvatarCustomizationFromFile()
        {
            try
            {
                return File.Exists(AvatarCustomizationPath)
                    ? JsonConvert.DeserializeObject<AvatarCustomization>(File.ReadAllText(AvatarCustomizationPath))
                    : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static void SavePlayerSaveGameToFile(PlayerSaveGame playerSaveGame)
        {
            File.WriteAllText(PlayerSaveGamePath, JsonConvert.SerializeObject(playerSaveGame, Formatting.Indented));
        }

        public static PlayerSaveGame LoadPlayerSaveGameFromFile()
        {
            try
            {
                return File.Exists(PlayerSaveGamePath)
                    ? JsonConvert.DeserializeObject<PlayerSaveGame>(File.ReadAllText(PlayerSaveGamePath))
                    : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static void SaveMasterSaveGamesToFile(IEnumerable masterSaveGames)
        {
            File.WriteAllText(MasterSaveGamePath, JsonConvert.SerializeObject(masterSaveGames, Formatting.Indented));
        }

        private static MasterSaveGame[] LoadMasterSaveGamesFromFile()
        {
            try
            {
                return File.Exists(MasterSaveGamePath)
                    ? JsonConvert.DeserializeObject<MasterSaveGame[]>(File.ReadAllText(MasterSaveGamePath))
                    : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Removes all items from the list of saved games where no related backend
        /// instance is known.
        /// </summary>
        /// <param name="backendInstances">A list of backend instances with their instanceUuid</param>
        public static List<MasterSaveGame> RefreshSaveGameData(List<string> backendInstances)
        {
            MasterSaveGame[] saveGames = LoadMasterSaveGamesFromFile();
            if (saveGames == null || saveGames.Length == 0) return null;
            if (backendInstances == null || backendInstances.Count == 0) return null;
            List<MasterSaveGame> list = saveGames.ToList();
            list.RemoveAll(saveGame => !backendInstances.Contains(saveGame.instanceUuid));
            saveGames = list.ToArray();
            SaveMasterSaveGamesToFile(saveGames);
            return list;
        }

        public static void AddOrUpdateMasterSaveGame(MasterSaveGame saveGame)
        {
            MasterSaveGame[] saveGames = LoadMasterSaveGamesFromFile();
            if (saveGames == null || saveGames.Length == 0)
            {
                saveGames = new[] { saveGame };
            }
            else
            {
                List<MasterSaveGame> list = saveGames.ToList();
                try
                {
                    list.Remove(list.Find((element) => element.instanceUuid == saveGame.instanceUuid));
                }
                catch (ArgumentNullException)
                {
                }
                finally
                {
                    list.Add(saveGame);
                    saveGames = list.ToArray();
                }
            }

            SaveMasterSaveGamesToFile(saveGames);
        }

        /// <summary>
        /// Adds or updates a saved game to the file.
        /// </summary>
        /// <param name="masterSaveGame">Instance of MasterSaveGame</param>
        public static void CreateOrUpdateMasterSaveGame(MasterSaveGame masterSaveGame)
        {
            MasterSaveGame[] saveGames = LoadMasterSaveGamesFromFile();
            if (saveGames == null || saveGames.Length == 0)
            {
                List<MasterSaveGame> list = new List<MasterSaveGame> { masterSaveGame };
                saveGames = list.ToArray();
            }
            else
            {
                List<MasterSaveGame> list = saveGames.ToList();
                if (list.Contains(masterSaveGame))
                {
                    list.Remove(masterSaveGame);
                }

                list.Add(masterSaveGame);
                saveGames = list.ToArray();
            }

            SaveMasterSaveGamesToFile(saveGames);
        }

        public static void ClearMasterSaveGames()
        {
            File.WriteAllText(MasterSaveGamePath, "");
        }
    }
}