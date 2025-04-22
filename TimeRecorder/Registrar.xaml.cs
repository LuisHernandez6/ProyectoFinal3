using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Configuration;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;
using MonitorActividad;
using System.Collections.ObjectModel;

namespace MonitorActividad
{
    /// <summary>
    /// Interaction logic for Registrar.xaml
    /// </summary>
    public partial class Registrar : Window
    {
        public static Registrar WndObject;
        private List<Usuario> Usuarios;
        public ObservableCollection<string> Items;
        public ObservableCollection<string> Items1;
        private List<Perfil> Perfiles;
        private int nivel = 0;
        private int? usuarioID = null;

        public Registrar(int? UsuarioID = null, int Nivel = 0)
        {
            if (WndObject == null)
            {
                InitializeComponent();
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                WndObject = this;

                Items = new ObservableCollection<string> { };
                Items1 = new ObservableCollection<string> { };

                if (UsuarioID != null)
                {
                    // Modificar Mode
                    this.Title = "Modificar";
                    button.Content = "Guardar";

                    string connStr = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
                    var repo = new UsuarioRepository(connStr);

                    Usuarios = repo.GetAllUsuarios();

                    Usuario Usuario = Usuarios.FirstOrDefault(a => a.UsuarioID == UsuarioID);

                    usuarioID = UsuarioID;

                    textBox.Text = Usuario.NombreUsuario;
                    textBox1.Text = Usuario.Nombre;
                    textBox2.Text = Usuario.Apellido;
                    textBox3.Text = Usuario.Correo;
                    textBox4.Text = Usuario.Telefono;

                    // Hidde elements
                    //textBox6.Visibility = Visibility.Collapsed;
                    //label6.Visibility = Visibility.Collapsed;

                    // Move elements up
                    //button.Margin = new Thickness(button.Margin.Left, button.Margin.Top - 30, button.Margin.Right, button.Margin.Bottom);
                    //comboBox.Margin = new Thickness(comboBox.Margin.Left, comboBox.Margin.Top - 30, comboBox.Margin.Right, comboBox.Margin.Bottom);
                    //comboBox2.Margin = new Thickness(comboBox2.Margin.Left, comboBox2.Margin.Top - 30, comboBox2.Margin.Right, comboBox2.Margin.Bottom);
                    //label3_Copy.Margin = new Thickness(label3_Copy.Margin.Left, label3_Copy.Margin.Top - 30, label3_Copy.Margin.Right, label3_Copy.Margin.Bottom);
                    //label4_Copy.Margin = new Thickness(label4_Copy.Margin.Left, label4_Copy.Margin.Top - 30, label4_Copy.Margin.Right, label4_Copy.Margin.Bottom);
                }

                if (Nivel < 1)
                {
                    // Hide elements for default Nivel
                    comboBox.Visibility = Visibility.Collapsed;
                    comboBox2.Visibility = Visibility.Collapsed;
                    label3_Copy.Visibility = Visibility.Collapsed;
                    label4_Copy.Visibility = Visibility.Collapsed;

                    // Move button up to compensate
                    button.Margin = new Thickness(button.Margin.Left, button.Margin.Top - 65, button.Margin.Right, button.Margin.Bottom);
                    this.Height -= 65; // Reduce window height
                }
                else 
                {
                    string connStr = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
                    var repo = new UsuarioRepository(connStr);
                    Usuarios = repo.GetAllUsuarios();
                    Usuario Usuario = Usuarios.FirstOrDefault(a => a.UsuarioID == UsuarioID);

                    var repoPerfil = new PerfilRepository(connStr);
                    Perfiles = repoPerfil.GetAllPerfiles();

                    nivel = Nivel;

                    Items.Add("Usuario");
                    Items.Add("Administrador");
                    comboBox.ItemsSource = Items;

                    bool exists = false;
                    int? foundAt = null;
                    int count = 0;

                    foreach (Perfil perfil in Perfiles)
                    {
                        Items1.Add(perfil.Nombre);

                        if (Usuario != null && perfil.PerfilID == Usuario.PerfilID)
                        {
                            exists = true;
                            foundAt = count;
                        }
                        count++;
                    }
                    comboBox2.ItemsSource = Items1;

                    if (Usuario != null)
                    {
                        comboBox.SelectedIndex = Math.Max(Math.Min(Usuario.Nivel, 1), 0);
                    }
                    else
                    {
                        comboBox.SelectedIndex = 0;
                    }

                    if (exists && foundAt != null)
                    {
                        comboBox2.SelectedIndex = (int)foundAt;
                    }
                    else
                    {
                        if (Usuario == null && count > 0)
                        {
                            comboBox2.SelectedIndex = 0;
                        }
                    }
                }
                this.DataContext = this;
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            WndObject = null;
        }

        public static Registrar GetForm() 
        {
            return WndObject;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            registrarUsuario();
        }

        private void registrarUsuario()
        {
            string connStr = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
            var repo = new UsuarioRepository(connStr);

            // Get all users
            List<Usuario> usuarios = repo.GetAllUsuarios();

            if (string.IsNullOrWhiteSpace(textBox.Text.Trim()) ||
                string.IsNullOrWhiteSpace(textBox1.Text.Trim()) ||
                string.IsNullOrWhiteSpace(textBox2.Text.Trim()) ||
                string.IsNullOrWhiteSpace(textBox3.Text.Trim()) ||
                string.IsNullOrWhiteSpace(textBox4.Text.Trim()) ||
                string.IsNullOrWhiteSpace(textBox5.Password.Trim()) ||
                string.IsNullOrWhiteSpace(textBox6.Password.Trim()))
            {
                MessageBox.Show("Todos los campos deben estar llenos.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (textBox5.Password.Trim() != textBox6.Password.Trim())
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            foreach (var usuario in usuarios)
            {
                if (usuario.NombreUsuario == textBox.Text.Trim() && (usuario.UsuarioID != usuarioID)) 
                {
                    MessageBox.Show("Nombre de usuario ya en uso.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (usuario.Contraseña == textBox5.Password.Trim() && (usuario.UsuarioID != usuarioID))
                {
                    MessageBox.Show("Contraseña no disponible.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            if (usuarioID == null)
            {
                // Add new user
                repo.AddUsuario(new Usuario
                {
                    NombreUsuario = textBox.Text.Trim(),
                    Nombre = textBox1.Text.Trim(),
                    Apellido = textBox2.Text.Trim(),
                    Correo = textBox3.Text.Trim(),
                    Telefono = textBox4.Text.Trim(),
                    Contraseña = textBox5.Password.Trim(),
                    Nivel = 0,
                    PerfilID = null
                });
            }
            else
            {
                int? PerfilID = null;
                int Nivel = 0;

                if (comboBox2.SelectedIndex != -1)
                {
                    PerfilID = Perfiles[comboBox2.SelectedIndex].PerfilID;
                }

                if (nivel != 0)
                {
                    Nivel = comboBox.SelectedIndex;
                }
                else
                {
                    PerfilID = Usuarios.FirstOrDefault(a => a.UsuarioID == usuarioID).PerfilID;
                }
                // modify
                repo.UpdateUsuario(new Usuario
                {
                    UsuarioID = (int)usuarioID,
                    NombreUsuario = textBox.Text.Trim(),
                    Nombre = textBox1.Text.Trim(),
                    Apellido = textBox2.Text.Trim(),
                    Correo = textBox3.Text.Trim(),
                    Telefono = textBox4.Text.Trim(),
                    Contraseña = textBox5.Password.Trim(),
                    Nivel = Nivel,
                    PerfilID = PerfilID
                });
            }

            Close();
        }
    }
}
