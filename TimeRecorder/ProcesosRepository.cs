using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Media.Imaging;

public class Proceso
{
    public int ProcesoID { get; set; }
    public string Nombre { get; set; }
    public int Categoria { get; set; }
    public decimal Neutral { get; set; }
    public decimal Inactivo { get; set; }
    public int PerfilID { get; set; }
    public string Name { get; set; }
    public BitmapSource Icon { get; set; }

    public Proceso() { }

    public Proceso(int procesoID, string nombre, int categoria, decimal neutral, decimal inactivo, int perfilID)
    {
        ProcesoID = procesoID;
        Nombre = nombre;
        Categoria = categoria;
        Neutral = neutral;
        Inactivo = inactivo;
        PerfilID = perfilID;
    }
}

public class ProcesosRepository
{
    private readonly string _connectionString;

    public ProcesosRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<Proceso> GetAllProcesos()
    {
        var procesos = new List<Proceso>();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            string query = "SELECT * FROM Procesos";
            SqlCommand cmd = new SqlCommand(query, conn);

            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                procesos.Add(new Proceso
                {
                    ProcesoID = (int)reader["ProcesoID"],
                    Nombre = reader["Nombre"].ToString(),
                    Categoria = (int)reader["Categoria"],
                    Neutral = (decimal)reader["Neutral"],
                    Inactivo = (decimal)reader["Inactivo"],
                    PerfilID = (int)reader["PerfilID"]
                });
            }
        }

        return procesos;
    }

    public void AddProceso(Proceso proceso)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            string query = @"INSERT INTO Procesos 
                            (Nombre, Categoria, Neutral, Inactivo, PerfilID) 
                            VALUES 
                            (@Nombre, @Categoria, @Neutral, @Inactivo, @PerfilID)";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Nombre", proceso.Nombre);
            cmd.Parameters.AddWithValue("@Categoria", proceso.Categoria);
            cmd.Parameters.AddWithValue("@Neutral", proceso.Neutral);
            cmd.Parameters.AddWithValue("@Inactivo", proceso.Inactivo);
            cmd.Parameters.AddWithValue("@PerfilID", proceso.PerfilID);

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public void UpdateProceso(Proceso proceso)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            string query = @"UPDATE Procesos SET 
                             Nombre = @Nombre, 
                             Categoria = @Categoria,
                             Neutral = @Neutral,
                             Inactivo = @Inactivo,
                             PerfilID = @PerfilID
                             WHERE ProcesoID = @ProcesoID";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ProcesoID", proceso.ProcesoID);
            cmd.Parameters.AddWithValue("@Nombre", proceso.Nombre);
            cmd.Parameters.AddWithValue("@Categoria", proceso.Categoria);
            cmd.Parameters.AddWithValue("@Neutral", proceso.Neutral);
            cmd.Parameters.AddWithValue("@Inactivo", proceso.Inactivo);
            cmd.Parameters.AddWithValue("@PerfilID", proceso.PerfilID);

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public void DeleteProceso(int procesoID)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            string query = "DELETE FROM Procesos WHERE ProcesoID = @ProcesoID";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ProcesoID", procesoID);

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
