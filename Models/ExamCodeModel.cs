using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StonePACS.Models
{
    /// <summary>
    /// Master data สำหรับ Exam Code Template
    /// เก็บข้อมูล exam ต่างๆ ที่มีในโรงพยาบาล
    /// </summary>
    public class ExamCodeModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// รหัส Exam เช่น "CXR", "ABD", "SKULL"
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// ชื่อ Exam เช่น "Chest X-Ray"
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// คำอธิบาย Exam เช่น "PA and Lateral view"
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Modality: DX, CR, CT, MR, US, etc.
        /// </summary>
        public string Modality { get; set; } = "DX";

        /// <summary>
        /// วันที่สร้าง record
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// สถานะ Active/Inactive
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
