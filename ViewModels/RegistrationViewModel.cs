using System;
using System.Linq;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StonePACS.Models;
using StonePACS.Data;
using StonePACS.Services;

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

        // ✅ DatePicker binding - เริ่มต้นเป็น null เพื่อแสดง placeholder
        [ObservableProperty]
        private DateTimeOffset? _birthDateOffset = null;

        // ✅ Exam Code Search
        [ObservableProperty]
        private string _examCodeSearch = string.Empty;

        // ✅ AutoComplete for Exam Code
        [ObservableProperty]
        private ObservableCollection<string> _examCodeList = new();

        private string? _selectedExamCode;
        public string? SelectedExamCode
        {
            get => _selectedExamCode;
            set
            {
                SetProperty(ref _selectedExamCode, value);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    LoadExamByCode(value);
                }
            }
        }

        public ObservableCollection<string> TitleOptions { get; } = new() { "Mr.", "Mrs.", "Miss.", "Ms.", "Dr.", "ด.ช.", "ด.ญ.", "นาย", "นาง", "นางสาว" };
        public ObservableCollection<string> SexOptions { get; } = new() { "M", "F", "O" };
        public ObservableCollection<string> ModalityOptions { get; } = new() { "DX", "CR", "CT", "MR", "US", "OT" };

        public RegistrationViewModel()
        {
            // กำหนดค่าเริ่มต้น
            NewPatient.Title = "Mr.";
            NewPatient.Sex = "M";
            NewPatient.Modality = "DX";
            GenerateAccessionNumber();
            LoadExamCodeList();
        }

        private void LoadExamCodeList()
        {
            try
            {
                using (var db = new StoneDbContext())
                {
                    var codes = db.ExamCodes
                                   .Where(e => e.IsActive)
                                   .Select(e => e.Code)
                                   .ToList();
                    ExamCodeList.Clear();
                    foreach (var code in codes)
                    {
                        ExamCodeList.Add(code);
                    }
                }
            }
            catch { }
        }

        private void LoadExamByCode(string code)
        {
            try
            {
                using (var db = new StoneDbContext())
                {
                    var exam = db.ExamCodes
                                  .Where(e => e.Code.ToLower() == code.ToLower() && e.IsActive)
                                  .FirstOrDefault();

                    if (exam != null)
                    {
                        NewPatient.Modality = exam.Modality;
                        NewPatient.StudyDescription = $"{exam.Name} - {exam.Description}";
                        StatusMessage = $"✅ Loaded: {exam.Code} - {exam.Name}";
                        OnPropertyChanged(nameof(NewPatient));
                    }
                }
            }
            catch { }
        }

        private void GenerateAccessionNumber()
        {
            // สร้างเลข Accession: ST + ปีเดือนวัน + เวลา (เพื่อไม่ให้ซ้ำ)
            var timestamp = DateTime.Now.ToString("yyMMddHHmmss");
            NewPatient.ExamCode = $"ST{timestamp}"; 
            OnPropertyChanged(nameof(NewPatient));
        }

        [RelayCommand]
        private void SearchPatient()
        {
            // ตรวจสอบ HN ว่าไม่ว่างเปล่า
            if (string.IsNullOrWhiteSpace(NewPatient.HN)) 
            {
                StatusMessage = "⚠️ กรุณากรอก HN เพื่อค้นหาข้อมูล";
                return;
            }

            IsBusy = true;
            StatusMessage = "🔍 กำลังค้นหาข้อมูลผู้ป่วย...";
            
            try 
            {
                using (var db = new StoneDbContext())
                {
                    // ค้นหา HN ล่าสุด
                    var existing = db.Patients
                                     .Where(p => p.HN == NewPatient.HN)
                                     .OrderByDescending(p => p.Id)
                                     .FirstOrDefault();

                    if (existing != null)
                    {
                        // Map ข้อมูลเดิมใส่ฟอร์ม
                        NewPatient.Title = existing.Title ?? "Mr.";
                        NewPatient.FirstName = existing.FirstName;
                        NewPatient.LastName = existing.LastName;
                        NewPatient.Sex = existing.Sex;
                        
                        // ✅ โหลดข้อมูลติดต่อ
                        NewPatient.Address = existing.Address;
                        NewPatient.PhoneNumber = existing.PhoneNumber;
                        
                        // ✅ แปลงวันที่จาก DB (DateTime) ไปเป็น DateTimeOffset สำหรับ DatePicker
                        if (existing.DateOfBirth != DateTime.MinValue && existing.DateOfBirth.Year > 1900)
                        {
                            BirthDateOffset = new DateTimeOffset(existing.DateOfBirth, TimeSpan.Zero);
                        }
                        else
                        {
                            BirthDateOffset = null; // ถ้าไม่มีวันเกิด ให้เป็น null
                        }

                        // Reset ID เพื่อสร้าง Order ใหม่ (แต่ใช้ข้อมูลคนไข้เดิม)
                        NewPatient.Id = 0; 
                        GenerateAccessionNumber(); // สร้างเลข Order ใหม่

                        StatusMessage = $"✅ พบข้อมูลผู้ป่วย HN: {existing.HN} - {existing.FullName}";
                        
                        // แจ้ง UI ให้รีเฟรชค่า
                        OnPropertyChanged(nameof(NewPatient));
                    }
                    else
                    {
                        StatusMessage = "ℹ️ ไม่พบข้อมูล HN นี้ในระบบ - กำลังลงทะเบียนผู้ป่วยใหม่";
                        // เคลียร์ชื่อทิ้ง ถ้าหาไม่เจอ
                        NewPatient.FirstName = "";
                        NewPatient.LastName = "";
                        BirthDateOffset = null; // รีเซ็ตวันที่ให้เป็นว่าง
                        OnPropertyChanged(nameof(NewPatient));
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ เกิดข้อผิดพลาดในการค้นหา: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void SearchExamCode()
        {
            if (string.IsNullOrWhiteSpace(ExamCodeSearch))
            {
                StatusMessage = "⚠️ กรุณากรอกรหัสการตรวจ (Exam Code)";
                return;
            }

            IsBusy = true;
            try
            {
                using (var db = new StoneDbContext())
                {
                    var exam = db.ExamCodes
                                  .Where(e => e.Code.ToLower() == ExamCodeSearch.ToLower() && e.IsActive)
                                  .FirstOrDefault();

                    if (exam != null)
                    {
                        // พบข้อมูล Exam -> ดึงมาแสดง
                        NewPatient.Modality = exam.Modality;
                        NewPatient.StudyDescription = $"{exam.Name} - {exam.Description}";
                        
                        StatusMessage = $"✅ พบ Exam Code: {exam.Code} - {exam.Name}";
                        OnPropertyChanged(nameof(NewPatient));
                    }
                    else
                    {
                        StatusMessage = $"ℹ️ ไม่พบ Exam Code: {ExamCodeSearch}";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ เกิดข้อผิดพลาด: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void SavePatient()
        {
            // ตรวจสอบข้อมูลที่จำเป็น
            if (string.IsNullOrWhiteSpace(NewPatient.HN) || string.IsNullOrWhiteSpace(NewPatient.FirstName))
            {
                StatusMessage = "⚠️ กรุณากรอก HN และชื่อผู้ป่วย";
                return;
            }

            IsBusy = true;
            StatusMessage = "💾 กำลังบันทึกข้อมูล...";
            
            try 
            {
                // ✅ 1. แปลงวันที่จาก DatePicker (DateTimeOffset?) ไปเป็น DateTime
                if (BirthDateOffset.HasValue)
                {
                    // แปลงเป็น UTC DateTime เพื่อหลีกเลี่ยงปัญหา timezone
                    NewPatient.DateOfBirth = DateTime.SpecifyKind(BirthDateOffset.Value.DateTime, DateTimeKind.Utc);
                }
                else
                {
                    // ถ้าไม่ได้เลือกวันเกิด ให้ใช้ค่า default (1900-01-01)
                    NewPatient.DateOfBirth = DateTime.SpecifyKind(new DateTime(1900, 1, 1), DateTimeKind.Utc);
                }
                
                NewPatient.RegistrationDate = DateTime.UtcNow;
                NewPatient.Status = "Scheduled";
                NewPatient.StatusColor = "Orange";

                // 2. บันทึกลง Database
                using (var db = new StoneDbContext())
                {
                    db.Patients.Add(NewPatient);
                    db.SaveChanges();
                }

                // 3. สร้างไฟล์ DICOM Worklist (.wl)
                var dicomService = new DicomService();
                string filePath = dicomService.CreateWorklistFile(NewPatient);
                string fileName = System.IO.Path.GetFileName(filePath);

                StatusMessage = $"✅ บันทึกสำเร็จ! สร้าง Worklist: {fileName}";
                
                // 4. เคลียร์ฟอร์มเตรียมรับคนต่อไป
                ClearForm();
            }
            catch (Exception ex)
            {
                // แสดง error แบบละเอียด
                string errorDetail = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                StatusMessage = $"❌ บันทึกไม่สำเร็จ: {errorDetail}";
                Console.WriteLine($"[ERROR] SavePatient: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ClearForm()
        {
            // สร้าง Object ใหม่
            NewPatient = new PatientModel
            {
                Title = "Mr.",
                Sex = "M",
                Modality = "DX"
            };
            
            // รีเซ็ตวันที่ให้เป็นว่าง (null) เพื่อให้แสดง placeholder
            BirthDateOffset = null;
            
            // สร้างเลขใหม่
            GenerateAccessionNumber();
            
            OnPropertyChanged(nameof(NewPatient));
        }
    }
}