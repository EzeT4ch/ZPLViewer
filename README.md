# ZPL Viewer

Visualizador de cadenas ZPL para el depurador de Visual Studio. Renderiza localmente con BinaryKits, presenta el código formateado y permite aplicar una copia editada a la variable cuando el depurador admite reemplazarla.

## Compilar y probar

Requisitos: Windows, Visual Studio 2026 con desarrollo de extensiones y .NET Framework 4.8, SDK .NET 10 y runtime .NET 8 Desktop. El proyecto de extensión usa el SDK VisualStudio.Extensibility 17.14 y `net8.0-windows8.0`; las aplicaciones depuradas son independientes de ese runtime.

```powershell
./scripts/Build.ps1
```

El paquete se copia a `artifacts/ZplViewer.vsix`. No se publica ni se ejecutan operaciones de Git. Las pruebas del objeto fuente usan los ensamblados de la instalación real de Visual Studio, porque el paquete NuGet solo contiene referencias. Para otra edición o ubicación:

```powershell
dotnet test tests/ZplViewer.Tests/ZplViewer.Tests.csproj '-p:VisualStudioRoot=C:\ruta\a\Visual Studio'
```

## Uso

1. Instalar el VSIX y abrir una aplicación .NET con el depurador.
2. Detenerse donde exista una variable `string` con ZPL. Desde Locals, Watch o el DataTip, abrir la lista de la lupa y elegir **ZPL Viewer**.
3. Ajustar ancho, alto y puntos/mm. Los valores iniciales son 100 × 150 mm y 8 puntos/mm. El preset **4 × 8 pulgadas** selecciona 101,6 × 203,2 mm y 8 puntos/mm; pulsar **Actualizar vista previa** después de seleccionarlo. El tamaño seleccionado determina el lienzo de la vista previa.
4. Editar en **ZPL editable** y pulsar **Actualizar vista previa**. La pestaña de formato es una presentación de lectura; sus saltos de línea nunca se escriben en la variable.
5. Pulsar **Aplicar a variable** para escribir el texto editado exacto y verificarlo con una nueva lectura. No se escribe al abrir, formatear, renderizar ni cerrar.
6. Si el valor o la expresión cambian mientras hay un borrador, copiarlo si se desea conservar y utilizar **Recargar variable (descartar borrador)** antes de aplicar.

Al reanudar la aplicación se conserva una instantánea y se deshabilita la escritura. Un error de renderizado deja disponible el editor y marca la imagen anterior como desactualizada. Una escritura cuyo resultado no pudo verificarse exige recargar, sin reintentar automáticamente.

## Desarrollo en la instancia experimental

Abrir `ZplViewer.Extension.slnf` en Visual Studio, establecer `ZplViewer.Extension` como proyecto de inicio y ejecutar F5 con el perfil de Visual Studio instalado. La solución incluye la configuración **Deploy** necesaria para registrar la extensión moderna en la instancia experimental. Copiar el paquete a la carpeta heredada `Extensions` no basta.

En la instancia experimental, abrir `ZplViewer.sln`, elegir uno de los proyectos `ZplViewer.Sample.*` como inicio y ejecutar F5. Ambos ejemplos se detienen automáticamente si hay un depurador adjunto. El visor puede ampliarse o acoplarse como cualquier ventana de herramientas.

La inscripción usa identidades completas de `System.String` para .NET 10 y .NET Framework 4.x. No se afirma compatibilidad con otras versiones de .NET sin ampliar y verificar esos registros. Los detalles de las comprobaciones y sus límites se documentan en [VALIDATION.md](VALIDATION.md).

## Proyectos y límites

El renderizador incluye Roboto Condensed Bold como recurso del ensamblado para la fuente `0` y la sustitución explícita de `q`. No requiere instalar fuentes ni descargar recursos durante el uso. La sustitución de `q` se advierte en el visor y no convierte el identificador a `Q`; las demás fuentes conservan el cargador de BinaryKits. Véanse [licencia y procedencia](src/ZplViewer.Core/Fonts/README.md).

La tipografía por sí sola todavía deja tres rótulos superpuestos en la plantilla de envío: las pruebas de aceptación de anchura lo detectan. No se ajustan textos para hacerlos caber ni se cambian coordenadas. Los casos de comparación están en [samples/Labels](samples/Labels/README.md).

- `src/ZplViewer.Core`: formato por segmentos sin pérdida de contenido, estado del borrador y renderizador.
- `src/ZplViewer.ObjectSource`: puente `netstandard2.0` mínimo cargado en el proceso depurado. No contiene el renderizador ni requiere agregar paquetes a la aplicación.
- `src/ZplViewer.Extension`: proveedor, Remote UI, lectura/escritura y ciclo de vida de imágenes temporales.
- `samples`: ejecutables .NET 10 y .NET Framework 4.8 con dos pausas deliberadas; permiten observar cambios de valor y varias etiquetas.
- `tests`: formato, estado, serialización/reemplazo con el runtime de Visual Studio y PNG generado por BinaryKits.

La vista previa no equivale a una emulación completa de la impresora. Las fuentes, comandos no implementados y recursos que solo existen en la memoria de una impresora pueden producir diferencias. No se descargan fuentes de impresora. No hay red, impresión física ni diseñador visual. Se admiten flujos de texto con delimitadores estándar `^XA`/`^XZ`; los gráficos binarios opacos y los delimitadores de renderizado personalizados no forman parte de la cobertura de esta versión. El formateador conserva los flujos binarios sin dividirlos.

Límites de vista previa: 2 millones de caracteres, 50 etiquetas, 32 megapíxeles por etiqueta y 64 megapíxeles por documento. Se comprueba cancelación antes del análisis y entre páginas; BinaryKits tiene operaciones síncronas que no se pueden interrumpir a mitad de una página. La vista previa se ejecuta fuera del hilo de UI y fuera de la aplicación depurada. Las imágenes se guardan temporalmente bajo `%TEMP%/ZplViewer/<sesión>`; se intenta eliminarlas al reemplazar la vista y cerrar la ventana. Un cierre abrupto del host puede dejar archivos temporales.

El SDK marca como experimental el evento de cambio de expresión; se habilita únicamente ese diagnóstico, con una versión de SDK fijada, para impedir que un borrador se aplique a otra expresión. No se silencian advertencias de nulabilidad.

## Referencias

- [Debugger visualizers](https://learn.microsoft.com/en-us/visualstudio/extensibility/visualstudio.extensibility/debugger-visualizer/debugger-visualizers)
- [Remote UI](https://learn.microsoft.com/en-us/visualstudio/extensibility/visualstudio.extensibility/inside-the-sdk/remote-ui)
- [Depurar extensiones](https://learn.microsoft.com/en-us/visualstudio/extensibility/visualstudio.extensibility/get-started/debug-extensions)
- [BinaryKits.Zpl y limitaciones del motor](https://github.com/BinaryKits/BinaryKits.Zpl)
