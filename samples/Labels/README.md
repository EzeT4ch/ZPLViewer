# Etiquetas de regresión

`shipping-template.zpl` transcribe el ZPL proporcionado en la conversación, con los placeholders intactos. Los escapes de Markdown (`\~`, `\_`) y las entidades `&#x20;` del mensaje se transcribieron como caracteres ZPL y líneas vacías, según la captura del editor. El visor no realiza esa normalización sobre las variables.

`shipping-example.zpl` usa exclusivamente datos ficticios; no es una etiqueta apta para envíos reales.

Comparar a 101,6 × 203,2 mm y 8 puntos/mm. BinaryKits redondea el lienzo a 813 × 1626 píxeles; la captura de Labelary limita el ancho a 812. No se cambia `^PW1000` en estas muestras.

Para generar la comparación local antes/después:

```powershell
dotnet run --project scripts/RenderComparison -- samples/Labels/shipping-template.zpl artifacts/comparison-template
dotnet run --project scripts/RenderComparison -- samples/Labels/shipping-example.zpl artifacts/comparison-example
```
