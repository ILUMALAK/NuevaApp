Imports System
Imports System.IO
Imports System.Text
Imports System.Management
Imports System.DirectoryServices
Imports System.Threading.Tasks

Partial Public Class VentanaAvanzada

    Private ReadOnly _equipoInicial As String

    '--- Controles (creados en BuildUI)
    Private txtEquipo As TextBox
    Private btnAuditar As Button
    Private btnGuardar As Button
    Private btnBuscarAD As Button
    Private lstAD As ListBox
    Private lblEstado As Label
    Private txtSalida As TextBox

    Public Sub New(Optional equipo As String = "")
        InitializeComponent()   ' del Designer
        _equipoInicial = equipo
        BuildUI()               ' UI por código
    End Sub

    '------------------------------------------------
    ' UI por código (NO duplicar InitializeComponent)
    '------------------------------------------------
    Private Sub BuildUI()
        Me.Text = "Diagnóstico avanzado"
        Me.Size = New Size(900, 650)
        Me.MinimumSize = New Size(820, 560)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.KeyPreview = True

        ' --- Fila superior
        Dim pnlTop As New FlowLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 42,
            .FlowDirection = FlowDirection.LeftToRight,
            .Padding = New Padding(10, 8, 10, 8),
            .WrapContents = False
        }

        Dim lblEquipo As New Label With {.Text = "Equipo remoto:", .AutoSize = True, .Margin = New Padding(0, 6, 6, 0)}
        txtEquipo = New TextBox With {.Width = 300}
        btnAuditar = New Button With {.Text = "Auditar equipo", .AutoSize = True}
        btnGuardar = New Button With {.Text = "Guardar reporte", .AutoSize = True}

        AddHandler btnAuditar.Click, AddressOf BtnAuditar_Click
        AddHandler btnGuardar.Click, AddressOf BtnGuardar_Click
        AddHandler txtEquipo.KeyDown,
            Sub(sender As Object, e As KeyEventArgs)
                If e.KeyCode = Keys.Enter Then
                    e.SuppressKeyPress = True
                    Auditar()
                End If
            End Sub

        pnlTop.Controls.Add(lblEquipo)
        pnlTop.Controls.Add(txtEquipo)
        pnlTop.Controls.Add(btnAuditar)
        pnlTop.Controls.Add(btnGuardar)

        ' --- Búsqueda AD
        Dim pnlAD As New FlowLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 46,
            .FlowDirection = FlowDirection.LeftToRight,
            .Padding = New Padding(10, 4, 10, 4),
            .WrapContents = False
        }
        btnBuscarAD = New Button With {.Text = "Buscar en AD", .AutoSize = True}
        AddHandler btnBuscarAD.Click, AddressOf BtnBuscarAD_Click
        pnlAD.Controls.Add(btnBuscarAD)

        ' --- Estado
        lblEstado = New Label With {
            .Dock = DockStyle.Top,
            .Height = 20,
            .Text = "Listo.",
            .ForeColor = Color.DimGray,
            .Padding = New Padding(10, 0, 0, 0)
        }

        ' --- Lista AD
        lstAD = New ListBox With {.Height = 110, .Width = 840}
        AddHandler lstAD.DoubleClick,
            Sub()
                If lstAD.SelectedIndex >= 0 Then
                    Dim it = TryCast(lstAD.SelectedItem, ADItem)
                    If it IsNot Nothing Then txtEquipo.Text = it.FQDN
                End If
            End Sub
        Dim pnlADList As New Panel With {.Dock = DockStyle.Top, .Height = lstAD.Height + 8, .Padding = New Padding(10, 0, 10, 8)}
        pnlADList.Controls.Add(lstAD)

        ' --- Salida tipo consola
        txtSalida = New TextBox With {
            .Dock = DockStyle.Fill,
            .Multiline = True,
            .ScrollBars = ScrollBars.Both,
            .Font = New Font("Consolas", 10.0F, FontStyle.Regular),
            .ReadOnly = True,
            .WordWrap = False
        }

        ' --- Atajos globales
        AddHandler Me.KeyDown, AddressOf VentanaAvanzada_KeyDown

        ' Ensamblar
        Me.Controls.Add(txtSalida)
        Me.Controls.Add(lblEstado)
        Me.Controls.Add(pnlADList)
        Me.Controls.Add(pnlAD)
        Me.Controls.Add(pnlTop)
    End Sub

    '------------------------------------------------
    ' Load: precargar equipo si vino del principal
    '------------------------------------------------
    Private Sub VentanaAvanzada_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not String.IsNullOrWhiteSpace(_equipoInicial) Then
            txtEquipo.Text = _equipoInicial
        End If
        If txtEquipo IsNot Nothing Then txtEquipo.Select()
    End Sub

    '------------------------------------------------
    ' Atajos: Ctrl+A / Ctrl+C / Ctrl+S
    '------------------------------------------------
    Private Sub VentanaAvanzada_KeyDown(sender As Object, e As KeyEventArgs)
        If e.Control AndAlso e.KeyCode = Keys.A Then
            If txtSalida IsNot Nothing Then txtSalida.SelectAll()
            e.SuppressKeyPress = True
        ElseIf e.Control AndAlso e.KeyCode = Keys.C Then
            If txtSalida IsNot Nothing Then
                If txtSalida.SelectionLength > 0 Then
                    Clipboard.SetText(txtSalida.SelectedText)
                Else
                    Clipboard.SetText(txtSalida.Text)
                End If
            End If
            e.SuppressKeyPress = True
        ElseIf e.Control AndAlso e.KeyCode = Keys.S Then
            GuardarReporte()
            e.SuppressKeyPress = True
        End If
    End Sub

    '------------------------------------------------
    ' Handlers de botones (los que faltaban)
    '------------------------------------------------
    Private Sub BtnAuditar_Click(sender As Object, e As EventArgs)
        Auditar()
    End Sub

    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs)
        GuardarReporte()
    End Sub

    Private Async Sub BtnBuscarAD_Click(sender As Object, e As EventArgs)
        Await BuscarEnAD()
    End Sub

    '===============================
    ' Búsqueda en AD
    '===============================
    Private Async Function BuscarEnAD() As Task
        Try
            lblEstado.Text = "Buscando en Active Directory…"
            lstAD.Items.Clear()

            Dim base As String = Await Task.Run(Function() GetDefaultNamingContext())
            If String.IsNullOrEmpty(base) Then
                MessageBox.Show("No se pudo resolver el contexto de dominio (rootDSE). Ejecutá en equipo unido al dominio.",
                                "AD", MessageBoxButtons.OK, MessageBoxIcon.Information)
                lblEstado.Text = "Listo."
                Return
            End If

            Dim criterio As String = If(txtEquipo IsNot Nothing, txtEquipo.Text.Trim(), "")
            Dim objetos = Await Task.Run(Function() QueryComputersAD(base, criterio))

            For Each it In objetos
                lstAD.Items.Add(it)
            Next

            lblEstado.Text = "Búsqueda AD completada. Resultados: " & lstAD.Items.Count
        Catch ex As Exception
            MessageBox.Show("Error al consultar AD: " & ex.Message, "AD", MessageBoxButtons.OK, MessageBoxIcon.Error)
            lblEstado.Text = "Error en consulta AD."
        End Try
    End Function

    Private Function GetDefaultNamingContext() As String
        Try
            Using root As New DirectoryEntry("LDAP://rootDSE")
                Return CStr(root.Properties("defaultNamingContext").Value)
            End Using
        Catch
            Return Nothing
        End Try
    End Function

    Private Function QueryComputersAD(baseDn As String, criterio As String) As List(Of ADItem)
        Dim lista As New List(Of ADItem)
        Using srch As New DirectorySearcher(New DirectoryEntry("LDAP://" & baseDn))
            Dim filtroName As String
            If String.IsNullOrWhiteSpace(criterio) Then
                filtroName = "(objectClass=computer)"
            Else
                Dim c = EscapeLdap(criterio)
                filtroName = "(| (cn=" & c & "*) (name=" & c & "*) (dnsHostName=" & c & "*) )"
            End If
            srch.Filter = "(&" & filtroName & "(objectCategory=computer))"
            srch.SearchScope = SearchScope.Subtree
            srch.PageSize = 200
            srch.PropertiesToLoad.Add("cn")
            srch.PropertiesToLoad.Add("dnsHostName")
            srch.PropertiesToLoad.Add("operatingSystem")
            srch.PropertiesToLoad.Add("distinguishedName")

            For Each r As SearchResult In srch.FindAll()
                Dim fqdn = If(r.Properties.Contains("dnsHostName"), CStr(r.Properties("dnsHostName")(0)), Nothing)
                Dim cn = If(r.Properties.Contains("cn"), CStr(r.Properties("cn")(0)), "")
                Dim os = If(r.Properties.Contains("operatingSystem"), CStr(r.Properties("operatingSystem")(0)), "")
                Dim dn = If(r.Properties.Contains("distinguishedName"), CStr(r.Properties("distinguishedName")(0)), "")
                lista.Add(New ADItem(If(String.IsNullOrWhiteSpace(fqdn), cn, fqdn),
                                     (If(String.IsNullOrWhiteSpace(fqdn), cn, fqdn)) & " | " &
                                     (If(String.IsNullOrWhiteSpace(os), "SO desconocido", os)) & " | " & dn))
            Next
        End Using
        Return lista
    End Function

    Private Function EscapeLdap(s As String) As String
        Dim sb As New StringBuilder()
        For Each ch As Char In s
            Select Case ch
                Case "\"c : sb.Append("\5c")
                Case "*"c : sb.Append("\2a")
                Case "("c : sb.Append("\28")
                Case ")"c : sb.Append("\29")
                Case Else : sb.Append(ch)
            End Select
        Next
        Return sb.ToString()
    End Function

    Private Class ADItem
        Public ReadOnly FQDN As String
        Public ReadOnly Texto As String
        Public Sub New(fqdn As String, texto As String)
            Me.FQDN = fqdn
            Me.Texto = texto
        End Sub
        Public Overrides Function ToString() As String
            Return Texto
        End Function
    End Class

    '===============================
    ' Auditoría WMI (sin SMART)
    '===============================
    Private Sub Auditar()
        txtSalida.Clear()
        Dim host As String = If(txtEquipo IsNot Nothing, txtEquipo.Text.Trim(), "")
        If String.IsNullOrWhiteSpace(host) Then
            MessageBox.Show("Ingresá un nombre de equipo (NetBIOS o FQDN).", "Auditoría", MessageBoxButtons.OK, MessageBoxIcon.Information)
            If txtEquipo IsNot Nothing Then txtEquipo.Select()
            Return
        End If

        AppendLine("========== Auditoría remota v1.0 ==========")
        AppendLine("Equipo: " & host)
        AppendLine("Inicio: " & NowStr())
        AppendLine("")

        lblEstado.Text = "Conectando a WMI…"
        Try
            ' (1) NO usar Using con ManagementScope: no es IDisposable.
            Dim scope As ManagementScope = WmiConnect(host, "root\cimv2")
            If scope Is Nothing OrElse Not scope.IsConnected Then
                lblEstado.Text = "Fallo de conexión WMI."
                Exit Sub
            End If

            ' --- SISTEMA
            AppendLine("[Sistema]")
            Try
                Using csSearcher As New ManagementObjectSearcher(scope, New ObjectQuery(
        "SELECT Manufacturer,Model,TotalPhysicalMemory,Domain,UserName FROM Win32_ComputerSystem"))
                    For Each x As ManagementObject In csSearcher.Get()
                        AppendLine("Fabricante/Modelo: " & x("Manufacturer") & " / " & x("Model"))
                        AppendLine("Dominio/Usuario: " & x("Domain") & " / " & x("UserName"))
                        AppendLine("Memoria física total: " & SizeGB(x("TotalPhysicalMemory")) & " GB")
                        Exit For
                    Next
                End Using

                Using osSearcher As New ManagementObjectSearcher(scope, New ObjectQuery(
        "SELECT Caption,Version,CSName,LastBootUpTime,OSArchitecture FROM Win32_OperatingSystem"))
                    For Each x As ManagementObject In osSearcher.Get()
                        AppendLine("Sistema Operativo: " & x("Caption") & " " & x("Version") &
                       " (" & x("OSArchitecture") & ")")
                        AppendLine("Nombre del sistema: " & x("CSName"))
                        AppendLine("Último arranque: " & x("LastBootUpTime"))
                        Exit For
                    Next
                End Using

                Using biosSearcher As New ManagementObjectSearcher(scope, New ObjectQuery(
        "SELECT Manufacturer, SMBIOSBIOSVersion, SerialNumber, ReleaseDate FROM Win32_BIOS"))
                    For Each x As ManagementObject In biosSearcher.Get()
                        AppendLine("BIOS: " & x("Manufacturer") & " " & x("SMBIOSBIOSVersion") &
                       " | Serie: " & x("SerialNumber") & " | Fecha: " & x("ReleaseDate"))
                        Exit For
                    Next
                End Using

                Using cspSearcher As New ManagementObjectSearcher(scope, New ObjectQuery(
        "SELECT UUID, Name FROM Win32_ComputerSystemProduct"))
                    For Each x As ManagementObject In cspSearcher.Get()
                        AppendLine("UUID: " & x("UUID") & " | Producto: " & x("Name"))
                        Exit For
                    Next
                End Using
            Catch ex As Exception
                AppendLine("Error consultando información del sistema: " & ex.Message)
            End Try

            AppendLine("")
            ' --- CPU
            AppendLine("[CPU]")
            Try
                Using cpuSearcher As New ManagementObjectSearcher(scope, New ObjectQuery(
        "SELECT Name, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed, LoadPercentage, CurrentClockSpeed FROM Win32_Processor"))
                    For Each c As ManagementObject In cpuSearcher.Get()
                        Dim max As Integer = If(c("MaxClockSpeed") IsNot Nothing, CInt(c("MaxClockSpeed")), 0)
                        Dim cur As Integer = If(c("CurrentClockSpeed") IsNot Nothing, CInt(c("CurrentClockSpeed")), 0)
                        Dim throttle As Boolean = (max > 0 AndAlso cur < (0.6 * max))
                        AppendLine("Modelo: " & c("Name"))
                        AppendLine("Cores/Hilos: " & c("NumberOfCores") & " / " & c("NumberOfLogicalProcessors"))
                        AppendLine("Frecuencia: Max " & max & " MHz | Actual " & cur & " MHz")
                        AppendLine("Carga instantánea: " & If(c("LoadPercentage") IsNot Nothing, c("LoadPercentage").ToString() & "%", "N/D"))
                        AppendLine("Diagnóstico rápido: " & If(throttle, "Posible throttling (frecuencia muy por debajo del máximo).", "Sin signos evidentes de throttling."))
                    Next
                End Using
            Catch ex As Exception
                AppendLine("Error consultando CPU: " & ex.Message)
            End Try

            AppendLine("")
            ' --- RAM
            AppendLine("[Memoria RAM]")
            Try
                Using memSearcher As New ManagementObjectSearcher(scope, New ObjectQuery(
        "SELECT BankLabel, Capacity, Manufacturer, PartNumber, Speed, ConfiguredClockSpeed, SerialNumber, Status FROM Win32_PhysicalMemory"))
                    Dim total As Double = 0
                    For Each m As ManagementObject In memSearcher.Get()
                        Dim cap As Double = CDbl(If(m("Capacity"), 0))
                        total += cap
                        AppendLine("Módulo: " & m("BankLabel") & " | " & SizeGB(cap) & " GB | Fabricante: " & m("Manufacturer") &
                       " | PN: " & m("PartNumber") & " | Velocidad: " & If(m("Speed"), "N/D") &
                       " (config: " & If(m("ConfiguredClockSpeed"), "N/D") & ") | Estado: " & If(m("Status"), "N/D") &
                       " | Serie: " & If(m("SerialNumber"), "N/D"))
                    Next
                    AppendLine("Total RAM instalada: " & SizeGB(total) & " GB")
                End Using
            Catch ex As Exception
                AppendLine("Error consultando memoria: " & ex.Message)
            End Try

            AppendLine("")
            ' --- Almacenamiento
            AppendLine("[Almacenamiento]")
            Try
                Using diskSearcher As New ManagementObjectSearcher(scope, New ObjectQuery(
        "SELECT Index, Model, SerialNumber, Size, Status, InterfaceType FROM Win32_DiskDrive"))
                    For Each d As ManagementObject In diskSearcher.Get()
                        AppendLine("Disco #" & d("Index") & ": " & d("Model") &
                       " | Serie: " & If(d("SerialNumber"), "N/D") &
                       " | Tamaño: " & SizeGB(d("Size")) & " GB | Interfaz: " & d("InterfaceType") &
                       " | Estado: " & If(d("Status"), "N/D"))
                    Next
                End Using

                Using volSearcher As New ManagementObjectSearcher(scope, New ObjectQuery(
        "SELECT DeviceID, VolumeName, FileSystem, Size, FreeSpace FROM Win32_LogicalDisk WHERE DriveType=3"))
                    For Each v As ManagementObject In volSearcher.Get()
                        Dim total As Double = CDbl(If(v("Size"), 0))
                        Dim free As Double = CDbl(If(v("FreeSpace"), 0))
                        Dim used As Double = Math.Max(0, total - free)
                        AppendLine("Volumen " & v("DeviceID") & " (" & If(v("VolumeName"), "") & "): " &
                       SizeGB(used) & "/" & SizeGB(total) & " GB usados (" & Percent(used, total) & ") | FS: " & v("FileSystem"))
                    Next
                End Using
            Catch ex As Exception
                AppendLine("Error consultando discos/volúmenes: " & ex.Message)
            End Try

            AppendLine("")
            AppendLine("Fin: " & NowStr())
            lblEstado.Text = "Auditoría completada."

        Catch ex As UnauthorizedAccessException
            lblEstado.Text = "Acceso denegado (WMI/credenciales)."
            AppendLine("Error: Acceso denegado. Verificar permisos WMI en el remoto.")
        Catch ex As Exception
            lblEstado.Text = "Error de auditoría."
            AppendLine("Error general: " & ex.Message)
        End Try
    End Sub

    '===============================
    ' Guardar reporte
    '===============================
    Private Sub GuardarReporte()
        Try
            Using sfd As New SaveFileDialog()
                sfd.Filter = "Texto (*.txt)|*.txt"
                Dim nombre = If(String.IsNullOrWhiteSpace(txtEquipo.Text), "equipo", Sanitize(txtEquipo.Text))
                sfd.FileName = "Reporte_" & nombre & "_" & TimeStamp() & ".txt"
                If sfd.ShowDialog(Me) = DialogResult.OK Then
                    File.WriteAllText(sfd.FileName, txtSalida.Text, Encoding.UTF8)
                    MessageBox.Show("Reporte guardado:" & Environment.NewLine & sfd.FileName,
                                    "Guardar", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show("No se pudo guardar el archivo: " & ex.Message, "Guardar", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    '===============================
    ' Helpers
    '===============================
    Private Function WmiConnect(host As String, Optional ns As String = "root\cimv2") As ManagementScope
        Dim options As New ConnectionOptions With {
            .Impersonation = ImpersonationLevel.Impersonate,
            .Authentication = AuthenticationLevel.PacketPrivacy,
            .EnablePrivileges = True,
            .Timeout = TimeSpan.FromSeconds(5)
        }
        Dim scope = New ManagementScope("\\" & host & "\" & ns, options)
        scope.Connect()
        Return scope
    End Function

    Private Sub AppendLine(s As String)
        If txtSalida IsNot Nothing Then
            txtSalida.AppendText(s & Environment.NewLine)
        End If
    End Sub

    Private Function SizeGB(bytesObj As Object) As String
        Dim bytes As Double = 0
        If bytesObj IsNot Nothing Then
            Double.TryParse(bytesObj.ToString(), bytes)
        End If
        Dim gb = bytes / (1024.0 ^ 3)
        Return gb.ToString("0.00")
    End Function

    Private Function Percent(used As Double, total As Double) As String
        If total <= 0 Then Return "0%"
        Return Math.Round((used / total) * 100).ToString() & "%"
    End Function

    Private Function NowStr() As String
        Dim d = DateTime.Now
        Return d.ToString("yyyy-MM-dd HH:mm:ss")
    End Function

    Private Function TimeStamp() As String
        Dim d = DateTime.Now
        Return d.ToString("yyyyMMdd_HHmmss")
    End Function

    Private Function Sanitize(s As String) As String
        Dim invalid = Path.GetInvalidFileNameChars()
        For Each ch In invalid
            s = s.Replace(ch, "_"c)
        Next
        Return s
    End Function

End Class
