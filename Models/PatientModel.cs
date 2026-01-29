using System;

namespace StonePACS.Models
{
    // คลาสสำหรับเก็บข้อมูลคนไข้ 1 คน
    public class PatientModel
    {
        public string HN { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        // เก็บเพศเป็น string ง่ายๆ ไปก่อน (Male/Female)
        public string Sex { get; set; } = "Male"; 
        // ใช้วันที่ปัจจุบันเป็นค่าเริ่มต้น
        public DateTimeOffset DateOfBirth { get; set; } = DateTimeOffset.Now;

        // Property เสริมเพื่อรวมชื่อ (เอาไว้โชว์)
        public string FullName => $"{FirstName} {LastName}";
    }
}