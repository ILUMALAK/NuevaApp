<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Requerido por el Diseñador de Windows Forms
    Private components As System.ComponentModel.IContainer
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip


    'NOTA: el Diseñador de Windows Forms necesita el siguiente procedimiento
    'Se puede modificar usando el Diseñador de Windows Forms.  
    'No lo modifique con el editor de código.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.TabPage4 = New System.Windows.Forms.TabPage()
        Me.BtnLimpiarHistorial = New System.Windows.Forms.Button()
        Me.TextBoxHistorial = New System.Windows.Forms.TextBox()
        Me.TabPage3 = New System.Windows.Forms.TabPage()
        Me.TabPage2 = New System.Windows.Forms.TabPage()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.BtnSalir = New System.Windows.Forms.Button()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.LabelEstadoCasilla = New System.Windows.Forms.Label()
        Me.LabelEstadoPuesto = New System.Windows.Forms.Label()
        Me.LabelEstadoLegajo = New System.Windows.Forms.Label()
        Me.LabelEstadoRed = New System.Windows.Forms.Label()
        Me.TextBoxInfoGeneral = New System.Windows.Forms.TextBox()
        Me.TextBoxCasilla = New System.Windows.Forms.TextBox()
        Me.CheckBoxInformacion = New System.Windows.Forms.CheckBox()
        Me.TextBoxPuesto = New System.Windows.Forms.TextBox()
        Me.TextBoxLegajo = New System.Windows.Forms.TextBox()
        Me.ComboBoxSucursal = New System.Windows.Forms.ComboBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.CheckBox3 = New System.Windows.Forms.CheckBox()
        Me.CheckBox2 = New System.Windows.Forms.CheckBox()
        Me.Button8 = New System.Windows.Forms.Button()
        Me.Button7 = New System.Windows.Forms.Button()
        Me.Button6 = New System.Windows.Forms.Button()
        Me.Button5 = New System.Windows.Forms.Button()
        Me.BtnReset = New System.Windows.Forms.Button()
        Me.BtnActualizar = New System.Windows.Forms.Button()
        Me.BtnRetroceder = New System.Windows.Forms.Button()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.ToolTip2 = New System.Windows.Forms.ToolTip(Me.components)
        Me.TabPage4.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.TabControl1.SuspendLayout()
        Me.SuspendLayout()
        '
        'TabPage4
        '
        Me.TabPage4.Controls.Add(Me.BtnLimpiarHistorial)
        Me.TabPage4.Controls.Add(Me.TextBoxHistorial)
        Me.TabPage4.Location = New System.Drawing.Point(4, 22)
        Me.TabPage4.Name = "TabPage4"
        Me.TabPage4.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage4.Size = New System.Drawing.Size(979, 595)
        Me.TabPage4.TabIndex = 3
        Me.TabPage4.Text = "TabPage4"
        Me.TabPage4.UseVisualStyleBackColor = True
        '
        'BtnLimpiarHistorial
        '
        Me.BtnLimpiarHistorial.Location = New System.Drawing.Point(486, 39)
        Me.BtnLimpiarHistorial.Name = "BtnLimpiarHistorial"
        Me.BtnLimpiarHistorial.Size = New System.Drawing.Size(75, 23)
        Me.BtnLimpiarHistorial.TabIndex = 1
        Me.BtnLimpiarHistorial.Text = "Button1"
        Me.BtnLimpiarHistorial.UseVisualStyleBackColor = True
        '
        'TextBoxHistorial
        '
        Me.TextBoxHistorial.Location = New System.Drawing.Point(64, 51)
        Me.TextBoxHistorial.Name = "TextBoxHistorial"
        Me.TextBoxHistorial.Size = New System.Drawing.Size(329, 20)
        Me.TextBoxHistorial.TabIndex = 0
        '
        'TabPage3
        '
        Me.TabPage3.Location = New System.Drawing.Point(4, 22)
        Me.TabPage3.Name = "TabPage3"
        Me.TabPage3.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage3.Size = New System.Drawing.Size(979, 595)
        Me.TabPage3.TabIndex = 2
        Me.TabPage3.Text = "TabPage3"
        Me.TabPage3.UseVisualStyleBackColor = True
        '
        'TabPage2
        '
        Me.TabPage2.Location = New System.Drawing.Point(4, 22)
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage2.Size = New System.Drawing.Size(979, 595)
        Me.TabPage2.TabIndex = 1
        Me.TabPage2.Text = "TabPage2"
        Me.TabPage2.UseVisualStyleBackColor = True
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.GroupBox1)
        Me.TabPage1.Location = New System.Drawing.Point(4, 22)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(979, 595)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "TabPage1"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.BtnSalir)
        Me.GroupBox1.Controls.Add(Me.GroupBox2)
        Me.GroupBox1.Controls.Add(Me.BtnReset)
        Me.GroupBox1.Controls.Add(Me.BtnActualizar)
        Me.GroupBox1.Controls.Add(Me.BtnRetroceder)
        Me.GroupBox1.Location = New System.Drawing.Point(22, 20)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(891, 554)
        Me.GroupBox1.TabIndex = 1
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "GroupBox1"
        '
        'BtnSalir
        '
        Me.BtnSalir.Location = New System.Drawing.Point(772, 115)
        Me.BtnSalir.Name = "BtnSalir"
        Me.BtnSalir.Size = New System.Drawing.Size(75, 23)
        Me.BtnSalir.TabIndex = 4
        Me.BtnSalir.Text = "Button4"
        Me.BtnSalir.UseVisualStyleBackColor = True
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.LabelEstadoCasilla)
        Me.GroupBox2.Controls.Add(Me.LabelEstadoPuesto)
        Me.GroupBox2.Controls.Add(Me.LabelEstadoLegajo)
        Me.GroupBox2.Controls.Add(Me.LabelEstadoRed)
        Me.GroupBox2.Controls.Add(Me.TextBoxInfoGeneral)
        Me.GroupBox2.Controls.Add(Me.TextBoxCasilla)
        Me.GroupBox2.Controls.Add(Me.CheckBoxInformacion)
        Me.GroupBox2.Controls.Add(Me.TextBoxPuesto)
        Me.GroupBox2.Controls.Add(Me.TextBoxLegajo)
        Me.GroupBox2.Controls.Add(Me.ComboBoxSucursal)
        Me.GroupBox2.Controls.Add(Me.Label4)
        Me.GroupBox2.Controls.Add(Me.Label3)
        Me.GroupBox2.Controls.Add(Me.Label2)
        Me.GroupBox2.Controls.Add(Me.Label1)
        Me.GroupBox2.Controls.Add(Me.GroupBox3)
        Me.GroupBox2.Location = New System.Drawing.Point(17, 49)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(708, 443)
        Me.GroupBox2.TabIndex = 0
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "GroupBox2"
        '
        'LabelEstadoCasilla
        '
        Me.LabelEstadoCasilla.AutoSize = True
        Me.LabelEstadoCasilla.Location = New System.Drawing.Point(278, 136)
        Me.LabelEstadoCasilla.Name = "LabelEstadoCasilla"
        Me.LabelEstadoCasilla.Size = New System.Drawing.Size(39, 13)
        Me.LabelEstadoCasilla.TabIndex = 13
        Me.LabelEstadoCasilla.Text = "Label8"
        '
        'LabelEstadoPuesto
        '
        Me.LabelEstadoPuesto.AutoSize = True
        Me.LabelEstadoPuesto.Location = New System.Drawing.Point(278, 103)
        Me.LabelEstadoPuesto.Name = "LabelEstadoPuesto"
        Me.LabelEstadoPuesto.Size = New System.Drawing.Size(39, 13)
        Me.LabelEstadoPuesto.TabIndex = 12
        Me.LabelEstadoPuesto.Text = "Label7"
        '
        'LabelEstadoLegajo
        '
        Me.LabelEstadoLegajo.AutoSize = True
        Me.LabelEstadoLegajo.Location = New System.Drawing.Point(278, 66)
        Me.LabelEstadoLegajo.Name = "LabelEstadoLegajo"
        Me.LabelEstadoLegajo.Size = New System.Drawing.Size(39, 13)
        Me.LabelEstadoLegajo.TabIndex = 11
        Me.LabelEstadoLegajo.Text = "Label6"
        '
        'LabelEstadoRed
        '
        Me.LabelEstadoRed.AutoSize = True
        Me.LabelEstadoRed.Location = New System.Drawing.Point(278, 41)
        Me.LabelEstadoRed.Name = "LabelEstadoRed"
        Me.LabelEstadoRed.Size = New System.Drawing.Size(39, 13)
        Me.LabelEstadoRed.TabIndex = 10
        Me.LabelEstadoRed.Text = "Label5"
        '
        'TextBoxInfoGeneral
        '
        Me.TextBoxInfoGeneral.Location = New System.Drawing.Point(467, 55)
        Me.TextBoxInfoGeneral.Name = "TextBoxInfoGeneral"
        Me.TextBoxInfoGeneral.Size = New System.Drawing.Size(119, 20)
        Me.TextBoxInfoGeneral.TabIndex = 9
        '
        'TextBoxCasilla
        '
        Me.TextBoxCasilla.Location = New System.Drawing.Point(99, 129)
        Me.TextBoxCasilla.Name = "TextBoxCasilla"
        Me.TextBoxCasilla.Size = New System.Drawing.Size(100, 20)
        Me.TextBoxCasilla.TabIndex = 8
        '
        'CheckBoxInformacion
        '
        Me.CheckBoxInformacion.AutoSize = True
        Me.CheckBoxInformacion.Location = New System.Drawing.Point(27, 77)
        Me.CheckBoxInformacion.Name = "CheckBoxInformacion"
        Me.CheckBoxInformacion.Size = New System.Drawing.Size(81, 17)
        Me.CheckBoxInformacion.TabIndex = 9
        Me.CheckBoxInformacion.Text = "CheckBox1"
        Me.CheckBoxInformacion.UseVisualStyleBackColor = True
        '
        'TextBoxPuesto
        '
        Me.TextBoxPuesto.Location = New System.Drawing.Point(99, 100)
        Me.TextBoxPuesto.Name = "TextBoxPuesto"
        Me.TextBoxPuesto.Size = New System.Drawing.Size(100, 20)
        Me.TextBoxPuesto.TabIndex = 7
        '
        'TextBoxLegajo
        '
        Me.TextBoxLegajo.Location = New System.Drawing.Point(99, 59)
        Me.TextBoxLegajo.Name = "TextBoxLegajo"
        Me.TextBoxLegajo.Size = New System.Drawing.Size(100, 20)
        Me.TextBoxLegajo.TabIndex = 6
        '
        'ComboBoxSucursal
        '
        Me.ComboBoxSucursal.FormattingEnabled = True
        Me.ComboBoxSucursal.Location = New System.Drawing.Point(99, 33)
        Me.ComboBoxSucursal.Name = "ComboBoxSucursal"
        Me.ComboBoxSucursal.Size = New System.Drawing.Size(121, 21)
        Me.ComboBoxSucursal.TabIndex = 5
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(34, 129)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(39, 13)
        Me.Label4.TabIndex = 4
        Me.Label4.Text = "Label4"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(40, 100)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(39, 13)
        Me.Label3.TabIndex = 3
        Me.Label3.Text = "Label3"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(34, 62)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(39, 13)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Label2"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(37, 36)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(39, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Label1"
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.CheckBox3)
        Me.GroupBox3.Controls.Add(Me.CheckBox2)
        Me.GroupBox3.Controls.Add(Me.Button8)
        Me.GroupBox3.Controls.Add(Me.Button7)
        Me.GroupBox3.Controls.Add(Me.Button6)
        Me.GroupBox3.Controls.Add(Me.Button5)
        Me.GroupBox3.Location = New System.Drawing.Point(37, 255)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(452, 164)
        Me.GroupBox3.TabIndex = 0
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "GroupBox3"
        '
        'CheckBox3
        '
        Me.CheckBox3.AutoSize = True
        Me.CheckBox3.Location = New System.Drawing.Point(52, 127)
        Me.CheckBox3.Name = "CheckBox3"
        Me.CheckBox3.Size = New System.Drawing.Size(81, 17)
        Me.CheckBox3.TabIndex = 10
        Me.CheckBox3.Text = "CheckBox3"
        Me.CheckBox3.UseVisualStyleBackColor = True
        '
        'CheckBox2
        '
        Me.CheckBox2.AutoSize = True
        Me.CheckBox2.Location = New System.Drawing.Point(37, 104)
        Me.CheckBox2.Name = "CheckBox2"
        Me.CheckBox2.Size = New System.Drawing.Size(81, 17)
        Me.CheckBox2.TabIndex = 10
        Me.CheckBox2.Text = "CheckBox2"
        Me.CheckBox2.UseVisualStyleBackColor = True
        '
        'Button8
        '
        Me.Button8.Location = New System.Drawing.Point(305, 31)
        Me.Button8.Name = "Button8"
        Me.Button8.Size = New System.Drawing.Size(75, 23)
        Me.Button8.TabIndex = 8
        Me.Button8.Text = "Button8"
        Me.Button8.UseVisualStyleBackColor = True
        '
        'Button7
        '
        Me.Button7.Location = New System.Drawing.Point(205, 31)
        Me.Button7.Name = "Button7"
        Me.Button7.Size = New System.Drawing.Size(75, 23)
        Me.Button7.TabIndex = 7
        Me.Button7.Text = "Button7"
        Me.Button7.UseVisualStyleBackColor = True
        '
        'Button6
        '
        Me.Button6.Location = New System.Drawing.Point(108, 31)
        Me.Button6.Name = "Button6"
        Me.Button6.Size = New System.Drawing.Size(75, 23)
        Me.Button6.TabIndex = 6
        Me.Button6.Text = "Button6"
        Me.Button6.UseVisualStyleBackColor = True
        '
        'Button5
        '
        Me.Button5.Location = New System.Drawing.Point(22, 31)
        Me.Button5.Name = "Button5"
        Me.Button5.Size = New System.Drawing.Size(75, 23)
        Me.Button5.TabIndex = 5
        Me.Button5.Text = "Button5"
        Me.Button5.UseVisualStyleBackColor = True
        '
        'BtnReset
        '
        Me.BtnReset.Location = New System.Drawing.Point(766, 80)
        Me.BtnReset.Name = "BtnReset"
        Me.BtnReset.Size = New System.Drawing.Size(75, 23)
        Me.BtnReset.TabIndex = 3
        Me.BtnReset.Text = "Button3"
        Me.BtnReset.UseVisualStyleBackColor = True
        '
        'BtnActualizar
        '
        Me.BtnActualizar.Location = New System.Drawing.Point(766, 19)
        Me.BtnActualizar.Name = "BtnActualizar"
        Me.BtnActualizar.Size = New System.Drawing.Size(75, 23)
        Me.BtnActualizar.TabIndex = 1
        Me.BtnActualizar.Text = "Button1"
        Me.BtnActualizar.UseVisualStyleBackColor = True
        '
        'BtnRetroceder
        '
        Me.BtnRetroceder.Location = New System.Drawing.Point(779, 48)
        Me.BtnRetroceder.Name = "BtnRetroceder"
        Me.BtnRetroceder.Size = New System.Drawing.Size(62, 30)
        Me.BtnRetroceder.TabIndex = 2
        Me.BtnRetroceder.Text = "Button2"
        Me.BtnRetroceder.UseVisualStyleBackColor = True
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPage1)
        Me.TabControl1.Controls.Add(Me.TabPage2)
        Me.TabControl1.Controls.Add(Me.TabPage3)
        Me.TabControl1.Controls.Add(Me.TabPage4)
        Me.TabControl1.Location = New System.Drawing.Point(12, 12)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(987, 621)
        Me.TabControl1.TabIndex = 0
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1001, 645)
        Me.Controls.Add(Me.TabControl1)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.TabPage4.ResumeLayout(False)
        Me.TabPage4.PerformLayout()
        Me.TabPage1.ResumeLayout(False)
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.TabControl1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TabPage4 As TabPage
    Friend WithEvents BtnLimpiarHistorial As Button
    Friend WithEvents TextBoxHistorial As TextBox
    Friend WithEvents TabPage3 As TabPage
    Friend WithEvents TabPage2 As TabPage
    Friend WithEvents TabPage1 As TabPage
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents BtnSalir As Button
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents LabelEstadoCasilla As Label
    Friend WithEvents LabelEstadoPuesto As Label
    Friend WithEvents LabelEstadoLegajo As Label
    Friend WithEvents LabelEstadoRed As Label
    Friend WithEvents TextBoxInfoGeneral As TextBox
    Friend WithEvents TextBoxCasilla As TextBox
    Friend WithEvents CheckBoxInformacion As CheckBox
    Friend WithEvents TextBoxPuesto As TextBox
    Friend WithEvents TextBoxLegajo As TextBox
    Friend WithEvents ComboBoxSucursal As ComboBox
    Friend WithEvents Label4 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents CheckBox3 As CheckBox
    Friend WithEvents CheckBox2 As CheckBox
    Friend WithEvents Button8 As Button
    Friend WithEvents Button7 As Button
    Friend WithEvents Button6 As Button
    Friend WithEvents Button5 As Button
    Friend WithEvents BtnReset As Button
    Friend WithEvents BtnActualizar As Button
    Friend WithEvents BtnRetroceder As Button
    Friend WithEvents TabControl1 As TabControl
    Friend WithEvents ToolTip2 As ToolTip
End Class
