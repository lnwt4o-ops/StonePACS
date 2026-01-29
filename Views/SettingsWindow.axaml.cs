using Avalonia.Controls;
using Avalonia.Interactivity;
using StonePACS.Services;

namespace StonePACS.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            LoadValues();
        }

        private void LoadValues()
        {
            var s = SettingsService.Current;
            
            // ✅ ใส่ ! หรือ ? เพื่อแก้ Warning
            this.FindControl<TextBox>("TxtIp")!.Text = s.OrthancIp;
            this.FindControl<TextBox>("TxtWebPort")!.Text = s.OrthancWebPort.ToString();
            this.FindControl<TextBox>("TxtDicomPort")!.Text = s.OrthancDicomPort.ToString();
            this.FindControl<TextBox>("TxtMyAE")!.Text = s.AETitle;
            this.FindControl<TextBox>("TxtServerAE")!.Text = s.OrthancAETitle;
            
            // Client-Server Paths
            this.FindControl<TextBox>("TxtWorklistPath")!.Text = s.WorklistFolder;
            this.FindControl<TextBox>("TxtDbPath")!.Text = s.DatabasePath;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var s = SettingsService.Current;
            
            // ✅ ใส่ ! และใช้ ?? "" กันค่าว่าง
            s.OrthancIp = this.FindControl<TextBox>("TxtIp")!.Text ?? "127.0.0.1";
            
            if (int.TryParse(this.FindControl<TextBox>("TxtWebPort")!.Text, out int webPort))
                s.OrthancWebPort = webPort;

            if (int.TryParse(this.FindControl<TextBox>("TxtDicomPort")!.Text, out int dicomPort))
                s.OrthancDicomPort = dicomPort;

            s.AETitle = this.FindControl<TextBox>("TxtMyAE")!.Text ?? "STONEPACS";
            s.OrthancAETitle = this.FindControl<TextBox>("TxtServerAE")!.Text ?? "ORTHANC";
            
            s.WorklistFolder = this.FindControl<TextBox>("TxtWorklistPath")!.Text ?? "";
            s.DatabasePath = this.FindControl<TextBox>("TxtDbPath")!.Text ?? "StonePACS.db";

            s.Save();
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}