using System;
using System.IO;
using System.Text.Json;

namespace StonePACS.Services
{
    public class SettingsService
    {
        // Singleton Instance (เรียกใช้ได้จากทุกหน้า)
        private static SettingsService? _current;
        public static SettingsService Current => _current ??= LoadSettings();

        // Path ของไฟล์ settings.json (เก็บข้างๆ ไฟล์ .exe)
        private static string SettingsPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        // --- 1. Orthanc Connection ---
        public string OrthancIp { get; set; } = "127.0.0.1";
        public int OrthancWebPort { get; set; } = 8042;
        public int OrthancDicomPort { get; set; } = 4242;
        public string OrthancAETitle { get; set; } = "MYORTHANC";
        
        // --- 2. Local Config ---
        public string AETitle { get; set; } = "STONEPACS";
        
        // --- 3. File Paths (สำคัญสำหรับระบบ Client-Server) ---
        
        // Path สำหรับเก็บไฟล์ Worklist (.wl)
        // ถ้าเป็น Windows จะใช้ Path C:\... ถ้าเป็น Mac ใช้ Path ของคุณ
        public string WorklistFolder { get; set; } = 
            OperatingSystem.IsWindows() 
            ? @"C:\Orthanc\Worklist" 
            : "/Users/lnw/Downloads/Orthanc-MacOS-25.12.3-stable/Worklist";

        // Path สำหรับ Database SQLite
        // ค่า Default คือ "StonePACS.db" (สร้างไฟล์ข้างๆ โปรแกรม)
        // ถ้าจะทำ Client-Server ให้แก้เป็น \\ServerIP\ShareFolder\StonePACS.db
        public string DatabasePath { get; set; } = "StonePACS.db"; 

        // --- Functions ---

        public void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        private static SettingsService LoadSettings()
        {
            if (File.Exists(SettingsPath))
            {
                try
                {
                    string json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<SettingsService>(json) ?? new SettingsService();
                }
                catch 
                { 
                    // ถ้าไฟล์เสีย ให้ใช้ค่า Default
                    return new SettingsService(); 
                }
            }
            // ถ้าไม่มีไฟล์ ให้ใช้ค่า Default
            return new SettingsService();
        }
    }
}