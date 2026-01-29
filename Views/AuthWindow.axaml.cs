using Avalonia.Controls;
using Avalonia.Interactivity;

namespace StonePACS.Views
{
    public partial class AuthWindow : Window
    {
        public bool IsAuthenticated { get; private set; } = false;

        public AuthWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            // ✅ ใส่ !
            var user = this.FindControl<TextBox>("UserBox")!.Text;
            var pass = this.FindControl<TextBox>("PassBox")!.Text;

            if (user == "pacsadmin" && pass == "adminpacs")
            {
                IsAuthenticated = true;
                Close();
            }
            else
            {
                // ✅ ใส่ !
                var passBox = this.FindControl<TextBox>("PassBox")!;
                passBox.Text = "";
                passBox.Watermark = "Wrong Password!";
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}