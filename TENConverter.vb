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
Imports Microsoft.Extensions.FileSystemGlobbing
Imports Microsoft.Extensions.FileSystemGlobbing.Abstractions

''' <summary>
''' TENConverter
''' A command-line utility for converting old binary TeeChart TEN files to the new JSON format
''' </summary>
Module TENConverter

    Dim outputDir As String = String.Empty

    Sub Main()

        Dim v As Version = Reflection.Assembly.GetExecutingAssembly.GetName().Version()
        Dim currentVersion As New Version($"{v.Major}.{v.Minor}.{v.Build}")

        Console.WriteLine($"BlueM.TENConverter v{currentVersion}")
        Console.WriteLine("Converts old binary TeeChart TEN files to the new JSON format")
        Console.WriteLine()

        Dim args As List(Of String) = System.Environment.GetCommandLineArgs().Skip(1).ToList() ' Skip the executable name

        Dim n_option_args As Integer = 0

        Dim iArg As Integer = 0
        Do While iArg < args.Count
            Dim arg As String = args(iArg).Trim().ToLower()

            'handle option arguments first
            If arg = "--help" Or arg = "-h" Then
                'Display help and exit
                Console.WriteLine("Usage: TENConverter.exe [-o outputdirectory] paths")
                Console.WriteLine("    -o: optional output directory. If not provided, converted files are saved to the same directory as the original")
                Console.WriteLine("    paths: one or more space-separated paths to TEN files to convert. If path is a directory, all TEN files found in this directory including subdirectories will be converted.")
                Console.WriteLine("Converted files will have the same filename as the original but with file extension .json.ten")
                Return

            ElseIf arg = "-o" Then
                'Handle output directory option
                iArg += 1
                If args.Count < iArg + 1 Then
                    Console.WriteLine("ERROR: Output directory not specified after -o option")
                    Return
                End If
                outputDir = args(iArg).Trim()
                If Not IO.Directory.Exists(outputDir) Then
                    Console.WriteLine($"ERROR: Output directory does not exist: {outputDir}")
                    Return
                End If
                n_option_args += 2
            End If
            iArg += 1
        Loop

        'handle path arguments
        Dim pathArgs As List(Of String) = args.Skip(n_option_args).ToList()

        If pathArgs.Count = 0 Then
            Console.WriteLine("ERROR: No paths specified for conversion")
            Return
        End If

        For Each arg As String In pathArgs

            Dim path As String = arg.Trim()

            If IO.File.Exists(path) Then
                'process a single file
                Call ProcessFile(path)

            ElseIf IO.Directory.Exists(path) Then
                'process a directory - search for TEN files and convert them
                Console.WriteLine($"INFO: Searching for TEN files in directory: {path}...")
                Dim matcher As New Matcher()
                matcher.AddInclude("**/*.ten")

                Dim result As PatternMatchingResult = matcher.Execute(
                    New DirectoryInfoWrapper(
                        New IO.DirectoryInfo(path)
                    )
                )

                If result.HasMatches Then
                    Console.WriteLine($"INFO: Found {result.Files.Count} TEN files to convert")
                    For Each file In result.Files
                        Dim fullPath As String = IO.Path.Combine(path, file.Path)
                        Call ProcessFile(fullPath)
                    Next
                Else
                    Console.WriteLine($"WARNING: No TEN files found in directory: {path}")
                End If
            Else
                Console.WriteLine($"ERROR: File/directory not found: {path}")
                Continue For
            End If

        Next

    End Sub

    ''' <summary>
    ''' Process a single TEN file
    ''' Determines output path, checks if output file already exists and if not, calls ConvertFile to convert it
    ''' </summary>
    ''' <param name="file">path to file to process</param>
    Public Sub ProcessFile(file As String)

        Console.WriteLine($"INFO: Processing file: {file}...")

        'determine output file path
        Dim file_new As String
        If outputDir = String.Empty Then
            file_new = IO.Path.Combine(
                    IO.Path.GetDirectoryName(file),
                    IO.Path.GetFileNameWithoutExtension(file) & ".json.ten"
                )
        Else
            file_new = IO.Path.Combine(
                    outputDir,
                    IO.Path.GetFileNameWithoutExtension(file) & ".json.ten"
                )
        End If

        'check if output file already exists
        If IO.File.Exists(file_new) Then
            Console.WriteLine($"WARNING: Output file already exists, skipping: {file_new}")
            Return
        End If

        'convert the file
        Call ConvertFile(file, file_new)

    End Sub

    ''' <summary>
    ''' Converts a single TEN file from the old binary format to the new JSON format
    ''' </summary>
    ''' <param name="file_old">path to the old binary file</param>
    ''' <param name="file_new">path to the new json file</param>
    Public Sub ConvertFile(file_old As String, file_new As String)

        Dim tchart As New Steema.TeeChart.TChart()

        'import TEN file
        Try
            tchart.Import.Template.Load(file_old)
        Catch ex As Exception
            Console.WriteLine($"ERROR: Error while importing file {file_old}: {ex.Message}")
            Return
        End Try

        'export as JSON
        Try
            tchart.Export.TemplateJSON.Save(file_new)
            Console.WriteLine($"INFO: File successfully converted: {file_new}")
        Catch ex As Exception
            Console.WriteLine($"ERROR: Error while exporting file {file_new}: {ex.Message}")
            'delete incomplete file if it was created
            If IO.File.Exists(file_new) Then
                Try
                    IO.File.Delete(file_new)
                Catch deleteEx As Exception
                    Console.WriteLine($"ERROR: Error while deleting incomplete file {file_new}: {deleteEx.Message}")
                End Try
            End If
        End Try

    End Sub

End Module
