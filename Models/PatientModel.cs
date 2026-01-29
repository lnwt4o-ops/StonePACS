using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StonePACS.Models
{
    public class PatientModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Title { get; set; } = "Mr."; // คำนำหน้า

        public string HN { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Sex { get; set; } = "M";
        public DateTime DateOfBirth { get; set; }
        
        // ✅ ข้อมูลติดต่อ
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        
        public string Modality { get; set; } = "DX";
        public string ExamCode { get; set; } = string.Empty; // Accession No.
        public string StudyDescription { get; set; } = string.Empty;

        // ✅ เพิ่มตัวแปรเหล่านี้ครับ (ที่ Error เพราะขาดบรรทัดนี้)
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        
        // สถานะงาน (Scheduled, Completed)
        public string Status { get; set; } = "Scheduled";
        
        // สีสถานะ (Orange, Green)
        public string StatusColor { get; set; } = "Orange"; 
        
        // Helper สำหรับรวมชื่อ
        [NotMapped]
        public string FullName => $"{Title} {FirstName} {LastName}".Trim();
    }
}