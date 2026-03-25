using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TartarusMUD.Models;

namespace TartarusMUD.Core
{
    public class SaveManager
    {
        private readonly string _saveDirectory = "Saves";

        public SaveManager()
        {
            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }
        }

        // Zahashuje heslo pomocí SHA-256 (bezpečnostní standard)
        public string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public bool AccountExists(string playerName)
        {
            return File.Exists(GetSaveFilePath(playerName));
        }

        public void SavePlayer(Player player, string passwordHash)
        {
            var saveData = new PlayerSaveData
            {
                Name = player.Name,
                PasswordHash = passwordHash,
                CurrentRoomId = player.CurrentRoom?.Id,
                Inventory = player.Inventory
            };

            string json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(GetSaveFilePath(player.Name), json);
        }

        public PlayerSaveData LoadPlayer(string playerName)
        {
            string filePath = GetSaveFilePath(playerName);
            if (!File.Exists(filePath)) return null;

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<PlayerSaveData>(json);
        }

        private string GetSaveFilePath(string playerName)
        {
            // Očistíme jméno od neplatných znaků, aby šlo použít jako název souboru
            string safeName = string.Join("_", playerName.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(_saveDirectory, $"{safeName}.json");
        }
    }
}