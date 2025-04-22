using MonitorActividad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

public class UsuarioRepository
{
    private readonly string _connectionString;

    public UsuarioRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<Usuario> GetAllUsuarios()
    {
        var usuarios = new List<Usuario>();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            string query = "SELECT * FROM Usuarios";
            SqlCommand cmd = new SqlCommand(query, conn);

            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                usuarios.Add(new Usuario
                {
                    UsuarioID = (int)reader["UsuarioID"],
                    NombreUsuario = reader["NombreUsuario"].ToString(),
                    Nombre = reader["Nombre"].ToString(),
                    Apellido = reader["Apellido"].ToString(),
                    Correo = reader["Correo"].ToString(),
                    Telefono = reader["Telefono"].ToString(),
                    Contraseña = reader["Contraseña"].ToString(),
                    Nivel = (int)reader["Nivel"],
                    PerfilID = reader["PerfilID"] != DBNull.Value ? (int)reader["PerfilID"] : (int?)null
                });
            }
        }

        return usuarios;
    }

    public void AddUsuario(Usuario usuario)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            string query = @"INSERT INTO Usuarios 
                        (NombreUsuario, Nombre, Apellido, Correo, Telefono, Contraseña, Nivel, PerfilID) 
                         VALUES 
                        (@NombreUsuario, @Nombre, @Apellido, @Correo, @Telefono, @Contraseña, @Nivel, @PerfilID)";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@NombreUsuario", usuario.NombreUsuario);
            cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
            cmd.Parameters.AddWithValue("@Apellido", usuario.Apellido);
            cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
            cmd.Parameters.AddWithValue("@Telefono", usuario.Telefono);
            cmd.Parameters.AddWithValue("@Contraseña", usuario.Contraseña);
            cmd.Parameters.AddWithValue("@Nivel", usuario.Nivel);
            cmd.Parameters.AddWithValue("@PerfilID", usuario.PerfilID ?? (object)DBNull.Value);
            //Console.WriteLine("AAAAAAA"+usuario.PerfilID);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
        if (MainWindow.WndObject != null)
        {
            MainWindow.noChange = true;
            MainWindow.WndObject.ActualizarSeleccion(MainWindow.RefrescarUsuarios());
        }
    }

    public void UpdateUsuario(Usuario usuario)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            string query = @"UPDATE Usuarios SET 
                         NombreUsuario = @NombreUsuario, 
                         Nombre = @Nombre,
                         Apellido = @Apellido,
                         Correo = @Correo,
                         Telefono = @Telefono,
                         Contraseña = @Contraseña,
                         Nivel = @Nivel,
                         PerfilID = @PerfilID
                         WHERE UsuarioID = @UsuarioID";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UsuarioID", usuario.UsuarioID);
            cmd.Parameters.AddWithValue("@NombreUsuario", usuario.NombreUsuario);
            cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
            cmd.Parameters.AddWithValue("@Apellido", usuario.Apellido);
            cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
            cmd.Parameters.AddWithValue("@Telefono", usuario.Telefono);
            cmd.Parameters.AddWithValue("@Contraseña", usuario.Contraseña);
            cmd.Parameters.AddWithValue("@Nivel", usuario.Nivel);
            cmd.Parameters.AddWithValue("@PerfilID", usuario.PerfilID ?? (object)DBNull.Value);

            conn.Open();
            cmd.ExecuteNonQuery();
        }
        if (MainWindow.WndObject != null)
        {
            MainWindow.noChange = true;
            App.CargarProcesosPerfil();
            MainWindow.WndObject.ActualizarSeleccion(MainWindow.RefrescarUsuarios());
        }
    }

    public void DeleteUsuario(int usuarioID)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            string query = "DELETE FROM Usuarios WHERE UsuarioID = @UsuarioID";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UsuarioID", usuarioID);

            conn.Open();
            cmd.ExecuteNonQuery();
        }
        if (MainWindow.WndObject != null)
        {
            MainWindow.noChange = true;
            MainWindow.WndObject.ActualizarSeleccion(MainWindow.RefrescarUsuarios());
        }
    }
}