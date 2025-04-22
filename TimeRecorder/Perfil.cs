using System;

namespace MonitorActividad
{
    public class Perfil
    {
        public int PerfilID { get; set; }
        public string Nombre { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }

        public Perfil() { }

        public Perfil(int perfilID, string nombre, TimeSpan horaInicio, TimeSpan horaFin)
        {
            PerfilID = perfilID;
            Nombre = nombre;
            HoraInicio = horaInicio;
            HoraFin = horaFin;
        }
    }
}