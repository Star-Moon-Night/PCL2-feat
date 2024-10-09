﻿Public Class PageOtherFeedback

    Public Class Feedback
        Public User As String
        Public Title As String
        Public Time As Date
        Public Content As String
        Public Url As String
        Public ID As String
        Public Tags As New List(Of String)
    End Class

    Enum TagID As Int64
        NewIssue = 4365827012
        Bug = 4365944566
        Improve = 4365949262
        Processing = 4365819896
        WaitingResponse = 4365816377
        Completed = 4365809832
        Decline = 4365654603
        NewFeture = 4365949953
    End Enum

    Private Shadows IsLoaded As Boolean = False
    Private Sub PageOtherFeedback_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        PageLoaderInit(Load, PanLoad, PanContent, PanInfo, Loader, AddressOf RefreshList, AddressOf LoaderInput)
        '重复加载部分
        PanBack.ScrollToHome()
        '非重复加载部分
        If IsLoaded Then Exit Sub
        IsLoaded = True

    End Sub

    Public Shared Loader As New LoaderTask(Of String, List(Of Feedback))("FeedbackList", AddressOf FeedbackListGet, AddressOf LoaderInput)

    Private Shared Function LoaderInput() As String
        Return "" ' awa?
    End Function

    Public Shared Sub FeedbackListGet(Task As LoaderTask(Of String, List(Of Feedback)))
        Dim list As JArray
        list = NetGetCodeByRequestRetry("https://api.github.com/repos/Hex-Dragon/PCL2/issues", IsJson:=True, UseBrowserUserAgent:=True)
        If list Is Nothing Then Throw New Exception("无法获取到内容")
        Dim res As List(Of Feedback) = New List(Of Feedback)
        For Each i As JObject In list
            Dim item As Feedback = New Feedback With {.Title = i("title").ToString(),
                .Url = i("html_url").ToString(),
                .Content = i("body").ToString(),
                .Time = Date.Parse(i("created_at").ToString()),
                .User = i("user")("login").ToString(),
                .ID = i("number")}
            Dim thisTags As JArray = i("labels")
            For Each thisTag As JObject In thisTags
                item.Tags.Add(thisTag("id"))
            Next
            res.Add(item)
        Next
        Task.Output = res
    End Sub

    Public Sub RefreshList()
        PanListCompleted.Children.Clear()
        PanListProcessing.Children.Clear()
        PanListWaitingResponse.Children.Clear()
        For Each item In Loader.Output
            Dim ele As New MyListItem With {.Title = item.Title, .Type = MyListItem.CheckType.Clickable}
            Dim StatusDesc As String = "???"
            If item.Tags.Contains(TagID.NewIssue) Then
                ele.Logo = PathImage & "Blocks/Grass.png"
                StatusDesc = "未查看"
            End If
            If item.Tags.Contains(TagID.Processing) Then
                ele.Logo = PathImage & "Blocks/CommandBlock.png"
                StatusDesc = "处理中"
            End If
            If item.Tags.Contains(TagID.Bug) Then
                ele.Logo = PathImage & "Blocks/RedstoneBlock.png"
                StatusDesc = "处理中-Bug"
            End If
            If item.Tags.Contains(TagID.Improve) Then
                ele.Logo = PathImage & "Blocks/Anvil.png"
                StatusDesc = "处理中-优化"
            End If
            If item.Tags.Contains(TagID.Completed) Then
                ele.Logo = PathImage & "Blocks/GrassPath.png"
                StatusDesc = "已完成"
            End If
            If item.Tags.Contains(TagID.WaitingResponse) Then
                ele.Logo = PathImage & "Blocks/RedstoneLampOff.png"
                StatusDesc = "等待提交者"
            End If
            If item.Tags.Contains(TagID.NewFeture) Then
                ele.Logo = PathImage & "Blocks/Egg.png"
                StatusDesc = "处理中-新功能"
            End If
            ele.Info = StatusDesc & " | " & item.User & " | " & item.Time
            AddHandler ele.Click, Sub()
                                      MyMsgBox($"提交者：{item.User}（{GetTimeSpanString(item.Time - DateTime.Now, False)}）{vbCrLf}状态：{StatusDesc}{vbCrLf}{vbCrLf}{item.Content}", "#" & item.ID & " " & item.Title, Button2:="查看详情", Button2Action:=Sub()
                                                                                                                                                                                                                                                    OpenWebsite(item.Url)
                                                                                                                                                                                                                                                End Sub)
                                  End Sub
            If StatusDesc.StartsWithF("处理中") Then
                PanListProcessing.Children.Add(ele)
            ElseIf StatusDesc.Equals("等待提交者") Then
                PanListWaitingResponse.Children.Add(ele)
            ElseIf StatusDesc.Equals("完成") Then
                PanListCompleted.Children.Add(ele)
            ElseIf StatusDesc.Equals("未查看") Then
                PanListNewIssue.Children.Add(ele)
            Else
                Log("出现了未知的项：" & ele.Title)
            End If
            PanContentCompleted.Visibility = If(PanListCompleted.Children.Count.Equals(0), Visibility.Collapsed, Visibility.Visible)
            PanContentNewIssue.Visibility = If(PanListNewIssue.Children.Count.Equals(0), Visibility.Collapsed, Visibility.Visible)
            PanContentWaitingResponse.Visibility = If(PanListWaitingResponse.Children.Count.Equals(0), Visibility.Collapsed, Visibility.Visible)
            PanContentProcessing.Visibility = If(PanListProcessing.Children.Count.Equals(0), Visibility.Collapsed, Visibility.Visible)
        Next
    End Sub

    Private Sub Feedback_Click(sender As Object, e As MouseButtonEventArgs)
        PageOtherLeft.TryFeedback()
    End Sub
End Class
