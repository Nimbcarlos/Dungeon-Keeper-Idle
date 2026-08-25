using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace DungeonKeeper
{
    public static class SaveSystem
    {
        private static readonly byte[] SecretKey = new byte[32] 
        {
            0x44, 0x75, 0x6E, 0x67, 0x65, 0x6F, 0x6E, 0x4B, 0x65, 0x65, 0x70, 0x65, 0x72, 0x4B, 0x65, 0x79,
            0x32, 0x30, 0x32, 0x36, 0x5F, 0x53, 0x65, 0x63, 0x72, 0x65, 0x74, 0x4B, 0x65, 0x79, 0x21, 0x21
        };

        private static readonly byte[] SecretIV = new byte[16] 
        {
            0x44, 0x75, 0x6E, 0x67, 0x65, 0x6F, 0x6E, 0x49, 0x56, 0x5F, 0x39, 0x38, 0x37, 0x36, 0x35, 0x34
        };

        private static string SavePath => Path.Combine(Application.persistentDataPath, "save.dat");

        public static void Save(SaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, false);
                byte[] encryptedBytes = EncryptString(json, SecretKey, SecretIV);

                string tempPath = SavePath + ".tmp";
                File.WriteAllBytes(tempPath, encryptedBytes);

                if (File.Exists(SavePath))
                    File.Delete(SavePath);

                File.Move(tempPath, SavePath);
                Debug.Log($"🔒 Jogo salvo e criptografado em: {SavePath}");
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
                Debug.Log("Nenhum save encontrado — iniciando novo jogo");
                return new SaveData();
            }

            try
            {
                byte[] encryptedBytes = File.ReadAllBytes(SavePath);
                string json = DecryptString(encryptedBytes, SecretKey, SecretIV);

                Debug.Log("Jogo carregado com sucesso!");
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"🚨 Erro ao carregar save criptografado! Resetando... ({e.Message})");
                return new SaveData();
            }
        }

        public static bool HasSave() => File.Exists(SavePath);

        public static void DeleteSave()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
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