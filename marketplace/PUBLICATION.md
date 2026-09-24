# Primera publicación: preparación

Estado: **no publicado**. Publisher identificado mediante la URL de la extensión existente proporcionada por el usuario: `EzequielBenitez.omega25`. El portal ya responde, pero la sesión del navegador solicita crear un perfil para Brian Benitez. Falta acceder con la cuenta administradora del publisher existente.

## Datos propuestos para la ficha

- Nombre: ZPL Viewer.
- Nombre interno propuesto: zpl-viewer (la disponibilidad se comprueba en el portal).
- Versión del proyecto: 0.1.1. Presentación pública: versión preliminar.
- Descripción breve: Visualizador local de etiquetas ZPL desde el depurador de Visual Studio, con editor, formato y vista previa.
- Tipo: Tools.
- Etiquetas propuestas: ZPL, debugger, labels, Zebra, local, visualizer.
- Descripción extensa: `OVERVIEW.md` de esta carpeta.
- Publisher: `EzequielBenitez`, indicado por el usuario mediante su extensión existente. Pendiente verificar acceso de administración en el portal.
- Precio: pendiente de configurar en el portal.
- Repositorio y soporte: no se inventaron URLs públicas.

## Artefacto disponible

`artifacts/ZplViewer-0.1.1-marketplace.vsix`

SHA256: `2F003E0CFB6EEC789FDE53454A86D622B110F87310DB0E0136C4F57F9BC1C3BD`.

Recompilado en Release sin errores ni advertencias tras establecer `Publisher=Ezequiel Benitez`. Manifiesto comprobado: versión `0.1.1.0`, `Preview=true` e identificador VSIX original conservado. No cambia el renderizador respecto del candidato anterior.

Este paquete es el candidato de revisión de la iteración tipográfica. El informe de validación registra 41 pruebas aprobadas y 3 fallidas por superposición. No presentarlo como una versión que corrige todas las superposiciones ni como reemplazo de variables validado. Los detalles están en `VALIDATION.md`.

Conservar el identificador VSIX existente para las futuras actualizaciones. Verificar acceso al publisher en el portal antes de la subida. Si se recompila, recalcular el hash y actualizar esta referencia.

## Recorrido de publicación

En [Manage Publishers & Extensions](https://marketplace.visualstudio.com/manage), iniciar sesión con la cuenta del usuario, seleccionar el publisher y elegir **New extension → Visual Studio**. Cargar el VSIX, completar la ficha y comprobar la vista previa. **Save & Upload** no equivale a una publicación pública: Microsoft documenta un paso posterior **Make Public**. Verificar la URL pública y la descarga antes de informar que se publicó.

Referencia: [Publicar una extensión de Visual Studio](https://learn.microsoft.com/en-us/visualstudio/extensibility/walkthrough-publishing-a-visual-studio-extension?view=vs-2022).

El rechazo de Marketplace confirmó que el autor del VSIX debe coincidir con el nombre visible del publisher: Ezequiel Benitez. EzequielBenitez es el ID de la cuenta para la URL; no es el valor de Publisher en el manifiesto. Paquete corregido y manifiesto interno verificado, pendiente de volver a subir.
