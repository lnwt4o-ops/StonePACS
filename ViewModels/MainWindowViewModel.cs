using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StonePACS.Services;

namespace StonePACS.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        [ObservableProperty] private string _systemMessage = "Ready";

        // คุม IIS (ใช้ iisreset)
        [RelayCommand]
        public void ManageIIS(string action)
        {
            SystemMessage = $"IIS: {action}ing...";
            ExecuteCommand("iisreset.exe", $"/{action}");
        }

        // คุม Orthanc (ใช้ net start/stop)
        [RelayCommand]
        public void ManageOrthanc(string action)
        {
            SystemMessage = $"Orthanc: {action}ing...";
            string cmd = (action == "restart") 
                ? "/c net stop Orthanc & net start Orthanc" 
                : $"/c net {action} Orthanc";
            
            ExecuteCommand("cmd.exe", cmd);
        }

        // ปุ่มเปิดหน้าเว็บ
        [RelayCommand]
        public void OpenWebRegistration()
        {
            var s = SettingsService.Current;
            string url = $"http://localhost:{s.OrthancWebPort}/app/explorer.html"; // หรือ URL ของ IIS
            
            try {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            } catch (Exception ex) {
                SystemMessage = $"Error: {ex.Message}";
            }
        }

        // ฟังก์ชันช่วยรัน Command Prompt แบบ Admin
        private void ExecuteCommand(string fileName, string args)
        {
            try {
                ProcessStartInfo psi = new ProcessStartInfo(fileName, args)
                {
                    Verb = "runas", // ⚠️ สำคัญ: บังคับขอสิทธิ์ Admin
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(psi);
                SystemMessage = "Command executed successfully.";
            }
            catch (Exception ex) {
                SystemMessage = "Admin access denied or Error.";
            }
        }
    }
}