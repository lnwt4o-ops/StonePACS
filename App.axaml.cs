using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using StonePACS.ViewModels;
using StonePACS.Views;
using System.Linq;

namespace StonePACS
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            // ✅ ตั้งค่าภาษาเป็น English เพื่อแก้ปัญหา DatePicker แสดงผลเพี้ยน
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            System.Globalization.CultureInfo.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // ปิดการตรวจสอบ DataAnnotation ซ้ำซ้อน
                DisableAvaloniaDataAnnotationValidation();

                // ✅ เชื่อมต่อ MainWindow เข้ากับ MainViewModel (Server Dashboard)
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}