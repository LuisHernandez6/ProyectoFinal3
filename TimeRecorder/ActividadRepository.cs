using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

public class Actividad
{
    public int ActividadID { get; set; }
    public long Inactivo { get; set; }
    public long Productivo { get; set; }
    public long NoProductivo { get; set; }
    public long NoCategorizado { get; set; }
    public long Neutral { get; set; }
    public DateTime FechaInicial { get; set; }
    public DateTime UltimaHora { get; set; }
    public decimal HoraInicio { get; set; }
    public decimal HoraFinal { get; set; }
    public int UsuarioID { get; set; }
}

public class ActividadView
{
    public int ActividadID { get; set; }
    public double Inactivo { get; set; }
    public double Productivo { get; set; }
    public double NoProductivo { get; set; }
    public double NoCategorizado { get; set; }
    public double Neutral { get; set; }
    public DateTime FechaInicial { get; set; }
    public DateTime UltimaHora { get; set; }
    public decimal HoraInicio { get; set; }
    public decimal HoraFinal { get; set; }
    public int UsuarioID { get; set; }
}

public class ActividadRepository
{
    private readonly string _connectionString;

    public ActividadRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<Actividad> GetAllActividades()
    {
        var actividades = new List<Actividad>();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            string query = "SELECT * FROM Actividad";
            SqlCommand cmd = new SqlCommand(query, conn);

            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                actividades.Add(new Actividad
                {
                    ActividadID = (int)reader["ActividadID"],
                    Inactivo = (long)reader["Inactivo"],
                    Productivo = (long)reader["Productivo"],
                    NoProductivo = (long)reader["NoProductivo"],
                    NoCategorizado = (long)reader["NoCategorizado"],
                    Neutral = (long)reader["Neutral"],
                    FechaInicial = (DateTime)reader["FechaInicial"],
                    UltimaHora = (DateTime)reader["UltimaHora"],
                    HoraInicio = (decimal)reader["HoraInicio"],
                    HoraFinal = (decimal)reader["HoraFinal"],
                    UsuarioID = (int)reader["UsuarioID"]
                });
            }
        }

        return actividades;
    }

    public void AddActividad(Actividad actividad)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            string query = @"INSERT INTO Actividad 
                        (Inactivo, Productivo, NoProductivo, NoCategorizado, Neutral, FechaInicial, UltimaHora, HoraInicio, HoraFinal, UsuarioID) 
                         VALUES 
                        (@Inactivo, @Productivo, @NoProductivo, @NoCategorizado, @Neutral, @FechaInicial, @UltimaHora, @HoraInicio, @HoraFinal, @UsuarioID)";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Inactivo", actividad.Inactivo);
            cmd.Parameters.AddWithValue("@Productivo", actividad.Productivo);
            cmd.Parameters.AddWithValue("@NoProductivo", actividad.NoProductivo);
            cmd.Parameters.AddWithValue("@NoCategorizado", actividad.NoCategorizado);
            cmd.Parameters.AddWithValue("@Neutral", actividad.Neutral);
            cmd.Parameters.AddWithValue("@FechaInicial", actividad.FechaInicial);
            cmd.Parameters.AddWithValue("@UltimaHora", actividad.UltimaHora);
            cmd.Parameters.AddWithValue("@HoraInicio", actividad.HoraInicio);
            cmd.Parameters.AddWithValue("@HoraFinal", actividad.HoraFinal);
            cmd.Parameters.AddWithValue("@UsuarioID", actividad.UsuarioID);

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public void UpdateActividad(Actividad actividad)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            string query = @"UPDATE Actividad SET 
                         Inactivo = @Inactivo, 
                         Productivo = @Productivo,
                         NoProductivo = @NoProductivo,
                         NoCategorizado = @NoCategorizado,
                         Neutral = @Neutral,
                         FechaInicial = @FechaInicial,
                         UltimaHora = @UltimaHora,
                         HoraInicio = @HoraInicio,
                         HoraFinal = @HoraFinal,
                         UsuarioID = @UsuarioID
                         WHERE ActividadID = @ActividadID";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ActividadID", actividad.ActividadID);
            cmd.Parameters.AddWithValue("@Inactivo", actividad.Inactivo);
            cmd.Parameters.AddWithValue("@Productivo", actividad.Productivo);
            cmd.Parameters.AddWithValue("@NoProductivo", actividad.NoProductivo);
            cmd.Parameters.AddWithValue("@NoCategorizado", actividad.NoCategorizado);
            cmd.Parameters.AddWithValue("@Neutral", actividad.Neutral);
            cmd.Parameters.AddWithValue("@FechaInicial", actividad.FechaInicial);
            cmd.Parameters.AddWithValue("@UltimaHora", actividad.UltimaHora);
            cmd.Parameters.AddWithValue("@HoraInicio", actividad.HoraInicio);
            cmd.Parameters.AddWithValue("@HoraFinal", actividad.HoraFinal);
            cmd.Parameters.AddWithValue("@UsuarioID", actividad.UsuarioID);

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public void DeleteActividad(int actividadID)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            string query = "DELETE FROM Actividad WHERE ActividadID = @ActividadID";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ActividadID", actividadID);

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
