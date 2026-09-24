using System;
using System.Diagnostics;

namespace ZplViewer.Sample;

internal static class Program
{
    private static string ReadOnlyLabel => "^XA^FO30,30^A0N,40,40^FDSolo lectura^FS^XZ";

    private static void Main()
    {
        var zpl = "^XA\n^MMT\n^PR9,12,12\n~TA000\n^LH0,0\n~SD25\n^PW1000\n \n^FX ================== LINEAS ==================\n^FO18,137^GB752,6,6,,0^FS\n^FO18,317^GB752,6,6,,0^FS\n^FO18,467^GB752,6,6,,0^FS\n^FO18,617^GB752,6,6,,0^FS\n^FO18,797^GB752,6,6,,0^FS\n^FO18,977^GB752,6,6,,0^FS\n^FO18,1127^GB752,6,6,,0^FS\n^FO18,1277^GB752,6,6,,0^FS\n^FO18,1397^GB752,6,6,,0^FS\n \n^FO452,320^GB6,150,6,,0^FS\n^FO452,470^GB6,150,6,,0^FS\n^FO452,620^GB6,180,6,,0^FS\n^FO452,980^GB6,150,6,,0^FS\n^FO387,1130^GB6,150,6,,0^FS\n \n^FX ================== FROM ==================\n^FO40,20^AqN,28,28^FDFROM:^FS\n^FO140,20^AqN,28,28^FD{{fromCompany}}^FS\n^FO140,50^AqN,28,28^FD{{fromAddress2}}^FS\n^FO140,80^AqN,28,28^FD{{fromAddress2}}^FS\n \n^FO500,20^AqN,28,28^FD{{customer1}}^FS\n^FO500,50^AqN,28,28^FD{{customer2}}^FS\n^FO500,80^AqN,28,28^FD{{ucc}}^FS\n \n^FX ================== TO ==================\n^FO40,170^AqN,28,28^FDTO:^FS\n^FO140,170^AqN,34,34^FD{{toCompany}}^FS\n^FO140,200^AqN,34,34^FD{{toAddress1}}^FS\n^FO140,230^AqN,34,34^FD{{toCity}} {{toZip}}^FS\n^FO140,260^AqN,34,34^FD{{toAddress2}}^FS\n \n \n^FX ================== TIPO ==================\n^FO480,350^AqN,40,40^FDTIPO:^FS\n^FO590,350^AqN,40,40^FD{{orderType}}^FS\n \n^FO480,410^AqN,28,28^FDTIPO DE CONT:^FS\n^FO640,410^AqN,28,28^FD{{caseType}}^FS\n \n^FX ================== DROP ==================\n^FO040,370^AqN,46,46^FD{{dropId}}^FS\n \n^FX ================== CANALIZADOR ==================\n^FO40,500^AqN,35,35^FDCANALIZADOR:^FS\n^FO260,500^AqN,35,35^FDOCASA^FS\n \n^FO40,530^BY2^BCN,50,N^FDOCASA^FS\n \n \n^FX ================== REMITO ==================\n^FO490,500^AqN,28,28^FDREMITO/PEDIDO EXT:^FS\n^FO470,530^AqN,44,44^FD{{remito}}^FS\n \n^FX ================== ETIQUETA ==================\n^FO480,650^AqN,28,28^FDETIQUETA:^FS\n^FO690,650^AqN,28,28^FD{{unidadesEtiqueta}}^FS\n \n^FO480,680^AqN,30,30^FDTOTAL ETIQUETAS:^FS\n^FO690,680^AqN,30,30^FD{{totalEtiquetas}}^FS\n \n^FO580,770^AqN,30,30^FD{{unidadesEtiqueta}} de {{totalEtiquetas}}^FS\n \n^FX ================== TRACKING ==================\n^FO40,830^AqN,28,28^FDTRACKING:^FS\n^FO40,860^BY2^BCN,60,N^FD{{caseType}}^FS\n^FO50,920^AqN,50,50^FD{{caseType}}^FS\n \n^FX ================== IATA ==================\n^FO575,830^AqN,28,28^FDIATA:^FS\n^FO575,860^AqN,28,28^FD{{iata}}^FS\n \n^FX ================== GUIA ==================\n^FO40,1010^AqN,20,20^FDGUIA:^FS\n^FO140,1010^BY2^BCN,60,N^FD{{guia}}^FS\n^FO170,1070^AqN,50,50^FD{{guia}}^FS\n \n^FX ================== SORT ==================\n^FO480,1010^AqN,30,30^FDSORT:^FS\n^FO540,1040^AqN,50,50^FD{{waveId}}^FS\n \n^FX ================== UNIDADES ==================\n^FO40,1160^AqN,28,28^FDUNIDADES ETIQUETA:^FS\n^FO270,1160^AqN,28,28^FD{{totalunidadeslpn}}^FS\n \n^FO40,1190^AqN,28,28^FDUNIDADES PEDIDO:^FS\n^FO270,1190^AqN,28,28^FD{{ext_udf_str1}}^FS\n \n^FO40,1220^AqN,28,28^FDCAJAS ETIQUETA:^FS\n^FO270,1220^AqN,28,28^FD{{totalEtiquetas}}^FS\n \n^FX ================== OBS ==================\n^FO270,1310^AqN,50,50^FD{{susr5}}^FS\n \n^FX ================== SSCC ==================\n^BY4,3,120\n^FT110,1550^BC,,N,N^FH\\^FD{{sscc}}^FS\n^FO160,1560^AqN,59,59^FD{{sscc}}^FS\n \n^XZ";
        var multipleLabels = zpl + "^XA\n~TA000\n~JSN\n^LT0\n^MNW\n^MTD\n^PON\n^PMN\n^LH0,0\n^JMA\n^PR14,14\n~SD15\n^JUS\n^LRN\n^CI27\n^PA0,1,1,0\n^XZ\n^XA\n^MMT\n^PW799\n^LL480\n^LS0\n^FO57,39^GB692,129,8^FS\n^FT91,121^A0N,45,46^FH\\^CI28^FDPALLET^FS^CI27\n^FPH,1^FT385,125^A0N,51,51^FH\\^CI28^FDJLQ{{COUNTER}}^FS^CI27\n^FT72,230^A0N,39,38^FH\\^CI28^FDCuenta: JULERIAQUE^FS^CI27\n^BY4,2,120^FT79,422^B3N,N,,Y,N\n^FDJLQ{{COUNTER}}^FS\n^PQ1,0,1,Y\n^XZ";
        Console.WriteLine("Abrí la lupa de zpl y elegí ZPL Viewer. Aplicá un cambio y continuá.");
        if (Debugger.IsAttached) Debugger.Break();
        Console.WriteLine(zpl); // Set a breakpoint here if not launched with the debugger.
        Console.WriteLine(multipleLabels.Length);
        Console.WriteLine(ReadOnlyLabel);
        zpl = "^XA^FO40,40^A0N,40,40^FDValor cambiado por el programa^FS^XZ";
        if (Debugger.IsAttached) Debugger.Break();
        Console.WriteLine(zpl);
        Console.WriteLine("Fin. Enter para cerrar.");
        Console.ReadLine();
    }
}
