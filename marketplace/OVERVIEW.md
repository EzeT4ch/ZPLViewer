# ZPL Viewer

Visualizá etiquetas ZPL desde la lupa de una variable `string` en el depurador de Visual Studio, sin salir del IDE.

**Primera versión preliminar (0.1.1).** El renderizado es aproximado y conserva limitaciones tipográficas conocidas. No sustituye la validación con una impresora real.

## Funciones

- Renderizado completamente local con BinaryKits.Zpl.Viewer. El visor no envía el ZPL a servicios externos ni descarga fuentes durante el uso.
- Editor con el texto original, vista formateada de lectura y vista previa de la etiqueta.
- Roboto Condensed Bold incluida para la fuente `0` y como sustitución aproximada de `q`, con advertencia visible.
- Tamaño de etiqueta y densidad configurables, zoom y navegación entre etiquetas.
- Preset de 4 × 8 pulgadas: 101,6 × 203,2 mm y 8 puntos/mm.
- Actualización explícita de la imagen e indicador de vista desactualizada.

## Uso

1. Detené el depurador donde exista una cadena con ZPL completo (`^XA` … `^XZ`).
2. Abrí la lista de la lupa en Locals, Watch o el DataTip y seleccioná **ZPL Viewer**.
3. Ajustá las dimensiones y pulsá **Actualizar vista previa**.
4. Usá **ZPL editable** para trabajar sobre un borrador. La pestaña **Formato (solo lectura)** no cambia el texto editable.

Abrir, formatear y renderizar no escriben la variable. **Aplicar a variable** solo se habilita cuando el depurador admite reemplazar el destino. Si el valor cambia mientras existe un borrador, el visor exige recargar antes de aplicar.

## Entorno comprobado

Windows x64, Visual Studio Community 2026 18.9.2, con aplicaciones .NET 10 y .NET Framework 4.8. La extensión usa .NET 8 y VisualStudio.Extensibility. La interfaz está en español. No se afirma validación en otras versiones de Visual Studio, otros runtimes depurados ni ARM64.

## Limitaciones de esta versión

- Persisten superposiciones en algunos rótulos de la plantilla de envío utilizada para las pruebas. La sustitución de `q` mejora la tipografía, pero no reproduce las métricas de todas las impresoras ni equivale a Labelary. Los textos largos no se ajustan automáticamente.
- El reemplazo real de variables no quedó validado: Visual Studio informó `IsTargetReplaceable=false` para las cadenas locales de los ejemplos comprobados. En esos casos, **Aplicar a variable** permanece deshabilitado.
- Los comandos no implementados por BinaryKits y los recursos almacenados únicamente en la impresora pueden producir resultados distintos. No se emulan completamente las fuentes descargadas de una impresora.
- Hasta 2 millones de caracteres, 50 etiquetas, 32 megapíxeles por página y 64 megapíxeles por documento.
- No incluye impresión física, diseñador visual ni modo conectado.

## Componentes

Renderizado con [BinaryKits.Zpl](https://github.com/BinaryKits/BinaryKits.Zpl). Fuente [Roboto Condensed](https://github.com/google/fonts/tree/main/ofl/robotocondensed), distribuida bajo SIL Open Font License; su licencia está incluida en el paquete.
