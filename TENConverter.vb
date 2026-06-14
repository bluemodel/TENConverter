'BlueM.Wave
'Copyright (C) BlueM Dev Group
'<https://www.bluemodel.org>
'
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU Lesser General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.
'
'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
'GNU Lesser General Public License for more details.
'
'You should have received a copy of the GNU Lesser General Public License
'along with this program.  If not, see <https://www.gnu.org/licenses/>.
'
''' <summary>
''' TENConverter
''' A command-line utility for converting old binary TeeChart TEN files to the new JSON format
''' </summary>
Module TENConverter

    Sub Main()

        Dim v As Version = Reflection.Assembly.GetExecutingAssembly.GetName().Version()
        Dim currentVersion As New Version($"{v.Major}.{v.Minor}.{v.Build}")

        Console.WriteLine($"BlueM.TENConverter v{currentVersion}")
        Console.WriteLine("Converts old binary TeeChart TEN files to the new JSON format")
        Console.WriteLine()

        Dim args As List(Of String) = System.Environment.GetCommandLineArgs().Skip(1).ToList() ' Skip the executable name

        Dim n_option_args As Integer = 0
        Dim outputDir As String = String.Empty

        Dim iArg As Integer = 0
        Do While iArg < args.Count
            Dim arg As String = args(iArg).Trim().ToLower()

            'handle option arguments first
            If arg = "--help" Or arg = "-h" Then
                'Display help and exit
                Console.WriteLine("Usage: TENConverter.exe [-o outputdirectory] files")
                Console.WriteLine("    -o: optional output directory. If not provided, converted files are saved to the same directory as the original")
                Console.WriteLine("    files: one or more space-separated paths to TEN files to convert")
                Console.WriteLine("Converted files will have the same filename as the original but with file extension .json.ten")
                Return

            ElseIf arg = "-o" Then
                'Handle output directory option
                iArg += 1
                If args.Count < iArg + 1 Then
                    Console.WriteLine("Error: Output directory not specified after -o option")
                    Return
                End If
                outputDir = args(iArg).Trim()
                If Not IO.Directory.Exists(outputDir) Then
                    Console.WriteLine($"Error: Output directory does not exist: {outputDir}")
                    Return
                End If
                n_option_args += 2
            End If
            iArg += 1
        Loop

        'handle file arguments
        Dim fileArgs As List(Of String) = args.Skip(n_option_args).ToList()

        If fileArgs.Count = 0 Then
            Console.WriteLine("Error: No files specified for conversion")
            Return
        End If

        For Each arg As String In fileArgs

            Dim file_old As String = arg.Trim()
            If Not IO.File.Exists(file_old) Then
                Console.WriteLine($"Error: File not found: {file_old}")
                Continue For
            End If

            Console.WriteLine($"Processing file: {file_old}...")

            Dim tchart As New Steema.TeeChart.TChart()
            'import TEN file
            Try
                tchart.Import.Template.Load(file_old)
            Catch ex As Exception
                Console.WriteLine($"Error importing file {file_old}: {ex.Message}")
                Continue For
            End Try

            'export as JSON
            Dim file_new As String
            If outputDir = String.Empty Then
                file_new = IO.Path.Combine(
                    IO.Path.GetDirectoryName(file_old),
                    IO.Path.GetFileNameWithoutExtension(file_old) & ".json.ten"
                )
            Else
                file_new = IO.Path.Combine(
                    outputDir,
                    IO.Path.GetFileNameWithoutExtension(file_old) & ".json.ten"
                )
            End If
            If IO.File.Exists(file_new) Then
                Console.WriteLine($"File already exists, skipping: {file_new}")
                Continue For
            End If
            Try
                tchart.Export.TemplateJSON.Save(file_new)
                Console.WriteLine($"File successfully converted: {file_new}")
            Catch ex As Exception
                Console.WriteLine($"Error exporting file {file_new}: {ex.Message}")
                'delete incomplete file if it was created
                If IO.File.Exists(file_new) Then
                    Try
                        IO.File.Delete(file_new)
                    Catch deleteEx As Exception
                        Console.WriteLine($"Error deleting incomplete file {file_new}: {deleteEx.Message}")
                    End Try
                End If
            End Try

        Next

    End Sub

End Module
