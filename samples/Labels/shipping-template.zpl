^XA
^MMT
^PR9,12,12
~TA000
^LH0,0
~SD25
^PW1000

^FX ================== LINEAS ==================
^FO18,137^GB752,6,6,,0^FS
^FO18,317^GB752,6,6,,0^FS
^FO18,467^GB752,6,6,,0^FS
^FO18,617^GB752,6,6,,0^FS
^FO18,797^GB752,6,6,,0^FS
^FO18,977^GB752,6,6,,0^FS
^FO18,1127^GB752,6,6,,0^FS
^FO18,1277^GB752,6,6,,0^FS
^FO18,1397^GB752,6,6,,0^FS
^FO452,320^GB6,150,6,,0^FS
^FO452,470^GB6,150,6,,0^FS
^FO452,620^GB6,180,6,,0^FS
^FO452,980^GB6,150,6,,0^FS
^FO387,1130^GB6,150,6,,0^FS

^FX ================== FROM ==================
^FO40,20^AqN,28,28^FDFROM:^FS
^FO140,20^AqN,28,28^FD{{fromCompany}}^FS
^FO140,50^AqN,28,28^FD{{fromAddress2}}^FS
^FO140,80^AqN,28,28^FD{{fromAddress2}}^FS
^FO500,20^AqN,28,28^FD{{customer1}}^FS
^FO500,50^AqN,28,28^FD{{customer2}}^FS
^FO500,80^AqN,28,28^FD{{ucc}}^FS

^FX ================== TO ==================
^FO40,170^AqN,28,28^FDTO:^FS
^FO140,170^AqN,34,34^FD{{toCompany}}^FS
^FO140,200^AqN,34,34^FD{{toAddress1}}^FS
^FO140,230^AqN,34,34^FD{{toCity}} {{toZip}}^FS
^FO140,260^AqN,34,34^FD{{toAddress2}}^FS

^FX ================== TIPO ==================
^FO480,350^AqN,40,40^FDTIPO:^FS
^FO590,350^AqN,40,40^FD{{orderType}}^FS
^FO480,410^AqN,28,28^FDTIPO DE CONT:^FS
^FO640,410^AqN,28,28^FD{{caseType}}^FS

^FX ================== DROP ==================
^FO040,370^AqN,46,46^FD{{dropId}}^FS

^FX ================== CANALIZADOR ==================
^FO40,500^AqN,35,35^FDCANALIZADOR:^FS
^FO260,500^AqN,35,35^FDOCASA^FS
^FO40,530^BY2^BCN,50,N^FDOCASA^FS

^FX ================== REMITO ==================
^FO490,500^AqN,28,28^FDREMITO/PEDIDO EXT:^FS
^FO470,530^AqN,44,44^FD{{remito}}^FS

^FX ================== ETIQUETA ==================
^FO480,650^AqN,28,28^FDETIQUETA:^FS
^FO690,650^AqN,28,28^FD{{unidadesEtiqueta}}^FS
^FO480,680^AqN,30,30^FDTOTAL ETIQUETAS:^FS
^FO690,680^AqN,30,30^FD{{totalEtiquetas}}^FS
^FO580,770^AqN,30,30^FD{{unidadesEtiqueta}} de {{totalEtiquetas}}^FS

^FX ================== TRACKING ==================
^FO40,830^AqN,28,28^FDTRACKING:^FS
^FO40,860^BY2^BCN,60,N^FD{{caseType}}^FS
^FO50,920^AqN,50,50^FD{{caseType}}^FS
^FX ================== IATA ==================
^FO575,830^AqN,28,28^FDIATA:^FS
^FO575,860^AqN,28,28^FD{{iata}}^FS

^FX ================== GUIA ==================
^FO40,1010^AqN,20,20^FDGUIA:^FS
^FO140,1010^BY2^BCN,60,N^FD{{guia}}^FS
^FO170,1070^AqN,50,50^FD{{guia}}^FS
^FX ================== SORT ==================
^FO480,1010^AqN,30,30^FDSORT:^FS
^FO540,1040^AqN,50,50^FD{{waveId}}^FS

^FX ================== UNIDADES ==================
^FO40,1160^AqN,28,28^FDUNIDADES ETIQUETA:^FS
^FO270,1160^AqN,28,28^FD{{totalunidadeslpn}}^FS
^FO40,1190^AqN,28,28^FDUNIDADES PEDIDO:^FS
^FO270,1190^AqN,28,28^FD{{ext_udf_str1}}^FS
^FO40,1220^AqN,28,28^FDCAJAS ETIQUETA:^FS
^FO270,1220^AqN,28,28^FD{{totalEtiquetas}}^FS
^FX ================== OBS ==================
^FO270,1310^AqN,50,50^FD{{susr5}}^FS
^FX ================== SSCC ==================
^BY4,3,120
^FT110,1550^BC,,N,N^FH\^FD{{sscc}}^FS
^FO160,1560^AqN,59,59^FD{{sscc}}^FS
^XZ
