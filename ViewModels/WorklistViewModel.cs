using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks; // <--- ตัวนี้แก้ error Task และ MVVMTK0007
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StonePACS.Data;
using StonePACS.Models;
using StonePACS.Services;

namespace StonePACS.ViewModels
{
    public partial class WorklistViewModel : ViewModelBase
    {
        public ObservableCollection<PatientModel> Patients { get; set; } = new();

        public WorklistViewModel()
        {
            LoadPatients();
        }

        [RelayCommand]
        public void LoadPatients()
        {
            using (var db = new StoneDbContext())
            {
                var list = db.Patients.OrderByDescending(p => p.Id).ToList();
                Patients.Clear();
                foreach (var p in list) Patients.Add(p);
            }
        }

        // ปุ่มกดทดสอบ
        [RelayCommand]
        public async Task TestConnection()
        {
            var service = new DicomService();
            var results = await service.TestQueryWorklist();

            foreach (var line in results)
            {
                System.Diagnostics.Debug.WriteLine(line);
                Console.WriteLine(line);
            }
        }
    }
}