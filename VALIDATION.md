# Validación de ZPL Viewer

## Iteración 0.1.1: fuentes locales (en revisión)

Compilación Release completada sin errores ni advertencias. El VSIX contiene `ZplViewer.Core.dll` con Roboto Condensed Bold como recurso y `Fonts/OFL.txt`. No se descargan fuentes durante el uso.

Comprobado mediante despliegue F5 en Visual Studio experimental, con el ejemplo .NET 10: la etiqueta utiliza la fuente incluida, aparece la advertencia de sustitución de `q`, los valores iniciales siguen siendo 100 × 150 mm y el preset cambia a 101,6 × 203,2 mm / 8 puntos por mm. Actualizar muestra la etiqueta completa y alternar a la pestaña de formato conserva la imagen. No se editó ni se aplicó la variable durante esta comprobación. También se ven las superposiciones pendientes en el IDE.

Paquete de revisión: `artifacts/ZplViewer-0.1.1-preview.vsix`, SHA256 `57DB776223B7A1057E1390F3195EA26826B04A5FBEB2318F7D0A650E559C296E`. No reemplaza la entrega anterior como versión validada; `scripts/Build.ps1` exige que pasen todas las pruebas antes de copiar `ZplViewer.vsix`.

Resultado actual: **41 de 44 pruebas aprobadas, 3 fallidas**. Las pruebas de aceptación de anchura detectan desbordamientos reales que el cambio de fuente por sí solo no corrige:

| Rótulo | Ancho ocupado en puntos | Espacio hasta el siguiente campo |
| --- | ---: | ---: |
| TIPO DE CONT: | 169 | 160 |
| TOTAL ETIQUETAS: | 232 | 210 |
| UNIDADES ETIQUETA: | 244 | 230 |

Se conserva la prueba fallida, sin omitirla ni ajustar la expectativa para ocultar la superposición. Está pendiente decidir si se calibran las métricas horizontales de la sustitución de `q`. No se aplica ajuste por contenido ni se mueven campos.

Las pruebas aprobadas comprueban `^AqN` y prefijos personalizados en el formateador, conservación exacta de la fuente editable, distinción de `q`/`Q`, anchura y altura independientes, posición `^FO`/`^FT`, acentos y regiones de líneas y códigos de barras idénticas al renderizador anterior. Incluyen la plantilla con placeholders y otra con datos ficticios.

Comparaciones generadas a 101,6 × 203,2 mm y 8 puntos/mm (813 × 1626 píxeles):

- [Plantilla antes/después](artifacts/comparison-template/comparison.png).
- [Datos ficticios antes/después](artifacts/comparison-example/comparison.png).

Son comparaciones entre BinaryKits predeterminado y la fuente incluida. La captura de Labelary se usa como referencia visual; no se afirma equivalencia píxel a píxel ni se utiliza su API. La aceptación tipográfica de esta iteración todavía no está completada.

## Validación anterior: 0.1.0

Entorno: Windows x64, Visual Studio Community 2026 18.9.2, instancia experimental, SDK VisualStudio.Extensibility 17.14.40608. Las comprobaciones se realizaron con las aplicaciones de ejemplo del repositorio.

## Automatización

`./scripts/Build.ps1` completado en Release: compilación sin errores ni advertencias, **31 pruebas aprobadas**, ninguna omitida. Se generó `artifacts/ZplViewer.vsix`.

Cobertura: preservación de segmentos y campos ZPL, escapes, gráficos, delimitadores personalizados en el formateador, borradores y conflictos, cancelación, límites de tamaño, entradas inválidas, PNG con contenido, texto acentuado, Code128, QR, gráficos ASCII embebidos/descargados y documentos de varias etiquetas. El objeto fuente se prueba contra el runtime real de visualizadores instalado con Visual Studio: lectura JSON, reemplazo exacto y rechazo de null.

Estas pruebas del objeto fuente no equivalen a una escritura dentro de una sesión real del depurador.

## Comprobaciones dentro del IDE

| Comprobación | Resultado |
| --- | --- |
| Registro en la lupa de una cadena | Confirmado mediante despliegue F5 |
| Lectura desde .NET 10 | Confirmada |
| Lectura desde .NET Framework 4.8 | Confirmada |
| Renderizado local y transferencia de PNG a Remote UI | Confirmados en ambos runtimes |
| Texto acentuado, Code128 y QR en el IDE | Visibles en la etiqueta de ejemplo |
| Editar borrador y actualizar la imagen | Confirmado en .NET 10 |
| Indicador de imagen desactualizada | Confirmado al editar |
| Continuar con borrador y cambio del valor | Borrador conservado, conflicto indicado y recarga exigida |
| Recarga explícita | Recupera y renderiza el valor cambiado por el programa |
| Abrir/renderizar sin modificar variable | Confirmado por el valor original impreso por el ejemplo .NET 10 |
| Varias etiquetas en .NET Framework | Documento de dos etiquetas cargado |
| Reemplazo real de variable | **No validado: el depurador devuelve IsTargetReplaceable=false para las cadenas locales de ambos ejemplos** |

La extensión respeta la capacidad informada por el debugger y mantiene deshabilitado **Aplicar a variable** en esos casos. La ruta `ReplaceTargetObjectAsync`, su comprobación previa y su lectura posterior están implementadas, pero no se afirma que la escritura funcione en este entorno. No se fuerza el reemplazo cuando la API lo deniega. Este punto del plan sigue pendiente de una sesión que exponga un destino reemplazable o de resolver la limitación del host.

## Correcciones surgidas de la integración

- Los registros sin versión aparecían en la lupa, pero el host fallaba al construir `System.Version`. Se usan identidades completas para .NET 10 y Framework 4.x.
- ServiceHub no resolvía las bibliotecas nativas bajo `runtimes`: se agregó resolución limitada a SkiaSharp/HarfBuzzSharp desde el directorio de la extensión y la arquitectura del host.
- Los PNG transparentes necesitan un fondo blanco en la presentación, especialmente en el tema oscuro.
- La interfaz se adapta al tamaño inicial de la ventana y permite ampliarla; no exige un tamaño mínimo que oculte los botones.

## Límites de la entrega

El VSIX está construido y fue desplegado mediante el flujo de desarrollo F5. La instalación independiente con VSIXInstaller no quedó validada: el intento por línea de comandos derivó al instalador de Visual Studio y no completó esa ruta. No se publicó en Marketplace ni se modificó una instalación productiva de la extensión.

No se verificaron ARM64, otras versiones de Visual Studio/.NET, ni equivalencia con etiquetas impresas reales. La matriz de expresiones de solo lectura y el reemplazo real requieren ampliar la validación. La compatibilidad productiva debe comprobarse con muestras representativas de impresoras, fuentes y comandos utilizados.
