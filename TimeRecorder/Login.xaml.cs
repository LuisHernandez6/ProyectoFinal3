using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MonitorActividad;
using MessageBox = System.Windows.Forms.MessageBox;

namespace MonitorActividad
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void buttonLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginUsuario();
        }

        private void buttonRegistrar_Click(object sender, RoutedEventArgs e)
        {
            if (Registrar.GetForm() != null)
            {
                if (Registrar.GetForm().WindowState == WindowState.Minimized)
                {
                    Registrar.GetForm().WindowState = WindowState.Normal;
                }
                Registrar.GetForm().Activate(); return;
            }

            new Registrar();
            Registrar.GetForm().Show();
        }

        private void LoginUsuario()
        {
            string connStr = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
            var repo = new UsuarioRepository(connStr);

            // Get all users
            List<Usuario> usuarios = repo.GetAllUsuarios();

            foreach (var usuario in usuarios)
            {
                if (usuario.NombreUsuario == textBox.Text.Trim() && usuario.Contraseña == textBox1.Password.Trim())
                {
                    MainWindow Main = new MainWindow(usuario.UsuarioID);
                    Main.Show();
                    Close();
                    return;
                }
            }

            MessageBox.Show("Credenciales incorrectas.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
