Imports System.IO
Imports System.Net.NetworkInformation
Imports System.Management
Imports System.Diagnostics
Imports System.Text.RegularExpressions
Imports System.Net


Public Class Form1
    'Carga
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LabelEstadoRed.Text = "Esperando puesto..."
        LabelEstadoRed.ForeColor = Color.Gray

        LabelEstadoLegajo.Text = "Esperando legajo..."
        LabelEstadoLegajo.ForeColor = Color.Gray

        LabelEstadoCasilla.Text = "Esperando Casilla..."
        LabelEstadoLegajo.ForeColor = Color.Gray

        ' Inicializar el CheckBox desmarcado
        CheckBoxInformacion.Checked = False
    End Sub

    'Validar Servidor
    Private Sub ComboBoxSucursal_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxSucursal.SelectedIndexChanged
        ProcesarSucursal()
    End Sub

    Private Sub ComboBoxSucursal_TextChanged(sender As Object, e As EventArgs) Handles ComboBoxSucursal.TextChanged
        ' También procesamos cuando el usuario escribe manualmente
        ProcesarSucursal()
    End Sub

    Private Sub ProcesarSucursal()
        Dim textoIngresado As String = ComboBoxSucursal.Text.Trim()

        If String.IsNullOrEmpty(textoIngresado) Then Exit Sub

        ' Intentar convertir a número
        Dim sucursalNumero As Integer
        If Integer.TryParse(textoIngresado, sucursalNumero) Then
            ' Buscar coincidencia exacta en la colección
            Dim sucursalFormateada As String = sucursalNumero.ToString().PadLeft(4, "0"c)
            Dim encontrado As Boolean = False

            For Each item As Object In ComboBoxSucursal.Items
                If item.ToString().PadLeft(4, "0"c) = sucursalFormateada Then
                    ComboBoxSucursal.SelectedItem = item
                    encontrado = True
                    Exit For
                End If
            Next

            ' Si no se encontró, buscar el más cercano
            If Not encontrado Then
                Dim listaSucursales As New List(Of Integer)
                For Each item As Object In ComboBoxSucursal.Items
                    listaSucursales.Add(Integer.Parse(item.ToString()))
                Next

                ' Buscar el número más cercano
                Dim masCercano As Integer = listaSucursales.OrderBy(Function(x) Math.Abs(x - sucursalNumero)).First()
                ComboBoxSucursal.SelectedItem = masCercano.ToString().PadLeft(4, "0"c)
            End If

            ' Detectar servidor operativo
            Dim servidorActivo As String = DetectarSMF(ComboBoxSucursal.SelectedItem.ToString().PadLeft(4, "0"c))

            ' Actualizar el LabelServidor
            If servidorActivo <> "" Then
                LabelEstadoRed.Text = servidorActivo & "SC" & ComboBoxSucursal.SelectedItem.ToString().PadLeft(4, "0"c)
                LabelEstadoRed.ForeColor = Color.Green
            Else
                LabelEstadoRed.Text = "Ningún servidor responde para sucursal " & ComboBoxSucursal.SelectedItem.ToString().PadLeft(4, "0"c)
                LabelEstadoRed.ForeColor = Color.Red
            End If

            ' Actualizar info general y registrar acción
            ActualizarInfoGeneral()
            RegistrarAccionHistorial("Servidor", TextBoxInfoGeneral.Text)
        End If
    End Sub


    'Informa SMF
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

    'Ping de puesto no muestra nada
    Private Function HacerPing(host As String) As Boolean
        Try
            Dim pingSender As New Net.NetworkInformation.Ping()
            Dim reply As Net.NetworkInformation.PingReply = pingSender.Send(host, 1000)
            Return reply.Status = Net.NetworkInformation.IPStatus.Success
        Catch
            Return False
        End Try
    End Function

    'Validar y mostrar TextboxPuesto
    Private Sub TextBoxPuesto_TextChanged(sender As Object, e As EventArgs) Handles TextBoxPuesto.TextChanged
        Dim puesto As String = TextBoxPuesto.Text.Trim()

        ' Validar formato básico
        If puesto.Length >= 7 AndAlso Regex.IsMatch(puesto, "^[A-Z]\d{4}SC\d+$") Then
            If HacerPing(puesto) Then
                LabelEstadoRed.Text = "En red"
                LabelEstadoRed.ForeColor = Color.Green
                ' Obtener datos y mostrar ventana
                Dim datos As Dictionary(Of String, Tuple(Of String, Integer)) = ObtenerInfoHardware(puesto)
                ' Solo abrir ventana si el CheckBox está marcado
                If CheckBoxInformacion.Checked Then
                    MostrarInfoEnVentana(puesto, datos, TextBoxLegajo)
                End If

            Else
                LabelEstadoRed.Text = "Fuera de red"
                LabelEstadoRed.ForeColor = Color.Red
            End If
        Else
            LabelEstadoRed.Text = "Esperando puesto válido..."
            LabelEstadoRed.ForeColor = Color.Gray
        End If
        ' ... tu validación de puesto ... 
        ActualizarInfoGeneral()
        RegistrarAccionHistorial("Red", TextBoxInfoGeneral.Text)
    End Sub

    'Busqueda Informacion de Puesto
    Private Function ObtenerInfoHardware(puesto As String) As Dictionary(Of String, Tuple(Of String, Integer))
        Dim datos As New Dictionary(Of String, Tuple(Of String, Integer))
        Try
            Dim scope As New ManagementScope("\\" & puesto & "\root\cimv2")
            scope.Connect()

            ' SO y uptime
            Dim searcherOS As New ManagementObjectSearcher(scope, New ObjectQuery("SELECT Caption, Version, LastBootUpTime FROM Win32_OperatingSystem"))
            For Each obj As ManagementObject In searcherOS.Get()
                datos("Sistema operativo") = Tuple.Create(If(obj("Caption"), "N/A").ToString(), -1)
                datos("Versión") = Tuple.Create(If(obj("Version"), "N/A").ToString(), -1)
                If obj("LastBootUpTime") IsNot Nothing Then
                    Dim bootTime As DateTime = ManagementDateTimeConverter.ToDateTime(obj("LastBootUpTime").ToString())
                    datos("Último reinicio") = Tuple.Create(bootTime.ToString(), -1)
                    datos("Tiempo de actividad") = Tuple.Create((DateTime.Now - bootTime).ToString(), -1)
                End If
            Next

            ' Usuario logueado
            Dim searcherUser As New ManagementObjectSearcher(scope, New ObjectQuery("SELECT UserName FROM Win32_ComputerSystem"))
            For Each obj As ManagementObject In searcherUser.Get()
                datos("Usuario logueado") = Tuple.Create(If(obj("UserName"), "N/A").ToString(), -1)
            Next

            ' Marca, modelo, serial
            Dim searcherProduct As New ManagementObjectSearcher(scope, New ObjectQuery("SELECT Vendor, Name, IdentifyingNumber FROM Win32_ComputerSystemProduct"))
            For Each obj As ManagementObject In searcherProduct.Get()
                datos("Marca") = Tuple.Create(If(obj("Vendor"), "N/A").ToString(), -1)
                datos("Modelo") = Tuple.Create(If(obj("Name"), "N/A").ToString(), -1)
                datos("Serial") = Tuple.Create(If(obj("IdentifyingNumber"), "N/A").ToString(), -1)
            Next

            ' IP
            Dim searcherIP As New ManagementObjectSearcher(scope, New ObjectQuery("SELECT IPAddress FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = True"))
            For Each obj As ManagementObject In searcherIP.Get()
                If obj("IPAddress") IsNot Nothing Then
                    Dim ipAddresses As String() = CType(obj("IPAddress"), String())
                    If ipAddresses.Length > 0 Then datos("Dirección IP") = Tuple.Create(ipAddresses(0), -1)
                End If
            Next

            ' Memoria RAM total/libre
            Dim searcherRAM As New ManagementObjectSearcher(scope, New ObjectQuery("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem"))
            For Each obj As ManagementObject In searcherRAM.Get()
                Dim totalRAM As Double = CDbl(obj("TotalVisibleMemorySize")) / 1024
                Dim freeRAM As Double = CDbl(obj("FreePhysicalMemory")) / 1024
                datos("Memoria RAM total") = Tuple.Create(Math.Round(totalRAM / 1024, 2).ToString() & " GB", -1)
                datos("Memoria RAM libre") = Tuple.Create(Math.Round(freeRAM / 1024, 2).ToString() & " GB", -1)
            Next

            ' Módulos de memoria física
            Dim searcherMem As New ManagementObjectSearcher(scope, New ObjectQuery("SELECT Capacity, Manufacturer, PartNumber, SerialNumber FROM Win32_PhysicalMemory"))
            Dim idx As Integer = 1
            For Each obj As ManagementObject In searcherMem.Get()
                Dim capGB As Double = CDbl(obj("Capacity")) / (1024 ^ 3)
                datos("Módulo RAM " & idx) = Tuple.Create(capGB.ToString("0.00") & " GB - " &
                                                             If(obj("Manufacturer"), "N/A").ToString() & " - " &
                                                             If(obj("PartNumber"), "N/A").ToString() & " - SN:" & If(obj("SerialNumber"), "N/A").ToString(), -1)
                idx += 1
            Next

            ' Discos físicos con estado
            Dim searcherDrive As New ManagementObjectSearcher(scope, New ObjectQuery("SELECT Model, Size, Status FROM Win32_DiskDrive"))
            Dim dIdx As Integer = 1
            For Each obj As ManagementObject In searcherDrive.Get()
                Dim sizeGB As Double = CDbl(obj("Size")) / (1024 ^ 3)
                Dim status As String = If(obj("Status"), "Desconocido").ToString()
                Dim iconIndex As Integer = If(status.ToUpper() = "OK", 0, 1) ' 0=verde, 1=rojo
                datos("Disco físico " & dIdx) = Tuple.Create(If(obj("Model"), "N/A").ToString() & " - " &
                                                                 sizeGB.ToString("0.00") & " GB - Estado: " & status, iconIndex)
                dIdx += 1
            Next

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

        ' Botón abrir disco C$
        Dim btnAbrirC As New Button() With {
        .Text = "Abrir disco C$",
        .Dock = DockStyle.Bottom
    }
        AddHandler btnAbrirC.Click,
        Sub()
            Try
                Process.Start("explorer.exe", "\\" & puesto & "\C$")
            Catch ex As Exception
                MessageBox.Show("No se pudo abrir el disco C$: " & ex.Message)
            End Try
            RegistrarAccionHistorial("Abrir Disco C", "Ruta: C:\")
        End Sub

        ' Botón abrir carpeta de legajo
        Dim btnAbrirLegajo As New Button() With {
        .Text = "Abrir carpeta de legajo",
        .Dock = DockStyle.Bottom,
        .Enabled = False
    }

        ' Habilitar/deshabilitar según lo que escriba el usuario en TextBoxLegajo
        AddHandler txtLegajo.TextChanged,
        Sub()
            Dim legajoActual As String = txtLegajo.Text.Trim()
            If Regex.IsMatch(legajoActual, "^[NTE]\d{5}$") Then
                btnAbrirLegajo.Enabled = True
                btnAbrirLegajo.Text = "Abrir carpeta " & legajoActual
            Else
                btnAbrirLegajo.Enabled = False
                btnAbrirLegajo.Text = "Abrir carpeta de legajo"
            End If
        End Sub

        ' Evento del botón abrir carpeta de legajo
        AddHandler btnAbrirLegajo.Click,
        Sub()
            Try
                Dim legajoActual As String = txtLegajo.Text.Trim()
                Dim rutaBase As String = "\\" & puesto & "\c$\Users\"
                Dim carpetas As String() = Directory.GetDirectories(rutaBase, legajoActual & ".SUC*")

                If carpetas.Length > 0 Then
                    Dim carpetaElegida As String = carpetas.OrderByDescending(
                        Function(c) Path.GetFileName(c).Count(Function(ch) Char.IsDigit(ch))
                    ).First()
                    Process.Start("explorer.exe", carpetaElegida)
                Else
                    Dim carpetaDirecta As String = Path.Combine(rutaBase, legajoActual)
                    If Directory.Exists(carpetaDirecta) Then
                        Process.Start("explorer.exe", carpetaDirecta)
                    Else
                        MessageBox.Show("No se encontró carpeta para el legajo en " & rutaBase)
                    End If
                End If
            Catch ex As Exception
                MessageBox.Show("Error al abrir carpeta de usuario: " & ex.Message)
            End Try

        End Sub


        ' Botón cerrar
        Dim btnCerrar As New Button() With {
        .Text = "Cerrar",
        .Dock = DockStyle.Bottom
    }
        AddHandler btnCerrar.Click, Sub() ventanaInfo.Close()

        ' Agregar controles al formulario
        ventanaInfo.Controls.Add(lvInfo)
        ventanaInfo.Controls.Add(lblResumen)
        ventanaInfo.Controls.Add(btnAbrirLegajo)
        ventanaInfo.Controls.Add(btnAbrirC)
        ventanaInfo.Controls.Add(btnCerrar)

        ' Mostrar ventana
        ventanaInfo.Show()
    End Sub


    ' Evento cuando cambia el texto del TextBoxLegajo

    Private Sub TextBoxLegajo_TextChanged(sender As Object, e As EventArgs) Handles TextBoxLegajo.TextChanged
        Dim puestoActual As String = TextBoxPuesto.Text.Trim()
        Dim legajoActual As String = TextBoxLegajo.Text.Trim()

        If Regex.IsMatch(legajoActual, "^[NTE]\d{5}$") Then
            If HacerPing(puestoActual) Then
                Try
                    Dim rutaBase As String = "\\" & puestoActual & "\c$\Users\"
                    Dim carpetas As String() = Directory.GetDirectories(rutaBase, legajoActual & ".SUC*")

                    If carpetas.Length > 0 Then
                        LabelEstadoLegajo.Text = "Legajo encontrado (variante .SUC)"
                        LabelEstadoLegajo.ForeColor = Color.Green
                    Else
                        Dim carpetaDirecta As String = Path.Combine(rutaBase, legajoActual)
                        If Directory.Exists(carpetaDirecta) Then
                            LabelEstadoLegajo.Text = "Legajo encontrado"
                            LabelEstadoLegajo.ForeColor = Color.Green
                        Else
                            LabelEstadoLegajo.Text = "Legajo no encontrado"
                            LabelEstadoLegajo.ForeColor = Color.Orange
                        End If
                    End If
                Catch ex As Exception
                    LabelEstadoLegajo.Text = "Error al validar legajo"
                    LabelEstadoLegajo.ForeColor = Color.Red
                End Try
            Else
                LabelEstadoLegajo.Text = "Puesto fuera de red"
                LabelEstadoLegajo.ForeColor = Color.Red
            End If
        Else
            LabelEstadoLegajo.Text = "Esperando legajo válido..."
            LabelEstadoLegajo.ForeColor = Color.Gray
        End If
        ' ... tu validación de puesto ... 
        ActualizarInfoGeneral()
    End Sub


    'Validar Casilla
    Private Sub TextBoxCasilla_TextChanged(sender As Object, e As EventArgs) Handles TextBoxCasilla.TextChanged
        Dim servidorBase As String = LabelEstadoRed.Text.Trim()
        Dim casillaIngresada As String = TextBoxCasilla.Text.Trim().ToUpper()

        ' Si el usuario no escribió CRRO, lo agregamos automáticamente en el TextBox
        If casillaIngresada <> "" AndAlso Not casillaIngresada.StartsWith("CRRO") Then
            casillaIngresada = "CRRO" & casillaIngresada
            ' Actualizamos el TextBox para que el usuario vea el prefijo CRRO
            TextBoxCasilla.Text = casillaIngresada
            TextBoxCasilla.SelectionStart = TextBoxCasilla.Text.Length ' mover cursor al final
        End If

        ' Validar formato: CRRO + letras/números (hasta 5 caracteres)
        If Regex.IsMatch(casillaIngresada, "^CRRO[A-Z0-9]{3,5}$") Then
            If Not String.IsNullOrEmpty(servidorBase) AndAlso HacerPing(servidorBase) Then
                Try
                    Dim rutaCasilla As String = "\\" & servidorBase & "\notes$\" & casillaIngresada

                    If Directory.Exists(rutaCasilla) Then
                        LabelEstadoCasilla.Text = "Casilla encontrada: " & casillaIngresada
                        LabelEstadoCasilla.ForeColor = Color.Green
                        LabelEstadoCasilla.Tag = rutaCasilla
                        LabelEstadoCasilla.Cursor = Cursors.Hand
                        LabelEstadoCasilla.Font = New Font(LabelEstadoCasilla.Font, FontStyle.Underline)
                        ToolTip1.SetToolTip(LabelEstadoCasilla, rutaCasilla)
                    Else
                        LabelEstadoCasilla.Text = "Casilla no encontrada en " & servidorBase
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
            LabelEstadoCasilla.Text = "Esperando casilla válida..."
            LabelEstadoCasilla.ForeColor = Color.Gray
            LabelEstadoCasilla.Tag = Nothing
            LabelEstadoCasilla.Cursor = Cursors.Default
            LabelEstadoCasilla.Font = New Font(LabelEstadoCasilla.Font, FontStyle.Regular)
            ToolTip1.SetToolTip(LabelEstadoCasilla, "")
        End If
        ' ... tu validación de puesto ... 
        ActualizarInfoGeneral()
        RegistrarAccionHistorial("Casilla", TextBoxInfoGeneral.Text)
    End Sub

    'Evento LabelCasilla
    Private Sub LabelEstadoCasilla_Click(sender As Object, e As EventArgs) Handles LabelEstadoCasilla.Click
        If LabelEstadoCasilla.ForeColor = Color.Green AndAlso LabelEstadoCasilla.Tag IsNot Nothing Then
            Try
                Process.Start("explorer.exe", LabelEstadoCasilla.Tag.ToString())
            Catch ex As Exception
                MessageBox.Show("No se pudo abrir la casilla: " & ex.Message)
            End Try
        End If
    End Sub






    ' Método para actualizar la info consolidada
    Private Sub ActualizarInfoGeneral()
        Dim servidorBase As String = LabelEstadoRed.Text.Trim()

        ' Legajo: primera letra en mayúscula
        Dim legajoActual As String = TextBoxLegajo.Text.Trim()
        If legajoActual.Length > 0 Then
            legajoActual = Char.ToUpper(legajoActual(0)) & legajoActual.Substring(1)
        End If

        ' Puesto
        Dim puestoActual As String = TextBoxPuesto.Text.Trim()

        ' Sucursal
        Dim sucursalActual As String = ""
        If ComboBoxSucursal.SelectedItem IsNot Nothing Then
            sucursalActual = ComboBoxSucursal.SelectedItem.ToString().PadLeft(4, "0"c)
        End If

        ' Casilla: siempre con CRRO y en mayúsculas
        Dim casillaActual As String = TextBoxCasilla.Text.Trim().ToUpper()
        If casillaActual <> "" AndAlso Not casillaActual.StartsWith("CRRO") Then
            casillaActual = "CRRO" & casillaActual
        End If

        ' Formato casilla: nro sucursal + sufijo de casilla (sin CRRO)
        Dim casillaFormateada As String = ""
        If sucursalActual <> "" AndAlso casillaActual <> "" Then
            casillaFormateada = sucursalActual & casillaActual.Replace("CRRO", "")
        End If

        ' Construir la información en varias líneas
        Dim infoCompleta As String =
        $"Servidor: {servidorBase}" & Environment.NewLine &
        $"Legajo: {legajoActual}" & Environment.NewLine &
        $"Puesto: {puestoActual}" & Environment.NewLine &
        $"Casilla: {casillaFormateada}"

        ' Mostrar en el TextBox de info general
        TextBoxInfoGeneral.Text = infoCompleta
    End Sub

    'Evento actualizar servidor
    Private Sub LabelServidor_TextChanged(sender As Object, e As EventArgs) Handles LabelEstadoRed.TextChanged
        ' Si el servidor cambia, refrescamos la info
        ActualizarInfoGeneral()
    End Sub




    'Funcion Retroceder
    Private Sub BtnRetroceder_Click(sender As Object, e As EventArgs) Handles BtnRetroceder.Click
        ' Tomar la info consolidada del TextBoxInfoGeneral
        Dim lineas() As String = TextBoxInfoGeneral.Text.Split(Environment.NewLine)

        For Each linea As String In lineas
            If linea.StartsWith("Servidor:") Then
                LabelEstadoRed.Text = linea.Replace("Servidor:", "").Trim()

            ElseIf linea.StartsWith("Legajo:") Then
                TextBoxLegajo.Text = linea.Replace("Legajo:", "").Trim()

            ElseIf linea.StartsWith("Puesto:") Then
                TextBoxPuesto.Text = linea.Replace("Puesto:", "").Trim()

            ElseIf linea.StartsWith("Casilla:") Then
                Dim casillaFormateada As String = linea.Replace("Casilla:", "").Trim()

                ' La casilla formateada es sucursal + sufijo (ej: 3590POP)
                If casillaFormateada.Length >= 4 Then
                    Dim sucursal As String = casillaFormateada.Substring(0, 4)
                    Dim sufijo As String = casillaFormateada.Substring(4)

                    ' Actualizar ComboBoxSucursal si existe ese valor
                    For i As Integer = 0 To ComboBoxSucursal.Items.Count - 1
                        If ComboBoxSucursal.Items(i).ToString().PadLeft(4, "0"c) = sucursal Then
                            ComboBoxSucursal.SelectedIndex = i
                            Exit For
                        End If
                    Next

                    ' Actualizar TextBoxCasilla con CRRO + sufijo
                    TextBoxCasilla.Text = "CRRO" & sufijo.ToUpper()
                End If
            End If
        Next

        ' Refrescar automáticamente los labels de estado
        ComboBoxSucursal_SelectedIndexChanged(ComboBoxSucursal, EventArgs.Empty)
        TextBoxPuesto_TextChanged(TextBoxPuesto, EventArgs.Empty)
        TextBoxLegajo_TextChanged(TextBoxLegajo, EventArgs.Empty)
        TextBoxCasilla_TextChanged(TextBoxCasilla, EventArgs.Empty)

        ' Mostrar mensaje de confirmación
        MessageBox.Show("Campos restaurados desde la información consolidada.", "Retroceder", MessageBoxButtons.OK, MessageBoxIcon.Information)

        RegistrarAccionHistorial("Retroceder", TextBoxInfoGeneral.Text)
    End Sub






    'Funcion registro historial
    Private Sub RegistrarAccionHistorial(nombreAccion As String, detalle As String)
        Dim separador As String = New String("-"c, 40)
        Dim entradaHistorial As String =
        $"Acción: {nombreAccion}" & Environment.NewLine &
        $"Fecha/Hora: {DateTime.Now}" & Environment.NewLine &
        detalle & Environment.NewLine &
        separador & Environment.NewLine

        TextBoxHistorial.AppendText(entradaHistorial)
    End Sub


    'Funcion Actualizar
    Private Sub BtnActualizar_Click(sender As Object, e As EventArgs) Handles BtnActualizar.Click
        ' Forzar actualización de cada campo
        ComboBoxSucursal_SelectedIndexChanged(ComboBoxSucursal, EventArgs.Empty)
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
        ' Guardar en historial 
        RegistrarAccionHistorial("Actualizar", TextBoxInfoGeneral.Text)
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

    'Funcion Reset
    Private Sub BtnReset_Click(sender As Object, e As EventArgs) Handles BtnReset.Click
        ' Llamamos a la función que limpia todos los controles del formulario
        LimpiarControles(Me)

        ' Resetear el LabelServidor al valor inicial
        LabelEstadoRed.Text = "SMF00SC0000"
        LabelEstadoRed.ForeColor = Color.Black
    End Sub

    ' Función recursiva para limpiar controles dentro de cualquier contenedor
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

            ' Si el control tiene hijos (ej: GroupBox, Panel, TabPage), limpiar también
            If ctrl.HasChildren Then
                LimpiarControles(ctrl)
            End If
        Next
    End Sub

    'Funcion limpiar historial
    Private Sub BtnLimpiarHistorial_Click(sender As Object, e As EventArgs) Handles BtnLimpiarHistorial.Click
        TextBoxHistorial.Clear()
        RegistrarAccionHistorial("Historial", "Se limpió el historial de acciones.")
    End Sub

    'crear diccionario



End Class
