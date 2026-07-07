using System;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;

public class SaveLoadManager
{
    public static readonly object _fileLock = new();

    // Serialisasi operasi I/O async — tulis & baca file tidak boleh tumpang
    // tindih (race korupsi save, B-14). lock() tidak bisa dipakai lintas await.
    private static readonly System.Threading.SemaphoreSlim _ioLock = new(1, 1);

    private static readonly bool UseEncryption = true;

    // CATATAN: key hardcoded = obfuscation agar save tidak bisa diedit kasual,
    // BUKAN keamanan sungguhan (siapa pun yang decompile bisa membacanya).
    // Untuk save game single-player ini cukup — jangan over-engineer.
    private static readonly string EncryptionKey = "mySecureKey12345"; // 16 chars for AES-128

    // Save data asynchronously with optional encryption
    public static async Task SaveAsync<T>(T data) where T : BaseSaveData
    {
        if (data == null)
        {
            Debug.LogError("Save data is null.");
            return;
        }

        string filePath = GetFilePath(data.FileName);
        string backupPath = filePath + ".bak";

        await _ioLock.WaitAsync();
        try
        {
            string json = JsonUtility.ToJson(data, prettyPrint: true);

            if (UseEncryption)
                json = Encrypt(json);

            // Backup the old file before saving
            if (File.Exists(filePath))
                File.Copy(filePath, backupPath, true);

            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saving {typeof(T).Name} to {filePath}: {ex.Message}");
        }
        finally
        {
            _ioLock.Release();
        }
    }

    // Load data asynchronously with optional decryption
    public static async Task<T> LoadAsync<T>() where T : BaseSaveData, new()
    {
        T data = new();
        string filePath = GetFilePath(data.FileName);

        await _ioLock.WaitAsync();
        try
        {
            if (File.Exists(filePath))
            {
                string json = await File.ReadAllTextAsync(filePath);

                if (UseEncryption)
                    json = Decrypt(json);

                data = JsonUtility.FromJson<T>(json);
            }
            else
            {
                Debug.LogWarning($"File not found at {filePath}. Returning default data.");
            }
        }
        catch (Exception ex) when (ex is CryptographicException || ex is FormatException)
        {
            // Save korup / format lama (pra-IV) — mulai fresh, jangan crash (B-14)
            Debug.LogWarning($"Save {typeof(T).Name} korup atau format lama ({ex.GetType().Name}). Memulai dengan data default. Path: {filePath}");
            data = new();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading {typeof(T).Name} from {filePath}: {ex.Message}");
        }
        finally
        {
            _ioLock.Release();
        }

        return data;
    }

