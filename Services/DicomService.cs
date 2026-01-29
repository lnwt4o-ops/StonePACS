using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json; // ✅ สำคัญ: ใช้สำหรับแกะ JSON จาก Orthanc
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
            try 
            {
                // ใช้ค่าจาก SettingsService (ถ้ามี)
                _worklistFolder = SettingsService.Current.WorklistFolder;
            }
            catch
            {
                 // Fallback กรณี SettingsService ยังไม่พร้อม
                 _worklistFolder = "/Users/lnw/Downloads/Orthanc-MacOS-25.12.3-stable/Worklist";
            }
            
            if (!Directory.Exists(_worklistFolder))
            {
                Directory.CreateDirectory(_worklistFolder);
            }
        }

        public string CreateWorklistFile(PatientModel patient)
        {
            var dataset = new DicomDataset();
            
            // Patient Info
            dataset.Add(DicomTag.PatientName, patient.FullName);
            dataset.Add(DicomTag.PatientID, patient.HN);
            dataset.Add(DicomTag.PatientSex, patient.Sex?.Substring(0, 1) ?? "O");
            dataset.Add(DicomTag.PatientBirthDate, patient.DateOfBirth.ToString("yyyyMMdd"));

            // Exam Info
            dataset.Add(DicomTag.AccessionNumber, patient.ExamCode);
            dataset.Add(DicomTag.StudyID, patient.ExamCode);
            dataset.Add(DicomTag.Modality, patient.Modality ?? "DX");
            dataset.Add(DicomTag.StudyDescription, patient.StudyDescription ?? "");
            
            // Settings Integration
            var aeTitle = "STONEPACS";
            try { aeTitle = SettingsService.Current.AETitle; } catch {}
            
            dataset.Add(DicomTag.ScheduledStationAETitle, aeTitle); 
            dataset.Add(DicomTag.ScheduledProcedureStepStartDate, DateTime.Now.ToString("yyyyMMdd"));
            dataset.Add(DicomTag.ScheduledProcedureStepStartTime, DateTime.Now.ToString("HHmmss"));

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

        // ตรวจสอบว่ามีภาพเข้ามาหรือยัง (เช็ค AccessionNumber ด้วย)
        public async Task<bool> CheckIfStudyExists(string hn, string accessionNumber)
        {
            try
            {
                var settings = SettingsService.Current;
                string url = $"http://{settings.OrthancIp}:{settings.OrthancWebPort}/studies?PatientID={hn}";

                using (var http = new HttpClient())
                {
                    http.Timeout = TimeSpan.FromSeconds(2);
                    var response = await http.GetStringAsync(url);
                    
                    // ถ้าไม่มีข้อมูลเลย
                    if (response == "[]" || response.Length <= 5) return false;

                    // ถ้ามีข้อมูล ต้องเช็คว่า AccessionNumber ตรงไหม
                    using (JsonDocument doc = JsonDocument.Parse(response))
                    {
                        if (doc.RootElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var study in doc.RootElement.EnumerateArray())
                            {
                                // Orthanc จะคืนค่า PatientMainDicomTags มาให้
                                if (study.TryGetProperty("PatientMainDicomTags", out var tags))
                                {
                                    // ลองหา AccessionNumber (บางทีอาจจะอยู่ใน MainDicomTags หรือต้อง query ลึกกว่านี้ 
                                    // แต่เบื้องต้น Orthanc API /studies?PatientID=... อาจจะไม่คืน AccessionNumber มาใน Level นี้ทันที
                                    // ดังนั้นเพื่อความชัวร์ เราจะ assume ว่า "ถ้ามี Study ของคนนี้มาใหม่ เวลาใกล้เคียงกัน" คือใช่
                                    // หรือถ้าจะให้ชัวร์ต้องเรียก /studies/{id} อีกที แต่จะช้า
                                    
                                    // วิธีที่ดีกว่า: ใช้ /tools/find เพื่อหา AccessionNumber โดยตรง
                                    return await VerifyAccessionIgnoringCase(accessionNumber);
                                }
                            }
                        }
                    }

                    // Fallback: ถ้าเช็ค Accession ไม่ได้ ให้ถือว่ามีภาพไปก่อน (Logic เดิม)
                    return true;
                }
            }
            catch 
            {
                return false; 
            }
        }
        
        private async Task<bool> VerifyAccessionIgnoringCase(string acc)
        {
             try 
             {
                // ใช้ QIDO-RS หรือ Tools/Find ของ Orthanc
                var settings = SettingsService.Current;
                string url = $"http://{settings.OrthancIp}:{settings.OrthancWebPort}/tools/find";
                
                var query = new
                {
                    Level = "Study",
                    Query = new { AccessionNumber = acc }
                };

                using (var http = new HttpClient())
                {
                    var content = new StringContent(JsonSerializer.Serialize(query));
                    var response = await http.PostAsync(url, content);
                    var result = await response.Content.ReadAsStringAsync();
                    return result != "[]" && result.Length > 5;
                }
             }
             catch { return false; }
        }

        // ✅ ฟังก์ชันใหม่: แปลง HN เป็น UUID ของ Orthanc (แก้ Error 404)
        public async Task<string?> GetOrthancIdFromHn(string hn)
        {
            try
            {
                var settings = SettingsService.Current;
                string url = $"http://{settings.OrthancIp}:{settings.OrthancWebPort}/tools/lookup";

                using (var http = new HttpClient())
                {
                    var content = new StringContent(hn);
                    var response = await http.PostAsync(url, content);
                    
                    if (!response.IsSuccessStatusCode) return null;

                    var jsonString = await response.Content.ReadAsStringAsync();
                    
                    // แกะ JSON หา ID ที่ Type เป็น Patient
                    using (JsonDocument doc = JsonDocument.Parse(jsonString))
                    {
                        if (doc.RootElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var element in doc.RootElement.EnumerateArray())
                            {
                                if (element.TryGetProperty("Type", out var type) && type.GetString() == "Patient")
                                {
                                    if (element.TryGetProperty("ID", out var id))
                                    {
                                        return id.GetString(); // เจอแล้ว! คืนค่า UUID กลับไป
                                    }
                                }
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lookup Error: {ex.Message}");
                return null;
            }
        }

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
                    
                    if (modality == "?" && response.Dataset.Contains(DicomTag.ScheduledProcedureStepSequence))
                    {
                        var seq = response.Dataset.GetSequence(DicomTag.ScheduledProcedureStepSequence);
                        if (seq.Items.Count > 0) modality = seq.Items[0].GetSingleValueOrDefault(DicomTag.Modality, "?");
                    }
                    results.Add($"✅ FOUND: [{modality}] {name} (Acc: {acc})");
                }
            };

            string ip = "127.0.0.1";
            int port = 4242;
            string myAe = "STONEPACS";
            string serverAe = "MYORTHANC";

            try 
            {
                var s = SettingsService.Current;
                ip = s.OrthancIp; port = s.OrthancDicomPort; myAe = s.AETitle; serverAe = s.OrthancAETitle;
            }
            catch {}

            var client = DicomClientFactory.Create(ip, port, false, myAe, serverAe); 

            await client.AddRequestAsync(cfind);
            try { await client.SendAsync(); }
            catch (Exception ex) { results.Add($"❌ CONNECTION FAILED: {ex.Message}"); }

            if (results.Count == 0) results.Add("⚠️ No worklist items found.");
            
            return results;
        }
    }
}