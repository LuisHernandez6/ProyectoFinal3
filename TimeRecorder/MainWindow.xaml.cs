using System;
using System.Collections.ObjectModel;
//using System.Diagnostics;
using System.IO;
//using System.Linq;
//using System.Timers;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Management;
using System.Drawing;
using System.Windows.Data;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Threading;
using static MonitorActividad.SystemInputsRefresh;
using static MonitorActividad.SystemProcessesTracker;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.Generic;
using System.Linq;
using MonitorActividad;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using Forms = System.Windows.Forms;

namespace MonitorActividad
{
    public partial class MainWindow : Window
    {
        public static bool IsOpen = false;
        public static MainWindow WndObject;
        static private int UsuarioSeleccion;
        //public List<Actividad> UsuarioActividad;
        static private List<Usuario> Usuarios = new List<Usuario>();
        static public Timer ActualizarDiagramaTimer;

        static Timer WindowListLastDateRefresh;

        public ObservableCollection<string> Dias { get; set; } = new ObservableCollection<string>();
        public ChartValues<double> TiempoIdle { get; set; } = new ChartValues<double>();
        public ChartValues<double> NoProductivo { get; set; } = new ChartValues<double>();
        public ChartValues<double> Productivo { get; set; } = new ChartValues<double>();
        public ChartValues<double> NoCategorizado { get; set; } = new ChartValues<double>();
        public ChartValues<double> Neutral { get; set; } = new ChartValues<double>();

        static public ObservableCollection<string> Items { get; set; }

        public MainWindow(int UsuarioID)
        {
            string connStr = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
            var repo = new ActividadRepository(connStr);

            var allActividades = repo.GetAllActividades();

            Actividad actividad = allActividades.FirstOrDefault(a => a.FechaInicial.Date == DateTime.Now.Date);

            if (actividad != null)
            {
                App.productivo = actividad.Productivo;
                App.noProductivo = actividad.NoProductivo;
                App.neutral = actividad.Neutral;
                App.inactivo = actividad.Inactivo;
                App.noCategorizado = actividad.NoCategorizado;
            }

            App.UsuarioSesion = UsuarioID;
            App.CargarProcesosPerfil();

            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //InitTimer();
            //AddRunningProcessesOnStartUp();
            //LoadListIcons();

            IsOpen = true;
            WndObject = this;
            this.SizeToContent = SizeToContent.Width;

            switch (WindowState)
            {
                case WindowState.Normal:
                case WindowState.Maximized:
                    break;

                case WindowState.Minimized:
                    break;
            }

            Items = new ObservableCollection<string> { };
            DataContext = this;

            ActualizarSeleccion(RefrescarUsuarios());
            App.UsuarioActividad = CargarActividadUsuario(DateTime.Now, UsuarioID);

            ActualizarDiagramaTimer = new Timer(ActualizarDiagrama, null, 1000, Timeout.Infinite);

            CheckAllSystemProcessesIntervals = new Timer(CheckAllSystemProcesses, null, 1000, Timeout.Infinite);
            FocusInputsIntervals = new Timer(RefreshAllFocusInputs, null, 20, Timeout.Infinite);

            LoadChart(DateTime.Now);

            chartActividad.AxisY[0].MaxValue = 200;
        }

        static bool ActualizandoDiagrama = false;

        public void ActualizarDiagrama(object stateInfo)
        {
            if (ActualizandoDiagrama || App.UsuarioSesion == -1) { return; } // Lazy quick-fix to avoid executing this method if it's already being executed.
            ActualizandoDiagrama = true;

            App.ActualizarActividad(App.UsuarioActividadHoy);
            LoadChart(CurrentDate);

            ActualizandoDiagrama = false;
            ActualizarDiagramaTimer.Change(1000, Timeout.Infinite);
        }

