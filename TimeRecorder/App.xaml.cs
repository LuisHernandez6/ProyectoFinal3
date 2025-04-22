using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Forms = System.Windows.Forms;
using static MonitorActividad.App.NativeMethods;
using static MonitorActividad.SystemInputsRefresh;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Shapes;
using System.Data.SqlTypes;
using System.Collections.ObjectModel;
using System.Runtime.Remoting.Lifetime;
using System.Configuration;
using System.Data.SqlClient;

namespace MonitorActividad
{
    public partial class App : Application
    {
        private readonly Forms.NotifyIcon _notifyIcon;
        public static ObservableCollection<Proceso> SelectedProcesses { get; set; }
        public static ObservableCollection<Proceso> ProcesosPerfilActual { get; set; }

        public static List<Actividad> UsuarioActividad;
        public static int UsuarioActividadHoy;
        public static int UsuarioSesion = -1;
        public static bool enPerfil = false;

        static public int estado = 0;
        static public int categoria = 0;

        static public long productivo = 0;
        static public long noProductivo = 0;
        static public long neutral = 0;
        static public long inactivo = 0;
        static public long noCategorizado = 0;

        static public double tiempoNeutral = 0;
        static public double tiempoInactivo = 0;

        static public TimeSpan horarioIni = new TimeSpan(0, 0, 0);
        static public TimeSpan horarioFin = new TimeSpan(24, 0, 0);

        //ManagementEventWatcher processStartEvent = new ManagementEventWatcher(@"\\.\root\CIMV2", "SELECT * FROM __InstanceCreationEvent WITHIN .025 WHERE TargetInstance ISA 'Win32_Process'");
        //ManagementEventWatcher processStopEvent = new ManagementEventWatcher(@"\\.\root\CIMV2", "SELECT * FROM __InstanceDeletionEvent WITHIN .025 WHERE TargetInstance ISA 'Win32_Process'");

        static List<PHook> PHooks = new List<PHook>();

        public App()
        {
            InitializeAppSettingsFile();

            _notifyIcon = new Forms.NotifyIcon();
            SelectedProcesses = new ObservableCollection<Proceso>();
            ProcesosPerfilActual = new ObservableCollection<Proceso>();
            //processStartEvent.EventArrived += processStartEvent_EventArrived;
            //processStartEvent.Start();
            //processStopEvent.EventArrived += processStopEvent_EventArrived;
            //processStopEvent.Start();
            CanConnectToDatabase(ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString);
            EnsureTablesExist(ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString);
        }

