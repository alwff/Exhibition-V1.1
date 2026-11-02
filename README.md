Alejandra María Cardona Cabrera 

Universidad del Valle de Guatemala

2026

# Exhibition-V1.1

Proyecto de Unity desarrollado como parte del trabajo de graduación **“Diseño e implementación de una exhibición virtual para colecciones biológicas utilizando códigos de barras y entornos inmersivos”**.

## Descripción

En este proyecto se encuentra el espacio virtual de exhibición, junto con las escenas, modelos, materiales y scripts empleados para su funcionamiento.

El entorno permite recorrer el espacio y consultar los ejemplares exhibidos, incluyendo su visualización mediante secuencias de imágenes de 360°. También cuenta con un panel de administración para gestionar el contenido de la exhibición.

La comunicación con la aplicación web se realiza mediante una API desarrollada en Flask. De esta manera, Unity consulta la información de los ejemplares sin conectarse directamente a la base de datos PostgreSQL.

## Organización del proyecto

Las carpetas principales son:

* **Assets:** contiene las escenas, scripts y recursos utilizados para construir la exhibición. Los scripts propios del proyecto se encuentran en `Assets/Scripts`.
* **Packages:** contiene la información de los paquetes y dependencias del proyecto.
* **ProjectSettings:** contiene la configuración del proyecto en Unity.

Dentro de `Assets/Scripts` se encuentra el código relacionado con la visualización de imágenes de 360°, la interfaz, el panel de administración, la configuración de la exhibición y la comunicación con la API.

Los archivos `.meta` deben conservarse junto con sus respectivos recursos para mantener las referencias de Unity.

## Apertura del proyecto

1. Clonar el repositorio o descargar y extraer su archivo ZIP.
2. Desde Unity Hub, agregar la carpeta que contiene `Assets`, `Packages` y `ProjectSettings`.
3. Abrir el proyecto con la versión de Unity indicada en `ProjectSettings/ProjectVersion.txt`.
4. Esperar a que Unity importe los recursos y resuelva los paquetes necesarios.

Para probar las funciones que consultan información de la aplicación web, es necesario contar con el servidor Flask en funcionamiento y revisar la dirección de la API configurada en el proyecto.

## Alcance del repositorio

Este repositorio se utilizó principalmente para llevar el control de los cambios en la programación. Algunas texturas pueden no cargarse correctamente al abrir una copia descargada desde GitHub.

## Versión para navegador

El proyecto permite generar una versión WebGL de la exhibición. Para compilarla, la instalación de Unity debe incluir el módulo de compilación para WebGL.

El código del servidor Flask se mantiene en un proyecto separado.

## Autoría y recursos

**Alejandra María Cardona Cabrera**  
Universidad del Valle de Guatemala  
Departamento de Ingeniería Electrónica, Mecatrónica y Biomédica  
2026

Los recursos de terceros utilizados conservan las condiciones de sus respectivas licencias.
