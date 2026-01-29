using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StonePACS.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        // ตัวแปรเก็บว่าตอนนี้กำลังโชว์หน้าไหนอยู่
        [ObservableProperty]
        private ViewModelBase _currentView;

        public MainWindowViewModel()
        {
            // เปิดมาเจอหน้า Worklist ก่อนเลย
            CurrentView = new WorklistViewModel();
        }

        // คำสั่งเปลี่ยนหน้า
        [RelayCommand]
        public void GoToWorklist() => CurrentView = new WorklistViewModel();

        [RelayCommand]
        public void GoToRegistration() => CurrentView = new RegistrationViewModel();
    }
}