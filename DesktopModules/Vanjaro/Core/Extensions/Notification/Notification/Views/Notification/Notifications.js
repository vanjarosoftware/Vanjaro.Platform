app.controller('notification_notifications', function ($scope, $sce, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);

    $scope.onInit = function () {
        $('.Texttab').removeClass('active');
        $('.Messagetab').addClass('active');
    };

    $scope.Pipe_NotificationsPages = function (tableState) {
        var SearchKeys = {};
        if (tableState != null && tableState != 'undefiend' && tableState != '') {
            tableState.pagination.numberOfPages = 0;
            SearchKeys.skip = tableState.pagination.start;
            SearchKeys.pagesize = tableState.pagination.number;
        }
        else {
            SearchKeys.skip = $scope.NotificationsPagestableState.pagination.start;
            SearchKeys.pagesize = $scope.NotificationsPagestableState.pagination.number;
        }
        if (SearchKeys.skip == parseInt($('#Notification .Messagetab a>span', window.document).html()))
            SearchKeys.skip = SearchKeys.skip - SearchKeys.pagesize;

        common.webApi.get('Notification/GetNotificationList', 'Page=' + SearchKeys.skip + '&PageSize=' + SearchKeys.pagesize).then(function (data) {
            if (data.data) {
                tableState.pagination.numberOfPages = Math.ceil(parseInt($('#Notification .Messagetab a>span', window.document).html()) / SearchKeys.pagesize)
                $scope.NotificationsPagestableState = tableState;
                $scope.Notifications = data.data.Notifications;
            }
        });
    };

    $scope.Click_Dismiss = function (row) {
        var postData = {
            NotificationId: row.NotificationId
        };
        common.webApi.post('Notification/Dismiss', '', postData).then(function (data) {
            if (data.data.IsSuccess) {
                $("#VJnotifycount", parent.document).text(parseInt($("#VJnotifycount", parent.document).text()) - 1);
                $('#Notification .Messagetab a>span', window.document).html(parseInt($('#Notification .Messagetab a>span', window.document).html()) - 1);
                $('.registerlink-notification > sup > strong', parent.document).text(parseInt($('#Notification .Messagetab a>span', window.document).html()) - 1);
                $scope.Pipe_NotificationsPages($scope.NotificationsPagestableState);
            }
        });
    };

    $scope.Click_DismissAll = function () {
        window.parent.swal({
            title: "[LS:AreYouSure]",
            text: "[L:DismissAllNotification]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[L:DismissAll]",
            cancelButtonText: "[LS:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('Notification/DismissAll', '',).then(function (data) {
                        if (data.data.IsSuccess) {
                            $("#VJnotifycount", parent.document).text(parseInt($("#VJnotifycount", parent.document).text()) - data.data.TotalNotifications);
                            $('#Notification .Messagetab a>span', window.document).html(parseInt($('#Notification .Messagetab a>span', window.document).html()) - data.data.TotalNotifications);
                            $('.registerlink-notification > sup > strong', parent.document).text(parseInt($('#Notification .Messagetab a>span', window.document).html()) - data.data.TotalNotifications);
                            $scope.Pipe_NotificationsPages($scope.NotificationsPagestableState);
                        }
                    });
                }
            }
        );

    };

    $scope.to_trusted = function (html_code) {
        return $sce.trustAsHtml(html_code);
    }

    Click_RedirectURL = function (url) {
        parent.window.location.href = parent.window.location.protocol + url;
    }
});