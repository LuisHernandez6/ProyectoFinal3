using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace MonitorActividad
{
    public class PerfilRepository
    {
        private readonly string connectionString;

        public PerfilRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<Perfil> GetAllPerfiles()
        {
            List<Perfil> perfiles = new List<Perfil>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT PerfilID, Nombre, HoraInicio, HoraFin FROM Perfiles";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Perfil perfil = new Perfil
                            {
                                PerfilID = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                HoraInicio = reader.GetTimeSpan(2),
                                HoraFin = reader.GetTimeSpan(3)
                            };
                            perfiles.Add(perfil);
                        }
                    }
                }
            }

            return perfiles;
        }

        // Create a new profile
        public void CreateProfile(Perfil perfil)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Perfiles (Nombre, HoraInicio, HoraFin) VALUES (@Nombre, @HoraInicio, @HoraFin)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Nombre", perfil.Nombre);
                    cmd.Parameters.AddWithValue("@HoraInicio", perfil.HoraInicio);
                    cmd.Parameters.AddWithValue("@HoraFin", perfil.HoraFin);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Delete a profile by ID
        public void DeleteProfile(int perfilID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM Perfiles WHERE PerfilID = @PerfilID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PerfilID", perfilID);
                    cmd.ExecuteNonQuery();
                }
            }
            if (MainWindow.WndObject != null)
            {
                MainWindow.noChange = true;
                MainWindow.WndObject.ActualizarSeleccion(MainWindow.RefrescarUsuarios());
            }
        }

        // Copy an existing profile
        public void CopyProfile(int perfilID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Perfiles (Nombre, HoraInicio, HoraFin) SELECT Nombre, HoraInicio, HoraFin FROM Perfiles WHERE PerfilID = @PerfilID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PerfilID", perfilID);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void AddProfile(Perfil perfil)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Perfiles (Nombre, HoraInicio, HoraFin) VALUES (@Nombre, @HoraInicio, @HoraFin)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Nombre", perfil.Nombre);
                    cmd.Parameters.AddWithValue("@HoraInicio", perfil.HoraInicio);
                    cmd.Parameters.AddWithValue("@HoraFin", perfil.HoraFin);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateProfile(Perfil perfil)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"UPDATE Perfiles 
                         SET Nombre = @Nombre, 
                             HoraInicio = @HoraInicio, 
                             HoraFin = @HoraFin 
                         WHERE PerfilID = @PerfilID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PerfilID", perfil.PerfilID);
                    cmd.Parameters.AddWithValue("@Nombre", perfil.Nombre);
                    cmd.Parameters.AddWithValue("@HoraInicio", perfil.HoraInicio);
                    cmd.Parameters.AddWithValue("@HoraFin", perfil.HoraFin);
                    cmd.ExecuteNonQuery();
                }
            }
            App.CargarProcesosPerfil();
        }
    }
 }