        public List<Actividad> CargarActividadUsuario(DateTime referenceDate, int usuarioID)
        {
            string connStr = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
            var repo = new ActividadRepository(connStr);

            var allActividades = repo.GetAllActividades();

            var monthStart = new DateTime(referenceDate.Year, referenceDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // Check if there's an activity with the current date
            var currentDateActividad = allActividades.FirstOrDefault(a => a.FechaInicial.Date == DateTime.Now.Date);

            // If no activity exists with the current date, add a new one
            if (currentDateActividad == null)
            {
                var nuevaActividad = new Actividad
                {
                    FechaInicial = DateTime.Now.Date,
                    UltimaHora = DateTime.Now,
                    UsuarioID = usuarioID,
                    // Add other default properties as needed
                };
                repo.AddActividad(nuevaActividad);
                allActividades = repo.GetAllActividades();
            }

            var filteredActividades = allActividades
                .Where(a => a.FechaInicial.Date >= monthStart.Date && a.FechaInicial.Date <= monthEnd.Date && a.UsuarioID == usuarioID)
                .OrderBy(a => a.FechaInicial)
                .ToList();

            App.UsuarioActividadHoy = allActividades.FirstOrDefault(a => a.FechaInicial.Date == DateTime.Now.Date && a.UsuarioID == App.UsuarioSesion).ActividadID;

            return filteredActividades;
        }

        public void LoadChart(DateTime referenceDate)
        {
            var monthStart = new DateTime(referenceDate.Year, referenceDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var chartData = App.UsuarioActividad;

            // Ensure all days of the month exist
            var actividadesMap = Enumerable.Range(0, (monthEnd - monthStart).Days + 1)
                .Select(i => monthStart.AddDays(i))
                .ToDictionary(d => d, d => new ActividadView
                {
                    FechaInicial = d,
                    Inactivo = 0,
                    Productivo = 0,
                    NoProductivo = 0,
                    NoCategorizado = 0,
                    Neutral = 0
                });

            // Populate actual data
            foreach (var actividad in chartData)
            {
                DateTime actividadDate = actividad.FechaInicial.Date;

                if (actividadesMap.ContainsKey(actividadDate))
                {
                    var entry = actividadesMap[actividadDate];
                    entry.Inactivo += (double)actividad.Inactivo/1000;
                    entry.Productivo += (double)actividad.Productivo/1000;
                    entry.NoProductivo += (double)actividad.NoProductivo/1000;
                    entry.NoCategorizado += (double)actividad.NoCategorizado/1000;
                    entry.Neutral += (double)actividad.Neutral/1000;
                }
            }

            // Bind data to the chart
            Dias.Clear();
            TiempoIdle.Clear();
            NoProductivo.Clear();
            Productivo.Clear();
            NoCategorizado.Clear();
            Neutral.Clear();

            foreach (var actividad in actividadesMap.Values.OrderBy(a => a.FechaInicial))
            {
                Dias.Add(actividad.FechaInicial.ToString("yyyy-MM-dd"));
                TiempoIdle.Add(actividad.Inactivo);
                NoProductivo.Add(actividad.NoProductivo);
                Productivo.Add(actividad.Productivo);
                NoCategorizado.Add(actividad.NoCategorizado);
                Neutral.Add(actividad.Neutral);
            }
        }

        public DateTime CurrentDate { get; set; } = DateTime.Now;

        private void UpdateMonthYearLabel()
        {
            CultureInfo culture = new CultureInfo("es-ES"); // Replace with your desired language/culture code
            lblCurrentMonthYear.Content = CurrentDate.ToString("MMMM, yyyy", culture);
        }

        private void BtnPreviousMonth_Click(object sender, RoutedEventArgs e)
        {
            CurrentDate = CurrentDate.AddMonths(-1);
            App.UsuarioActividad = CargarActividadUsuario(CurrentDate, UsuarioSeleccion);
            LoadChart(CurrentDate);
            UpdateMonthYearLabel();
        }

        private void BtnNextMonth_Click(object sender, RoutedEventArgs e)
        {
            CurrentDate = CurrentDate.AddMonths(1);
            App.UsuarioActividad = CargarActividadUsuario(CurrentDate, UsuarioSeleccion);
            LoadChart(CurrentDate);
            UpdateMonthYearLabel();
        }

        public static int RefrescarUsuarios()
        {
            Usuarios.Clear();
            Items.Clear();

            string connStr = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
            var repo = new UsuarioRepository(connStr);

            List<Usuario> usuarios = repo.GetAllUsuarios();

            int NuevoUsuarioSeleccion = 0;

            foreach (Usuario usuario in usuarios)
            {
                if(usuario.UsuarioID == App.UsuarioSesion)
                {
                    Usuarios.Add(usuario);
                    Items.Add("(Yo)");

                    if (usuario.PerfilID != null)
                    {
                        if (!App.enPerfil)
                        {
                            App.RestaurarSoloEstadoLocal();
                        }
                        App.enPerfil = true;
                    }
                    else
                    {
                        App.enPerfil = false;
                    }
                }
            }

            int count = 1;
            foreach (Usuario usuario in usuarios)
            {
                if (usuario.UsuarioID != App.UsuarioSesion)
                {
                    Usuarios.Add(usuario);
                    Items.Add(usuario.Nombre + " " + usuario.Apellido);

                    if (usuario.UsuarioID == UsuarioSeleccion)
                    {
                        NuevoUsuarioSeleccion = count;
                    }
                    count++;
                }
            }

            //Console.WriteLine(NuevoUsuarioSeleccion);
            return NuevoUsuarioSeleccion;
        }

        public void ActualizarSeleccion(int NuevoUsuarioSeleccion)
        {
            comboBox.SelectedIndex = NuevoUsuarioSeleccion;
        }

        //private Timer rtimer;
        //private void InitTimer()
        //{
        //rtimer = new Timer();
        //rtimer.AutoReset = true;
        //rtimer.Interval = 1000; // in miliseconds
        //rtimer.Elapsed += new ElapsedEventHandler(RefreshList);
        //rtimer.Start();
        //}

        //private void AddRunningProcessesOnStartUp()
        //{
        //var plist = Processes.ProcessList;
        //string pname;

        //for (int i = 0; i < plist.Count; i++)
        //{
        //pname = Path.GetFileNameWithoutExtension(plist[i].PName);

        //using (var searcher = new ManagementObjectSearcher("SELECT ProcessId FROM Win32_Process"))
        //using (var results = searcher.Get())
        //{
        //var query = from p in Process.GetProcessesByName(pname)
        //join mo in results.Cast<ManagementObject>()
        //on p.Id equals (int)(uint)mo["ProcessId"]
        //select new
        //{
        ////Process = p,
        //Id = (int)(uint)mo["ProcessId"],
        //};

        //foreach (var p in query)
        //{
        //App.AddRunningProcess(p.Id, IntPtr.Zero);
        //}
        //}
        //}
        //}

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (AddProcess.IsOpen) 
            {
                if(AddProcess.WndObject.WindowState == WindowState.Minimized) 
                {
                    AddProcess.WndObject.WindowState = WindowState.Normal;
                }
                AddProcess.WndObject.Activate(); return; 
            }

            AddProcess.WndObject = new AddProcess();
            AddProcess.WndObject.Show();
        }

        //private void BoxStartup_Click(object sender, RoutedEventArgs e)
        //{
        //    RegistryKey key = Registry.CurrentUser.OpenSubKey(RegPath, true);

        //    if ((bool)BoxStartup.IsChecked)
        //    {
        //        key.SetValue("TimeRecorder", "\""+System.Reflection.Assembly.GetEntryAssembly().Location+"\" -silent");
        //    }
        //    else
        //    {
        //        key.DeleteValue("TimeRecorder", false);
        //    }
        //}

        private void Window_StateChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Normal:
                case WindowState.Maximized:
                    this.ShowInTaskbar = true;
                    //rtimer.Start();
                    IsOpen = true;
                    WindowListLastDateRefresh.Change(0, Timeout.Infinite);
                    //LoadListIcons();
                    break;

                case WindowState.Minimized:
                    this.ShowInTaskbar = false;
                    //rtimer.Stop();
                    IsOpen = false;
                    WindowListLastDateRefresh.Change(Timeout.Infinite, Timeout.Infinite);
                    CleanUpAfterUnloadingIcons(); //Attempt to free some memory of the BitmapSource objects right away so it just doesn't stay there.
                    if (AddProcess.IsOpen) 
                    {
                        AddProcess.WndObject.Close();
                    }
                    break;
            }
        }

