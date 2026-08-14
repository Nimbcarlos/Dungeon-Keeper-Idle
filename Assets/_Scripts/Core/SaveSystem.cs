using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace DungeonKeeper
{
    public static class SaveSystem
    {
        // Chave e IV secretos do seu jogo (32 bytes e 16 bytes)
        private static readonly byte[] SecretKey = Encoding.UTF8.GetBytes("DungeonKeeperSecretKey2026_12345"); // 32 chars
        private static readonly byte[] SecretIV  = Encoding.UTF8.GetBytes("DungeonIV_9876543");               // 16 chars

        private static string SavePath => Path.Combine(Application.persistentDataPath, "save.dat");

        public static void Save(SaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                byte[] encryptedBytes = EncryptString(json, SecretKey, SecretIV);

                File.WriteAllBytes(SavePath, encryptedBytes);
                Debug.Log("🔒 Jogo salvo e criptografado com sucesso!");
            }
            catch (Exception e)
            {
                Debug.LogError($"Erro ao salvar o jogo: {e.Message}");
            }
        }

        public static SaveData Load()
        {
            if (!File.Exists(SavePath))
            {
                Debug.Log("Nenhum save encontrado. Criando novo jogo...");
                return new SaveData();
            }

            try
            {
                byte[] encryptedBytes = File.ReadAllBytes(SavePath);
                string json = DecryptString(encryptedBytes, SecretKey, SecretIV);

                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"🚨 Tentativa de alteração no Save ou arquivo corrompido! Resetando... ({e.Message})");
                return new SaveData();
            }
        }

        // ── AES CRYPTOGRAPHY ──

        private static byte[] EncryptString(string plainText, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }
                    return ms.ToArray();
                }
            }
        }

        private static string DecryptString(byte[] cipherText, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(cipherText))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}