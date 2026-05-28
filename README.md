# SIB Manager API

Backend del sistema de gestión de empleados desarrollado en ASP.NET Core 8 con Entity Framework y JWT.

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) o SQL Server Express
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o VS Code

## Configuración de la base de datos

1. Abre SQL Server Management Studio (SSMS)
2. Ejecuta el script `database.sql` incluido en el repositorio
3. Esto creará la base de datos `GestionEmpleados` con todas las tablas y datos iniciales

## Configuración del proyecto

1. Clona el repositorio:
```bash
git clone https://github.com/josemirandaa01/SIBManagerAPI.git
cd SIBManagerAPI
```

2. Crea el archivo `appsettings.Development.json` en la carpeta `SIBManagerAPI/` basándote en `appsettings.example.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=GestionEmpleados;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "TU_CLAVE_SECRETA_MINIMO_32_CARACTERES",
    "Issuer": "SIBManagerAPI",
    "Audience": "SIBManagerCliente",
    "ExpiresInHours": 8
  }
}
```

3. Restaura los paquetes NuGet:
```bash
dotnet restore
```

## Correr el proyecto

```bash
dotnet run --project SIBManagerAPI
```

O desde Visual Studio presiona **F5**.

La API estará disponible en `https://localhost:7236`.

## Documentación

La documentación de los endpoints está disponible en Swagger:
```
https://localhost:7236/swagger
```

## Usuario por defecto

Después de ejecutar el script de base de datos puedes crear un usuario Admin desde Swagger:

```
POST /api/auth/registro
{
  "nombreUsuario": "admin",
  "email": "admin@empresa.com",
  "password": "Admin123!",
  "rolId": 1
}
```

## Roles disponibles

| RolId | Nombre | Permisos |
|---|---|---|
| 1 | Admin | Acceso total |
| 2 | RRHH | Solo lectura |
| 3 | Consulta | Solo lectura |

## Tecnologías

- ASP.NET Core 8
- Entity Framework Core
- JWT Bearer Authentication
- BCrypt.Net
- SQL Server