        public void CleanUpAfterUnloadingIcons()
        {
            for (int i = 1; i <= 5; i++)
            {
                CleanUp(i * 1000);
            }
            async void CleanUp(int delay)
            {
                await Task.Delay(delay);

                if (IsOpen) { return; }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            App.UsuarioSesion = -1;
            App.RestaurarActividadLocal();
            base.OnClosed(e);
            //Application.Current.Shutdown();
            //rtimer.Dispose();
        }

        public static void CheckIfDataFileNeedsReplacement()
        {
            string programPath = Directory.GetCurrentDirectory();

            if (File.Exists(programPath+@"\data\processlist.csv.bak"))
            {
                if (File.Exists(programPath + @"\data\processlist.csv"))
                {
                    byte[] fileBytes = File.ReadAllBytes(programPath + @"\data\processlist.csv");

                    if (fileBytes[0] == 0)
                    {
                        File.Delete(programPath + @"\data\processlist.csv");
                        File.Copy(programPath + @"\data\processlist.csv.bak", programPath + @"\data\processlist.csv");
                    }
                }
                else
                {
                    File.Copy(programPath + @"\data\processlist.csv.bak", programPath + @"\data\processlist.csv");
                }
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Login LoginForm = new Login();
            LoginForm.Show();
            Close();
        }

        public static bool noChange = false;

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (noChange) { noChange = false;  return; }
            UsuarioSeleccion = Usuarios[comboBox.SelectedIndex].UsuarioID;
            label.Content = Usuarios[comboBox.SelectedIndex].Nombre + " " + Usuarios[comboBox.SelectedIndex].Apellido;
            App.UsuarioActividad = CargarActividadUsuario(CurrentDate, UsuarioSeleccion);
            LoadChart(CurrentDate);
        }

        private void buttonActividad_Click(object sender, RoutedEventArgs e)
        {
            ProcesosWindow ProcesosForm = new ProcesosWindow();
            ProcesosForm.Show();
        }

        private void buttonModificar_Click(object sender, RoutedEventArgs e)
        {
            Usuario InstUsuarioSesion = Usuarios.FirstOrDefault(a => a.UsuarioID == App.UsuarioSesion);

            Registrar Modificar = new Registrar(UsuarioSeleccion, InstUsuarioSesion.Nivel);
            Modificar.Show();
        }

        private void buttonNuevo_Click(object sender, RoutedEventArgs e)
        {
            Usuario InstUsuarioSesion = Usuarios.FirstOrDefault(a => a.UsuarioID == App.UsuarioSesion);

            Registrar Crear = new Registrar(null, InstUsuarioSesion.Nivel);
            Crear.Show();
        }

        private void buttonEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (App.UsuarioSesion == UsuarioSeleccion) { Forms.MessageBox.Show("No puede eliminar al usuarios de la sesion actual.", "Warning", Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Warning); return; }

            string name = Usuarios.FirstOrDefault(a => a.UsuarioID == UsuarioSeleccion).NombreUsuario;

            MessageBoxResult result = MessageBox.Show("¿Seguro que quiere eliminar al usuario \""+name+"\"?",
                                                      "Confirmacion",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                string connStr = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
                var repo = new UsuarioRepository(connStr);

                repo.DeleteUsuario(UsuarioSeleccion);
            }
        }
    }

    public static class TRIconConverter
    {
        public static ImageSource ToImageSource(this String icon, string dir)
        {
            if (string.IsNullOrEmpty(icon))
            {
                icon = dir;
            }
            if (!File.Exists(icon))
            {
                return null;
            }

            if (icon.Substring(0, 2) == @".\")
            {
                icon = Directory.GetCurrentDirectory() + icon.Substring(1, icon.Length-1);
            }
            try
            {
                Uri image = new Uri(icon);

                if (image != null)
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = image;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    return bitmap;
                }
                else
                {
                    return Imaging.CreateBitmapSourceFromHIcon(
                        Icon.ExtractAssociatedIcon(icon).Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
            catch
            {
                return Imaging.CreateBitmapSourceFromHIcon(
                    Icon.ExtractAssociatedIcon(icon).Handle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            } 
        }
    }

    public class TRProcess : INotifyPropertyChanged
    {
        public bool Enabled { get; set; }
        public bool RecordWnd { get; set; }
        public bool MatchMode { get; set; }
        public string Dir { get; set; }
        public string IcoDir { get; set; }

        public long Hours { get; set; }
        public long MinH { get; set; }
        public long FocusH { get; set; }
        public long InputH { get; set; }
        public long InputKeyH { get; set; }
        public long InputMouseH { get; set; }
        public long InputKMH { get; set; }
        public long InputJoyH { get; set; }

        public long InputWaitT { get; set; }
        public long InputSaveT { get; set; }

        // Elements shown in UI

        public string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); }
        }
        public string _pname { get; set; }
        public string PName
        {
            get { return _pname; }
            set { _pname = value; OnPropertyChanged("PName"); }
        }
        public string _wndname { get; set; }
        public string WndName
        {
            get { return _wndname; }
            set { _wndname = value; OnPropertyChanged("WndName"); }
        }
        public float _viewhours { get; set; }
        public float ViewHours
        {
            get { return _viewhours; }
            set { _viewhours = value; OnPropertyChanged("ViewHours"); }
        }
        public float _viewminh { get; set; }
        public float ViewMinH
        {
            get { return _viewminh; }
            set { _viewminh = value; OnPropertyChanged("ViewMinH"); }
        }
        public float _viewfocush { get; set; }
        public float ViewFocusH
        {
            get { return _viewfocush; }
            set { _viewfocush = value; OnPropertyChanged("ViewFocusH"); }
        }
        public float _viewinputh { get; set; }
        public float ViewInputH
        {
            get { return _viewinputh; }
            set { _viewinputh = value; OnPropertyChanged("ViewInputH"); }
        }
        public float _viewinputkeyh { get; set; }
        public float ViewInputKeyH
        {
            get { return _viewinputkeyh; }
            set { _viewinputkeyh = value; OnPropertyChanged("ViewInputKeyH"); }
        }
        public float _viewinputmouseh { get; set; }
        public float ViewInputMouseH
        {
            get { return _viewinputmouseh; }
            set { _viewinputmouseh = value; OnPropertyChanged("ViewInputMouseH"); }
        }
        public float _viewinputkmh { get; set; }
        public float ViewInputKMH
        {
            get { return _viewinputkmh; }
            set { _viewinputkmh = value; OnPropertyChanged("ViewInputKMH"); }
        }
        public float _viewinputjoyh { get; set; }
        public float ViewInputJoyH
        {
            get { return _viewinputjoyh; }
            set { _viewinputjoyh = value; OnPropertyChanged("ViewInputJoyH"); }
        }
        public string _first { get; set; }
        public string First
        {
            get { return _first; }
            set { _first = value; OnPropertyChanged("First"); }
        }
        public string _last { get; set; }
        public string Last 
        {
            get { return _last; }
            set { _last = value; OnPropertyChanged("Last"); }
        }
        public ImageSource _ico { get; set; }
        public ImageSource Ico
        {
            get { return _ico; }
            set { _ico = value; OnPropertyChanged("Ico"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            //if (MainWindow.IsOpen) 
            //{
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            //}
        }
    }
}