        public static bool CanConnectToDatabase(string connectionString)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open(); // Try opening the connection
                    return true; // Success
                }
            }
            catch (Exception ex)
            {
                Forms.MessageBox.Show(
                    $"No se pudo conectar a la Base de Datos.",
                    "Error al conectar",
                    Forms.MessageBoxButtons.OK,
                    Forms.MessageBoxIcon.Error
                );
                Application.Current.Shutdown();
                return false; // Failure
            }
        }

        public static void CargarProcesosPerfil()
        {
            string connStr = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;

            ProcesosRepository repo = new ProcesosRepository(connStr);
            List<Proceso> procesos = repo.GetAllProcesos();

            var repoUs = new UsuarioRepository(connStr);
            List<Usuario> usuarios = repoUs.GetAllUsuarios();

            Usuario usuario = usuarios.FirstOrDefault(a => a.UsuarioID == UsuarioSesion);

            var repoPe = new PerfilRepository(connStr);
            Perfil perfil = repoPe.GetAllPerfiles().FirstOrDefault(a => a.PerfilID == usuario.PerfilID);

            ProcesosPerfilActual.Clear();

            if (usuario != null)
            {
                foreach (Proceso proceso in procesos)
                {
                    if (proceso.PerfilID == usuario.PerfilID)
                    {
                        ProcesosPerfilActual.Add(proceso);
                    }
                }

                /////////////////////
                if (perfil != null)
                {
                    horarioIni = perfil.HoraInicio;
                    horarioFin = perfil.HoraFin;
                }
            }
            //CambioDeVentana(ultimoProceso);
            //Console.WriteLine("DIPLO");
        }

        public static void RestaurarActividadLocal()
        {
            lastState = false;
            lastActiveTime = GetTimeSinceSysStart();
            initialActiveTime = GetTimeSinceSysStart();
            estateActiveTime = GetTimeSinceSysStart();

            estado = 0;

            productivo = 0;
            noProductivo = 0;
            neutral = 0;
            inactivo = 0;
            noCategorizado = 0;
            CambioDeVentana(ultimoProceso);
        }

        public static void RestaurarSoloEstadoLocal()
        {
            lastState = false;
            lastActiveTime = GetTimeSinceSysStart();
            initialActiveTime = GetTimeSinceSysStart();
            estateActiveTime = GetTimeSinceSysStart();

            estado = 0;
            CambioDeVentana(ultimoProceso);
        }

        public static void ActualizarActividad(int ActividadID)
        {
            long productivo = App.productivo;
            long noProductivo = App.noProductivo;
            long neutral = App.neutral;
            long inactivo = App.inactivo;
            long noCategorizado = App.noCategorizado;

            //Console.WriteLine("aaaaaaaaaaaaaaaaaaaaaaaa " + ProcesosPerfilActual.Count);

            if (App.categoria == 0 && App.estado == 1 && enPerfil)
            {
                neutral += GetTimeSinceSysStart() - estateActiveTime;
            }
            if (App.estado == 2 && enPerfil)
            {
                inactivo += GetTimeSinceSysStart() - estateActiveTime;
            }
            if (App.estado == 0 && enPerfil)
            {
                if (App.categoria == 0)
                {
                    productivo += GetTimeSinceSysStart() - initialActiveTime;
                }
                if (App.categoria == 1)
                {
                    neutral += GetTimeSinceSysStart() - initialActiveTime;
                }
                if (App.categoria == 2)
                {
                    noProductivo += GetTimeSinceSysStart() - initialActiveTime;
                }
                if (App.categoria == 3)
                {
                    noCategorizado += GetTimeSinceSysStart() - initialActiveTime;
                }
            }

            string connStr = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
            var repo = new ActividadRepository(connStr);

            var actividad = UsuarioActividad.FirstOrDefault(a => a.ActividadID == ActividadID);
            var allActividades = repo.GetAllActividades();

            if (actividad != null)
            {
                actividad.Productivo = productivo;
                actividad.Inactivo = inactivo;
                actividad.NoProductivo = noProductivo;
                actividad.NoCategorizado = noCategorizado;
                actividad.Neutral = neutral;

                //Console.WriteLine(App.productivo);
                //Console.WriteLine("productivo " + actividad.Productivo);
                repo.UpdateActividad(actividad);
            }
            else
            {
                actividad = repo.GetAllActividades().FirstOrDefault(a => a.ActividadID == ActividadID);

                if (actividad != null)
                {
                    actividad.Productivo = productivo;
                    actividad.Inactivo = inactivo;
                    actividad.NoProductivo = noProductivo;
                    actividad.NoCategorizado = noCategorizado;
                    actividad.Neutral = neutral;

                    repo.UpdateActividad(actividad);
                }
            }
        }

        public static void EnsureTablesExist(string connectionString)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    if (!TableExists(conn, "Perfiles"))
                    {
                        ExecuteNonQuery(conn, @"
                        CREATE TABLE Perfiles (
                            PerfilID INT IDENTITY(1,1) PRIMARY KEY,
                            Nombre NVARCHAR(100) NOT NULL,
                            HoraInicio TIME,
                            HoraFin TIME
                        );
                    ");
                    }

                    if (!TableExists(conn, "Usuarios"))
                    {
                        ExecuteNonQuery(conn, @"
                        CREATE TABLE Usuarios (
                            UsuarioID INT IDENTITY(1,1) PRIMARY KEY,
                            NombreUsuario NVARCHAR(100) NOT NULL,
                            Nombre NVARCHAR(100) NOT NULL,
                            Apellido NVARCHAR(100) NOT NULL,
                            Correo NVARCHAR(100) NOT NULL,
                            Telefono NVARCHAR(100) NOT NULL,
                            Contraseña NVARCHAR(100) NOT NULL,
                            Nivel INT NOT NULL,
                            PerfilID INT,
                            FOREIGN KEY (PerfilID) REFERENCES Perfiles(PerfilID) ON DELETE SET NULL
                        );
                    ");
                    }

                    if (!TableExists(conn, "Actividad"))
                    {
                        ExecuteNonQuery(conn, @"
                        CREATE TABLE Actividad (
                            ActividadID INT IDENTITY(1,1) PRIMARY KEY,
                            Inactivo BIGINT,
                            Productivo BIGINT,
                            NoProductivo BIGINT,
                            NoCategorizado BIGINT,
                            Neutral BIGINT,
                            FechaInicial DATE NOT NULL,
                            UltimaHora DATETIME NOT NULL,
                            HoraInicio DECIMAL(4,2) NOT NULL,
                            HoraFinal DECIMAL(4,2) NOT NULL,
                            UsuarioID INT NOT NULL
                        );
                    ");
                    }

                    if (!TableExists(conn, "Procesos"))
                    {
                        ExecuteNonQuery(conn, @"
                        CREATE TABLE Procesos (
                            ProcesoID INT IDENTITY(1,1) PRIMARY KEY,
                            Nombre NVARCHAR(100) NOT NULL,
                            Categoria INT NOT NULL,
                            Neutral DECIMAL(8,2),
                            Inactivo DECIMAL(8,2),
                            PerfilID INT NOT NULL,
                            FOREIGN KEY (PerfilID) REFERENCES Perfiles(PerfilID) ON DELETE CASCADE
                        );
                    ");
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private static bool TableExists(SqlConnection conn, string tableName)
        {
            using (SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName", conn))
            {
                cmd.Parameters.AddWithValue("@tableName", tableName);
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        private static void ExecuteNonQuery(SqlConnection conn, string sql)
        {
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _notifyIcon.Icon = Icon.ExtractAssociatedIcon(Directory.GetCurrentDirectory() + @"\" + Process.GetCurrentProcess().ProcessName + ".exe");
            _notifyIcon.Text = "Time Recorder";
            _notifyIcon.MouseClick += NotifyIcon_Click;

            _notifyIcon.ContextMenuStrip = new Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Show", null, Show_Click);
            _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, Exit_Click);

            _notifyIcon.Visible = true;
        }

        private void InitializeAppSettingsFile()
        {
            string programPath = Directory.GetCurrentDirectory();
            string configFile = @"\settings.txt";

            var file = programPath + configFile;

            if (!File.Exists(file))
            {
                File.AppendAllText(file,
                    $"50" +
                $"\n");
            }

            string[] lines = File.ReadAllLines(file);
            CustomJoyDeadZone = int.Parse(lines[0]);
        }

        private void ShowWindow()
        {
            MainWindow.WindowState = WindowState.Normal;
            MainWindow.Activate();
        }
        private void NotifyIcon_Click(object sender, Forms.MouseEventArgs e)
        {
            if (e.Button == Forms.MouseButtons.Left)
            {
                ShowWindow();
            }
        }
        private void Show_Click(object sender, EventArgs e)
        {
            ShowWindow();
        }
        private void Exit_Click(object sender, EventArgs e)
        {
            Current.Shutdown();
        }

        private static string GetWindowTitle(IntPtr hWnd)
        {
            int textLength = NativeMethods.GetWindowTextLength(hWnd);
            StringBuilder sb = new StringBuilder(textLength + 1);
            NativeMethods.GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        public static long GetTimeSinceSysStart()
        {
            ulong time = 0;
            QueryUnbiasedInterruptTime(ref time);
            return (long)(time/10000);
        }

        internal static class NativeMethods
        {
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool IsWindowVisible(IntPtr hWnd);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            internal static extern int GetWindowTextLength(IntPtr hWnd);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool EnumThreadWindows(uint dwThreadId, EnumThreadWndProc lpfn, IntPtr lParam);
            internal delegate bool EnumThreadWndProc(IntPtr hWnd, IntPtr lParam);

            [DllImport("user32.dll", SetLastError = true)]
            internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref uint lpdwProcessId);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool IsIconic(IntPtr hWnd);

            [DllImport("User32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GetCursorPos(ref POINT lpPoint);

            [DllImport("User32.dll")]
            internal static extern short GetKeyState(int nVirtKey);

            [DllImport("user32.dll")]
            internal static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll")]
            internal static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

            [DllImport("user32.dll")]
            internal static extern IntPtr GetLastActivePopup(IntPtr hWnd);

            [DllImport("user32.dll")]
            internal static extern uint GetClassLong(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll")]
            internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

            internal delegate void WinEventProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime);
            [DllImport("user32.dll", SetLastError = true)]
            internal static extern IntPtr SetWinEventHook(int eventMin, int eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, int idProcess, int idThread, SetWinEventHookFlags dwflags);
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool UnhookWinEvent(IntPtr hWinEventHook);
            internal enum SetWinEventHookFlags
            {
                WINEVENT_INCONTEXT = 4,
                WINEVENT_OUTOFCONTEXT = 0,
                WINEVENT_SKIPOWNPROCESS = 2,
                WINEVENT_SKIPOWNTHREAD = 1
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int x;
                public int y;
            }

            [DllImport("Winmm.dll")]
            internal static extern uint joyGetNumDevs();

            [DllImport("Winmm.dll")]
            internal static extern uint joyGetPosEx(uint uJoyID, ref LPJOYINFOEX pji);

            [StructLayout(LayoutKind.Sequential)]
            public struct LPJOYINFOEX
            {
                public uint dwSize;
                public uint dwFlags;
                public uint dwXpos;
                public uint dwYpos;
                public uint dwZpos;
                public uint dwRpos;
                public uint dwUpos;
                public uint dwVpos;
                public uint dwButtons;
                public uint dwButtonNumber;
                public uint dwPOV;
                public uint dwReserved1;
                public uint dwReserved2;
            }

            [DllImport("Winmm.dll")]
            internal static extern uint joyGetThreshold(uint uJoyID, ref int joyGetThreshold);

            [DllImport("Winmm.dll", CharSet = CharSet.Unicode)]
            internal static extern uint joyGetDevCaps(uint uJoyID, ref LPJOYCAPS pjc, uint cbjc);

            [StructLayout(LayoutKind.Sequential)]
            public struct LPJOYCAPS
            {
                public ushort wMid;
                public ushort wPid;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32 * 2)]
                public char[] szPname;
                public uint wXmin;
                public uint wXmax;
                public uint wYmin;
                public uint wYmax;
                public uint wZmin;
                public uint wZmax;
                public uint wNumButtons;
                public uint wPeriodMin;
                public uint wPeriodMax;
                public uint wRmin;
                public uint wRmax;
                public uint wUmin;
                public uint wUmax;
                public uint wVmin;
                public uint wVmax;
                public uint wCaps;
                public uint wMaxAxes;
                public uint wNumAxes;
                public uint wMaxButtons;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32 * 2)]
                public char[] szRegKey;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260 * 2)]
                public char[] szOEMVxD;
            }

            [DllImport("Kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool QueryUnbiasedInterruptTime(ref ulong UnbiasedTime);
        }

        static string ultimoProceso = "";

        IntPtr FocusPEventHook = SetWinEventHook(0x0003, 0x0003, IntPtr.Zero, (hWinEventHook, iEvent, hWnd, idObject, idChild, dwEventThread, dwmsEventTime) =>
        {
            uint ProcessId = 0;
            GetWindowThreadProcessId(hWnd, ref ProcessId);

            Process proc;
            try { proc = Process.GetProcessById((int)ProcessId); } catch { return; }

            //RefreshAllFocus(proc.ProcessName + ".exe", (int)hWnd);
            //Console.WriteLine("***Window Foreground*** " + hWnd + " " + proc.ProcessName);

            ultimoProceso = proc.ProcessName;
            CambioDeVentana(proc.ProcessName);

            //long elapsedTicks = currentTick - rlist[i].TimeTick;

        }, 0, 0, SetWinEventHookFlags.WINEVENT_OUTOFCONTEXT);

        public static void CambioDeVentana(string ProcessName)
        {
            foreach (Proceso process in ProcesosPerfilActual)
            {
                if (ProcessName == process.Nombre)
                {
                    //Console.WriteLine(ProcessName + " OOOO " + process.Categoria);

                    if (process.Categoria != categoria)
                    {
                        if (App.estado == 2 && enPerfil)
                        {
                            App.inactivo += GetTimeSinceSysStart() - estateActiveTime;
                            estateActiveTime = GetTimeSinceSysStart();
                        }
                        if (App.categoria == 0 && App.estado == 1 && enPerfil)
                        {
                            App.productivo += GetTimeSinceSysStart() - initialActiveTime;
                            App.neutral += GetTimeSinceSysStart() - estateActiveTime;
                            App.estado = 2;
                            estateActiveTime = GetTimeSinceSysStart();
                        }
                        if (App.estado == 0 && enPerfil)
                        {
                            if (App.categoria == 0)
                            {
                                App.productivo += GetTimeSinceSysStart() - initialActiveTime;
                            }
                            if (App.categoria == 1)
                            {
                                App.neutral += GetTimeSinceSysStart() - initialActiveTime;
                            }
                            if (App.categoria == 2)
                            {
                                App.noProductivo += GetTimeSinceSysStart() - initialActiveTime;
                            }
                            if (App.categoria == 3)
                            {
                                App.noCategorizado += GetTimeSinceSysStart() - initialActiveTime;
                            }
                        }
                        initialActiveTime = GetTimeSinceSysStart();

                        categoria = process.Categoria;
                        tiempoNeutral = (double)process.Neutral;
                        tiempoInactivo = (double)process.Inactivo;
                    }
                    return;
                }
            }
            categoria = 3;
            tiempoNeutral = 2.5;
            tiempoInactivo = 5.0;
        }

        static bool IsAltTabWindow(IntPtr hwnd)
        {
            // Start at the root owner
            IntPtr hwndWalk = GetAncestor(hwnd, 3);
            // See if we are the last active visible popup
            IntPtr hwndTry;
            while ((hwndTry = GetLastActivePopup(hwndWalk)) != hwndTry)
            {
                if (IsWindowVisible(hwndTry)) break;
                hwndWalk = hwndTry;
            }

            return hwndWalk == hwnd;
        }

        //void processStartEvent_EventArrived(object sender, EventArrivedEventArgs e)
        //{
        //var targetInstance = (ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value;
        //int pid = Convert.ToInt32(targetInstance.Properties["ProcessID"].Value);
        //AddRunningProcess(pid,IntPtr.Zero);
        //}

        public static void RemoveProcessWnd(string name, int wnd)
        {
            var rlist = RunningProcesses.RunningProcessesList;

            for (int i = 0; i < rlist.Count; i++)
            {
                if (rlist[i].PName.Equals(name) && rlist[i].hWnds != null && rlist[i].hWnds.Contains(wnd))
                {
                    Process proc;
                    uint ProcessID = 0;
                    GetWindowThreadProcessId((IntPtr)wnd, ref ProcessID);
                    try { proc = Process.GetProcessById((int)ProcessID); } catch { return; }

                    rlist[i].hWnds.Remove(wnd);
                    //Console.WriteLine("***Removed Window*** "+ wnd);

                    if (rlist[i].hWnds.Count == 0)
                    {
                        rlist.RemoveAt(i);
                    }
                }
            }

            if (rlist.Count == 0)
            {
                saveListFile(false);
                rtimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        public static void processStopEvent_EventArrived(int pid, string Name)
        {
            var rlist = RunningProcesses.RunningProcessesList;
            //var targetInstance = (ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value;
            //int pid = Convert.ToInt32(targetInstance.Properties["ProcessID"].Value);
            //string Name = (string)targetInstance.Properties["Name"].Value;

            for (int i = rlist.Count - 1; i > -1; i--)
            {
                if (rlist[i].PName.Equals(Name))
                {
                    if (rlist[i].WndName == null)
                    {
                        rlist[i].IDs.Remove(pid);

                        if (rlist[i].IDs.Count == 0)
                        {
                            //long SaveTime = Environment.TickCount-rlist[i].LastInputTick;

                            //if (SaveTime > rlist[i].InputSaveT)
                            //{
                            //SaveTime = rlist[i].InputSaveT;
                            //}
                            //CheckRemoveInputOnProcess(i, 0, SaveTime); //We stop waiting for InputWait, we just save.

                            //Console.WriteLine("Removed ProcessF: " + i);
                            rlist.RemoveAt(i);
                        }
                    }
                    else
                    {
                        foreach (PHook hook in PHooks)
                        {
                            if (hook.Id == pid)
                            {
                                for (int i2 = rlist[i].hWnds.Count - 1; i2 > -1; i2--)
                                {
                                    foreach (int hwndId in hook.hWnds)
                                    {
                                        if (hwndId == rlist[i].hWnds[i2])
                                        {
                                            //Console.WriteLine("Removed: " + hwndId);
                                            rlist[i].hWnds.RemoveAt(i2);

                                            if (rlist[i].hWnds.Count == 0)
                                            {
                                                //long SaveTime = Environment.TickCount-rlist[i].LastInputTick;

                                                //if (SaveTime > rlist[i].InputSaveT)
                                                //{
                                                //SaveTime = rlist[i].InputSaveT;
                                                //}
                                                //CheckRemoveInputOnProcess(i, 0, SaveTime); //We stop waiting for InputWait, we just save.

                                                rlist.RemoveAt(i);
                                                //Console.WriteLine("Removed Process: " + i);
                                            }
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }

            if (rlist.Count == 0)
            {
                saveListFile(false);
                rtimer.Change(Timeout.Infinite, Timeout.Infinite);
            }

            for (int i2 = PHooks.Count - 1; i2 > -1; i2--)
            {
                PHook hook = PHooks[i2];

                if (hook.Id == pid)
                {
                    foreach (IntPtr actualhook in hook.Hooks)
                    {
                        UnhookWinEvent(actualhook);
                    }
                    PHooks.RemoveAt(i2);
                    //Console.WriteLine("Removed Process Hooks: " + i2);

                    break;
                }
            }
        }

        //static public AutoResetEvent autoEvent = new AutoResetEvent(false);
        static public Timer rtimer = new Timer(rtimer_Tick, null, Timeout.Infinite, Timeout.Infinite);

        static string programPath = Directory.GetCurrentDirectory();
        static string dataFolder = @"\data";
        static string listFile = @"\processlist.csv";

        static string file = programPath + dataFolder + listFile;
        //static string tempfile = programPath + dataFolder + @"\processlist.tmp";

        private const int NumOfRetries = 3;
        private const int DelayOnRetry = 10;

        static private void rtimer_Tick(object stateInfo)
        {
            saveListFile(true);
            rtimer.Change(5000, Timeout.Infinite);
        }

        static private void saveListFile(bool doBackup)
        {
            string[] fileLines = new string[] { };

            long currentTick = GetTimeSinceSysStart();
            string currentDate = DateTime.Now.ToString();

            foreach(SystemProcessesTracker.SystemProcessInfo process in SystemProcessesTracker.TrackingProcessesInfo)
            {
                
            }

            for (int i = 0; i <= NumOfRetries; ++i) // try catch statements had to be used on this function because of erros while opening or exiting games.
            {
                try
                {
                    fileLines = File.ReadAllLines(file);
                    break;
                }
                catch (IOException) when (i <= NumOfRetries)
                {
                    if (i == NumOfRetries)
                    {
                        return;
                    }
                    Thread.Sleep(DelayOnRetry);
                }
            }

            var rlist = RunningProcesses.RunningProcessesList;

            for (int i = 0; i < rlist.Count; i++)
            {
                foreach (var line in fileLines)
                {
                    //string dir = line.Split(',').ElementAt(6);
                    string PName = line.Split(',').ElementAt(4);

                    if (PName.Equals(rlist[i].PName))
                    {
                        string RecordWnd = line.Split(',').ElementAt(1);
                        string WndName = line.Split(',').ElementAt(5);

                        if (RecordWnd == "0")
                        {
                            if (rlist[i].WndName != null)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (rlist[i].WndName != WndName)
                            {
                                continue;
                            }
                        }

                        int pid = Array.FindIndex(fileLines, m => m == line);

                        string realhours = line.Split(',').ElementAt(8);
                        string realminh = line.Split(',').ElementAt(9);
                        string realfocush = line.Split(',').ElementAt(10);

                        string realinputh = line.Split(',').ElementAt(11);
                        string realinputkeyh = line.Split(',').ElementAt(12);
                        string realinputmouseh = line.Split(',').ElementAt(13);
                        string realinputkmh = line.Split(',').ElementAt(14);
                        string realinputjoyh = line.Split(',').ElementAt(15);

                        string realfirst = line.Split(',').ElementAt(18);
                        string reallast = line.Split(',').ElementAt(19);

                        long elapsedTicks = currentTick - rlist[i].TimeTick;
                        long newHours = rlist[i].AddHours + elapsedTicks;

                        if (line.Split(',').ElementAt(7) == "waitWnd")
                        {
                            IntPtr hWnd = IntPtr.Zero;
                            string pngName;

                            if (RecordWnd == "0")
                            {

                            }
                            else
                            {

                            }

                            IntPtr hIcon;
                            uint WM_GETICON = 0x007f;
                            IntPtr ICON_SMALL2 = new IntPtr(2);

                            hIcon = SendMessage(hWnd, WM_GETICON, ICON_SMALL2, IntPtr.Zero);

                            if (hIcon == IntPtr.Zero)
                            {
                                hIcon = (IntPtr)GetClassLong(hWnd, -14);
                            }

                            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                                hIcon,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions());

                            bitmapSource.Freeze();

                            try //Had an issue with this, not anymore but i left it just to make sure it is impossible it crashes.
                            {

                            }
                            catch
                            { 
                                break; 
                            }

                            if (MonitorActividad.MainWindow.IsOpen)
                            {
                                
                            }
                            

                            
                        }

                        
                        break;
                    }
                }
            }

            try
            {
                byte[] fileBytes = File.ReadAllBytes(file);
                bool nullChar = false; 

                for (int i = 0; i < fileBytes.Length; i++)
                {
                    if (fileBytes[i] == 0)
                    {
                        nullChar = true;
                        //Console.WriteLine("1");
                        break;
                    }
                }

                if (doBackup && !nullChar)
                {
                    //Console.WriteLine("2");
                    // Create a FileInfo  
                    System.IO.FileInfo fi = new System.IO.FileInfo(file);
                    // Check if file is there  
                    if (fi.Exists)
                    {
                        // Move file with a new name. Hence renamed.
                        File.Delete(file + ".bak");
                        fi.MoveTo(  file + ".bak");
                        //Console.WriteLine("3");
                    }
                    doBackup = false;
                }
                //Console.WriteLine("E");
                System.IO.FileInfo fi2 = new System.IO.FileInfo(file);
                if (!fi2.Exists)
                {
                    using (File.Create(file)){}
                    //Console.WriteLine("4");
                }
                else
                {
                    FileStream fs = File.Open(file, FileMode.Open);
                    fs.SetLength(0);
                    fs.Close();
                    //Console.WriteLine("5");
                }

                //for (int i = 0; i < fileLines.Length; i++)
                //{
                    string str = string.Join("\n", fileLines);
                    str = str + "\n";
                    
                    byte[] data = new UTF8Encoding(true).GetBytes(str);
                    using (FileStream fs = new FileStream(file, FileMode.Append, FileAccess.Write,
                                                    FileShare.Read, data.Length, FileOptions.WriteThrough))
                    {
                        fs.Write(data, 0, data.Length);
                        //Console.WriteLine("6");
                    }
                //}
            }
            catch (IOException)
            {
                return;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            _notifyIcon?.Dispose();
            rtimer?.Dispose();
            UnhookWinEvent(FocusPEventHook);

            if (RunningProcesses.RunningProcessesList.Count > 0)
            {
                saveListFile(false);
            }
        }
    }

    public class RunningProcesses
    {
        public static List<RunningProcess> RunningProcessesList { get; set; } = GetRunningProcessList();
        public static List<RunningProcess> GetRunningProcessList()
        {
            var list = new List<RunningProcess>();
            return list;
        }
    }

    public class RunningProcess
    {
        public int Index { get; set; }
        public List<int> IDs { get; set; }
        public List<int> hWnds { get; set; }
        public string Dir { get; set; }
        public string PName { get; set; }
        public string WndName { get; set; }
        public long AddHours { get; set; }
        public long TimeTick { get; set; }
        public bool IsMin { get; set; }
        public long AddMinH { get; set; }
        public long MinTick { get; set; }
        public bool IsFocus { get; set; }
        public long AddFocusH { get; set; }
        public long FocusTick { get; set; }
        public long InputWaitT { get; set; }
        public long InputSaveT { get; set; }
        //Any
        public long AddInputH { get; set; }
        public long LastInputTick { get; set; }
        public long InputTick { get; set; }
        //Keyboard&Mouse
        public long AddInputKMH { get; set; }
        public long LastInputKMTick { get; set; }
        public long InputKMTick { get; set; }
        //Mouse
        public long AddInputMouseH { get; set; }
        public long LastInputMouseTick { get; set; }
        public long InputMouseTick { get; set; }
        public bool IsInputMouse { get; set; }
        //Keyboard
        public long AddInputKeyH { get; set; }
        public long LastInputKeyTick { get; set; }
        public long InputKeyTick { get; set; }
        public bool IsInputKey { get; set; }
        //Controller
        public long AddInputJoyH { get; set; }
        public long LastInputJoyTick { get; set; }
        public long InputJoyTick { get; set; }
        public bool IsInputJoy { get; set; }
    }

    internal class PHook
    {
        public int Id { get; set; }
        public List<IntPtr> Hooks { get; set; }
        public List<int> hWnds { get; set; }
    }
}
