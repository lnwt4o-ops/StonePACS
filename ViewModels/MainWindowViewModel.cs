using CommunityToolkit.Mvvm.ComponentModel;

namespace StonePACS.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        // สร้าง Property เพื่อเก็บ ViewModel ของหน้าปัจจุบันที่จะแสดง
        [ObservableProperty]
        private ViewModelBase _currentView;

        public MainWindowViewModel()
        {
            // เริ่มต้นโปรแกรม ให้แสดงหน้า Registration เป็นหน้าแรก
            CurrentView = new RegistrationViewModel();
        }
    }
}