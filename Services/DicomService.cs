using System;
using System.IO;
using System.Collections.Generic; // <--- ตัวนี้แก้ error List<>
using System.Threading.Tasks;     // <--- ตัวนี้แก้ error Task<>
using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using StonePACS.Models;

namespace StonePACS.Services
{
    public class DicomService
    {
        private readonly string _worklistFolder;

        public DicomService()
{
    // เช็คว่าเป็น Windows หรือไม่?
    if (OperatingSystem.IsWindows())
    {
        // Path สำหรับ Windows (สมมติว่าลง Orthanc ไว้ไดรฟ์ C)
        _worklistFolder = @"C:\Orthanc\Worklist"; 
    }
    else if (OperatingSystem.IsMacOS())
    {
        // Path สำหรับ Mac ของคุณ
        _worklistFolder = "/Users/lnw/Downloads/Orthanc-MacOS-25.12.3-stable/Worklist";
    }
    else
    {
        // เผื่อใช้ Linux
        _worklistFolder = "/var/lib/orthanc/worklists";
    }

    // สร้างโฟลเดอร์ถ้ายั่งไม่มี
    if (!Directory.Exists(_worklistFolder))
    {
        Directory.CreateDirectory(_worklistFolder);
    }
}

        public string CreateWorklistFile(PatientModel patient)
        {
            var dataset = new DicomDataset();
            
            // Patient Module
            dataset.Add(DicomTag.PatientName, patient.FullName);
            dataset.Add(DicomTag.PatientID, patient.HN);
            dataset.Add(DicomTag.PatientSex, patient.Sex?.Substring(0, 1) ?? "O");
            dataset.Add(DicomTag.PatientBirthDate, patient.DateOfBirth.ToString("yyyyMMdd"));

            // Visit Module
            dataset.Add(DicomTag.AccessionNumber, patient.ExamCode);
            dataset.Add(DicomTag.StudyID, patient.ExamCode);
            dataset.Add(DicomTag.Modality, patient.Modality ?? "DX");
            dataset.Add(DicomTag.StudyDescription, patient.StudyDescription ?? "");
            
            // Scheduled Procedure Step
            var now = DateTime.Now;
            dataset.Add(DicomTag.ScheduledProcedureStepStartDate, now.ToString("yyyyMMdd"));
            dataset.Add(DicomTag.ScheduledProcedureStepStartTime, now.ToString("HHmmss"));
            dataset.Add(DicomTag.ScheduledProcedureStepDescription, patient.StudyDescription ?? "General Exam");
            dataset.Add(DicomTag.ScheduledStationAETitle, "ANY-MODALITY"); 

            // Required Tags
            dataset.Add(DicomTag.SpecificCharacterSet, "ISO_IR 100");
            dataset.Add(DicomTag.SOPClassUID, DicomUID.ModalityWorklistInformationModelFind);
            dataset.Add(DicomTag.SOPInstanceUID, DicomUID.Generate());

            string fileName = $"{patient.HN}_{patient.ExamCode}.wl";
            string fullPath = Path.Combine(_worklistFolder, fileName);

            var file = new DicomFile(dataset);
            file.Save(fullPath);

            return fullPath;
        }

        // ฟังก์ชันทดสอบการเชื่อมต่อ (จำลองเป็นเครื่อง X-Ray)
        public async Task<List<string>> TestQueryWorklist()
        {
            var results = new List<string>();
            var cfind = DicomCFindRequest.CreateWorklistQuery();

            cfind.OnResponseReceived = (req, response) =>
            {
                if (response.HasDataset)
                {
                    var name = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientName, "Unknown");
                    var acc = response.Dataset.GetSingleValueOrDefault(DicomTag.AccessionNumber, "No Acc");
                    var modality = response.Dataset.GetSingleValueOrDefault(DicomTag.Modality, "?");
                    
                    results.Add($"✅ FOUND: [{modality}] {name} (Acc: {acc})");
                }
            };

// "STONEPACS" = ชื่อเรา (แนะนำตัว)
// "MYORTHANC" = ชื่อเขา (ต้องตรงกับ configMacOS.json ตัวใหญ่หมด)
var client = DicomClientFactory.Create("127.0.0.1", 4242, false, "STONEPACS", "MYORTHANC");

            await client.AddRequestAsync(cfind);
            
            try 
            {
                await client.SendAsync();
            }
            catch (Exception ex)
            {
                results.Add($"❌ CONNECTION FAILED: {ex.Message}");
            }

            if (results.Count == 0) results.Add("⚠️ No worklist items found.");
            
            return results;
        }
    }
}