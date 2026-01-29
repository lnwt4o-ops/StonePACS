using Microsoft.EntityFrameworkCore;
using StonePACS.Models;

namespace StonePACS.Data
{
    public class StoneDbContext : DbContext
    {
        public DbSet<PatientModel> Patients { get; set; }
        public DbSet<ExamCodeModel> ExamCodes { get; set; } // ตารางรหัสการตรวจ

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // แก้ไข Password ให้ตรงกับที่คุณตั้งไว้ใน pgAdmin
            string connString = "Host=localhost;Database=StonePACS_DB;Username=postgres;Password=lylxitp6m";
            optionsBuilder.UseNpgsql(connString);
        }
    }
}