using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StonePACS.Data;
using StonePACS.Models;

namespace StonePACS.ViewModels
{
    public partial class ExamCodeSetupViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<ExamCodeModel> _examCodes = new();

        [ObservableProperty]
        private ExamCodeModel _selectedExam = new ExamCodeModel();

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        public ObservableCollection<string> ModalityOptions { get; } = new() { "DX", "CR", "CT", "MR", "US", "NM", "PT", "OT" };

        public ExamCodeSetupViewModel()
        {
            LoadExamCodes();
        }

        [RelayCommand]
        private void LoadExamCodes()
        {
            try
            {
                using (var db = new StoneDbContext())
                {
                    var exams = db.ExamCodes.Where(e => e.IsActive).OrderBy(e => e.Code).ToList();
                    ExamCodes.Clear();
                    foreach (var exam in exams)
                    {
                        ExamCodes.Add(exam);
                    }
                }
                StatusMessage = $"✅ โหลดข้อมูล {ExamCodes.Count} รายการ";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ เกิดข้อผิดพลาด: {ex.Message}";
            }
        }

        [RelayCommand]
        private void NewExam()
        {
            SelectedExam = new ExamCodeModel 
            { 
                Modality = "DX",
                IsActive = true
            };
            StatusMessage = "";
        }

        [RelayCommand]
        private void SaveExam()
        {
            if (string.IsNullOrWhiteSpace(SelectedExam.Code) || string.IsNullOrWhiteSpace(SelectedExam.Name))
            {
                StatusMessage = "⚠️ กรุณากรอก Code และ Name";
                return;
            }

            try
            {
                using (var db = new StoneDbContext())
                {
                    if (SelectedExam.Id == 0)
                    {
                        // เพิ่มใหม่
                        SelectedExam.CreatedDate = DateTime.UtcNow;
                        db.ExamCodes.Add(SelectedExam);
                        StatusMessage = "✅ เพิ่ม Exam Code สำเร็จ";
                    }
                    else
                    {
                        // แก้ไข
                        db.ExamCodes.Update(SelectedExam);
                        StatusMessage = "✅ อัพเดท Exam Code สำเร็จ";
                    }
                    db.SaveChanges();
                }
                
                LoadExamCodes();
                NewExam();
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ บันทึกไม่สำเร็จ: {ex.Message}";
            }
        }

        [RelayCommand]
        private void DeleteExam(ExamCodeModel exam)
        {
            if (exam == null) return;

            try
            {
                using (var db = new StoneDbContext())
                {
                    // Soft delete
                    var existing = db.ExamCodes.Find(exam.Id);
                    if (existing != null)
                    {
                        existing.IsActive = false;
                        db.SaveChanges();
                    }
                }
                StatusMessage = $"✅ ลบ {exam.Code} สำเร็จ";
                LoadExamCodes();
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ ลบไม่สำเร็จ: {ex.Message}";
            }
        }

        [RelayCommand]
        private void EditExam(ExamCodeModel exam)
        {
            if (exam != null)
            {
                SelectedExam = new ExamCodeModel
                {
                    Id = exam.Id,
                    Code = exam.Code,
                    Name = exam.Name,
                    Description = exam.Description,
                    Modality = exam.Modality,
                    IsActive = exam.IsActive,
                    CreatedDate = exam.CreatedDate
                };
                StatusMessage = $"แก้ไข: {exam.Code}";
            }
        }
    }
}
