CREATE DATABASE bd_20231804_proyectofinal;
GO
USE bd_20231804_proyectofinal;
GO

CREATE TABLE Perfiles (
    PerfilID INT IDENTITY(1,1) PRIMARY KEY,
	Nombre NVARCHAR(100) NOT NULL,
	HoraInicio TIME,
	HoraFin TIME,
);

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

CREATE TABLE Procesos (
    ProcesoID INT IDENTITY(1,1) PRIMARY KEY,
	Nombre NVARCHAR(100) NOT NULL,
	Categoria INT NOT NULL,
	Neutral DECIMAL(8,2),
	Inactivo DECIMAL(8,2),
	PerfilID INT NOT NULL,
	FOREIGN KEY (PerfilID) REFERENCES Perfiles(PerfilID) ON DELETE CASCADE
);

UPDATE Usuarios SET Nivel = 1 WHERE UsuarioID = 1;
UPDATE Procesos SET Categoria = 1 WHERE ProcesoID = 6;

SELECT * FROM Usuarios;
SELECT * FROM Perfiles;
SELECT * FROM Procesos;
SELECT * FROM Actividad;

DROP TABLE Perfiles;
DROP TABLE Usuarios;
DROP TABLE Actividad;
DROP TABLE Procesos;