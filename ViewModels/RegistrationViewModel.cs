using System;
using System.Linq;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StonePACS.Models;
using StonePACS.Data;
using StonePACS.Services; // สำคัญมาก ต้องมีบรรทัดนี้

namespace StonePACS.ViewModels
{
    public partial class RegistrationViewModel : ViewModelBase
    {
        [ObservableProperty]
        private PatientModel _newPatient = new PatientModel();

        [ObservableProperty]
        private string _statusMessage = ""; 

        [ObservableProperty]
        private bool _isBusy = false;

        public ObservableCollection<string> SexOptions { get; } = new() { "Male", "Female", "Other" };
        public ObservableCollection<string> ModalityOptions { get; } = new() { "DX", "CR", "CT", "MR", "US", "OT" };

        public RegistrationViewModel()
        {
            GenerateAccessionNumber();
        }

        private void GenerateAccessionNumber()
        {
            // ST + ปี(2หลัก) + เดือน + วัน + เวลา(6หลัก)
            // ตัวอย่าง: ST260129114501
            // รวมทั้งหมด 14 ตัวอักษร (ไม่เกิน 16)
            var timestamp = DateTime.Now.ToString("yyMMddHHmmss");
            
            NewPatient.ExamCode = $"ST{timestamp}"; 
            
            OnPropertyChanged(nameof(NewPatient));
        }

        [RelayCommand]
        private void SearchPatient()
        {
            if (string.IsNullOrWhiteSpace(NewPatient.HN)) return;

            IsBusy = true;
            StatusMessage = "🔍 Searching...";
            
            try 
            {
                using (var db = new StoneDbContext())
                {
                    var existing = db.Patients
                                     .Where(p => p.HN == NewPatient.HN)
                                     .OrderByDescending(p => p.Id)
                                     .FirstOrDefault();

                    if (existing != null)
                    {
                        NewPatient.FirstName = existing.FirstName;
                        NewPatient.LastName = existing.LastName;
                        NewPatient.Sex = existing.Sex;
                        NewPatient.DateOfBirth = existing.DateOfBirth.ToLocalTime();
                        
                        NewPatient.Id = 0; // Reset ID เพื่อสร้าง Order ใหม่
                        GenerateAccessionNumber(); // สร้างเลขใหม่

                        StatusMessage = "✅ Patient Found! (Data loaded)";
                        OnPropertyChanged(nameof(NewPatient));
                    }
                    else
                    {
                        StatusMessage = "ℹ️ New Patient (HN not found)";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Search Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void SavePatient()
        {
            if (string.IsNullOrWhiteSpace(NewPatient.HN) || string.IsNullOrWhiteSpace(NewPatient.FirstName))
            {
                StatusMessage = "⚠️ Please enter HN and Name";
                return;
            }

            IsBusy = true;
            StatusMessage = "💾 Saving...";
            
            try 
            {
                // แปลงเวลาเป็น UTC ก่อนลง Database
                NewPatient.DateOfBirth = NewPatient.DateOfBirth.ToUniversalTime();

                // 1. ลง Database
                using (var db = new StoneDbContext())
                {
                    db.Patients.Add(NewPatient);
                    db.SaveChanges();
                }

                // 2. สร้างไฟล์ DICOM Worklist (.wl)
                var dicomService = new DicomService();
                string filePath = dicomService.CreateWorklistFile(NewPatient);
                string fileName = System.IO.Path.GetFileName(filePath);

                StatusMessage = $"✅ Success! Saved DB & Created DICOM: {fileName}";
                
                // เคลียร์ฟอร์ม
                NewPatient = new PatientModel();
                GenerateAccessionNumber();
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Save Failed: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
} 
// <-- ปีกกาตัวสุดท้ายนี่แหละครับที่มักจะหายไป