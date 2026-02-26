Imports System.IO
Imports System.Net.NetworkInformation
Imports System.Management
Imports System.Diagnostics
Imports System.Text.RegularExpressions
Imports System.Net


Public Class Form1

    ' Variables persistentes para datos de sucursal
    Private sucursalTipo As String = ""
    Private sucursalNombre As String = ""
    Private sucursalDependeDe As String = ""


    'Carga
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        LabelEstadoPuesto.Text = "Esperando Puesto"
        LabelEstadoPuesto.ForeColor = Color.Gray
        LabelEstadoServidor.Text = "Esperando Servidor..."
        LabelEstadoServidor.ForeColor = Color.Gray
        LabelEstadoLegajo.Text = "Esperando legajo..."
        LabelEstadoLegajo.ForeColor = Color.Gray
        LabelEstadoCasilla.Text = "Esperando Casilla..."
        LabelEstadoCasilla.ForeColor = Color.Gray
        TextBoxPuesto.MaxLength = 11



        ' cargar: SOLO códigos válidos (4 dígitos numéricos)
        Dim clavesValidas = DiccionarioSucursales.Keys.Where(Function(k) k.Length = 4 AndAlso k.All(AddressOf Char.IsDigit)).
        OrderBy(Function(k) k).
        ToList()

        ' Insertar 0000 como valor inicial
        clavesValidas.Insert(0, "0000")

        ComboBoxSucursales.DataSource = clavesValidas
        ComboBoxSucursales.SelectedIndex = 0

        ' Inicializar el CheckBox desmarcado
        CheckBoxInformacion.Checked = False
    End Sub
    Private Function HacerPing(host As String) As Boolean
        Try
            Dim ping As New Ping()
            Dim reply As PingReply = ping.Send(host, 1000) ' timeout 1 segundo
            Return reply.Status = IPStatus.Success
        Catch
            Return False
        End Try
    End Function

    Private Sub ComboBoxSucursales_TextChanged(sender As Object, e As EventArgs) Handles ComboBoxSucursales.TextChanged
        ' También procesamos cuando el usuario escribe manualmente
        ProcesarSucursal()
    End Sub


    ' Funcion combobox ingreso dato manual o seleccion
    Private Sub ComboBoxSucursales_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxSucursales.SelectedIndexChanged
        If _suspendEvents Then Exit Sub
        If ComboBoxSucursales.SelectedItem Is Nothing Then Exit Sub

        ' Tomamos el texto seleccionado
        Dim codigoTexto As String = ComboBoxSucursales.SelectedItem.ToString().Trim()

        ' Si es 0000, estado inicial y no procesar
        If codigoTexto = "0000" Then
            ResetLabelEstado(LabelEstadoServidor, "Esperando Servidor...")
            ' Si querés, también podés limpiar Tipo/Nombre/DependeDe si los usás:
            ' sucursalTipo = "" : sucursalNombre = "" : sucursalDependeDe = ""
            ActualizarInfoGeneral()
            Exit Sub
        End If

        ' Validar que sean solo números y exactamente 4 dígitos
        Dim codigoNum As Integer
        If Not Integer.TryParse(codigoTexto, codigoNum) OrElse codigoTexto.Length <> 4 Then
            MessageBox.Show("Error: ingrese exactamente 4 números.", "Entrada inválida", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End If

        ' Asegurar 4 dígitos con ceros a la izquierda
        Dim codigo As String = codigoNum.ToString().PadLeft(4, "0"c)

        ' Si la sucursal existe en el diccionario, actualizamos los datos; si no, solo refrescamos la info general
        If DiccionarioSucursales.ContainsKey(codigo) Then
            Dim datos = DiccionarioSucursales(codigo)
            ' Esto setea Tipo/Nombre/DependeDe en las variables persistentes dentro de ActualizarInfoGeneral
            ActualizarInfoGeneral(datos)
        Else
            ActualizarInfoGeneral()
        End If

        ' Opcional: si querés disparar la detección de servidor inmediatamente al elegir la sucursal válida:
        ProcesarServidor(codigo)
    End Sub


    'bloqueo tecla letras o caracter
    Private Sub ComboBoxSucursales_KeyPress(sender As Object, e As KeyPressEventArgs) Handles ComboBoxSucursales.KeyPress
        ' Permitir solo números y la tecla de retroceso
        If Not Char.IsDigit(e.KeyChar) AndAlso Not Char.IsControl(e.KeyChar) Then
            e.Handled = True
            MessageBox.Show("Solo se permiten números.", "Entrada inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub
    ' Diccionario local fijo con los datos importados del Excel
    Private DiccionarioSucursales As New Dictionary(Of String, Tuple(Of String, String, String)) From {
     {"0007", Tuple.Create("SUC", "LAVALLE (CAP. FED.) - SDWAN", "7")},
     {"0009", Tuple.Create("SUC", "BULNES (EX ALMAGRO SUR) - SDWAN", "9")},
     {"0010", Tuple.Create("SUC", "ABASTO - SDWAN", "10")},
     {"0011", Tuple.Create("ANEXO", "ANEXO OPERATIVO FLORES (depende de 0054)", "54")},
     {"0012", Tuple.Create("SUC", "ALMAGRO - SDWAN", "12")},
     {"0013", Tuple.Create("SUC", "AVDA. BOYACA - SDWAN", "13")},
     {"0014", Tuple.Create("SUC", "ARSENAL", "14")},
     {"0015", Tuple.Create("SUC", "AVDA. DE MAYO  - VDI - SDWAN", "15")},
     {"0016", Tuple.Create("SUC", "AVDA. ALVEAR - SDWAN", "16")},
     {"0017", Tuple.Create("SUC-BAJA", "AVDA. LAS HERAS  -  DADO DE BAJA 27/05/2025", "17")},
     {"0018", Tuple.Create("SUC", "AZCUENAGA - SDWAN", "18")},
    {"0019", Tuple.Create("SUC", "AVDA. DE LOS CONSTITUYENTES - SDWAN", "19")},
    {"0020", Tuple.Create("SUC", "AVDA. MARTIN GARCIA - SDWAN", "20")},
    {"0022", Tuple.Create("SUC", "BALVANERA - SDWAN", "22")},
    {"0023", Tuple.Create("ANEXO", "ANEXO OPERATIVO AFIP SEDE CENTRAL (depende de 0085)", "85")},
    {"0024", Tuple.Create("ANEXO", "ANEXO OPERATIVO BARTOLOME MITRE (depende de 0085)", "85")},
    {"0025", Tuple.Create("ANEXO", "ANEXO OPERATIVO MONTSERRAT (depende de 0066) - SDWAN", "66")},
    {"0026", Tuple.Create("SUC", "BARRACAS - SDWAN", "26")},
    {"0030", Tuple.Create("SUC", "BELGRANO - SDWAN", "30")},
    {"0034", Tuple.Create("SUC", "BOCA DEL RIACHUELO - VDI - SDWAN", "34")},
    {"0038", Tuple.Create("SUC", "BOEDO - SDWAN", "38")},
    {"0042", Tuple.Create("SUC", "CABALLITO - SDWAN", "42")},
    {"0043", Tuple.Create("SUC", "CABILDO - SDWAN", "43")},
    {"0044", Tuple.Create("SUC", "CABALLITO SUR - SDWAN", "44")},
    {"0046", Tuple.Create("SUC", "CARLOS CALVO", "46")},
    {"0048", Tuple.Create("SUC", "CARLOS PELLEGRINI (CAP. FED) - SDWAN", "48")},
    {"0049", Tuple.Create("SUC", "COLEGIALES - SDWAN", "49")},
    {"0050", Tuple.Create("SUC-CANX-ZNL-RGN", "CONGRESO (ZONAL REGIONAL)", "50")},
    {"0051", Tuple.Create("SUC", "CUENCA - SDWAN", "51")},
    {"0052", Tuple.Create("SUC", "FEDERICO LACROZE - SDWAN", "52")},
    {"0053", Tuple.Create("SUC", "EVA PER�N - SDWAN", "53")},
    {"0054", Tuple.Create("SUC-CANX-ZNL", "FLORES (ZONAL) - SDWAN", "54")},
    {"0056", Tuple.Create("ANEXO", "ANEXO OPERATIVO PALACIO DE HACIENDA (depende de 0085) - SDWAN", "85")},
    {"0058", Tuple.Create("SUC", "FLORESTA - SDWAN", "58")},
    {"0059", Tuple.Create("SUC-CANX-ZNL", "FLORIDA", "59")},
    {"0061", Tuple.Create("SUC", "GENERAL MOSCONI - SDWAN", "61")},
    {"0062", Tuple.Create("SUC-CANX", "GENERAL URQUIZA - SDWAN", "62")},
    {"0064", Tuple.Create("SUC-CANX-ZNL", "LINIERS (ZONAL) - SDWAN", "64")},
    {"0065", Tuple.Create("SUC", "MONROE", "65")},
    {"0066", Tuple.Create("SUC-CANX", "MONTSERRAT - SDWAN", "66")},
    {"0067", Tuple.Create("SUC", "MONTE CASTROO - SDWAN", "67")},
    {"0068", Tuple.Create("SUC", "NAZCA - SDWAN", "68")},
    {"0069", Tuple.Create("SUC-BAJA", "MICROCENTRO - BAJA (4/6/2024)", "69")},
    {"0070", Tuple.Create("SUC", "NUEVA CHICAGO - SDWAN", "70")},
    {"0071", Tuple.Create("SUC", "NUEVA POMPEYA - SDWAN", "71")},
    {"0074", Tuple.Create("SUC-ZNL-RGN", "PALERMO (ZONAL REGIONAL) - SDWAN", "74")},
    {"0078", Tuple.Create("SUC", "PARQUE PATRICIOS - SDWAN", "78")},
    {"0082", Tuple.Create("SUC", "PATERNAL - SDWAN", "82")},
    {"0083", Tuple.Create("ANEXO-BAJA", "CANE CONGRESO (depende de 0050) BAJA - Se mudo al Edificio de Suc 0069 Microcentro", "50")},
    {"0084", Tuple.Create("SUC", "AVDA. GAONA - SDWAN", "84")},
    {"0086", Tuple.Create("SUC", "PLAZA MISERERE - SDWAN", "86")},
    {"0087", Tuple.Create("SUC", "PLAZA SAN MARTIN - VDI - SDWAN", "87")},
    {"0088", Tuple.Create("SUC", "SAAVEDRA - SDWAN", "88")},
    {"0089", Tuple.Create("SUC", "TRIBUNALES - SDWAN", "89")},
    {"0090", Tuple.Create("SUC", "VILLA CRESPO - SDWAN", "90")},
    {"0091", Tuple.Create("SUC", "SAN CRISTOBAL (CAP. FED.) - SDWAN", "91")},
    {"0092", Tuple.Create("SUC", "VILLA ORTUZAR - SDWAN", "92")},
    {"0093", Tuple.Create("ANEXO", "PPP SEDE EDIFICIO LIBERTADOR (depende de 0085) - SDWAN", "85")},
    {"0094", Tuple.Create("SUC", "VILLA DEL PARQUE - SDWAN", "94")},
    {"0095", Tuple.Create("SUC-CANX", "VILLA DEVOTO - SDWAN", "95")},
    {"0096", Tuple.Create("SUC", "AVDA. CORRIENTES - SDWAN", "96")},
    {"0097", Tuple.Create("SUC", "VILLA LURO - SDWAN", "97")},
    {"0098", Tuple.Create("SUC-BAJA", "WARNES - SDWAN - DADO DE BAJA 31/12/2025", "98")},
    {"0099", Tuple.Create("SUC", "VILLA LUGANO - SDWAN", "99")},
    {"0101", Tuple.Create("ANEXO", "BANCO EN PLANTA SEDE BANCO CENTRAL (depende de 0059)", "59")},
    {"0102", Tuple.Create("ANEXO-BAJA", "PPP PLAZA HUINCUL (depende de 1652) ANEXO DADO DE BAJA 6/10/2023", "1652")},
    {"0105", Tuple.Create("ANEXO", "PPP EDIFICIO LIBERTAD (depende de 0085) - SDWAN", "85")},
    {"0107", Tuple.Create("ANEXO-BAJA", "PPP HOSPITAL MILITAR (depende de 9014)  -  DADO DE BAJA 18/06/2024", "9014")},
    {"0108", Tuple.Create("ANEXO", "PPP HOSPITAL POSADAS (depende de 9216) - SDWAN", "9216")},
    {"0109", Tuple.Create("ANEXO", "PPP SAN JOSE DE PIEDRA BLANCA (depende de 3755)", "3755")},
    {"0110", Tuple.Create("ANEXO", "BANCO EN PLANTA SEDE INTI (depende de 0062) - SDWAN", "62")},
    {"0115", Tuple.Create("ANEXO", "SEDE PALACIO LEGISLATIVO (depende de 0050)", "50")},
    {"0116", Tuple.Create("ANEXO", "PPP JOVITA (depende de 2383)", "2383")},
    {"0120", Tuple.Create("ANEXO", "BANCO EN PLANTA SEDE UNIVERSIDAD NACIONAL DE LA PLATA (depende de 2170) - SDWAN", "2170")},
    {"0206", Tuple.Create("ANEXO", "CANE BAHIA BLANCA (depende de 1130)", "1130")},
    {"0208", Tuple.Create("ANEXO-BAJA", "CANE LUJAN (depende de 2300) - SDWAN  -  Dado de BAJA  1/8/25", "2300")},
    {"0210", Tuple.Create("ANEXO-BAJA", "CANE NEUQUEN (depende de 2540)  - Dado de BAJA 27/05/25", "2540")},
    {"0211", Tuple.Create("ANEXO-BAJA", "CANE OLAVARRIA (Depende de 2600) - Dado de BAJA 19/11/24", "2600")},
    {"0212", Tuple.Create("ANEXO-BAJA", "CANE POSADAS (depende de 2720)  - Dado de BAJA 2/7/2025", "2720")},
    {"0214", Tuple.Create("ANEXO-BAJA", "CANE RECONQUISTA (depende de 2900)  Dado de BAJA  18/07/2025", "2900")},
    {"0215", Tuple.Create("ANEXO", "CANE SAN FRANCISCO (depende de 3160)", "3160")},
    {"0216", Tuple.Create("ANEXO-BAJA", "CANE SANTA FE (depende de 3330)", "3330")},
    {"0217", Tuple.Create("ANEXO-BAJA", "CANE TANDIL (depende de 3450) - SDWAN", "3450")},
    {"0218", Tuple.Create("ANEXO", "CANE VILLA MARIA (depende de 3810)", "3810")},
    {"0219", Tuple.Create("ANEXO", "CANE RIO CUARTO (depende de 2930) - SDWAN", "2930")},
    {"0220", Tuple.Create("ANEXO-BAJA", "CANE MAR DEL PLATA (depende de 2350)  -  Dado de BAJA  13/10/25", "2350")},
    {"0305", Tuple.Create("ANEXO-BAJA", "PPP CASA DE GOBIERNO MENDOZA (Ex 2908 Rentas MZA (depende de 2405) ### BAJA 17/7/25 ###", "2405")},
    {"0308", Tuple.Create("ANEXO-BAJA", "PPP EMBARCACION (depende de 3305)  -  Dado de BAJA el 8/8/24", "3305")},
    {"0309", Tuple.Create("ANEXO", "PPP GENERAL MOSCONI (depende de 3470)", "3470")},
    {"0310", Tuple.Create("ANEXO-BAJA", "PPP LA CALERA (depende de 9226)  -  BAJA el 31/10/24", "9226")},
    {"0318", Tuple.Create("ANEXO-BAJA", "PPP MALVINAS ARGENTINAS (depende de 2472)   Dado de BAJA el 31/10/24", "2472")},
    {"0325", Tuple.Create("ANEXO", "PPP RIO CEBALLOS (depende de 3601) - SDWAN (DADO DE BAJA 17/09/2025)", "3601")},
    {"0330", Tuple.Create("ANEXO-BAJA", "PPP SAN BERNARDO (depende de 3710)  --  Dado de BAJA  14/10/25", "3710")},
    {"0340", Tuple.Create("ANEXO", "PPP SECRETARIA DE SALUD Y DESARROLLO SOCIAL (depende de 0046)", "85")},
    {"0360", Tuple.Create("ANEXO", "PPP HOSPITAL GARRAHAN (depende de 0014)", "14")},
    {"1010", Tuple.Create("SUC", "ADELIA MARIA - SDWAN", "1010")},
    {"1011", Tuple.Create("ANEXO", "CENTRO DE PAGOS CIPOLLETTI (depende de 1495) - SDWAN", "1495")},
    {"1012", Tuple.Create("ANEXO", "ANEXO OPERATIVO CENTRO DE RECAUDACION MENDOZA - Ex Rentas (depende de 2405)", "2405")},
    {"1013", Tuple.Create("ANEXO", "ANEXO OPERATIVO GOYA (depende de 1960)", "1960")},
    {"1014", Tuple.Create("ANEXO-BAJA", "ANEXO OPERATIVO BARRIO INDEPENDENCIA (depende de 1180) - BAJA el 30/06/2025", "1180")},
    {"1015", Tuple.Create("SUC", "ADOLFO G. CHAVES - SDWAN", "1015")},
    {"1016", Tuple.Create("ANEXO", "CENTRO DE PAGOS BURZACO (depende de 1250) -  SDWAN", "1250")},
    {"1017", Tuple.Create("ANEXO", "ANEXO OPERATIVO AVDA. JUAN B. JUSTO (depende de 9205) - SDWAN", "9205")},
    {"1018", Tuple.Create("ANEXO", "ANEXO OPERATIVO BELEN DE ESCOBAR (depende de 1208) - SDWAN", "1208")},
    {"1019", Tuple.Create("ANEXO", "ANEXO OPERATIVO FORMOSA (depende de 1790) - SDWAN", "1790")},
    {"1021", Tuple.Create("ANEXO", "ANEXO OPERATIVO ISIDRO CASANOVA (depende de 2045)", "2045")},
    {"1023", Tuple.Create("ANEXO", "ANEXO OPERATIVO PARANA (depende de 2650)", "2650")},
    {"1025", Tuple.Create("ANEXO", "ANEXO OPERATIVO S.F. del VALLE de CATAMARCA (depende de 3155)", "3155")},
    {"1026", Tuple.Create("ANEXO", "CENTRO DE PAGOS BARILOCHE (depende de 3120) - SDWAN", "3120")},
    {"1027", Tuple.Create("ANEXO", "ANEXO OPERATIVO SAN RAM�N DE LA NUEVA OR�N (depende de 3305)", "3305")},
    {"1029", Tuple.Create("ANEXO", "SAN NICOLAS (depende de 3270) - SDWAN", "3270")},
    {"1030", Tuple.Create("SUC", "AGUILARES - SDWAN", "1030")},
    {"1031", Tuple.Create("ANEXO", "ANEXO OPERATIVO USPALLATA (depende de 3631) - SDWAN", "3631")},
    {"1033", Tuple.Create("SUC", "ADROGUE - SDWAN", "1033")},
    {"1034", Tuple.Create("ANEXO", "ANEXO OPERATIVO BERAZATEGUI (depende de 1222) - SDWAN", "1222")},
    {"1035", Tuple.Create("SUC", "AIMOGASTA - SDWAN", "1035")},
    {"1036", Tuple.Create("ANEXO", "ANEXO OPERATIVO AEROPUERTO MENDOZA (depende de 2400) - SDWAN", "2400")},
    {"1037", Tuple.Create("ANEXO", "CENTRO DE PAGOS TARTAGAL (depende de 3470) - SDWAN", "3470")},
    {"1038", Tuple.Create("ANEXO", "ANEXO OPERATIVO MAIPU (depende de 2327) - SDWAN", "2327")},
    {"1039", Tuple.Create("ANEXO", "ANEXO OPERATIVO SANTA FE (depende de 3330)", "3330")},
    {"1040", Tuple.Create("SUC", "ALCORTA - SDWAN", "1040")},
    {"1041", Tuple.Create("ANEXO", "ANEXO OPERATIVO NEUQUEN (depende de 2540) - SDWAN", "2540")},
    {"1042", Tuple.Create("ANEXO", "ANEXO OPERATIVO MORENO (depende de 2495) - SDWAN", "2495")},
    {"1043", Tuple.Create("SUC", "ALEJANDRO - SDWAN", "1043")},
    {"1044", Tuple.Create("ANEXO", "ANEXO OPERATIVO POSADAS (depende de 2720) - SDWAN", "2720")},
    {"1045", Tuple.Create("SUC", "ALLEN - SDWAN", "1045")},
    {"1046", Tuple.Create("SUC", "ALTA CORDOBA - SDWAN", "1046")},
    {"1047", Tuple.Create("SUC", "ALTA GRACIA - SDWAN", "1047")},
    {"1048", Tuple.Create("SUC", "ALTO RIO SENGUER - SDWAN", "1048")},
    {"1050", Tuple.Create("SUC", "ALVEAR (CTES) - SD WAN", "1050")},
    {"1054", Tuple.Create("SUC", "AMEGHINO - SDWAN", "1054")},
    {"1060", Tuple.Create("SUC", "ANDALGALA - SDWAN", "1060")},
    {"1065", Tuple.Create("SUC", "ANILLACO - SDWAN", "1065")},
    {"1070", Tuple.Create("SUC", "A�ATUYA - SDWAN", "1070")},
    {"1080", Tuple.Create("SUC", "APOSTOLES - SDWAN", "1080")},
    {"1089", Tuple.Create("SUC", "ARISTOBULO DEL VALLE - SDWAN", "1089")},
    {"1090", Tuple.Create("SUC", "ARIAS - SDWAN", "1090")},
    {"1091", Tuple.Create("SUC", "ARMSTRONG - SDWAN", "1091")},
    {"1092", Tuple.Create("ANEXO", "ANEXO OPERATIVO AVDA. LIBERTAD (depende de 3816) - SDWAN", "3816")},
    {"1093", Tuple.Create("SUC", "ARRECIFES - SDWAN", "1093")},
    {"1094", Tuple.Create("ANEXO", "ANEXO OPERATIVO AVDA. MITRE (depende de 3300) SDWAN", "3300")},
    {"1095", Tuple.Create("SUC", "ARROYITO - SDWAN", "1095")},
    {"1096", Tuple.Create("SUC", "AVDA. ARISTOBULO DEL VALLE", "1096")},
    {"1097", Tuple.Create("SUC", "ARROYO SECO - SDWAN", "1097")},
    {"1098", Tuple.Create("SUC", "AVDA. HUMBERTO PRIMO - SDWAN", "1098")},
    {"1099", Tuple.Create("SUC", "AVDA. SAN MARTIN - SDWAN", "1099")},
    {"1100", Tuple.Create("SUC", "AVELLANEDA - SDWAN", "1100")},
    {"1102", Tuple.Create("ANEXO", "ANEXO OPERATIVO AVDA. ALVEAR OESTE - SDWAN (depende de 1830)", "1830")},
    {"1103", Tuple.Create("SUC", "AVELLANEDA (STA. FE) - SDWAN", "1103")},
    {"1104", Tuple.Create("SUC", "AV ILLIA - SDWAN", "1104")},
    {"1106", Tuple.Create("SUC", "AVDA. 25 DE MAYO - RESISTENCIA - SDWAN", "1106")},
    {"1110", Tuple.Create("SUC", "AYACUCHO - SDWAN", "1110")},
    {"1115", Tuple.Create("SUC", "AVENIDA 13 - SDWAN", "1115")},
    {"1120", Tuple.Create("SUC-ZNL", "AZUL (ZONAL) - SDWAN", "1120")},
    {"1130", Tuple.Create("SUC-CANX-ZNL-RGN", "BAHIA BLANCA (ZONAL REGIONAL) - SDWAN", "1130")},
    {"1130-1", Tuple.Create("EDIF-ANX", "EDIFICIO ANEXO BRANDSEN (depende de 1130) - SDWAN", "1130")},
    {"1140", Tuple.Create("SUC", "BALCARCE - SDWAN", "1140")},
    {"1150", Tuple.Create("SUC", "BALNEARIA", "1150")},
    {"1155", Tuple.Create("SUC", "BANDERA - SDWAN", "1155")},
    {"1157", Tuple.Create("SUC", "BANFIELD - SDWAN", "1157")},
    {"1160", Tuple.Create("SUC", "BARADERO - SDWAN", "1160")},
    {"1163", Tuple.Create("SUC", "BARKER", "1163")},
    {"1166", Tuple.Create("SUC", "BARRANQUERAS - SDWAN", "1166")},
    {"1168", Tuple.Create("SUC", "BARREAL - SDWAN", "1168")},
    {"1170", Tuple.Create("SUC", "BARRIO ARROYITO - SDWAN", "1170")},
    {"1172", Tuple.Create("SUC", "BARRIO BELGRANO - SDWAN", "1172")},
    {"1175", Tuple.Create("SUC", "BARRIO ECHESORTU - SDWAN", "1175")},
    {"1180", Tuple.Create("SUC-CANX", "BARRIO INDEPENDENCIA - SDWAN", "1180")},
    {"1183", Tuple.Create("SUC", "BARRIO LOS NARANJOS - SDWAN", "1183")},
    {"1185", Tuple.Create("SUC", "BARRIO LURO - SDWAN", "1185")},
    {"1190", Tuple.Create("SUC", "BARRIO PUERTO - SDWAN", "1190")},
    {"1192", Tuple.Create("SUC", "BARRIO SAN VICENTE - SDWAN", "1192")},
    {"1200", Tuple.Create("SUC", "BASAVILBASO - SDWAN", "1200")},
    {"1202", Tuple.Create("SUC", "BASE NAVAL PUERTO GENERAL BELGRANO - SDWAN", "1202")},
    {"1204", Tuple.Create("SUC", "BATAN", "1204")},
    {"1207", Tuple.Create("SUC", "BELEN - SDWAN", "1207")},
    {"1208", Tuple.Create("SUC-CANX", "BELEN DE ESCOBAR - SDWAN", "1208")},
    {"1209", Tuple.Create("SUC", "PARQUE INDUSTRIAL PILAR - SDWAN", "1209")},
    {"1210", Tuple.Create("SUC", "BELLA VISTA - SDWAN", "1210")},
    {"1215", Tuple.Create("SUC", "BELLA VISTA (B.A.) - SDWAN", "1215")},
    {"1220", Tuple.Create("SUC", "BELL VILLE - SDWAN", "1220")},
    {"1221", Tuple.Create("SUC", "BENITO JUAREZ - SDWAN", "1221")},
    {"1222", Tuple.Create("SUC-CANX", "BERAZATEGUI - SDWAN", "1222")},
    {"1223", Tuple.Create("SUC", "BERNASCONI", "1223")},
    {"1224", Tuple.Create("ANEXO", "ANEXO OPERATIVO TRELEW (depende de 3550) - SDWAN", "3550")},
    {"1225", Tuple.Create("SUC", "BERROTARAN - SDWAN", "1225")},
    {"1226", Tuple.Create("ANEXO", "ANEXO OPERATIVO TUNUYAN (depende de 3590) - SDWAN", "3590")},
    {"1227", Tuple.Create("ANEXO", "ANEXO OPERATIVO TUPUNGATO (depende de 3592) - SDWAN", "3592")},
    {"1230", Tuple.Create("SUC", "BOLIVAR - SDWAN", "1230")},
    {"1234", Tuple.Create("SUC", "BOULOGNE - SDWAN", "1234")},
    {"1235", Tuple.Create("SUC", "BOVRIL - SDWAN", "1235")},
    {"1236", Tuple.Create("SUC", "BOWEN -SDWAN", "1236")},
    {"1240", Tuple.Create("SUC", "BRAGADO - SDWAN", "1240")},
    {"1241", Tuple.Create("SUC", "BRANDSEN -SDWAN", "1241")},
    {"1243", Tuple.Create("SUC", "BRINKMANN - SDWAN", "1243")},
    {"1250", Tuple.Create("SUC-CANX", "BURZACO - SDWAN", "1250")},
    {"1260", Tuple.Create("SUC", "CAFAYATE - SDWAN", "1260")},
    {"1261", Tuple.Create("ANEXO", "ANEXO OPERATIVO LAS HERAS (depende de 2213) - SDWAN", "2213")},
    {"1262", Tuple.Create("ANEXO", "ANEXO OPERATIVO MORON (depende de 2500) - SDWAN", "2500")},
    {"1263", Tuple.Create("SUC", "CALCHAQUI - SDWAN", "1263")},
    {"1264", Tuple.Create("SUC", "CALCHIN - SDWAN", "1264")},
    {"1265", Tuple.Create("SUC", "CALLE SAN LUIS - SDWAN", "1265")},
    {"1266", Tuple.Create("SUC", "CALETA OLIVIA - SDWAN", "1266")},
    {"1267", Tuple.Create("SUC", "CAMPANA - SDWAN", "1267")},
    {"1268", Tuple.Create("SUC", "CAMPO LARGO - SDWAN", "1268")},
    {"1269", Tuple.Create("ANEXO-BAJA", "ANEXO OPERATIVO CALLE SANTA FE (dep de 3020) Se dio de BAJA 14/6/24 !!!!!", "3020")},
    {"1270", Tuple.Create("SUC", "CANALS - SDWAN", "1270")},
    {"1271", Tuple.Create("SUC", "CAMPO DE MAYO - SDWAN", "1271")},
    {"1274", Tuple.Create("SUC", "CALLE 12 LA PLATA - SDWAN", "1274")},
    {"1275", Tuple.Create("ANEXO-BAJA", "CENTRO DE PAGOS CALLE MITRE (depende de 3245) Dado de BAJA 19/11/24", "3245")},
    {"1278", Tuple.Create("ANEXO", "ANEXO OPERATIVO SAN MIGUEL (depende de 3260) - SDWAN", "3260")},
    {"1280", Tuple.Create("SUC", "CA�ADA DE GOMEZ - SDWAN", "1280")},
    {"1290", Tuple.Create("SUC", "CA�UELAS - SDWAN", "1290")},
    {"1300", Tuple.Create("SUC", "CAPILLA DEL MONTE - SDWAN", "1300")},
    {"1305", Tuple.Create("SUC", "CAPILLA DEL SE�OR - SDWAN", "1305")},
    {"1310", Tuple.Create("SUC", "CAPITAN SARMIENTO - SDWAN", "1310")},
    {"1320", Tuple.Create("SUC", "CARCARA�� - SDWAN", "1320")},
    {"1330", Tuple.Create("SUC", "CARHU� - SDWAN", "1330")},
    {"1340", Tuple.Create("SUC", "CARLOS CASARES - SDWAN", "1340")},
    {"1346", Tuple.Create("SUC", "CARLOS PELLEGRINI (STA.FE) - SDWAN", "1346")},
    {"1350", Tuple.Create("SUC", "CARLOS TEJEDOR - SDWAN", "1350")},
    {"1352", Tuple.Create("SUC", "CARMEN DE ARECO - SDWAN", "1352")},
    {"1355", Tuple.Create("SUC", "CARRIL RODRIGUEZ PE�A - SDWAN", "1355")},
    {"1360", Tuple.Create("SUC", "CASBAS - SDWAN", "1360")},
    {"1370", Tuple.Create("SUC", "CASEROS - SDWAN", "1370")},
    {"1380", Tuple.Create("SUC", "CASILDA - SDWAN", "1380")},
    {"1386", Tuple.Create("SUC", "CASTELLI - SDWAN", "1386")},
    {"1387", Tuple.Create("SUC", "CATALINAS - SDWAN", "1387")},
    {"1388", Tuple.Create("SUC", "CATRIEL", "1388")},
    {"1389", Tuple.Create("ANEXO", "CENTRO DE PAGOS FLORENCIO VARELA (depende de 1785) - SDWAN", "1785")},
    {"1391", Tuple.Create("ANEXO", "CENTRO DE PAGOS GENERAL ROCA (depende de 1910) - SDWAN", "1910")},
    {"1392", Tuple.Create("ANEXO", "ANEXO OPERATIVO TIGRE (depende de 3480) - SDWAN", "3480")},
    {"1393", Tuple.Create("ANEXO", "CENTRO DE PAGOS GENERAL SAN MARTIN (depende de 1920) - SDWAN", "1920")},
    {"1394", Tuple.Create("ANEXO", "ANEXO OPERATIVO MERCEDES SAN LUIS (depende de 2430) - SDWAN", "2430")},
    {"1395", Tuple.Create("ANEXO", "CENTRO DE PAGOS RIVADAVIA (depende de 2990) - SDWAN", "2990")},
    {"1396", Tuple.Create("SUC", "CAUCETE - SDWAN", "1396")},
    {"1397", Tuple.Create("ANEXO", "CENTRO DE PAGOS GODOY CRUZ (depende de 1950)", "1950")},
    {"1398", Tuple.Create("ANEXO", "ANEXO OPERATIVO SAN SALVADOR DE JUJUY (depende de 3315) - SDWAN", "3315")},
    {"1399", Tuple.Create("ANEXO", "SAN LUIS (depende de 3240)", "3240")},
    {"1400", Tuple.Create("SUC", "CERES - SDWAN", "1400")},
    {"1401", Tuple.Create("ANEXO", "ANEXO OPERATIVO SAN JUAN (depende de 3200) - SDWAN", "3200")},
    {"1402", Tuple.Create("ANEXO", "CENTRO PYME LOMAS DE ZAMORA (depende de 2280)", "2280")},
    {"1403", Tuple.Create("ANEXO-BAJA", "CENTRO DE PAGOS MAR DEL PLATA (depende de 2350) DADO DE BAJA --> 4/2025", "2350")},
    {"1404", Tuple.Create("ANEXO", "CENTRO DE PAGOS SALTA (depende de 3070) - SDWAN", "3070")},
    {"1405", Tuple.Create("ANEXO", "CENTRO DE SERVICIOS MATE DE LUNA (depende de 3155)", "3155")},
    {"1406", Tuple.Create("ANEXO", "CENTRO DE PAGOS LA RIOJA (depende de 2200) - SDWAN", "2200")},
    {"1407", Tuple.Create("ANEXO", "CENTRO DE PAGOS MERLO (depende de 2432) - SDWAN", "2432")},
    {"1408", Tuple.Create("ANEXO", "CENTRO PYME QUILMES (depende de 2830)", "2830")},
    {"1409", Tuple.Create("ANEXO", "CENTRO DE PAGOS TUCUMAN (depende de 3265) - SDWAN", "3265")},
    {"1410", Tuple.Create("SUC", "CHACABUCO - SDWAN", "1410")},
    {"1420", Tuple.Create("SUC", "CHAJARI - SDWAN", "1420")},
    {"1425", Tuple.Create("SUC", "CHAMICAL - SDWAN", "1425")},
    {"1430", Tuple.Create("SUC", "CHA�AR LADEADO - SDWAN", "1430")},
    {"1440", Tuple.Create("SUC", "CHARATA - SDWAN", "1440")},
    {"1450", Tuple.Create("SUC", "CHASCOMUS - SDWAN", "1450")},
    {"1453", Tuple.Create("SUC", "CHEPES - SDWAN", "1453")},
    {"1460", Tuple.Create("SUC", "CHILECITO - SDWAN", "1460")},
    {"1470", Tuple.Create("SUC", "CHIVILCOY - SDWAN", "1470")},
    {"1480", Tuple.Create("SUC", "CHOELE CHOEL - SDWAN", "1480")},
    {"1485", Tuple.Create("ANEXO", "AGENCIA MOVIL CHUMBICHA (depende de 3155)", "3155")},
    {"1490", Tuple.Create("SUC", "CHOS MALAL - SDWAN", "1490")},
    {"1491", Tuple.Create("SUC", "CINCO SALTOS - SDWAN", "1491")},
    {"1495", Tuple.Create("SUC-CANX", "CIPOLLETTI - SDWAN", "1495")},
    {"1500", Tuple.Create("SUC", "CLORINDA - SDWAN", "1500")},
    {"1510", Tuple.Create("SUC", "COLON (B.A.) - SDWAN", "1510")},
    {"1520", Tuple.Create("SUC", "COLON (E.R.) - SDWAN", "1520")},
    {"1521", Tuple.Create("SUC", "COLONIA BARON SDWAN", "1521")},
    {"1525", Tuple.Create("SUC", "COLONIA J. J. CASTELLI - SDWAN", "1525")},
    {"1526", Tuple.Create("SUC", "COLONIAS UNIDAS - SDWAN", "1526")},
    {"1528", Tuple.Create("SUC", "CMTE. N. OTAMENDI - SDWAN", "1528")},
    {"1530", Tuple.Create("SUC-ZNL", "COMODORO RIVADAVIA (ZONAL) - SDWAN", "1530")},
    {"1532", Tuple.Create("SUC", "COMODORO RIVADAVIA SUR - SDWAN", "1532")},
    {"1542", Tuple.Create("SUC", "CONCEPCION - SDWAN", "1542")},
    {"1550", Tuple.Create("SUC", "CONCEPCION DEL URUGUAY - SDWAN", "1550")},
    {"1560", Tuple.Create("SUC-ZNL", "CONCORDIA (ZONAL) - SDWAN", "1560")},
    {"1570", Tuple.Create("SUC-CANX-ZNL-RGN", "CORDOBA (ZONAL REGIONAL) - SDWAN", "1570")},
    {"1572", Tuple.Create("SUC", "COMANDANTE LUIS PIEDRA BUENA - SDWAN", "1572")},
    {"1573", Tuple.Create("ANEXO", "CENTRO PYME MARTINEZ (depende de 9209) - SDWAN (DADO DE BAJA 05/09/2025)", "9209")},
    {"1580", Tuple.Create("SUC", "CORONDA - SDWAN", "1580")},
    {"1590", Tuple.Create("SUC", "CORONEL DORREGO - SDWAN", "1590")},
    {"1600", Tuple.Create("SUC", "CORONEL GRANADA", "1600")},
    {"1605", Tuple.Create("SUC", "CORONEL MOLDES - SDWAN", "1605")},
    {"1610", Tuple.Create("SUC", "CORONEL PRINGLES - SDWAN", "1610")},
    {"1620", Tuple.Create("SUC", "CORONEL SUAREZ - SDWAN", "1620")},
    {"1627", Tuple.Create("SUC", "CORRAL DE BUSTOS-IFFLINGER - SDWAN", "1627")},
    {"1630", Tuple.Create("SUC-ZNL", "CORRIENTES (ZONAL) - SDWAN", "1630")},
    {"1635", Tuple.Create("SUC", "COSQUIN SDWAN", "1635")},
    {"1636", Tuple.Create("SUC", "COSTA DE ARAUJO", "1636")},
    {"1638", Tuple.Create("SUC", "CRESPO - SDWAN", "1638")},
    {"1640", Tuple.Create("SUC", "CRUZ DEL EJE - SDWAN", "1640")},
    {"1650", Tuple.Create("SUC", "CURUZU CUATIA - SDWAN", "1650")},
    {"1652", Tuple.Create("SUC-CANX", "CUTRAL CO - SDWAN", "1652")},
    {"1653", Tuple.Create("SUC", "DAIREAUX - SDWAN", "1653")},
    {"1654", Tuple.Create("SUC", "DARDO ROCHA - SDWAN", "1654")},
    {"1655", Tuple.Create("SUC", "DARREGUEIRA - SDWAN", "1655")},
    {"1660", Tuple.Create("SUC", "DEAN FUNES - SDWAN", "1660")},
    {"1670", Tuple.Create("SUC", "DIAMANTE - SDWAN", "1670")},
    {"1675", Tuple.Create("ANEXO", "DGR PCIA. DE CATAMARCA (depende de 3155)", "3155")},
    {"1680", Tuple.Create("SUC-ZNL", "DOLORES (ZONAL) - SDWAN", "1680")},
    {"1690", Tuple.Create("SUC", "EDUARDO CASTEX - SDWAN", "1690")},
    {"1695", Tuple.Create("SUC", "EL BOLSON - SDWAN", "1695")},
    {"1697", Tuple.Create("SUC", "EL CALAFATE", "1697")},
    {"1698", Tuple.Create("SUC", "EL COLORADO - SDWAN", "1698")},
    {"1700", Tuple.Create("SUC", "EL DORADO - SDWAN", "1700")},
    {"1703", Tuple.Create("SUC", "ELENA - SDWAN", "1703")},
    {"1705", Tuple.Create("SUC", "EL MILAGRO - SDWAN", "1705")},
    {"1707", Tuple.Create("SUC", "EL PALOMAR - SDWAN", "1707")},
    {"1710", Tuple.Create("SUC", "EL TIO - SDWAN", "1710")},
    {"1712", Tuple.Create("SUC", "EL TREBOL - SDWAN", "1712")},
    {"1720", Tuple.Create("SUC", "ENSENADA - SDWAN", "1720")},
    {"1730", Tuple.Create("SUC", "ESPERANZA - SDWAN", "1730")},
    {"1740", Tuple.Create("SUC", "ESQUEL - SDWAN", "1740")},
    {"1750", Tuple.Create("SUC", "ESQUINA (CTES.) - SDWAN", "1750")},
    {"1751", Tuple.Create("SUC", "ESQUINA NORTE - SDWAN", "1751")},
    {"1753", Tuple.Create("SUC", "EUGENIO BUSTOS - SDWAN", "1753")},
    {"1756", Tuple.Create("SUC", "FAMAILLA - SDWAN", "1756")},
    {"1758", Tuple.Create("SUC", "FAMATINA - SDWAN", "1758")},
    {"1760", Tuple.Create("SUC", "FEDERACION - SDWAN", "1760")},
    {"1762", Tuple.Create("SUC", "FEDERAL - SDWAN", "1762")},
    {"1770", Tuple.Create("SUC", "FERNANDEZ - SDWAN", "1770")},
    {"1780", Tuple.Create("SUC", "FIRMAT - SDWAN", "1780")},
    {"1781", Tuple.Create("ANEXO-BAJA", "AV SANTA FE  (depende de 1780)  -  Dado de BAJA 10/2025", "1780")},
    {"1785", Tuple.Create("SUC-CANX", "FLORENCIO VARELA - SDWAN", "1785")},
    {"1790", Tuple.Create("SUC-CANX", "FORMOSA - SDWAN", "1790")},
    {"1795", Tuple.Create("SUC", "FRAY MAMERTO ESQUIU - SDWAN", "1795")},
    {"1800", Tuple.Create("SUC", "FRIAS - SDWAN", "1800")},
    {"1805", Tuple.Create("SUC", "GENERAL DEHEZA - SDWAN", "1805")},
    {"1810", Tuple.Create("SUC", "GALVEZ - SDWAN", "1810")},
    {"1820", Tuple.Create("SUC", "GENERAL ACHA - SDWAN", "1820")},
    {"1830", Tuple.Create("SUC-CANX", "GENERAL ALVEAR - SDWAN", "1830")},
    {"1840", Tuple.Create("SUC", "GENERAL ARENALES - SDWAN", "1840")},
    {"1850", Tuple.Create("SUC", "GENERAL BELGRANO - SDWAN", "1850")},
    {"1860", Tuple.Create("SUC", "GENERAL CABRERA - SDWAN", "1860")},
    {"1862", Tuple.Create("SUC", "GENERAL CONESA - SDWAN", "1862")},
    {"1865", Tuple.Create("SUC", "GENERAL JOSE DE SAN MARTIN (CHACO) - SDWAN", "1865")},
    {"1870", Tuple.Create("SUC", "GENERAL LAMADRID - SDWAN", "1870")},
    {"1871", Tuple.Create("SUC", "GENERAL LEVALLE - SDWAN", "1871")},
    {"1880", Tuple.Create("SUC", "GENERAL MADARIAGA - SDWAN", "1880")},
    {"1882", Tuple.Create("SUC", "GENERAL M. M. DE GUEMES - SDWAN", "1882")},
    {"1887", Tuple.Create("SUC", "GENERAL PACHECO - SDWAN", "1887")},
    {"1890", Tuple.Create("SUC", "GENERAL PAZ - SDWAN", "1890")},
    {"1900", Tuple.Create("SUC", "GENERAL PICO - SDWAN", "1900")},
    {"1905", Tuple.Create("SUC", "GENERAL PIRAN - SDWAN", "1905")},
    {"1910", Tuple.Create("SUC-CANX", "GENERAL ROCA - SDWAN", "1910")},
    {"1913", Tuple.Create("SUC", "GENERAL RODRIGUEZ (BS. AS.) - SDWAN", "1913")},
    {"1920", Tuple.Create("SUC-CANX", "GENERAL SAN MARTIN (MZA) - SDWAN", "1920")},
    {"1925", Tuple.Create("SUC", "HURLINGHAM - SDWAN", "1925")},
    {"1928", Tuple.Create("ANEXO", "DEPENDENCIA EN EMPRESA INSTITUTO AYUDA FINANCIERA - IAF (depende de 0050)", "50")},
    {"1930", Tuple.Create("SUC", "LOS TOLDOS - SDWAN", "1930")},
    {"1940", Tuple.Create("SUC", "GENERAL VILLEGAS - SDWAN", "1940")},
    {"1942", Tuple.Create("SUC", "GRAND BOURG - SDWAN", "1942")},
    {"1943", Tuple.Create("SUC", "GOBERNADOR CRESPO - SDWAN", "1943")},
    {"1945", Tuple.Create("SUC", "GOBERNADOR ROCA - SDWAN", "1945")},
    {"1947", Tuple.Create("SUC", "GOBERNADOR VALENTIN VIRASORO - SDWAN", "1947")},
    {"1950", Tuple.Create("SUC-CANX", "GODOY CRUZ - SDWAN", "1950")},
    {"1955", Tuple.Create("SUC", "GONZALEZ CATAN - SDWAN", "1955")},
    {"1960", Tuple.Create("SUC-CANX", "GOYA - SDWAN", "1960")},
    {"1965", Tuple.Create("SUC", "GREGORIO DE LAFERRERE - SDWAN", "1965")},
    {"1970", Tuple.Create("SUC", "GUALEGUAY - SDWAN", "1970")},
    {"1980", Tuple.Create("SUC", "GUALEGUAYCHU - SDWAN", "1980")},
    {"1982", Tuple.Create("SUC", "GUATRACHE - SDWAN", "1982")},
    {"1984", Tuple.Create("SUC", "HENDERSON", "1984")},
    {"1985", Tuple.Create("SUC", "HERNANDO - SDWAN", "1985")},
    {"1987", Tuple.Create("ANEXO-BAJA", "CENTRO PYME SAN MIGUEL (depende de 3260)  -  Dado de BAJA  1/8/2025", "3260")},
    {"1990", Tuple.Create("SUC", "HUANGUELEN - SDWAN", "1990")},
    {"1992", Tuple.Create("SUC", "HUMBERTO PRIMERO - SDWAN", "1992")},
    {"2000", Tuple.Create("SUC", "IBARRETA - SDWAN", "2000")},
    {"2005", Tuple.Create("ANEXO", "AGENCIA MOVIL ICA�O (depende de 2903) (OPERA MARTES Y JUEVES)", "2903")},
    {"2010", Tuple.Create("SUC", "INDIO RICO", "2010")},
    {"2020", Tuple.Create("SUC", "INGENIERO JACOBACCI - SDWAN", "2020")},
    {"2030", Tuple.Create("SUC", "INGENIERO LUIGGI - SDWAN", "2030")},
    {"2040", Tuple.Create("SUC", "INTENDENTE ALVEAR - SDWAN", "2040")},
    {"2045", Tuple.Create("SUC-CANX", "MARIO GERMAN CHAVEZ TORREZ - SDWAN", "2045")},
    {"2050", Tuple.Create("SUC", "ISLA VERDE - SDWAN", "2050")},
    {"2060", Tuple.Create("SUC", "JACHAL - SDWAN", "2060")},
    {"2065", Tuple.Create("SUC", "JARDIN AMERICA - SDWAN", "2065")},
    {"2070", Tuple.Create("SUC", "JESUS MARIA - SDWAN", "2070")},
    {"2072", Tuple.Create("SUC", "J. V. GONZALEZ - SDWAN", "2072")},
    {"2075", Tuple.Create("SUC", "JOSE C. PAZ - SDWAN", "2075")},
    {"2077", Tuple.Create("SUC", "JOSE V. ZAPATA - SDWAN", "2077")},
    {"2080", Tuple.Create("SUC", "JUAN B. ALBERDI (BS.AS.)", "2080")},
    {"2081", Tuple.Create("SUC", "JUAN B. ALBERDI (TUCUMAN) - SDWAN", "2081")},
    {"2084", Tuple.Create("SUC", "JUAN COUSTE", "2084")},
    {"2090", Tuple.Create("SUC", "JUAN N. FERNANDEZ - SDWAN", "2090")},
    {"2119", Tuple.Create("SUC", "JUNIN (MZA) - SDWAN", "2119")},
    {"2120", Tuple.Create("SUC-ZNL-RGN", "JUNIN (ZONAL REGIONAL) - SDWAN", "2120")},
    {"2121", Tuple.Create("SUC", "JUSTO DARACT - SDWAN", "2121")},
    {"2125", Tuple.Create("SUC", "LA BANDA - SDWAN", "2125")},
    {"2130", Tuple.Create("SUC", "LABOULAYE - SDWAN", "2130")},
    {"2140", Tuple.Create("SUC", "LA CARLOTA - SDWAN", "2140")},
    {"2141", Tuple.Create("SUC", "LA CIUDADELA - SDWAN", "2141")},
    {"2142", Tuple.Create("SUC", "LA CONSULTA", "2142")},
    {"2143", Tuple.Create("SUC", "LA CUMBRE - SDWAN", "2143")},
    {"2144", Tuple.Create("SUC", "LA DORMIDA - SDWAN", "2144")},
    {"2145", Tuple.Create("SUC", "LA FALDA - SDWAN", "2145")},
    {"2146", Tuple.Create("SUC", "LAGUNA LARGA - SDWAN", "2146")},
    {"2149", Tuple.Create("SUC", "LANUS ESTE - SDWAN", "2149")},
    {"2150", Tuple.Create("SUC", "LANUS OESTE - SDWAN", "2150")},
    {"2160", Tuple.Create("SUC", "LA PAZ (ER) - SDWAN", "2160")},
    {"2162", Tuple.Create("SUC", "LA PAZ (MZA) - SDWAN", "2162")},
    {"2170", Tuple.Create("SUC-CANX-ZNL", "LA PLATA (ZONAL) - SDWAN", "2170")},
    {"2172", Tuple.Create("SUC", "LA PLAYOSA", "2172")},
    {"2180", Tuple.Create("SUC", "LAPRIDA - SDWAN", "2180")},
    {"2186", Tuple.Create("SUC", "LA PUERTA - SDWAN", "2186")},
    {"2190", Tuple.Create("SUC", "LA QUIACA - SDWAN", "2190")},
    {"2197", Tuple.Create("ANEXO", "PPP Ministerio de Agricultura (depende de 0046)", "46")},
    {"2200", Tuple.Create("SUC-CANX", "LA RIOJA - SDWAN", "2200")},
    {"2205", Tuple.Create("SUC", "LAS BRE�AS - SDWAN", "2205")},
    {"2207", Tuple.Create("SUC", "LAS CATITAS - VDI - SDWAN", "2207")},
    {"2210", Tuple.Create("SUC", "LAS FLORES - SDWAN", "2210")},
    {"2213", Tuple.Create("SUC-CANX", "LAS HERAS (MZA) - SDWAN", "2213")},
    {"2215", Tuple.Create("SUC", "LAS HERAS (SC)", "2215")},
    {"2216", Tuple.Create("SUC", "LAS PAREJAS - SDWAN", "2216")},
    {"2217", Tuple.Create("SUC", "LA LEONESA - SDWAN", "2217")},
    {"2218", Tuple.Create("SUC", "LAS PE�AS - SDWAN", "2218")},
    {"2220", Tuple.Create("SUC", "LAS ROSAS - SDWAN", "2220")},
    {"2230", Tuple.Create("SUC", "LAS VARILLAS - SDWAN", "2230")},
    {"2235", Tuple.Create("SUC", "LAVALLE (MZA) SDWAN", "2235")},
    {"2240", Tuple.Create("SUC", "LEANDRO N. ALEM - SDWAN", "2240")},
    {"2242", Tuple.Create("SUC", "LEONES - SDWAN", "2242")},
    {"2246", Tuple.Create("SUC", "LIB. GENERAL SAN MARTIN (JUJUY)", "2246")},
    {"2249", Tuple.Create("SUC", "LIMA", "2249")},
    {"2250", Tuple.Create("SUC", "LINCOLN - SDWAN", "2250")},
    {"2260", Tuple.Create("SUC", "LOBERIA - SDWAN", "2260")},
    {"2270", Tuple.Create("SUC", "LOBOS - SDWAN", "2270")},
    {"2278", Tuple.Create("SUC", "LOMA NEGRA", "2278")},
    {"2280", Tuple.Create("SUC-CANX", "LOMAS DE ZAMORA - SDWAN", "2280")},
    {"2283", Tuple.Create("SUC", "LOS ALTOS", "2283")},
    {"2284", Tuple.Create("SUC", "LOS CORRALITOS", "2284")},
    {"2285", Tuple.Create("SUC", "LOS SURGENTES - SDWAN", "2285")},
    {"2290", Tuple.Create("SUC", "LUCAS GONZALEZ - SDWAN", "2290")},
    {"2300", Tuple.Create("SUC-CANX", "LUJAN - SDWAN", "2300")},
    {"2305", Tuple.Create("SUC", "LUJAN DE CUYO - SDWAN", "2305")},
    {"2310", Tuple.Create("SUC", "LULES - SDWAN", "2310")},
    {"2315", Tuple.Create("SUC", "LLAVALLOL - SDWAN", "2315")},
    {"2320", Tuple.Create("SUC", "MACACHIN - SDWAN", "2320")},
    {"2323", Tuple.Create("SUC", "MACHAGAI - SDWAN", "2323")},
    {"2325", Tuple.Create("SUC", "MAIPU (B.A.) - SDWAN", "2325")},
    {"2327", Tuple.Create("SUC-CANX", "MAIPU (MZA) - SDWAN", "2327")},
    {"2330", Tuple.Create("SUC", "MALARG�E - SDWAN", "2330")},
    {"2340", Tuple.Create("SUC", "MARCOS JUAREZ - SDWAN", "2340")},
    {"2345", Tuple.Create("SUC", "MARCOS PAZ - SDWAN", "2345")},
    {"2350", Tuple.Create("SUC-CANX-ZNL-RGN", "MAR DEL PLATA (ZONAL REGIONAL) - SDWAN", "2350")},
    {"2360", Tuple.Create("SUC", "MARIA GRANDE - SDWAN", "2360")},
    {"2370", Tuple.Create("SUC", "MARIA IGNACIA (EST. VELA) - SDWAN", "2370")},
    {"2380", Tuple.Create("SUC", "MARIA TERESA - SDWAN", "2380")},
    {"2383", Tuple.Create("SUC", "MATTALDI", "2383")},
    {"2385", Tuple.Create("SUC", "MEDRANO", "2385")},
    {"2390", Tuple.Create("SUC", "MELINCUE", "2390")},
    {"2400", Tuple.Create("SUC-CANX-ZNL", "MENDOZA (ZONAL MENDOZA OESTE) - SDWAN", "2400")},
    {"2405", Tuple.Create("SUC-CANX-ZNL-RGN", "EJERCITO DE LOS ANDES  (ZONAL REGIONAL MENDOZA ESTE)", "2405")},
    {"2407", Tuple.Create("SUC", "MERCADO CENTRAL DE BUENOS AIRES - SDWAN", "2407")},
    {"2410", Tuple.Create("SUC", "MERCEDES (B. A.) - SDWAN", "2410")},
    {"2420", Tuple.Create("SUC", "MERCEDES (CTES) - SDWAN", "2420")},
    {"2430", Tuple.Create("SUC-CANX", "MERCEDES SAN LUIS - SDWAN", "2430")},
    {"2432", Tuple.Create("SUC-CANX", "MERLO (B. A.) - SDWAN", "2432")},
    {"2433", Tuple.Create("SUC", "MERLO (S. L.) - SDWAN", "2433")},
    {"2439", Tuple.Create("SUC", "METRO MAX SAN LUIS - SDWAN", "2439")},
    {"2440", Tuple.Create("SUC", "METAN - SDWAN", "2440")},
    {"2441", Tuple.Create("SUC", "MINA CLAVERO - SDWAN", "2441")},
    {"2445", Tuple.Create("SUC", "MIRAMAR - SDWAN", "2445")},
    {"2450", Tuple.Create("SUC", "MOISES VILLE - SDWAN", "2450")},
    {"2458", Tuple.Create("SUC", "MONES CAZON", "2458")},
    {"2460", Tuple.Create("SUC", "MONTE - SDWAN", "2460")},
    {"2465", Tuple.Create("SUC", "MONTE BUEY - SDWAN", "2465")},
    {"2466", Tuple.Create("SUC", "MONTECARLO - SDWAN", "2466")},
    {"2470", Tuple.Create("SUC", "MONTE CASEROS (CTES.) - SDWAN", "2470")},
    {"2472", Tuple.Create("SUC-CANX", "MONTE CRISTO - SDWAN", "2472")},
    {"2480", Tuple.Create("SUC", "MONTE GRANDE - SDWAN", "2480")},
    {"2490", Tuple.Create("SUC", "MONTEROS - SDWAN", "2490")},
    {"2495", Tuple.Create("SUC-CANX", "MORENO - SDWAN", "2495")},
    {"2500", Tuple.Create("SUC-CANX", "MORON - SDWAN", "2500")},
    {"2502", Tuple.Create("SUC", "MORRISON - SDWAN", "2502")},
    {"2504", Tuple.Create("SUC", "MORTEROS - SDWAN", "2504")},
    {"2507", Tuple.Create("SUC", "MUNRO - SDWAN", "2507")},
    {"2510", Tuple.Create("SUC", "NAVARRO - SDWAN", "2510")},
    {"2520", Tuple.Create("SUC", "NECOCHEA - SDWAN", "2520")},
    {"2530", Tuple.Create("SUC", "NICANOR OLIVERA", "2530")},
    {"2540", Tuple.Create("SUC-CANX-ZNL", "NEUQUEN (ZONAL) - SDWAN", "2540")},
    {"2545", Tuple.Create("SUC", "NEUQUEN OESTE - SDWAN", "2545")},
    {"2550", Tuple.Create("SUC", "NOETINGER - SDWAN", "2550")},
    {"2560", Tuple.Create("SUC", "NOGOYA", "2560")},
    {"2570", Tuple.Create("SUC", "NORBERTO DE LA RIESTRA", "2570")},
    {"2580", Tuple.Create("SUC", "NUEVE DE JULIO - SDWAN", "2580")},
    {"2590", Tuple.Create("SUC", "OBERA - SDWAN", "2590")},
    {"2600", Tuple.Create("SUC-CANX", "OLAVARRIA - SDWAN", "2600")},
    {"2605", Tuple.Create("SUC", "QUINTA SECCION", "2605")},
    {"2610", Tuple.Create("SUC", "OLIVA - SDWAN", "2610")},
    {"2620", Tuple.Create("SUC", "OLIVOS - SDWAN", "2620")},
    {"2621", Tuple.Create("SUC", "OLTA", "2621")},
    {"2622", Tuple.Create("SUC", "ONCATIVO", "2622")},
    {"2623", Tuple.Create("SUC", "ORDO�EZ - SDWAN", "2623")},
    {"2635", Tuple.Create("SUC", "ORENSE", "2635")},
    {"2640", Tuple.Create("SUC", "ORIENTE", "2640")},
    {"2645", Tuple.Create("SUC", "PALMARES - SDWAN", "2645")},
    {"2646", Tuple.Create("SUC", "PAMPA DEL INFIERNO - SDWAN", "2646")},
    {"2650", Tuple.Create("SUC-CANX-ZNL-RGN", "PARANA (ZONAL REGIONAL) - SDWAN", "2650")},
    {"2655", Tuple.Create("SUC", "PASCANAS - SDWAN", "2655")},
    {"2660", Tuple.Create("SUC", "PASO DE LOS LIBRES - SDWAN", "2660")},
    {"2662", Tuple.Create("SUC", "PASO DEL BOSQUE - SDWAN", "2662")},
    {"2670", Tuple.Create("SUC", "PATAGONES - SDWAN", "2670")},
    {"2680", Tuple.Create("SUC", "PEDRO LURO - SDWAN", "2680")},
    {"2690", Tuple.Create("SUC", "PEHUAJO - SDWAN", "2690")},
    {"2692", Tuple.Create("SUC", "PEREZ MILLAN", "2692")},
    {"2700", Tuple.Create("SUC-CANX-ZNL", "PERGAMINO (ZONAL) - SDWAN", "2700")},
    {"2701", Tuple.Create("SUC", "PERITO MORENO - SDWAN", "2701")},
    {"2702", Tuple.Create("SUC", "PERICO - SDWAN", "2702")},
    {"2705", Tuple.Create("SUC", "PICO TRUNCADO - SDWAN", "2705")},
    {"2709", Tuple.Create("SUC", "PILAR (B. A.) - SDWAN", "2709")},
    {"2710", Tuple.Create("SUC", "PIGUE - SDWAN", "2710")},
    {"2711", Tuple.Create("SUC", "PILAR (STA. FE) - SDWAN", "2711")},
    {"2712", Tuple.Create("SUC", "PINAMAR - SDWAN", "2712")},
    {"2713", Tuple.Create("SUC", "PIRANE", "2713")},
    {"2717", Tuple.Create("SUC", "PLAZOLETA MITRE - SDWAN", "2717")},
    {"2718", Tuple.Create("SUC-CANX", "POMAN - SDWAN", "2718")},
    {"2719", Tuple.Create("SUC", "PLOTTIER -SDWAN", "2719")},
    {"2720", Tuple.Create("SUC-CANX-ZNL", "POSADAS (ZONAL) - SDWAN", "2720")},
    {"2730", Tuple.Create("SUC", "PCIA. R. SAENZ PE�A - SDWAN", "2730")},
    {"2740", Tuple.Create("SUC", "PCIA. DE LA PLAZA - SDWAN", "2740")},
    {"2748", Tuple.Create("SUC", "PROGRESO", "2748")},
    {"2750", Tuple.Create("SUC", "PUAN - SDWAN", "2750")},
    {"2770", Tuple.Create("ANEXO-BAJA", "ANEXO OPERATIVO PUENTE INT. SANTO TOME-SAO BORJA (depende de 3380) Dado de BAJA 25/2/25", "3380")},
    {"2775", Tuple.Create("SUC", "FIAMBALA - SDWAN", "2775")},
    {"2780", Tuple.Create("SUC", "PUERTO DESEADO - SDWAN", "2780")},
    {"2782", Tuple.Create("SUC", "PUERTO ESPERANZA - SDWAN", "2782")},
    {"2784", Tuple.Create("SUC", "PUERTO IGUAZU - SDWAN", "2784")},
    {"2790", Tuple.Create("SUC", "PUERTO MADRYN - SDWAN", "2790")},
    {"2800", Tuple.Create("SUC", "PUERTO RICO - SDWAN", "2800")},
    {"2805", Tuple.Create("SUC", "PUERTO GENERAL SAN MARTIN - SDWAN", "2805")},
    {"2808", Tuple.Create("ANEXO", "PPP TRISTAN SUAREZ (depende de 3587)", "3587")},
    {"2810", Tuple.Create("SUC", "PUNTA ALTA - SDWAN", "2810")},
    {"2820", Tuple.Create("SUC", "QUEMU QUEMU - SDWAN", "2820")},
    {"2830", Tuple.Create("SUC-CANX", "QUILMES - SDWAN", "2830")},
    {"2832", Tuple.Create("SUC", "QUIMILI - SDWAN", "2832")},
    {"2835", Tuple.Create("SUC", "QUIROGA - SDWAN", "2835")},
    {"2840", Tuple.Create("SUC", "QUITILIPI - SDWAN", "2840")},
    {"2850", Tuple.Create("SUC", "RAFAELA - SDWAN", "2850")},
    {"2860", Tuple.Create("SUC", "RAMIREZ - SDWAN", "2860")},
    {"2870", Tuple.Create("SUC-BAJA", "RAMOS MEJIA       ---- DADO DE BAJA 25/4/2025 -----", "2870")},
    {"2880", Tuple.Create("SUC", "RAUCH - SDWAN", "2880")},
    {"2885", Tuple.Create("SUC", "RAWSON - SDWAN", "2885")},
    {"2887", Tuple.Create("SUC", "REAL DEL PADRE - SDWAN", "2887")},
    {"2890", Tuple.Create("SUC", "REALICO - SDWAN", "2890")},
    {"2900", Tuple.Create("SUC-CANX-ZNL", "RECONQUISTA (ZONAL) - SDWAN", "2900")},
    {"2903", Tuple.Create("SUC-CANX", "RECREO", "2903")},
    {"2905", Tuple.Create("SUC", "REMEDIOS DE ESCALADA - SDWAN", "2905")},
    {"2910", Tuple.Create("SUC-ZNL", "RESISTENCIA (ZONAL) - SDWAN", "2910")},
    {"2920", Tuple.Create("SUC", "RIO COLORADO - SDWAN", "2920")},
    {"2930", Tuple.Create("SUC-CANX-ZNL", "RIO CUARTO (ZONAL) - SDWAN", "2930")},
    {"2940", Tuple.Create("SUC", "RIO GALLEGOS - SDWAN", "2940")},
    {"2950", Tuple.Create("SUC", "RIO GRANDE - SDWAN", "2950")},
    {"2960", Tuple.Create("SUC", "RIO SEGUNDO - SDWAN", "2960")},
    {"2970", Tuple.Create("SUC", "RIO TERCERO - SDWAN", "2970")},
    {"2980", Tuple.Create("SUC", "AMERICA (BA) - SDWAN", "2980")},
    {"2990", Tuple.Create("SUC-CANX", "RIVADAVIA - SDWAN", "2990")},
    {"2992", Tuple.Create("SUC", "RIVERA - SDWAN", "2992")},
    {"2995", Tuple.Create("SUC", "ROBERTS", "2995")},
    {"3000", Tuple.Create("SUC", "ROJAS - SDWAN", "3000")},
    {"3002", Tuple.Create("SUC", "ROLDAN - SDWAN", "3002")},
    {"3010", Tuple.Create("SUC", "ROQUE PEREZ - SDWAN", "3010")},
    {"3020", Tuple.Create("SUC-CANX-ZNL-RGN", "ROSARIO (ZONAL REGIONAL) - SDWAN", "3020")},
    {"3025", Tuple.Create("SUC", "ROSARIO DE LA FRONTERA - SDWAN", "3025")},
    {"3026", Tuple.Create("SUC", "ROSARIO DE LERMA - SDWAN", "3026")},
    {"3030", Tuple.Create("SUC", "ROSARIO DEL TALA - SDWAN", "3030")},
    {"3035", Tuple.Create("SUC", "ROSARIO SUD - SDWAN", "3035")},
    {"3040", Tuple.Create("SUC", "RUFINO -  SDWAN", "3040")},
    {"3050", Tuple.Create("SUC", "SALADAS - SDWAN", "3050")},
    {"3060", Tuple.Create("SUC", "SALADILLO - SDWAN", "3060")},
    {"3065", Tuple.Create("SUC", "SALICAS - SDWAN", "3065")},
    {"3070", Tuple.Create("SUC-CANX-ZNL", "SALTA (ZONAL) - SDWAN", "3070")},
    {"3080", Tuple.Create("SUC", "SALTO - SDWAN", "3080")},
    {"3090", Tuple.Create("SUC", "SALLIQUELO - SDWAN", "3090")},
    {"3092", Tuple.Create("SUC", "SAMPACHO", "3092")},
    {"3095", Tuple.Create("SUC", "SAN ANDRES DE GILES - SDWAN", "3095")},
    {"3097", Tuple.Create("SUC", "SAN ANTONIO DE ARECO - SDWAN", "3097")},
    {"3098", Tuple.Create("SUC", "SAN ANTONIO DE LITIN - SDWAN", "3098")},
    {"3100", Tuple.Create("SUC", "SAN ANTONIO OESTE - SDWAN", "3100")},
    {"3110", Tuple.Create("SUC", "SAN CARLOS CENTRO - SDWAN", "3110")},
    {"3120", Tuple.Create("SUC-CANX", "SAN CARLOS DE BARILOCHE - SDWAN", "3120")},
    {"3130", Tuple.Create("SUC", "SAN CAYETANO", "3130")},
    {"3140", Tuple.Create("SUC", "SAN CRISTOBAL (STA. FE) - SDWAN", "3140")},
    {"3142", Tuple.Create("SUC", "SANCTI SPIRITU", "3142")},
    {"3150", Tuple.Create("SUC", "SAN FERNANDO - SDWAN", "3150")},
    {"3155", Tuple.Create("SUC-CANX-ZNL", "S. F. DEL V. DE CATAMARCA  (ZONAL)", "3155")},
    {"3155-ZONAL", Tuple.Create("EDIF-ANX", "ZONAL CATAMARCA EN SEDE DE SUC FRAY MAMERTO ESQUIU (depende de 3155)", "3155")},
    {"3160", Tuple.Create("SUC-CANX-ZNL", "SAN FRANCISCO (ZONAL) - SDWAN", "3160")},
    {"3165", Tuple.Create("SUC", "SAN GENARO - SDWAN", "3165")},
    {"3166", Tuple.Create("SUC", "SAN GUILLERMO - SDWAN", "3166")},
    {"3167", Tuple.Create("SUC-CANX-ZNL", "SAN ISIDRO (ZONAL) - SDWAN", "3167")},
    {"3170", Tuple.Create("SUC", "SAN JAVIER (S. F.) - SDWAN", "3170")},
    {"3175", Tuple.Create("SUC", "SAN JAVIER (MNES.) - SDWAN", "3175")},
    {"3177", Tuple.Create("SUC", "SAN JORGE - SDWAN", "3177")},
    {"3180", Tuple.Create("SUC", "SAN JOSE DE FELICIANO - SDWAN", "3180")},
    {"3190", Tuple.Create("SUC", "SAN JOSE DE LA ESQUINA - SDWAN", "3190")},
    {"3200", Tuple.Create("SUC-CANX-ZNL", "SAN JUAN (ZONAL) - SDWAN", "3200")},
    {"3210", Tuple.Create("SUC", "SAN JULIAN SDWAN", "3210")},
    {"3219", Tuple.Create("SUC", "SAN JUSTO (B. A.) - SDWAN", "3219")},
    {"3220", Tuple.Create("SUC", "SAN JUSTO (S. F.) - SDWAN", "3220")},
    {"3230", Tuple.Create("SUC", "SAN LORENZO - SDWAN", "3230")},
    {"3240", Tuple.Create("SUC-CANX", "SAN LUIS - SDWAN", "3240")},
    {"3242", Tuple.Create("SUC", "SAN LUIS DEL PALMAR - SDWAN", "3242")},
    {"3245", Tuple.Create("SUC-CANX", "SAN MARTIN (B. A.) - SDWAN", "3245")},
    {"3250", Tuple.Create("SUC", "SAN MARTIN DE LOS ANDES", "3250")},
    {"3260", Tuple.Create("SUC-CANX", "SAN MIGUEL (B. A) - SDWAN", "3260")},
    {"3265", Tuple.Create("SUC-CANX-ZNL-RGN", "SAN M. TUCUMAN (ZONAL REGIONAL) - SDWAN", "3265")},
    {"3270", Tuple.Create("SUC-CANX", "SAN NICOLAS - SDWAN", "3270")},
    {"3280", Tuple.Create("SUC", "SAN PEDRO - SDWAN", "3280")},
    {"3290", Tuple.Create("SUC", "SAN PEDRO DE JUJUY - SDWAN", "3290")},
    {"3300", Tuple.Create("SUC-CANX-ZNL", "SAN RAFAEL (ZONAL) - SDWAN", "3300")},
    {"3305", Tuple.Create("SUC-CANX", "SAN RAM�N DE LA NUEVA OR�N - SDWAN", "3305")},
    {"3310", Tuple.Create("SUC", "SAN SALVADOR - SDWAN", "3310")},
    {"3315", Tuple.Create("SUC-CANX", "SAN SALVADOR DE JUJUY - SDWAN", "3315")},
    {"3316", Tuple.Create("SUC", "SAN VICENTE (MISIONES) - SDWAN", "3316")},
    {"3317", Tuple.Create("SUC", "SAN VICENTE", "3317")},
    {"3318", Tuple.Create("SUC", "SAN VICENTE (BA) - SDWAN", "3318")},
    {"3320", Tuple.Create("SUC", "SANTA CRUZ - SDWAN", "3320")},
    {"3325", Tuple.Create("SUC", "SANTA CLARA DEL MAR - SDWAN", "3325")},
    {"3330", Tuple.Create("SUC-CANX-ZNL", "SANTA FE (ZONAL) - SDWAN", "3330")},
    {"3340", Tuple.Create("SUC", "SANTA LUCIA - SDWAN", "3340")},
    {"3343", Tuple.Create("SUC", "SANTA MARIA - SDWAN", "3343")},
    {"3350", Tuple.Create("SUC-ZNL", "SANTA ROSA (ZONAL) - SDWAN", "3350")},
    {"3352", Tuple.Create("SUC", "SANTA ROSA (MZA) - SDWAN", "3352")},
    {"3353", Tuple.Create("SUC", "SANTA ROSA DE RIO PRIMERO - SDWAN", "3353")},
    {"3358", Tuple.Create("SUC", "SANTA SYLVINA", "3358")},
    {"3360", Tuple.Create("SUC", "SANTA TERESA - SDWAN", "3360")},
    {"3362", Tuple.Create("SUC", "SANTA TERESITA - SDWAN", "3362")},
    {"3370", Tuple.Create("SUC-ZNL", "SANTIAGO DEL ESTERO (ZONAL) - SDWAN", "3370")},
    {"3380", Tuple.Create("SUC-CANX", "SANTO TOME (CTES)", "3380")},
    {"3385", Tuple.Create("SUC", "SARANDI - SDWAN", "3385")},
    {"3390", Tuple.Create("SUC", "SARMIENTO - SDWAN", "3390")},
    {"3400", Tuple.Create("SUC", "SASTRE - SDWAN", "3400")},
    {"3409", Tuple.Create("SUC", "STROEDER - SDWAN", "3409")},
    {"3410", Tuple.Create("SUC", "SAUCE (CTES.) - SDWAN", "3410")},
    {"3412", Tuple.Create("ANEXO", "AGENCIA MOVIL SAUJIL (depende de 2718) abre Lun, Mi�r, Juev y Vier. - SDWAN", "2718")},
    {"3415", Tuple.Create("SUC", "SIERRA GRANDE - SDWAN", "3415")},
    {"3418", Tuple.Create("ANEXO-BAJA", "SUCURSAL ELECTRONICA AZUL (Depende de 1120)  -  Dado de BAJA  10/2025", "1120")},
    {"3419", Tuple.Create("ANEXO-BAJA", "SUCURSAL ELECTRONICA JARDIN ESPINOZA (depende de 1570) - SDWAN", "1570")},
    {"3420", Tuple.Create("SUC", "SUARDI", "3420")},
    {"3421", Tuple.Create("ANEXO", "SUCURSAL ELECTRONICA CATAMARCA NORTE (depende de 3155)", "3155")},
    {"3422", Tuple.Create("ANEXO", "SUCURSAL ELECTRONICA CATAMARCA SUR (depende de 3155)", "3155")},
    {"3423", Tuple.Create("ANEXO-BAJA", "SUCURSAL ELECTRONICA NORDELTA (depende de 3480)", "3480")},
    {"3424", Tuple.Create("ANEXO", "SUCURSAL ELECTRONICA NEUQUEN (depende de 2540) - SDWAN", "2540")},
    {"3427", Tuple.Create("ANEXO-BAJA", "SUCURSAL ELECTRONICA ROSARIO (depende de 3020) - Dado de BAJA 31/7/24", "3020")},
    {"3428", Tuple.Create("ANEXO-BAJA", "SUCURSAL ELECTRONICA PLAZA SERRANO (depende de 0074)", "74")},
    {"3429", Tuple.Create("ANEXO", "SUCURSAL ELECTRONICA PERGAMINO (depende de 2700) - SDWAN", "2700")},
    {"3430", Tuple.Create("SUC", "SUNCHALES - SDWAN", "3430")},
    {"3431", Tuple.Create("ANEXO-BAJA", "SUCURSAL ELECTRONICA POSADAS (depende de 2720) - SDWAN", "2720")},
    {"3432", Tuple.Create("ANEXO", "SUCURSAL ELECTRONICA GUAYMALLEN (depende de 2400) - SDWAN", "2400")},
    {"3434", Tuple.Create("ANEXO-BAJA", "SUCURSAL ELECTRONICA RIO CUARTO (depende de 2930) (DADO DE BAJA 26/08/2025)", "2930")},
    {"3436", Tuple.Create("ANEXO-BAJA", "SUCURSAL ELECTRONICA SAN ISIDRO (depende de 3167) Dado de BAJA el 26/12/24", "3167")},
    {"3438", Tuple.Create("ANEXO", "SUCURSAL ELECTRONICA SAN SALVADOR DE JUJUY (Depende de 3315) - SDWAN", "3315")},
    {"3440", Tuple.Create("SUC", "TAFI VIEJO - SDWAN", "3440")},
    {"3441", Tuple.Create("ANEXO-BAJA", "SUCURSAL ELECTRONICA VILLA DEVOTO (depende de 0095) ## Dado de BAJA 27/12/24  ##", "95")},
    {"3442", Tuple.Create("ANEXO", "SUCURSAL ELECTRONICA PARANA (depende de 2650) - SDWAN", "2650")},
    {"3448", Tuple.Create("SUC", "TANCACHA - SDWAN", "3448")},
    {"3450", Tuple.Create("SUC-CANX", "TANDIL - SDWAN", "3450")},
    {"3460", Tuple.Create("SUC", "TAPALQUE", "3460")},
    {"3470", Tuple.Create("SUC-CANX", "TARTAGAL - SDWAN", "3470")},
    {"3471", Tuple.Create("SUC", "TEODELINA - SDWAN", "3471")},
    {"3472", Tuple.Create("SUC", "TERMAS RIO HONDO - SDWAN", "3472")},
    {"3480", Tuple.Create("SUC-CANX", "TIGRE - SDWAN", "3480")},
    {"3490", Tuple.Create("SUC", "TILISARAO - SDWAN", "3490")},
    {"3500", Tuple.Create("SUC", "TINOGASTA - SDWAN", "3500")},
    {"3505", Tuple.Create("SUC", "TOLOSA - SDWAN", "3505")},
    {"3510", Tuple.Create("SUC", "TORNQUIST", "3510")},
    {"3520", Tuple.Create("SUC", "VICU�A MACKENNA - SDWAN", "3520")},
    {"3530", Tuple.Create("SUC", "TOSTADO - SDWAN", "3530")},
    {"3540", Tuple.Create("SUC", "TOTORAS - SDWAN", "3540")},
    {"3550", Tuple.Create("SUC-CANX-ZNL", "TRELEW (ZONAL) - SDWAN", "3550")},
    {"3560", Tuple.Create("SUC-ZNL", "TRENQUE LAUQUEN (ZONAL) SDWAN", "3560")},
    {"3570", Tuple.Create("SUC", "TRES ARROYOS - SDWAN", "3570")},
    {"3574", Tuple.Create("SUC", "TRES ISLETAS - SDWAN", "3574")},
    {"3575", Tuple.Create("SUC", "TRES LOMAS - SDWAN", "3575")},
    {"3576", Tuple.Create("SUC", "TRIBUNALES MENDOZA - SDWAN", "3576")},
    {"3580", Tuple.Create("ANEXO", "TRIBUNALES FED.DE ROSARIO (depende de 3020) - SDWAN", "3020")},
    {"3583", Tuple.Create("SUC", "TRIBUNALES PROVINCIALES SAN RAFAEL - SDWAN", "3583")},
    {"3587", Tuple.Create("SUC-CANX", "TRISTAN SUAREZ - SDWAN", "3587")},
    {"3590", Tuple.Create("SUC-CANX", "TUNUYAN - SDWAN", "3590")},
    {"3592", Tuple.Create("SUC-CANX", "TUPUNGATO - SDWAN", "3592")},
    {"3600", Tuple.Create("SUC", "UCACHA - SDWAN", "3600")},
    {"3601", Tuple.Create("SUC-CANX", "UNQUILLO - SDWAN", "3601")},
    {"3610", Tuple.Create("SUC", "URDAMPILLETA - SDWAN", "3610")},
    {"3620", Tuple.Create("SUC", "URDINARRAIN - SDWAN", "3620")},
    {"3630", Tuple.Create("SUC", "USHUAIA - SDWAN", "3630")},
    {"3631", Tuple.Create("SUC-CANX", "USPALLATA - SDWAN", "3631")},
    {"3633", Tuple.Create("SUC", "VALENTIN ALSINA - SDWAN", "3633")},
    {"3635", Tuple.Create("ANEXO-BAJA", "VALLE FERTIL (depende de 3200)", "3200")},
    {"3640", Tuple.Create("SUC", "VEDIA - SDWAN", "3640")},
    {"3650", Tuple.Create("SUC", "VEINTICINCO DE MAYO - SDWAN", "3650")},
    {"3655", Tuple.Create("SUC", "28 DE NOVIEMBRE - SDWAN", "3655")},
    {"3660", Tuple.Create("SUC-ZNL", "VENADO TUERTO (ZONAL) SDWAN", "3660")},
    {"3665", Tuple.Create("SUC", "VERA - SDWAN", "3665")},
    {"3670", Tuple.Create("SUC", "VERONICA - SDWAN", "3670")},
    {"3672", Tuple.Create("SUC", "VIALE - SDWAN", "3672")},
    {"3675", Tuple.Create("SUC", "VICENTE LOPEZ - SDWAN", "3675")},
    {"3680", Tuple.Create("SUC", "VICTORIA - SDWAN", "3680")},
    {"3690", Tuple.Create("SUC", "VICTORICA", "3690")},
    {"3700", Tuple.Create("SUC", "VIEDMA - SDWAN", "3700")},
    {"3707", Tuple.Create("SUC", "VILA", "3707")},
    {"3710", Tuple.Create("SUC", "VILLA ANGELA - SDWAN", "3710")},
    {"3711", Tuple.Create("SUC", "VILLA ATUEL - SDWAN", "3711")},
    {"3713", Tuple.Create("SUC", "VILLA BERTHET", "3713")},
    {"3714", Tuple.Create("SUC", "VILLA BALLESTER - SDWAN", "3714")},
    {"3716", Tuple.Create("SUC", "VILLA BOSCH - SDWAN", "3716")},
    {"3720", Tuple.Create("SUC", "VILLA CA�AS - SDWAN", "3720")},
    {"3722", Tuple.Create("SUC-ZNL", "VILLA CARLOS PAZ (ZONAL) - SDWAN", "3722")},
    {"3730", Tuple.Create("SUC", "VILLA CONSTITUCI�N - SDWAN", "3730")},
    {"3740", Tuple.Create("SUC", "VILLA DEL ROSARIO - SDWAN", "3740")},
    {"3750", Tuple.Create("SUC", "VILLA DOLORES (CORDOBA) - SDWAN", "3750")},
    {"3755", Tuple.Create("SUC-CANX", "VILLA DOLORES (CATAMARCA)", "3755")},
    {"3760", Tuple.Create("SUC", "VILLA GESELL - SDWAN", "3760")},
    {"3761", Tuple.Create("SUC", "VILLA GENERAL BELGRANO - SDWAN", "3761")},
    {"3765", Tuple.Create("SUC", "VILLA GOBERNADOR GALVEZ - SDWAN", "3765")},
    {"3770", Tuple.Create("SUC", "VILLAGUAY - SDWAN", "3770")},
    {"3780", Tuple.Create("SUC", "VILLA HERNANDARIAS - SDWAN", "3780")},
    {"3790", Tuple.Create("SUC", "VILLA HUIDOBRO", "3790")},
    {"3800", Tuple.Create("SUC", "VILLA IRIS - SDWAN", "3800")},
    {"3805", Tuple.Create("SUC", "VILLA KRAUSE - SDWAN", "3805")},
    {"3810", Tuple.Create("SUC-CANX-ZNL", "VILLA MARIA (ZONAL) - SDWAN", "3810")},
    {"3814", Tuple.Create("SUC", "VILLA MINETTI - SDWAN", "3814")},
    {"3815", Tuple.Create("SUC", "VILLA MAZA", "3815")},
    {"3816", Tuple.Create("SUC-CANX", "VILLA NUEVA - SDWAN", "3816")},
    {"3817", Tuple.Create("SUC", "VILLA OCAMPO - SDWAN", "3817")},
    {"3820", Tuple.Create("SUC", "VILLA RAMALLO - SDWAN", "3820")},
    {"3825", Tuple.Create("SUC", "VILLA REGINA - SDWAN", "3825")},
    {"3830", Tuple.Create("SUC", "VILLA SARMIENTO - SDWAN", "3830")},
    {"3831", Tuple.Create("SUC", "VINCHINA - SDWAN", "3831")},
    {"3832", Tuple.Create("SUC", "VILLA UNION - SDWAN", "3832")},
    {"3833", Tuple.Create("SUC", "VILLA TRINIDAD - SDWAN", "3833")},
    {"3834", Tuple.Create("SUC", "WILDE - SDWAN", "3834")},
    {"3835", Tuple.Create("SUC", "YACIMIENTO RIO TURBIO - SDWAN", "3835")},
    {"3836", Tuple.Create("SUC", "WINIFREDA - SDWAN", "3836")},
    {"3837", Tuple.Create("SUC", "VIRREYES - SDWAN", "3837")},
    {"3838", Tuple.Create("SUC", "VISTA FLORES - SDWAN", "3838")},
    {"3839", Tuple.Create("SUC", "YERBA BUENA - SDWAN", "3839")},
    {"3840", Tuple.Create("SUC", "ZAPALA - SDWAN", "3840")},
    {"3850", Tuple.Create("SUC", "ZARATE - SDWAN", "3850")},
    {"3853", Tuple.Create("SUC", "AVENIDA 60 - SDWAN", "3853")},
    {"3857", Tuple.Create("SUC", "CALLE 4 LA PLATA - SDWAN", "3857")},
    {"9014", Tuple.Create("SUC-CANX", "LUIS M. CAMPOS - SDWAN", "9014")},
    {"9015", Tuple.Create("SUC", "AVDA. LA PLATA - SDWAN", "9015")},
    {"9101", Tuple.Create("SUC", "AVDA. CORDOBA - VDI - SDWAN", "9101")},
    {"9201", Tuple.Create("SUC", "BARRIO CERRO LAS ROSAS - SDWAN", "9201")},
    {"9202", Tuple.Create("SUC", "BARRIO SAN MARTIN - SDWAN", "9202")},
    {"9204", Tuple.Create("SUC", "AVDA. SABATTINI - SDWAN", "9204")},
    {"9205", Tuple.Create("SUC-CANX", "AVDA. JUAN B. JUSTO - SDWAN", "9205")},
    {"9207", Tuple.Create("SUC", "VILLA MADERO - SDWAN", "9207")},
    {"9209", Tuple.Create("SUC-CANX", "MARTINEZ - SDWAN", "9209")},
    {"9210", Tuple.Create("SUC-CANX", "AEROP. INTERNACIONAL DE EZEIZA - SDWAN", "9210")},
    {"9210-2", Tuple.Create("EDIF-ANX", "EDCADASSA (DEPENDE DE 9210) - SDWAN", "9210")},
    {"9210-A", Tuple.Create("EDIF-ANX", "TERMINAL A INTERNACIONAL (DEPENDE DE 9210) - SDWAN", "9210")},
    {"9214", Tuple.Create("SUC", "BERNAL - SDWAN", "9214")},
    {"9216", Tuple.Create("SUC", "CIUDADELA - SDWAN", "9216")},
    {"9217", Tuple.Create("SUC", "DON BOSCO - SDWAN", "9217")},
    {"9218", Tuple.Create("SUC", "INGENIERO WHITE - SDWAN", "9218")},
    {"9223", Tuple.Create("SUC", "VILLA DIAZ VELEZ - SDWAN", "9223")},
    {"9224", Tuple.Create("SUC", "VILLA MITRE - SDWAN", "9224")},
    {"9226", Tuple.Create("SUC-CANX", "BARRIO ALTO ALBERDI - SDWAN", "9226")},
    {"9238", Tuple.Create("SUC", "SANTO TOME (S.F) - SDWAN", "9238")},
    {"9245", Tuple.Create("SUC", "BARRIO LA PERLA - SDWAN", "9245")},
    {"9246", Tuple.Create("SUC", "BARRIO ACEVEDO - SDWAN", "9246")},
    {"9248", Tuple.Create("SUC", "SALTO GRANDE - SDWAN", "9248")},
    {"9261", Tuple.Create("SUC", "AVDA. VELEZ SARSFIELD - SDWAN", "9261")},
    {"9265", Tuple.Create("SUC-BAJA", "AVDA. GENERAL BELGRANO (AD. ROSARIO) - BAJA el 2/5/2021", "9265")},
    {"9305", Tuple.Create("SUC", "ARIZONA - VDI - SDWAN", "9305")},
    {"9310", Tuple.Create("SUC", "BUENA ESPERANZA - VDI - SDWAN", "9310")},
    {"9315", Tuple.Create("SUC", "CANDELARIA - VDI - SDWAN", "9315")},
    {"9320", Tuple.Create("SUC", "CONCAR�N - VDI - SDWAN", "9320")},
    {"9340", Tuple.Create("SUC", "LA PUNTA - VDI - SDWAN", "9340")},
    {"9345", Tuple.Create("SUC", "LA TERMINAL - VDI - SDWAN", "9345")},
    {"9350", Tuple.Create("SUC", "NASCHEL - VDI - SDWAN", "9350")},
    {"9355", Tuple.Create("SUC", "NUEVA GALIA - VDI - SDWAN", "9355")},
    {"9360", Tuple.Create("SUC", "QUINES - VDI - SDWAN", "9360")},
    {"9365", Tuple.Create("SUC", "SAN FRANCISCO DE MONTE DE ORO - VDI - SDWAN", "9365")},
    {"9370", Tuple.Create("SUC", "SAN LUIS CENTRO - VDI - SDWAN", "9370")},
    {"9375", Tuple.Create("SUC", "SANTA ROSA DE CONLARA - VDI - SDWAN", "9375")},
    {"9380", Tuple.Create("SUC", "TERRAZAS DE PORTEZUELO - VDI - SDWAN", "9380")},
    {"9385", Tuple.Create("SUC", "UNION - VDI - SDWAN", "9385")},
    {"9395", Tuple.Create("ANEXO", "AO PODER JUDICIAL SAN LUIS (depende de 3240)", "9395")},
    {"9399", Tuple.Create("ANEXO", "AO RENTAS DPIP SL (depende de 3240)", "9399")}
    }




    'Event ProcesarSucursal: detecta servidor y actualiza LabelEstadoRed
    Private Sub ProcesarSucursal()
        Dim textoIngresado As String = ComboBoxSucursales.Text.Trim()
        If String.IsNullOrEmpty(textoIngresado) Then Exit Sub

        ' Caso: el usuario seleccionó directamente un ítem del ComboBox
        If ComboBoxSucursales.SelectedItem IsNot Nothing AndAlso
       ComboBoxSucursales.SelectedItem.ToString() = textoIngresado Then

            Dim sucursalFormateada As String = ComboBoxSucursales.SelectedItem.ToString().PadLeft(4, "0"c)
            ProcesarServidor(sucursalFormateada)
            Exit Sub
        End If

        ' Caso: el usuario está escribiendo manualmente
        ' Solo procesar si escribie exactamente 4 dígitos
        If textoIngresado.Length = 4 AndAlso Integer.TryParse(textoIngresado, Nothing) Then
            Dim sucursalNumero As Integer = Integer.Parse(textoIngresado)
            Dim sucursalFormateada As String = sucursalNumero.ToString().PadLeft(4, "0"c)

            ' Buscar coincidencia exacta
            Dim encontrado As Boolean = False
            For Each item As Object In ComboBoxSucursales.Items
                Dim valorItem As Integer
                If Integer.TryParse(item.ToString(), valorItem) Then
                    If valorItem.ToString().PadLeft(4, "0"c) = sucursalFormateada Then
                        ComboBoxSucursales.SelectedItem = item
                        encontrado = True
                        Exit For
                    End If
                End If
            Next

            ' Si no se encontró, buscar el más cercano
            If Not encontrado Then
                Dim listaSucursales As New List(Of Integer)
                For Each item As Object In ComboBoxSucursales.Items
                    Dim valorItem As Integer
                    If Integer.TryParse(item.ToString(), valorItem) Then
                        listaSucursales.Add(valorItem)
                    End If
                Next

                If listaSucursales.Count > 0 Then
                    Dim masCercano As Integer = listaSucursales.OrderBy(Function(x) Math.Abs(x - sucursalNumero)).First()
                    ComboBoxSucursales.SelectedItem = masCercano.ToString().PadLeft(4, "0"c)
                    sucursalFormateada = ComboBoxSucursales.SelectedItem.ToString().PadLeft(4, "0"c)
                End If
            End If

            ' Procesar servidor con la sucursal formateada
            ProcesarServidor(sucursalFormateada)
        End If
    End Sub

    ' Método auxiliar  de servidor
    Private Sub ProcesarServidor(sucursalFormateada As String)
        If sucursalFormateada = "0000" Then Exit Sub
        Dim servidorActivo As String = DetectarSMF(sucursalFormateada)

        If servidorActivo <> "" Then
            Dim servidorCompleto As String = servidorActivo & "SC" & sucursalFormateada
            Dim rutaServidor As String = "\\" & servidorCompleto & "\c$"

            LabelEstadoServidor.Text = servidorCompleto
            LabelEstadoServidor.ForeColor = Color.Green
            LabelEstadoServidor.Tag = rutaServidor
            LabelEstadoServidor.Cursor = Cursors.Hand
            LabelEstadoServidor.Font = New Font(LabelEstadoServidor.Font, FontStyle.Underline)
            ToolTip1.SetToolTip(LabelEstadoServidor, rutaServidor)
        Else
            LabelEstadoServidor.Text = "Ningún servidor responde para sucursal " & sucursalFormateada
            LabelEstadoServidor.ForeColor = Color.Red
            LabelEstadoServidor.Tag = Nothing
        End If

        ActualizarInfoGeneral()

    End Sub



    Private Function DetectarSMF(sucursal As String) As String

        Dim smf01 = "SMF01SC" & sucursal
        Dim smf02 = "SMF02SC" & sucursal
        If HacerPing(smf01) Then
            Return "SMF01"
        End If

        If HacerPing(smf02) Then
            Return "SMF02"
        End If

        Return ""
    End Function

    ' Evento LabelEstadoServidor: abre la ruta completa del servidor
    Private Sub LabelEstadoServidor_Click(sender As Object, e As EventArgs) Handles LabelEstadoServidor.Click
        If LabelEstadoServidor.ForeColor = Color.Green AndAlso LabelEstadoServidor.Tag IsNot Nothing Then
            Try
                Process.Start("explorer.exe", LabelEstadoServidor.Tag.ToString())
            Catch ex As Exception
                MessageBox.Show("No se pudo abrir la ubicación de red: " & ex.Message)
            End Try
        End If
    End Sub







    ' Evento cuando cambia el texto del TextBoxLegajo
    Private Sub TextBoxLegajo_TextChanged(sender As Object, e As EventArgs) Handles TextBoxLegajo.TextChanged
        If _suspendEvents Then Exit Sub
        Dim puestoActual As String = TextBoxPuesto.Text.Trim()

        ' Normalizar a MAYÚSCULAS y recortar a 6 (soporta pegado)
        Dim pos As Integer = TextBoxLegajo.SelectionStart
        If TextBoxLegajo.TextLength > 0 Then
            TextBoxLegajo.Text = TextBoxLegajo.Text.ToUpper()
            If TextBoxLegajo.TextLength > 6 Then
                TextBoxLegajo.Text = TextBoxLegajo.Text.Substring(0, 6)
            End If
            TextBoxLegajo.SelectionStart = Math.Min(pos, TextBoxLegajo.TextLength)
        End If

        Dim legajoActual As String = TextBoxLegajo.Text.Trim()

        ' Validar formato: 1 letra (N/T/E) + 5 dígitos
        If Regex.IsMatch(legajoActual, "^[NTE]\d{5}$", RegexOptions.IgnoreCase) Then
            ' Si el puesto no está informado, evitamos intentar conectar
            If String.IsNullOrWhiteSpace(puestoActual) Then
                LabelEstadoLegajo.Text = "Esperando puesto válido..."
                LabelEstadoLegajo.ForeColor = Color.Gray
                LabelEstadoLegajo.Tag = Nothing
                LabelEstadoLegajo.Cursor = Cursors.Default
                LabelEstadoLegajo.Font = New Font(LabelEstadoLegajo.Font, FontStyle.Regular)
                ToolTip1.SetToolTip(LabelEstadoLegajo, "")
                ActualizarInfoGeneral()
                Exit Sub
            End If

            ' Validamos que el puesto responda al ping
            If HacerPing(puestoActual) Then
                Try
                    Dim rutaBase As String = "\\" & puestoActual & "\c$\Users\"
                    Dim legajoUpper As String = legajoActual.ToUpper()

                    ' Buscar variantes con sufijo .SUC#
                    Dim carpetas As String() = Directory.GetDirectories(rutaBase, legajoUpper & ".SUC*")

                    If carpetas.Length > 0 Then
                        ' Elegir la variante con mayor número
                        Dim carpetaElegida As String = carpetas.OrderByDescending(
                        Function(c)
                            Dim nombre = Path.GetFileName(c)
                            Dim idx = nombre.IndexOf(".SUC", StringComparison.OrdinalIgnoreCase)
                            If idx >= 0 AndAlso nombre.Length > idx + 4 Then
                                Dim sufijo = nombre.Substring(idx + 4)
                                Dim num As Integer
                                If Integer.TryParse(sufijo, num) Then
                                    Return num
                                End If
                            End If
                            Return -1
                        End Function
                    ).First()

                        LabelEstadoLegajo.Text = "Legajo encontrado (variante .SUC)"
                        LabelEstadoLegajo.ForeColor = Color.Green
                        LabelEstadoLegajo.Tag = carpetaElegida
                        LabelEstadoLegajo.Cursor = Cursors.Hand
                        LabelEstadoLegajo.Font = New Font(LabelEstadoLegajo.Font, FontStyle.Underline)
                        ToolTip1.SetToolTip(LabelEstadoLegajo, carpetaElegida)

                    Else
                        ' Buscar carpeta directa sin .SUC
                        Dim carpetaDirecta As String = Path.Combine(rutaBase, legajoUpper)
                        If Directory.Exists(carpetaDirecta) Then
                            LabelEstadoLegajo.Text = "Legajo encontrado"
                            LabelEstadoLegajo.ForeColor = Color.Green
                            LabelEstadoLegajo.Tag = carpetaDirecta
                            LabelEstadoLegajo.Cursor = Cursors.Hand
                            LabelEstadoLegajo.Font = New Font(LabelEstadoLegajo.Font, FontStyle.Underline)
                            ToolTip1.SetToolTip(LabelEstadoLegajo, carpetaDirecta)
                        Else
                            LabelEstadoLegajo.Text = "Legajo no encontrado"
                            LabelEstadoLegajo.ForeColor = Color.Orange
                            LabelEstadoLegajo.Tag = Nothing
                            LabelEstadoLegajo.Cursor = Cursors.Default
                            LabelEstadoLegajo.Font = New Font(LabelEstadoLegajo.Font, FontStyle.Regular)
                            ToolTip1.SetToolTip(LabelEstadoLegajo, "")
                        End If
                    End If

                Catch ex As Exception
                    LabelEstadoLegajo.Text = "Error al validar legajo"
                    LabelEstadoLegajo.ForeColor = Color.Red
                    LabelEstadoLegajo.Tag = Nothing
                    LabelEstadoLegajo.Cursor = Cursors.Default
                    LabelEstadoLegajo.Font = New Font(LabelEstadoLegajo.Font, FontStyle.Regular)
                    ToolTip1.SetToolTip(LabelEstadoLegajo, "")
                End Try
            Else
                LabelEstadoLegajo.Text = "Puesto fuera de red"
                LabelEstadoLegajo.ForeColor = Color.Red
                LabelEstadoLegajo.Tag = Nothing
                LabelEstadoLegajo.Cursor = Cursors.Default
                LabelEstadoLegajo.Font = New Font(LabelEstadoLegajo.Font, FontStyle.Regular)
                ToolTip1.SetToolTip(LabelEstadoLegajo, "")
            End If
        Else
            LabelEstadoLegajo.Text = "Esperando legajo válido..."
            LabelEstadoLegajo.ForeColor = Color.Gray
            LabelEstadoLegajo.Tag = Nothing
            LabelEstadoLegajo.Cursor = Cursors.Default
            LabelEstadoLegajo.Font = New Font(LabelEstadoLegajo.Font, FontStyle.Regular)
            ToolTip1.SetToolTip(LabelEstadoLegajo, "")
        End If

        ActualizarInfoGeneral()
    End Sub

    ' Bloquear caracteres inválidos en Legajo
    ' Legajo: 1 letra (N/T/E) + 5 números
    Private Sub TextBoxLegajo_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBoxLegajo.KeyPress

        ' Permitir teclas de control (Backspace, Delete, etc.)
        If Char.IsControl(e.KeyChar) Then Exit Sub

        Dim tb = DirectCast(sender, TextBox)

        ' Primer carácter    N, T o E
        If tb.SelectionStart = 0 Then
            Dim ch As Char = Char.ToUpper(e.KeyChar)

            If ch <> "N"c AndAlso ch <> "T"c AndAlso ch <> "E"c Then
                e.Handled = True
            End If
            Exit Sub
        End If

        ' Del carácter 2 al 6 → SÓLO números
        If tb.SelectionStart >= 1 AndAlso tb.SelectionStart <= 5 Then
            If Not Char.IsDigit(e.KeyChar) Then
                e.Handled = True
            End If
            Exit Sub
        End If

        ' Bloquear si intenta escribir más de 6 caracteres (1 letra + 5 números)
        If tb.TextLength >= 6 AndAlso tb.SelectionLength = 0 Then
            e.Handled = True
        End If

    End Sub

    'Evento al presionar legajo
    Private Sub LabelEstadoLegajo_Click(sender As Object, e As EventArgs) Handles LabelEstadoLegajo.Click
        If LabelEstadoLegajo.ForeColor = Color.Green AndAlso LabelEstadoLegajo.Tag IsNot Nothing Then
            Try
                Process.Start("explorer.exe", LabelEstadoLegajo.Tag.ToString())
            Catch ex As Exception
                MessageBox.Show("No se pudo abrir puesto: " & ex.Message)
            End Try
        End If
    End Sub


    ' Evento cuando cambia el texto del TextBoxPuesto
    Private Sub TextBoxPuesto_TextChanged(sender As Object, e As EventArgs) Handles TextBoxPuesto.TextChanged
        If _suspendEvents Then Exit Sub
        Dim puestoActual As String = TextBoxPuesto.Text.Trim()

        ' Normalizar  mayúsculas en pantalla
        Dim selPos As Integer = TextBoxPuesto.SelectionStart
        TextBoxPuesto.Text = TextBoxPuesto.Text.ToUpper()
        TextBoxPuesto.SelectionStart = selPos

        ' Validar LONGITUD 
        If puestoActual.Length <> 11 Then
            LabelEstadoPuesto.Text = "Esperando puesto válido..."
            LabelEstadoPuesto.ForeColor = Color.Gray
            LabelEstadoPuesto.Tag = Nothing
            LabelEstadoPuesto.Cursor = Cursors.Default
            LabelEstadoPuesto.Font = New Font(LabelEstadoPuesto.Font, FontStyle.Regular)
            ToolTip1.SetToolTip(LabelEstadoPuesto, "")
            ActualizarInfoGeneral()
            Exit Sub
        End If

        ' Validar formato EXACTO:
        ' Letra válida + 4 números + SC + 4 números
        ' Letras válidas: A, B, C, L, M, X, O, H, G
        Dim regexFormato As String = "^[ABCLMXOHG]\d{4}SC\d{4}$"

        If Not Regex.IsMatch(puestoActual.ToUpper(), regexFormato) Then
            LabelEstadoPuesto.Text = "Formato de puesto inválido"
            LabelEstadoPuesto.ForeColor = Color.Orange
            LabelEstadoPuesto.Tag = Nothing
            LabelEstadoPuesto.Cursor = Cursors.Default
            LabelEstadoPuesto.Font = New Font(LabelEstadoPuesto.Font, FontStyle.Regular)
            ToolTip1.SetToolTip(LabelEstadoPuesto, "")
            ActualizarInfoGeneral()
            Exit Sub
        End If

        ' formato es válido → hacemos ping
        If HacerPing(puestoActual) Then
            Dim rutaServidor As String = "\\" & puestoActual & "\c$"

            ' Mostrar SIEMPRE el puesto en mayúsculas
            LabelEstadoPuesto.Text = puestoActual.ToUpper()
            LabelEstadoPuesto.ForeColor = Color.Green
            LabelEstadoPuesto.Tag = rutaServidor
            LabelEstadoPuesto.Cursor = Cursors.Hand
            LabelEstadoPuesto.Font = New Font(LabelEstadoPuesto.Font, FontStyle.Underline)
            ToolTip1.SetToolTip(LabelEstadoPuesto, rutaServidor)
        Else
            LabelEstadoPuesto.Text = "Puesto fuera de red"
            LabelEstadoPuesto.ForeColor = Color.Red
            LabelEstadoPuesto.Tag = Nothing
            LabelEstadoPuesto.Cursor = Cursors.Default
            LabelEstadoPuesto.Font = New Font(LabelEstadoPuesto.Font, FontStyle.Regular)
            ToolTip1.SetToolTip(LabelEstadoPuesto, "")
        End If

        ' Actualizar Info consolidada
        ActualizarInfoGeneral()
    End Sub

    ' Bloquear caracteres inválidos en Puesto (solo letras/números, sin símbolos)
    Private Sub TextBoxPuesto_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBoxPuesto.KeyPress
        If Char.IsControl(e.KeyChar) Then Exit Sub

        ' Solo letras y números
        If Not Char.IsLetterOrDigit(e.KeyChar) Then
            e.Handled = True
            Exit Sub
        End If
        ' Limitar a 11 caracteres (permitiendo reemplazo por selección)
        Dim tb = DirectCast(sender, TextBox)
        Dim seleccion = tb.SelectionLength
        Dim longitudEfectiva As Integer = tb.TextLength - seleccion

        If longitudEfectiva >= 11 Then
            e.Handled = True
        End If
    End Sub

    ' Evento click del LabelEstadoPuesto
    Private Sub LabelEstadoPuesto_Click(sender As Object, e As EventArgs) Handles LabelEstadoPuesto.Click
        If LabelEstadoPuesto.ForeColor = Color.Green AndAlso LabelEstadoPuesto.Tag IsNot Nothing Then
            Try
                Process.Start("explorer.exe", LabelEstadoPuesto.Tag.ToString())
            Catch ex As Exception
                MessageBox.Show("No se pudo abrir la unidad de red: " & ex.Message)
            End Try
        End If
    End Sub

    'Busqueda Informacion de Puesto
    Private Function ObtenerInfoHardware(puesto As String) As Dictionary(Of String, Tuple(Of String, Integer))
        Dim datos As New Dictionary(Of String, Tuple(Of String, Integer))

        Try
            ' --- Puesto (mostrar tal cual pero en mayúsculas) ---
            datos("Puesto") = Tuple.Create(puesto.ToUpperInvariant(), -1)

            ' --- Última masterización: \\<puesto>\c$\Program Files[\(x86\)]\Sequencer\<puesto>.dat ---
            Try
                Dim ruta1 As String = "\\" & puesto & "\c$\Program Files\Sequencer\" & puesto & ".dat"
                Dim ruta2 As String = "\\" & puesto & "\c$\Program Files (x86)\Sequencer\" & puesto & ".dat"

                Dim rutaEncontrada As String = Nothing
                If File.Exists(ruta1) Then
                    rutaEncontrada = ruta1
                ElseIf File.Exists(ruta2) Then
                    rutaEncontrada = ruta2
                End If

                If Not String.IsNullOrEmpty(rutaEncontrada) Then
                    Dim fecha As DateTime = File.GetCreationTime(rutaEncontrada)
                    datos("Última masterización") = Tuple.Create(fecha.ToString("dd/MM/yyyy"), 0) ' 0 = OK/verde
                Else
                    datos("Última masterización") = Tuple.Create("Puesto no realizó masterización.", 1) ' 1 = alerta/rojo
                End If
            Catch exIO As Exception
                datos("Última masterización") = Tuple.Create("No se pudo leer masterización: " & exIO.Message, 1)
            End Try

            ' --- WMI al equipo remoto ---
            Dim scope As New ManagementScope("\\" & puesto & "\root\cimv2")
            scope.Connect()
            ' --- Detectar tipo de equipo (Notebook vs CPU) y, si es notebook, mostrar modelo ---
            Dim esNotebook As Boolean = False
            Dim tipoEquipoTexto As String = "Desconocido"

            ' 1) Intento principal: Win32_SystemEnclosure.ChassisTypes
            Try
                Using searcherChassis As New ManagementObjectSearcher(scope, New ObjectQuery("SELECT ChassisTypes FROM Win32_SystemEnclosure"))
                    For Each obj As ManagementObject In searcherChassis.Get()
                        Dim arr = TryCast(obj("ChassisTypes"), UShort())
                        If arr IsNot Nothing AndAlso arr.Length > 0 Then
                            ' Mapear a categorías
                            For Each code As UShort In arr
                                Select Case code
                                    Case 8US, 9US, 10US, 14US, 30US ' Portable, Laptop, Notebook, Sub-Notebook, Tablet
                                        esNotebook = True
                                        Exit For
                                    Case 3US, 4US, 5US, 6US, 7US, 15US, 16US, 35US ' Desktop variants, All-in-One
                                        ' Si no es notebook, lo consideramos CPU/Escritorio
                                        ' (No seteamos aquí porque podría haber múltiples códigos; preferimos marcar notebook si aparece)
                                End Select
                            Next
                            Exit For
                        End If
                    Next
                End Using
            Catch
                ' Si falla, pasamos al fallback de batería
            End Try

            ' 2) Fallback: si el chasis no dio pista, buscamos batería
            If Not esNotebook Then
                Try
                    Using searcherBat As New ManagementObjectSearcher(scope, New ObjectQuery("SELECT * FROM Win32_Battery"))
                        For Each obj As ManagementObject In searcherBat.Get()
                            ' Si hay batería presente, casi seguro es portátil
                            esNotebook = True
                            Exit For
                        Next
                    End Using
                Catch
                    ' Si tampoco se puede consultar, lo dejamos en desconocido
                End Try
            End If

            If esNotebook Then
                tipoEquipoTexto = "Notebook/Portátil"
                datos("Tipo de equipo") = Tuple.Create(tipoEquipoTexto, 0) ' 0 = OK/verde

                ' Como ya abajo llenás Marca/Modelo/Serial con Win32_ComputerSystemProduct,
                ' podemos intentar pre-levantar el Modelo aquí para mostrarlo explícito si es notebook.
                Try
                    Using searcherProd As New ManagementObjectSearcher(scope, New ObjectQuery("SELECT Name FROM Win32_ComputerSystemProduct"))
                        For Each obj As ManagementObject In searcherProd.Get()
                            Dim modeloName As String = If(obj("Name"), "").ToString()
                            If Not String.IsNullOrWhiteSpace(modeloName) Then
                                datos("Modelo (notebook)") = Tuple.Create(modeloName, -1) ' neutro para ícono
                                Exit For
                            End If
                        Next
                    End Using
                Catch
                    ' Si falla, lo omitimos silenciosamente
                End Try
            Else
                tipoEquipoTexto = "CPU/Escritorio"
                datos("Tipo de equipo") = Tuple.Create(tipoEquipoTexto, -1) ' neutro
            End If
            ' SO, versión, uptime y RAM total/libre
            Using searcherOS As New ManagementObjectSearcher(scope, New ObjectQuery(
            "SELECT Caption, Version, LastBootUpTime, TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem"))
                For Each obj As ManagementObject In searcherOS.Get()
                    datos("Sistema operativo") = Tuple.Create(If(obj("Caption"), "N/A").ToString(), -1)
                    datos("Versión") = Tuple.Create(If(obj("Version"), "N/A").ToString(), -1)

                    If obj("LastBootUpTime") IsNot Nothing Then
                        Dim bootTime As DateTime = ManagementDateTimeConverter.ToDateTime(obj("LastBootUpTime").ToString())
                        datos("Último reinicio") = Tuple.Create(bootTime.ToString("yyyy-MM-dd HH:mm"), -1)
                        datos("Tiempo de actividad") = Tuple.Create((DateTime.Now - bootTime).ToString(), -1)
                    End If

                    Dim totalRAMKB As Double = CDbl(If(obj("TotalVisibleMemorySize"), 0))
                    Dim freeRAMKB As Double = CDbl(If(obj("FreePhysicalMemory"), 0))
                    datos("Memoria RAM total") = Tuple.Create(Math.Round(totalRAMKB / (1024 * 1024), 2).ToString() & " GB", -1)
                    datos("Memoria RAM libre") = Tuple.Create(Math.Round(freeRAMKB / (1024 * 1024), 2).ToString() & " GB", -1)
                Next
            End Using

            ' Usuario logueado
            Using searcherUser As New ManagementObjectSearcher(scope, New ObjectQuery("SELECT UserName FROM Win32_ComputerSystem"))
                For Each obj As ManagementObject In searcherUser.Get()
                    datos("Usuario logueado") = Tuple.Create(If(obj("UserName"), "N/A").ToString(), -1)
                Next
            End Using

            ' Marca, modelo, serial
            Using searcherProduct As New ManagementObjectSearcher(scope, New ObjectQuery("SELECT Vendor, Name, IdentifyingNumber FROM Win32_ComputerSystemProduct"))
                For Each obj As ManagementObject In searcherProduct.Get()
                    datos("Marca") = Tuple.Create(If(obj("Vendor"), "N/A").ToString(), -1)
                    datos("Modelo") = Tuple.Create(If(obj("Name"), "N/A").ToString(), -1)
                    datos("Serial") = Tuple.Create(If(obj("IdentifyingNumber"), "N/A").ToString(), -1)
                Next
            End Using

            ' IP (preferir IPv4)
            Using searcherIP As New ManagementObjectSearcher(scope, New ObjectQuery(
            "SELECT IPAddress FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = True"))
                For Each obj As ManagementObject In searcherIP.Get()
                    Dim ipAddresses = TryCast(obj("IPAddress"), String())
                    If ipAddresses IsNot Nothing AndAlso ipAddresses.Length > 0 Then
                        Dim ipv4 As String = Nothing
                        For Each ip In ipAddresses
                            If ip Like "*.*.*.*" Then
                                ipv4 = ip
                                Exit For
                            End If
                        Next
                        datos("Dirección IP") = Tuple.Create(If(String.IsNullOrEmpty(ipv4), ipAddresses(0), ipv4), -1)
                        Exit For
                    End If
                Next
            End Using

            ' Módulos de memoria física
            Using searcherMem As New ManagementObjectSearcher(scope, New ObjectQuery(
            "SELECT Capacity, Manufacturer, PartNumber, SerialNumber FROM Win32_PhysicalMemory"))
                Dim idx As Integer = 1
                For Each obj As ManagementObject In searcherMem.Get()
                    Dim capBytes As Double = CDbl(If(obj("Capacity"), 0))
                    Dim capGB As Double = capBytes / (1024 ^ 3)
                    datos("Módulo RAM " & idx) = Tuple.Create(
                    capGB.ToString("0.00") & " GB - " &
                    If(obj("Manufacturer"), "N/A").ToString() & " - " &
                    If(obj("PartNumber"), "N/A").ToString() & " - SN:" & If(obj("SerialNumber"), "N/A").ToString(),
                    -1
                )
                    idx += 1
                Next
            End Using

            ' Discos físicos con estado
            Using searcherDrive As New ManagementObjectSearcher(scope, New ObjectQuery("SELECT Model, Size, Status FROM Win32_DiskDrive"))
                Dim dIdx As Integer = 1
                For Each obj As ManagementObject In searcherDrive.Get()
                    Dim sizeGB As Double = CDbl(If(obj("Size"), 0)) / (1024 ^ 3)
                    Dim status As String = If(obj("Status"), "Desconocido").ToString()
                    Dim iconIndex As Integer = If(status.Equals("OK", StringComparison.OrdinalIgnoreCase), 0, 1) ' 0=verde, 1=rojo
                    datos("Disco físico " & dIdx) = Tuple.Create(
                    If(obj("Model"), "N/A").ToString() & " - " &
                    sizeGB.ToString("0.00") & " GB - Estado: " & status,
                    iconIndex
                )
                    dIdx += 1
                Next
            End Using

        Catch ex As Exception
            datos("Error") = Tuple.Create("No se pudo obtener información: " & ex.Message, 1)
        End Try

        Return datos
    End Function
    'Mostrar datos ventana Puesto
    Private Sub MostrarInfoEnVentana(puesto As String,
                                 datos As Dictionary(Of String, Tuple(Of String, Integer)),
                                 txtLegajo As TextBox)

        Dim ventanaInfo As New Form()
        ventanaInfo.Text = "Información del puesto " & puesto
        ventanaInfo.Size = New Size(700, 500)

        ' Label resumen general
        Dim lblResumen As New Label() With {
        .Dock = DockStyle.Top,
        .Height = 50,
        .Font = New Font("Segoe UI", 14, FontStyle.Bold),
        .TextAlign = ContentAlignment.MiddleCenter
    }

        ' Crear ListView
        Dim lvInfo As New ListView() With {
        .Dock = DockStyle.Fill,
        .View = View.Details,
        .FullRowSelect = True,
        .GridLines = True,
        .SmallImageList = New ImageList()
    }

        ' Íconos verde y rojo
        lvInfo.SmallImageList.Images.Add("ok", SystemIcons.Information)
        lvInfo.SmallImageList.Images.Add("fail", SystemIcons.Error)
        ' Definir columnas
        lvInfo.Columns.Add("Dato", 200, HorizontalAlignment.Left)
        lvInfo.Columns.Add("Valor", 450, HorizontalAlignment.Left)
        ' Determinar si hay problemas
        Dim hayProblemas As Boolean = False
        ' Cargar datos
        For Each kvp In datos
            Dim item As New ListViewItem(kvp.Key)
            item.SubItems.Add(kvp.Value.Item1)
            If kvp.Value.Item2 >= 0 Then
                item.ImageIndex = kvp.Value.Item2
                If kvp.Value.Item2 = 1 Then hayProblemas = True
            End If
            lvInfo.Items.Add(item)
        Next
        ' Resumen general
        If hayProblemas Then
            lblResumen.Text = "Equipo con problemas detectados"
            lblResumen.ForeColor = Color.Red
        Else
            lblResumen.Text = "Equipo en buen estado"
            lblResumen.ForeColor = Color.Green
        End If
        ' Botón cerrar
        Dim btnCerrar As New Button() With {
        .Text = "Cerrar",
        .Dock = DockStyle.Bottom
    }
        AddHandler btnCerrar.Click, Sub() ventanaInfo.Close()
        ' Agregar controles al formulario
        ventanaInfo.Controls.Add(lvInfo)
        ventanaInfo.Controls.Add(lblResumen)
        ventanaInfo.Controls.Add(btnCerrar)
        ' Mostrar ventana
        ventanaInfo.Show()
    End Sub




    'Validar Casilla (TextBox sin CRRO; Label muestra CRRO<casilla> y linkea a \\server\Notes$\CRRO<casilla>)
    Private Sub TextBoxCasilla_TextChanged(sender As Object, e As EventArgs) Handles TextBoxCasilla.TextChanged
        If _suspendEvents Then Exit Sub
        Dim servidorCompleto As String = LabelEstadoServidor.Text.Trim()
        ' Ahora tomamos la casilla tal cual la escribe el usuario (SIN CRRO)
        Dim casillaIngresada As String = TextBoxCasilla.Text.Trim()

        ' Validar formato: solo letras/números y hasta 5 caracteres (sin prefijo CRRO)
        If Regex.IsMatch(casillaIngresada, "^[A-Za-z0-9]{1,5}$") Then
            If Not String.IsNullOrEmpty(servidorCompleto) AndAlso Not servidorCompleto.StartsWith("Esperando", StringComparison.OrdinalIgnoreCase) Then
                Try
                    ' Construimos la casilla con prefijo para mostrar y para la ruta UNC
                    Dim casillaConPrefijo As String = "CRRO" & casillaIngresada.ToUpper()
                    Dim rutaCasilla As String = "\\" & servidorCompleto & "\Notes$\" & casillaConPrefijo

                    If Directory.Exists(rutaCasilla) Then
                        ' Visual: mostrar CRROxxxxx en el Label
                        LabelEstadoCasilla.Text = casillaConPrefijo
                        LabelEstadoCasilla.ForeColor = Color.Green
                        LabelEstadoCasilla.Tag = rutaCasilla              ' Ruta: \\SMF0XSCXXXX\Notes$\CRROxxxxx
                        LabelEstadoCasilla.Cursor = Cursors.Hand
                        LabelEstadoCasilla.Font = New Font(LabelEstadoCasilla.Font, FontStyle.Underline)
                        ToolTip1.SetToolTip(LabelEstadoCasilla, rutaCasilla)
                    Else
                        LabelEstadoCasilla.Text = "Casilla no encontrada en " & servidorCompleto
                        LabelEstadoCasilla.ForeColor = Color.Orange
                        LabelEstadoCasilla.Tag = Nothing
                        LabelEstadoCasilla.Cursor = Cursors.Default
                        LabelEstadoCasilla.Font = New Font(LabelEstadoCasilla.Font, FontStyle.Regular)
                        ToolTip1.SetToolTip(LabelEstadoCasilla, "")
                    End If
                Catch ex As Exception
                    LabelEstadoCasilla.Text = "Error al validar casilla: " & ex.Message
                    LabelEstadoCasilla.ForeColor = Color.Red
                    LabelEstadoCasilla.Tag = Nothing
                    LabelEstadoCasilla.Cursor = Cursors.Default
                    LabelEstadoCasilla.Font = New Font(LabelEstadoCasilla.Font, FontStyle.Regular)
                    ToolTip1.SetToolTip(LabelEstadoCasilla, "")
                End Try
            Else
                LabelEstadoCasilla.Text = "Servidor fuera de red"
                LabelEstadoCasilla.ForeColor = Color.Red
                LabelEstadoCasilla.Tag = Nothing
                LabelEstadoCasilla.Cursor = Cursors.Default
                LabelEstadoCasilla.Font = New Font(LabelEstadoCasilla.Font, FontStyle.Regular)
                ToolTip1.SetToolTip(LabelEstadoCasilla, "")
            End If
        Else
            ' Vuelve al estado de espera si no cumple la regla (o está vacío)
            LabelEstadoCasilla.Text = "Esperando casilla válida..."
            LabelEstadoCasilla.ForeColor = Color.Gray
            LabelEstadoCasilla.Tag = Nothing
            LabelEstadoCasilla.Cursor = Cursors.Default
            LabelEstadoCasilla.Font = New Font(LabelEstadoCasilla.Font, FontStyle.Regular)
            ToolTip1.SetToolTip(LabelEstadoCasilla, "")
        End If

        ' Actualizar info general y registrar acción (se mantiene tu lógica)
        ActualizarInfoGeneral()

    End Sub



    ' Evento LabelEstadoCasilla: abre la ruta completa de la casilla
    Private Sub LabelEstadoCasilla_Click(sender As Object, e As EventArgs) Handles LabelEstadoCasilla.Click
        If LabelEstadoCasilla.ForeColor = Color.Green AndAlso LabelEstadoCasilla.Tag IsNot Nothing Then
            Try
                Process.Start("explorer.exe", LabelEstadoCasilla.Tag.ToString())
            Catch ex As Exception
                MessageBox.Show("No se pudo abrir la casilla: " & ex.Message)
            End Try
        End If
    End Sub




    ' Actualiza la info consolidada
    Private Sub ActualizarInfoGeneral(Optional datosSucursal As Tuple(Of String, String, String) = Nothing)
        ' Si se pasan datos nuevos de sucursal, los guardamos en las variables persistentes

        Dim sucursalActual As String = ""
        If ComboBoxSucursales.SelectedItem IsNot Nothing Then
            sucursalActual = ComboBoxSucursales.SelectedItem.ToString().PadLeft(4, "0"c)
        End If

        ' Si la sucursal es "0000", no mostrar datos persistentes
        If sucursalActual = "0000" Then
            sucursalTipo = ""
            sucursalNombre = ""
            sucursalDependeDe = ""
        End If
        If datosSucursal IsNot Nothing Then
            sucursalTipo = datosSucursal.Item1
            sucursalNombre = datosSucursal.Item2
            sucursalDependeDe = datosSucursal.Item3
        End If

        ' Servidor
        Dim servidorBase As String = LabelEstadoServidor.Text.Trim()

        ' Legajo
        Dim legajoActual As String = TextBoxLegajo.Text.Trim()
        If legajoActual.Length > 0 Then
            legajoActual = Char.ToUpper(legajoActual(0)) & legajoActual.Substring(1)
        End If

        ' Puesto
        Dim puestoActual As String = TextBoxPuesto.Text.Trim()



        ' Casilla
        Dim casillaActual As String = TextBoxCasilla.Text.Trim().ToUpper()
        If casillaActual <> "" AndAlso Not casillaActual.StartsWith("CRRO") Then
            casillaActual = "CRRO" & casillaActual
        End If

        Dim casillaFormateada As String = ""
        If sucursalActual <> "" AndAlso casillaActual <> "" Then
            casillaFormateada = sucursalActual & casillaActual.Replace("CRRO", "")
        End If

        ' Construir la información en varias líneas
        Dim infoCompleta As String =
        $"Servidor: {servidorBase}" & Environment.NewLine &
        $"Tipo: {sucursalTipo}" & Environment.NewLine &
        $"Nombre: {sucursalNombre}" & Environment.NewLine &
        $"Depende de: {sucursalDependeDe}" & Environment.NewLine &
        $"Legajo: {legajoActual}" & Environment.NewLine &
        $"Puesto: {puestoActual}" & Environment.NewLine &
        $"Sucursal: {sucursalActual}" & Environment.NewLine &
        $"Casilla: {casillaFormateada}"

        ' Mostrar en el TextBox de info general
        TextBoxInfoGeneral.Text = infoCompleta
    End Sub








    ' Bloquear caracteres inválidos
    Private Sub TextBoxCasilla_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBoxCasilla.KeyPress
        If Char.IsControl(e.KeyChar) Then Exit Sub
        ' Sólo letras y números
        If Not Char.IsLetterOrDigit(e.KeyChar) Then
            e.Handled = True
            Exit Sub
        End If

        Dim tb = DirectCast(sender, TextBox)

        Dim seleccionLargo = tb.SelectionLength
        If tb.TextLength - seleccionLargo >= 5 Then
            e.Handled = True
        End If
    End Sub












    'Funcion Actualizar
    Private Sub BtnActualizar_Click(sender As Object, e As EventArgs) Handles BtnActualizar.Click
        ' Forzar actualización de cada campo
        ComboBoxSucursales_SelectedIndexChanged(ComboBoxSucursales, EventArgs.Empty)
        TextBoxPuesto_TextChanged(TextBoxPuesto, EventArgs.Empty)
        TextBoxLegajo_TextChanged(TextBoxLegajo, EventArgs.Empty)
        TextBoxCasilla_TextChanged(TextBoxCasilla, EventArgs.Empty)

        ' Abrir ventana de información si el checkbox está marcado
        If CheckBoxInformacion.Checked Then
            Dim puestoActual As String = TextBoxPuesto.Text.Trim()
            If Not String.IsNullOrEmpty(puestoActual) AndAlso HacerPing(puestoActual) Then
                Dim datos As Dictionary(Of String, Tuple(Of String, Integer)) = ObtenerInfoHardware(puestoActual)
                MostrarInfoEnVentana(puestoActual, datos, TextBoxLegajo)
            End If
        End If
    End Sub

    Private Sub BtnLimpiar_Click(sender As Object, e As EventArgs) Handles BtnLimpiar.Click
        ResetApp()
    End Sub


    Private Sub ResetApp()
        _suspendEvents = True
        Try
            ' 1) Limpiar campos visuales sin disparar validaciones ni pings
            LimpiarControles(Me)

            ' 2) Restaurar labels a estado “inicio”
            ResetLabelEstado(LabelEstadoServidor, "Esperando Servidor...")
            ResetLabelEstado(LabelEstadoPuesto, "Esperando Puesto válido...")
            ResetLabelEstado(LabelEstadoLegajo, "Esperando legajo válido...")
            ResetLabelEstado(LabelEstadoCasilla, "Esperando casilla válida...")

            ' 3) Reiniciar ComboBox de sucursales al “0000”
            If ComboBoxSucursales.DataSource IsNot Nothing Then
                ComboBoxSucursales.SelectedIndex = 0
            End If

            ' 4) Valores iniciales específicos
            TextBoxPuesto.MaxLength = 11
            CheckBoxInformacion.Checked = False

            ' 5) Actualizar información consolidada
            ActualizarInfoGeneral()

        Finally
            _suspendEvents = False
        End Try
    End Sub












    ' Evita ejecutar eventos mientras se limpia / re-inicializa
    Private _suspendEvents As Boolean = False

    ' Helper: resetear un label a estado “espera”
    Private Sub ResetLabelEstado(lbl As Label, texto As String)
        lbl.Text = texto
        lbl.ForeColor = Color.Gray
        lbl.Tag = Nothing
        lbl.Cursor = Cursors.Default
        lbl.Font = New Font(lbl.Font, FontStyle.Regular)
        ToolTip1.SetToolTip(lbl, "")
    End Sub

    ' Limpia recursivamente controles de texto, combos, checks, radios
    Private Sub LimpiarControles(container As Control)
        For Each ctrl As Control In container.Controls
            If TypeOf ctrl Is TextBox Then
                CType(ctrl, TextBox).Clear()
            ElseIf TypeOf ctrl Is ComboBox Then
                CType(ctrl, ComboBox).SelectedIndex = -1
            ElseIf TypeOf ctrl Is CheckBox Then
                CType(ctrl, CheckBox).Checked = False
            ElseIf TypeOf ctrl Is RadioButton Then
                CType(ctrl, RadioButton).Checked = False
            End If
            ' Si el control tiene hijos, limpiar también
            If ctrl.HasChildren Then
                LimpiarControles(ctrl)
            End If
        Next
    End Sub




    'Funcion Salir
    Private Sub BtnSalir_Click(sender As Object, e As EventArgs) Handles BtnSalir.Click
        Dim respuesta As DialogResult = MessageBox.Show("¿Desea salir de la aplicación?",
                                                        "Confirmar salida",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Question)
        If respuesta = DialogResult.Yes Then
            Application.Exit()
        End If
    End Sub
End Class
