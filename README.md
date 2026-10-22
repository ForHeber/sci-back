# Sci-backend
Sistema Control de Incidentes

# SCRIPTs HABITUALES
# Ejecución (desde la carpeta del proyecto principal [Sci.Web])
```
dotnet run
```
# Compilar/Limpiar
```
dotnet build
dotnet clean
```
# Publicación (Distribución para Linux x64)
```
dotnet publish -r linux-x64 -o ../Deploy
```

# AYUDA .NET 6
# crea una nueva solución vacia
```
dotnet new sln -n "Sci"
```

# crea un Sci web
```
dotnet new webapi -o Sci.Web
dotnet new webapi -n "Sci.Web"
```
# crea un Sci de tipo librería
```
dotnet new classlib -o Sci.Entidades
dotnet new classlib -o Sci.Datos
```
# agrega un Sci a la solución
```
dotnet sln Sci.sln add ./Sci.Web/Sci.Web.csproj
dotnet sln Sci.sln add ./Sci.Entidades/Sci.Entidades.csproj
dotnet sln Sci.sln add ./Sci.datos/Sci.Datos.csproj
```
# Añade una referencia de datos a web
# Primero referencias la bilbioteca de entidades a la biblioteca de datos
```
dotnet add ./Sci.Datos/Sci.Datos.csproj reference Sci.Entidades/Sci.Entidades.csproj
```
# Segundo referencias las dos bibliotecas al proyecto web
```
dotnet add ./Sci.Web/Sci.Web.csproj reference Sci.Datos/Proyecto.Datos.csproj
dotnet add ./Sci.Web/Sci.Web.csproj reference Sci.Entidades/Sci.Entidades.csproj
```
