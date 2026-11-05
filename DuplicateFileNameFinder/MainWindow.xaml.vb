Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Windows.Forms

Public Class MainWindow

    Private SelectedFolder As String = String.Empty

    Private Sub BtnSelectFolder_Click(sender As Object, e As RoutedEventArgs)
        Dim windowHandle = New Interop.WindowInteropHelper(Me).Handle
        Dim selectedPath As String = FolderPickerHelper.SelectFolder(windowHandle, "Please select a folder", allowNewFolderButton:=False, initialFolder:=Environment.SpecialFolder.MyComputer)
        If Not String.IsNullOrEmpty(selectedPath) Then
            SelectedFolder = selectedPath
            TxtFolder.Text = selectedPath
            ProcessFiles()
        End If
    End Sub

    Private Sub ProcessFiles()
        TreeResult.Items.Clear()
        If String.IsNullOrEmpty(SelectedFolder) Then Exit Sub

        Dim ignoreWords = TxtIgnoreWords.Text.
            Split(","c).
            Select(Function(w) If(ChkCaseSensitiveFilenames.IsChecked, w.Trim().ToLower(), w.Trim())).
            Where(Function(w) w <> String.Empty).ToList()

        Dim files = Directory.GetFiles(SelectedFolder)

        Dim wordMap As New Dictionary(Of String, List(Of String))

        ' لیست برای ثبت نام فایل‌هایی که قبلاً دیده شده‌اند
        Dim seenFileNames As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        For Each file In files
            Dim fileName As String = If(ChkIncludeExtension.IsChecked, Path.GetFileName(file), Path.GetFileNameWithoutExtension(file))

            ' بررسی Ignore Equal Filenames
            If ChkIgnoreEqualFilenames.IsChecked AndAlso seenFileNames.Contains(fileName) Then Continue For
            seenFileNames.Add(fileName)

            Dim lowerFileName As String = If(ChkIncludeExtension.IsChecked, Path.GetFileName(file), Path.GetFileNameWithoutExtension(file))
            If ChkCaseSensitiveFilenames.IsChecked Then lowerFileName = lowerFileName.ToLower()

            ' اگر اسم فایل شامل هر کلمه ignore باشد، نادیده بگیر
            If ignoreWords.Any(Function(ign) lowerFileName.Contains(ign)) Then
                Continue For
            End If

            ' جدا کردن کلمات
            Dim words = Regex.Split(fileName, "[^\p{L}\d]+").
                        Where(Function(w) w <> String.Empty).
                        Select(Function(w) If(ChkCaseSensitiveFilenames.IsChecked, w, w.ToLower()))

            For Each word In words
                If ignoreWords.Contains(word) Then Continue For

                If Not wordMap.ContainsKey(word) Then wordMap(word) = New List(Of String)

                If Not wordMap(word).Contains(Path.GetFileName(file)) Then wordMap(word).Add(Path.GetFileName(file))
            Next word
        Next file

        ' فقط مواردی که بیش از یک فایل دارند
        For Each pair In wordMap.Where(Function(p) p.Value.Distinct().Count() > 1)
            Dim parent As New TreeViewItem With {.Header = pair.Key}

            For Each f In pair.Value.Distinct()
                parent.Items.Add(New TreeViewItem With {.Header = f})
            Next f

            TreeResult.Items.Add(parent)
        Next pair
    End Sub

    Private Sub ChkIncludeExtension_Click(sender As Object, e As RoutedEventArgs)
        ProcessFiles()
    End Sub
    Private Sub ChkIgnoreEqualFilenames_Click(sender As Object, e As RoutedEventArgs)
        ProcessFiles()
    End Sub

    Private Sub TxtIgnoreWords_TextChanged(sender As Object, e As TextChangedEventArgs)
        ProcessFiles()
    End Sub

    Private Sub ChkCaseSensitiveFilenames_Click(sender As Object, e As RoutedEventArgs)
        ProcessFiles()
    End Sub
End Class
