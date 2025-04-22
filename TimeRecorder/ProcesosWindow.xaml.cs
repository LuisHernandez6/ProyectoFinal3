using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MonitorActividad;

namespace MonitorActividad
{
    public partial class ProcesosWindow : Window
    {
        public static int PerfilSeleccion;
        private List<Perfil> Perfiles = new List<Perfil>();

        public ObservableCollection<Proceso> AllProcesses { get; set; }
        //public ObservableCollection<Proceso> SelectedProcesses { get; set; }
        private ObservableCollection<Proceso> SelectedProcesses = MonitorActividad.App.SelectedProcesses;

        private readonly string[] allowedSystemProcesses = { "explorer", "Taskmgr" };

        public ObservableCollection<string> Items { get; set; }

        public ProcesosWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            AllProcesses = new ObservableCollection<Proceso>();
            //SelectedProcesses = new ObservableCollection<Proceso>();
            LeftListBox.ItemsSource = AllProcesses;
            RightListBox.ItemsSource = SelectedProcesses;

            Items = new ObservableCollection<string> { };
            DataContext = this;

            RefrescarPerfiles();
            //RefrescarProcesos();
        }

        private ObservableCollection<Proceso> GetFilteredProcesses()
        {
            var processList = new HashSet<string>();
            var filteredProcesses = new ObservableCollection<Proceso>();

            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    string processName = process.ProcessName;

                    // Ensure only processes with windows are shown, unless in allow list
                    bool hasWindow = process.MainWindowHandle != IntPtr.Zero;
                    bool isAllowed = allowedSystemProcesses.Contains(processName);

                    if (!hasWindow && !isAllowed) continue; // Skip processes without a window unless allowed

                    // Get window title if available
                    string windowTitle = string.IsNullOrWhiteSpace(process.MainWindowTitle) ? "" : $" ({process.MainWindowTitle})";
                    string displayName = $"{processName+".exe"}{windowTitle}";

                    if (!processList.Contains(processName))
                    {
                        processList.Add(processName);
                        filteredProcesses.Add(new Proceso
                        {
                            Nombre = processName,
                            Name = displayName,
                            Icon = GetProcessIcon(process)
                        });
                    }
                }
                catch { /* Prevent errors from inaccessible processes */ }
            }
            return filteredProcesses;
        }

        private void RefrescarProcesos()
        {
            var processList = new HashSet<string>();
            var filteredProcesses = new ObservableCollection<Proceso>();
            var newSelectedProcesses = new ObservableCollection<Proceso>();
            var tempProcesses = new List<Proceso>();

            string connStr = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
            ProcesosRepository repo = new ProcesosRepository(connStr);

            List<Proceso> procesos = repo.GetAllProcesos();

            AllProcesses.Clear();
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    string processName = process.ProcessName;

                    // Ensure only processes with windows are shown, unless in allow list
                    bool hasWindow = process.MainWindowHandle != IntPtr.Zero;
                    bool isAllowed = allowedSystemProcesses.Contains(processName);

                    if (!hasWindow && !isAllowed) continue; // Skip processes without a window unless allowed

                    // Get window title if available
                    string windowTitle = string.IsNullOrWhiteSpace(process.MainWindowTitle) ? "" : $" ({process.MainWindowTitle})";
                    string displayName = $"{processName + ".exe"}{windowTitle}";

                    if (!processList.Contains(processName) && !procesos.Any(p => p.Nombre == processName && p.PerfilID == PerfilSeleccion))
                    {
                        processList.Add(processName);
                        AllProcesses.Add(new Proceso
                        {
                            Nombre = processName,
                            Name = displayName,
                            Neutral = 2.5m,
                            Inactivo = 5.0m,
                            Icon = GetProcessIcon(process)
                        });
                        //Console.WriteLine("CRRRRo" + procesos.Any(p => p.Nombre == processName && p.PerfilID == PerfilSeleccion));
                    }
                    if (procesos.Any(p => p.Nombre == processName && p.PerfilID == PerfilSeleccion))
                    {
                        tempProcesses.Add(new Proceso
                        {
                            Nombre = processName,
                            Name = displayName,
                            Icon = GetProcessIcon(process)
                        });
                    }
                }
                catch { /* Prevent errors from inaccessible processes */ }
            }

            SelectedProcesses.Clear();
            foreach (var process in procesos)
            {
                //Console.WriteLine("HHHHHHHHHHHHH" + PerfilSeleccion);
                if (process.PerfilID == PerfilSeleccion)
                {
                    foreach (var process2 in tempProcesses)
                    {
                        if (process.Nombre == process2.Nombre)
                        {
                            process.Name = process2.Name;
                            process.Icon = process2.Icon; 
                            break;
                        }
                    }
                    SelectedProcesses.Add(process);
                }
            }
        }

        private void SoloRefrescarProcessosSeleccionados()
        {
            string connStr = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
            ProcesosRepository repo = new ProcesosRepository(connStr);

            List<Proceso> procesos = repo.GetAllProcesos();

            foreach (var process in procesos)
            {
                if (process.PerfilID == PerfilSeleccion)
                {
                    foreach (var process2 in SelectedProcesses)
                    {
                        if (process.ProcesoID == process2.ProcesoID)
                        {
                            process2.Categoria = process.Categoria;
                            process2.Neutral = process.Neutral;
                            process2.Inactivo = process.Inactivo;
                            //Console.WriteLine("KKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKK");
                            break;
                        }
                    }
                }
            }
        }

        private BitmapSource GetProcessIcon(Process process)
        {
            try
            {
                string path = process.MainModule?.FileName;
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(path))
                    using (var stream = new MemoryStream())
                    {
                        icon.ToBitmap().Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        stream.Seek(0, SeekOrigin.Begin);
                        return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                }
            }
            catch { /* Some system processes can't provide an icon */ }
            return null;
        }

        private void AddProcess_Click(object sender, RoutedEventArgs e)
        {
            if (LeftListBox.SelectedItem is Proceso selectedProcess)
            {
                string connStr = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
                ProcesosRepository repo = new ProcesosRepository(connStr);

                //List<Proceso> procesos = repo.GetAllProcesos();

                selectedProcess.PerfilID = PerfilSeleccion;
                repo.AddProceso(selectedProcess);

                //SelectedProcesses.Add(selectedProcess);
                //AllProcesses.Remove(selectedProcess);

                RefrescarProcesos();
            }
        }

        private void RemoveProcess_Click(object sender, RoutedEventArgs e)
        {
            if (RightListBox.SelectedItem is Proceso selectedProcess)
            {
                string connStr = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
                ProcesosRepository repo = new ProcesosRepository(connStr);

                selectedProcess.PerfilID = PerfilSeleccion;
                repo.DeleteProceso(selectedProcess.ProcesoID);

                RefrescarProcesos();
            }
        }

        Proceso prevProceso = null;

        private void RightListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SoloRefrescarProcessosSeleccionados();
            Proceso proceso = RightListBox.SelectedItem as Proceso;

            if (RightListBox.SelectedItem != null)
            {
                SaveProcessData();

                ModeComboBox.SelectedIndex = proceso.Categoria;
                NeutralTimeTextBox.Text = proceso.Neutral.ToString();
                InactiveTimeTextBox.Text = proceso.Inactivo.ToString();
            }
            else
            {
                ModeComboBox.SelectedIndex = -1;
                NeutralTimeTextBox.Text = "";
                InactiveTimeTextBox.Text = "";
            }
            prevProceso = proceso;
        }

        private void SaveProcessData()
        {
            if (prevProceso == null) { return; }
           
            string connStr = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
            ProcesosRepository repo = new ProcesosRepository(connStr);

            decimal neutral;
            decimal inactive;

            decimal.TryParse(NeutralTimeTextBox.Text, out neutral);
            decimal.TryParse(InactiveTimeTextBox.Text, out inactive);

            repo.UpdateProceso(new Proceso()
            {
                ProcesoID = prevProceso.ProcesoID,
                Nombre = prevProceso.Nombre,
                Categoria = Math.Max(ModeComboBox.SelectedIndex,0),
                Neutral = neutral,
                Inactivo = inactive,
                PerfilID = prevProceso.PerfilID
            });
        }

        private void CreateProfile_Click(object sender, RoutedEventArgs e)
        {
            Perfil newPerfil = new Perfil { Nombre = "Nuevo Perfil", HoraInicio = new TimeSpan(0, 0, 0), HoraFin = new TimeSpan(23, 59, 0) };

            string connStr = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
            PerfilRepository perfilRepo = new PerfilRepository(connStr);

            perfilRepo.CreateProfile(newPerfil);

            List<Perfil> perfiles = perfilRepo.GetAllPerfiles();

            if (perfiles.Count > 0)
            {
                PerfilSeleccion = perfiles[perfiles.Count-1].PerfilID + 1;
            }
            else
            {
                PerfilSeleccion = 1;
            }

            RefrescarPerfiles();
        }

        private void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            string connStr = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
            PerfilRepository perfilRepo = new PerfilRepository(connStr);
            ProcesosRepository procesosRepo = new ProcesosRepository(connStr);


            MessageBoxResult result = MessageBox.Show("¿Seguro que quiere eliminar el perfil?",
                                                      "Confirmacion",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // First, delete the associated Procesos
                var procesos = procesosRepo.GetAllProcesos().Where(p => p.PerfilID == PerfilSeleccion).ToList();

                foreach (var proceso in procesos)
                {
                    procesosRepo.DeleteProceso(proceso.ProcesoID);
                }

                // Then, delete the profile
                perfilRepo.DeleteProfile(PerfilSeleccion);
                PerfilSeleccion = 0;
                RefrescarPerfiles();
            }
        }

        private void CopiarProfile_Click(object sender, RoutedEventArgs e)
        {
            string connStr = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
            PerfilRepository perfilRepo = new PerfilRepository(connStr);
            ProcesosRepository procesosRepo = new ProcesosRepository(connStr);

            var All = perfilRepo.GetAllPerfiles();

            var perfil = All.FirstOrDefault(a => a.PerfilID == PerfilSeleccion);

            if (perfil != null)
            {
                perfil.Nombre = perfil.Nombre + " (copia)";
                perfilRepo.AddProfile(perfil);
            }

            //Seleccionamos el ultimo perfil

            List<Perfil> perfiles = perfilRepo.GetAllPerfiles();

            if (perfiles.Count > 0)
            {
                PerfilSeleccion = perfiles[perfiles.Count - 1].PerfilID;
            }
            else
            {
                PerfilSeleccion = 1;
            }

            // Copy associated Procesos and assign the new PerfilID
            var procesos = procesosRepo.GetAllProcesos().Where(p => p.PerfilID == perfil.PerfilID).ToList();

            foreach (var proceso in procesos)
            {
                var copiedProceso = new Proceso
                {
                    Nombre = proceso.Nombre,
                    Categoria = proceso.Categoria,
                    Neutral = proceso.Neutral,
                    Inactivo = proceso.Inactivo,
                    PerfilID = PerfilSeleccion // Assign the new PerfilID
                };

                procesosRepo.AddProceso(copiedProceso);
            }

            RefrescarPerfiles();
        }

        private void RefrescarPerfiles()
        {
            Perfiles.Clear();
            //Console.WriteLine("Mamo");
            Items.Clear();
            //Console.WriteLine("Camo");

            string connStr = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
            var repo = new PerfilRepository(connStr);

            List<Perfil> perfiles = repo.GetAllPerfiles();

            int NuevoPerfilSeleccion = 0;

            int count = 0;
            foreach (Perfil perfil in perfiles)
            {
                Perfiles.Add(perfil);
                Items.Add(perfil.Nombre);

                if (perfil.PerfilID == PerfilSeleccion)
                {
                    NuevoPerfilSeleccion = count;
                }
                count++;
            }
            //Console.WriteLine("CountH" + count);
            ProfileComboBox.SelectedIndex = NuevoPerfilSeleccion;
            //Console.WriteLine(NuevoPerfilSeleccion);
        }

        private void ModeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ModeComboBox.SelectedItem is ComboBoxItem selectedMode)
            {
                NeutralTimeTextBox.IsEnabled = selectedMode.Content.ToString() == "Productivo";
            }
        }

        bool dontRefresh = false;

        private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Console.WriteLine("CountST"+ Perfiles.Count);
            //Console.WriteLine("CountER" +Items.Count);
            if (Items.Count == 0 || (ProfileComboBox.SelectedIndex == -1)) return;
            PerfilSeleccion = Perfiles[ProfileComboBox.SelectedIndex].PerfilID;
            //Console.WriteLine("CountXDDDD"+PerfilSeleccion);

            // Retrieve the selected profile data
            var perfil = Perfiles.FirstOrDefault(p => p.PerfilID == PerfilSeleccion);
            if (perfil != null)
            {
                ProfileNameTextBox.Text = perfil.Nombre;
                StartTimeTextBox.Text = perfil.HoraInicio.ToString(); // Assuming it's a DateTime
                EndTimeTextBox.Text = perfil.HoraFin.ToString();
            }

            if (!dontRefresh)
            {
                RefrescarProcesos();
            }
            dontRefresh = false;
        }

        private void ProfileData_LostFocus(object sender, RoutedEventArgs e)
        {
            string connStr = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
            PerfilRepository perfilRepo = new PerfilRepository(connStr);

            var perfil = Perfiles.FirstOrDefault(p => p.PerfilID == PerfilSeleccion);
            if (perfil != null)
            {
                TimeSpan timeSpan;
                TimeSpan.TryParse(StartTimeTextBox.Text, out timeSpan);
                perfil.HoraInicio = timeSpan;
                TimeSpan.TryParse(EndTimeTextBox.Text, out timeSpan);
                perfil.HoraFin = timeSpan;

                perfil.Nombre = ProfileNameTextBox.Text;

                perfilRepo.UpdateProfile(perfil);

                int index = ProfileComboBox.SelectedIndex;
                if (index >= 0 && index < Items.Count)
                {
                    dontRefresh = true;
                    Items[index] = perfil.Nombre; // Update the name in the collection
                    ProfileComboBox.SelectedIndex = index;
                }
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveProcessData();
            ProfileData_LostFocus(null, null);
        }
    }
}