using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics; 
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StonePACS.Data;
using StonePACS.Models;
using StonePACS.Services;
using StonePACS.Views; // สำคัญ! ต้องมีบรรทัดนี้เพื่อเรียก AuthWindow

namespace StonePACS.ViewModels
{
    public partial class WorklistViewModel : ViewModelBase
    {
        public ObservableCollection<PatientModel> Patients { get; set; } = new();

        public WorklistViewModel()
        {
            _ = LoadPatients(); 
        }

        [RelayCommand]
        public async Task LoadPatients()
        {
            try 
            {
                using (var db = new StoneDbContext())
                {
                    var list = db.Patients.OrderByDescending(p => p.Id).ToList();
                    
                    var dicomService = new DicomService();
                    Patients.Clear();

                    foreach (var p in list) 
                    {
                        // ตรวจสอบว่ามีภาพเข้ามาหรือยัง
                        bool hasImage = await dicomService.CheckIfStudyExists(p.HN, p.ExamCode);
                        if (hasImage) 
                        { 
                            p.Status = "Completed"; 
                            p.StatusColor = "Green"; 
                        }
                        else 
                        { 
                            p.Status = "Scheduled"; 
                            p.StatusColor = "Orange"; 
                        }
                        Patients.Add(p);
                    }
                }
            }
            catch (Exception ex) 
            { 
                Console.WriteLine($"❌ เกิดข้อผิดพลาดในการโหลดข้อมูล: {ex.Message}"); 
            }
        }

        // ✅ ปุ่ม Config: ต้องเปิดหน้า Login ก่อน
        [RelayCommand]
        public async Task OpenSettings()
        {
            // 1. หาหน้าต่างหลักเพื่อเป็นแม่ข่าย
            var desktop = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            var owner = desktop?.MainWindow;

            if (owner == null) return; // ถ้าหาไม่เจอให้จบการทำงาน

            // 2. เปิดหน้าต่าง Login (AuthWindow)
            var login = new AuthWindow();
            await login.ShowDialog(owner);

            // 3. ถ้า Login ผ่าน ให้เปิดหน้า Settings
            if (login.IsAuthenticated)
            {
                var settings = new SettingsWindow();
                await settings.ShowDialog(owner);
                
                // Reload หลังจากแก้ Config
                _ = LoadPatients();
            }
        }

        // ✅ ปุ่ม View แบบ Standard
        [RelayCommand]
        public async Task OpenStandardViewer(PatientModel patient)
        {
            await OpenViewerHelper(patient, "standard");
        }

        // ✅ ปุ่ม View แบบ Stone (Modern)
        [RelayCommand]
        public async Task OpenStoneViewer(PatientModel patient)
        {
            await OpenViewerHelper(patient, "stone");
        }

        private async Task OpenViewerHelper(PatientModel patient, string type)
        {
            if (patient == null) return;
            var dicomService = new DicomService();
            string? orthancId = await dicomService.GetOrthancIdFromHn(patient.HN);

            if (string.IsNullOrEmpty(orthancId)) {
                Console.WriteLine("❌ Image not found in Orthanc");
                return;
            }

            var s = SettingsService.Current;
            string url = (type == "stone") 
                ? $"http://{s.OrthancIp}:{s.OrthancWebPort}/stone-webviewer/index.html?patient={orthancId}"
                : $"http://{s.OrthancIp}:{s.OrthancWebPort}/web-viewer/app/viewer?patient={orthancId}";

            try {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) Process.Start("open", url);
                else Process.Start("xdg-open", url);
            } catch (Exception ex) { Console.WriteLine($"Browser Error: {ex.Message}"); }
        }

        [RelayCommand]
        public void CancelOrder(PatientModel patient)
        {
            if (patient == null) return;
            
            try 
            {
                var s = SettingsService.Current;
                string path = Path.Combine(s.WorklistFolder, $"{patient.HN}_{patient.ExamCode}.wl");
                
                // ลบไฟล์ worklist
                if (File.Exists(path)) 
                {
                    File.Delete(path);
                    Console.WriteLine($"✅ ลบไฟล์ worklist: {path}");
                }

                // ลบจาก Database
                using (var db = new StoneDbContext()) 
                {
                    db.Patients.Remove(patient);
                    db.SaveChanges();
                    Console.WriteLine($"✅ ลบคำสั่งตรวจ HN: {patient.HN}");
                }
                
                Patients.Remove(patient);
            } 
            catch (Exception ex)
            {
                Console.WriteLine($"❌ เกิดข้อผิดพลาดในการลบคำสั่ง: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task TestConnection()
        {
            var s = new DicomService();
            await s.TestQueryWorklist();
        }
    }
}