using Microsoft.EntityFrameworkCore;
using StonePACS.Models;

namespace StonePACS.Data
{
    public class StoneDbContext : DbContext
    {
        // ตาราง Patients ใน Database
        public DbSet<PatientModel> Patients { get; set; }

        // ตั้งค่าการเชื่อมต่อ (Connection String)
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // เชื่อมต่อไปที่เครื่องตัวเอง (localhost), User ปกติของ Mac คือชื่อเครื่องเรา หรือ 'postgres'
            // หมายเหตุ: Postgres.app ปกติไม่ต้องใช้ password
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=stonepacs;Username=postgres;Password=");
        }
    }
}