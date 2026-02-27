
Imports Microsoft.VisualBasic.ApplicationServices
    Imports System.IO

    Namespace My
        Partial Friend Class MyApplication

            ' 1) Inicio de la aplicación (antes del Form de inicio)
            Private Sub MyApplication_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
                Try
                    ' Ejemplo: crear carpeta de logs local para la app
                    Dim logs = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NuevaApp", "logs")
                    Directory.CreateDirectory(logs)

                    ' Si querés cancelar porque falta algo:
                    ' If Not File.Exists("C:\algo\requerido.txt") Then
                    '     MessageBox.Show("Falta un prerequisito.", "Inicio", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    '     e.Cancel = True
                    ' End If

                Catch ex As Exception
                    MessageBox.Show("Error al iniciar: " & ex.Message, "Inicio", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    e.Cancel = True
                End Try
            End Sub

            ' 2) Excepciones no controladas (cualquier error que no atrapaste)
            Private Sub MyApplication_UnhandledException(sender As Object, e As UnhandledExceptionEventArgs) Handles Me.UnhandledException
                Try
                    Dim detalle = e.Exception.ToString()
                    Dim rutaLog = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NuevaApp", "logs", "errores.log")
                    File.AppendAllText(rutaLog, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {detalle}{Environment.NewLine}{New String("-"c, 60)}{Environment.NewLine}")

                    MessageBox.Show("Ocurrió un error no controlado:" & Environment.NewLine &
                                e.Exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)

                    ' Si querés que la app NO se cierre por el error:
                    e.ExitApplication = False
                    ' Si preferís cerrar todo:
                    ' e.ExitApplication = True

                Catch
                    ' Evitar fallas dentro del manejador
                End Try
            End Sub

            ' 3) Instancia única (solo si activaste "Convertir aplicación de instancia única")
            Private Sub MyApplication_StartupNextInstance(sender As Object, e As StartupNextInstanceEventArgs) Handles Me.StartupNextInstance
                Try
                    ' Llevar al frente la instancia ya abierta
                    If Application.OpenForms.Count > 0 Then
                        Dim f = Application.OpenForms(0)
                        If f IsNot Nothing Then
                            If f.WindowState = FormWindowState.Minimized Then f.WindowState = FormWindowState.Normal
                            f.Activate()
                            f.BringToFront()
                        End If
                    End If

                    ' Si pasaste argumentos en la segunda ejecución, están en e.CommandLine

                Catch
                End Try
            End Sub

        End Class


    ' Los siguientes eventos están disponibles para MyApplication:
    ' Inicio: Se genera cuando se inicia la aplicación, antes de que se cree el formulario de inicio.
    ' Apagado: Se genera después de haberse cerrado todos los formularios de aplicación.  Este evento no se genera si la aplicación termina de forma anómala.
    ' UnhandledException: Se genera si la aplicación encuentra una excepción no controlada.
    ' StartupNextInstance: Se genera cuando se inicia una aplicación de instancia única y dicha aplicación está ya activa. 
    ' NetworkAvailabilityChanged: Se genera cuando se conecta o desconecta la conexión de red.
    Partial Friend Class MyApplication
    End Class
End Namespace