    /// <summary>
    /// Deletes all save files where the corresponding BaseSaveData class
    /// has its 'IsResetable' property set to true.
    /// </summary>
    public static async Task<(bool success, string message)> ResetResettableDataAsync()
    {
        Debug.Log("[SaveLoadManager] Starting global data reset...");
        var saveDataTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseSaveData)))
            .ToList();

        if (!saveDataTypes.Any())
        {
            Debug.LogWarning("[SaveLoadManager] No classes inheriting from BaseSaveData were found in the project.");
            return (true, "No save data classes found to reset.");
        }

        Debug.Log($"[SaveLoadManager] Found {saveDataTypes.Count} classes inheriting from BaseSaveData.");

        var filesToDelete = new List<string>();
        var errorMessages = new List<string>();
        int deletedCount = 0;
        int skippedCount = 0;
        int filesThatExist = 0;

        foreach (var type in saveDataTypes)
        {
            try
            {
                var dataInstance = Activator.CreateInstance(type) as BaseSaveData;
                if (dataInstance == null)
                {
                    Debug.LogError($"[SaveLoadManager] Failed to create an instance of {type.Name}. Skipping.");
                    continue;
                }

                Debug.Log($"[SaveLoadManager] Checking '{type.Name}'. IsResetable = {dataInstance.IsResetable}.");

                if (dataInstance.IsResetable)
                {
                    string path = GetFilePath(dataInstance.FileName);
                    Debug.Log("[SaveLoadManager] -> Found resettable file: " + path);
                    if (File.Exists(path))
                    {
                        filesToDelete.Add(path);
                        filesThatExist++;
                        Debug.Log($"[SaveLoadManager] -> Queued for deletion: {dataInstance.FileName}");
                    }
                }
                else
                {
                    skippedCount++;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"CRITICAL ERROR on '{type.Name}': {ex.Message}";
                errorMessages.Add(errorMessage);
                Debug.LogError($"[SaveLoadManager] {errorMessage}");
            }
        }

        if (errorMessages.Any())
        {
            return (false, errorMessages.First());
        }

        if (filesThatExist == 0)
        {
            Debug.LogWarning("[SaveLoadManager] No existing save files were found for the resettable data types.");
        }

        await Task.Run(() =>
        {
            lock (_fileLock)
            {
                foreach (var filePath in filesToDelete)
                {
                    File.Delete(filePath);
                    deletedCount++;
                }
            }
        });

        string message = $"Success! Deleted {deletedCount} of {filesThatExist} existing files. Skipped {skippedCount} non-resettable files.";
        Debug.Log($"[SaveLoadManager] Reset complete. {message}");
        return (true, message);
    }
    /// <summary>
    /// Deletes specific save files based on a list of BaseSaveData types.
    /// </summary>
    public static async Task<(bool success, string message)> ResetSelectedDataAsync(List<Type> saveDataTypes)
    {
        if (saveDataTypes == null || !saveDataTypes.Any())
        {
            return (true, "No data selected to reset.");
        }

        var filesToDelete = new List<string>();
        var errorMessages = new List<string>();
        int deletedCount = 0;

        foreach (var type in saveDataTypes)
        {
            if (type == null) continue;
            try
            {
                if (Activator.CreateInstance(type) is BaseSaveData dataInstance)
                {
                    filesToDelete.Add(GetFilePath(dataInstance.FileName));
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Failed to create instance of '{type.Name}'. Details: {ex.Message}";
                errorMessages.Add(errorMessage);
                Debug.LogError($"[SaveLoadManager] {errorMessage}");
            }
        }

        if (errorMessages.Any())
        {
            return (false, $"Error: {errorMessages.First()}");
        }

        await Task.Run(() =>
        {
            lock (_fileLock)
            {
                foreach (var filePath in filesToDelete)
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        deletedCount++;
                    }
                }
            }
        });

        return (true, $"Success! {deletedCount} selected data file(s) have been deleted.");
    }

    public static bool Delete(BaseSaveData data)
    {
        string filePath = GetFilePath(data.FileName);

        lock (_fileLock)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                else
                {
                    Debug.LogWarning($"File not found: {filePath}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error deleting file {filePath}: {ex.Message}");
                return false;
            }
        }
    }

    // Delete saved file
    public static bool Delete<T>() where T : BaseSaveData, new()
    {
        T data = new();
        return Delete(data);
    }

    // Check if the save file exists
    public static bool Exists(string fileName)
    {
        string filePath = GetFilePath(fileName);
        return File.Exists(filePath);
    }

    // Get the file path in persistent data
    private static string GetFilePath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    // Encryption method using AES — IV acak per save, di-prepend ke ciphertext.
    // IV nol membuat enkripsi deterministik (dua save identik = ciphertext
    // identik) dan melemahkan AES-CBC (B-14).
    private static string Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = System.Text.Encoding.UTF8.GetBytes(EncryptionKey);
            aes.GenerateIV();

            using (var ms = new MemoryStream())
            {
                // 16 byte pertama payload = IV
                ms.Write(aes.IV, 0, aes.IV.Length);

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var writer = new StreamWriter(cs))
                {
                    writer.Write(plainText);
                }
                // StreamWriter/CryptoStream dispose = flush final block

                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    // Decryption method using AES — baca IV dari 16 byte pertama payload.
    private static string Decrypt(string cipherText)
    {
        byte[] payload = Convert.FromBase64String(cipherText);
        if (payload.Length <= 16)
        {
            throw new CryptographicException("Save data terlalu pendek — file korup atau bukan format save yang dikenal.");
        }

        using (Aes aes = Aes.Create())
        {
            aes.Key = System.Text.Encoding.UTF8.GetBytes(EncryptionKey);

            byte[] iv = new byte[16];
            Array.Copy(payload, 0, iv, 0, 16);
            aes.IV = iv;

            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream(payload, 16, payload.Length - 16))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var reader = new StreamReader(cs))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
