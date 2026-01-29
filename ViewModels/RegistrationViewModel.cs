using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StonePACS.Models;

namespace StonePACS.ViewModels
{
    // สืบทอดจาก ViewModelBase และใช้ [ObservableObject] เพื่อให้หน้าจออัปเดตอัตโนมัติ
    public partial class RegistrationViewModel : ViewModelBase
    {
        // ประกาศตัวแปร Model เพื่อเก็บข้อมูลที่กำลังกรอก
        [ObservableProperty]
        private PatientModel _newPatient = new PatientModel();

        // ตัวเลือกสำหรับ ComboBox เพศ
        public ObservableCollection<string> SexOptions { get; } = new() { "Male", "Female" };

        // คำสั่งเมื่อกดปุ่ม Save (ใช้ RelayCommand ของ CommunityToolkit)
        [RelayCommand]
        private void SavePatient()
        {
            // --- ตรงนี้คือจุดที่เราจะเขียนโค้ดส่งข้อมูลไปหา Orthanc ในอนาคต ---
            
            // ตอนนี้ให้แสดงข้อมูลออกมาทาง Console ของ VS Code เพื่อทดสอบก่อน
            System.Diagnostics.Debug.WriteLine($"[SAVING] HN: {NewPatient.HN}, Name: {NewPatient.FullName}, DOB: {NewPatient.DateOfBirth.Date.ToShortDateString()}, Sex: {NewPatient.Sex}");

            // (Optional) อาจจะเคลียร์ฟอร์มหลังจากบันทึก หรือแสดง popup แจ้งเตือน
        }
    }
}