using System;
using System.ComponentModel.DataAnnotations; // <--- ต้องมีบรรทัดนี้ ไม่งั้น Build Failed

namespace StonePACS.Models
{
    public class PatientModel
    {
        [Key] // <--- ต้องมีบรรทัดนี้
        public int Id { get; set; } 

        public string HN { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Sex { get; set; } = "Male"; 
        // เปลี่ยนจาก DateTimeOffset.Now เป็น DateTimeOffset.UtcNow
        public DateTimeOffset DateOfBirth { get; set; } = DateTimeOffset.UtcNow;

        public string FullName => $"{FirstName} {LastName}";

        // ข้อมูลการส่งตรวจ
        public string Modality { get; set; } = "DX"; 
        public string ExamCode { get; set; } = string.Empty; 
        public string StudyDescription { get; set; } = string.Empty; 
    }
}