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

        common.webApi.get('Notification/GetNotificationList', 'Page=' + SearchKeys.skip + '&PageSize=' + SearchKeys.pagesize).success(function (data) {
            if (data) {
                tableState.pagination.numberOfPages = Math.ceil(parseInt($('#Notification .Messagetab a>span', window.document).html()) / SearchKeys.pagesize)
                $scope.NotificationsPagestableState = tableState;
                $scope.Notifications = data.Notifications;
            }
        });
    };

    $scope.Click_Dismiss = function (row) {
        var postData = {
            NotificationId: row.NotificationId
        };
        common.webApi.post('Notification/Dismiss', '', postData).success(function (data) {
            if (data.IsSuccess) {
                $("#VJnotifycount", parent.document).text(parseInt($("#VJnotifycount", parent.document).text()) - 1);
                $('#Notification .Messagetab a>span', window.document).html(parseInt($('#Notification .Messagetab a>span', window.document).html()) - 1);
                //$scope.ui.data.NotificationsCount.Value = data.NotificationsCount;
                $scope.Pipe_NotificationsPages($scope.NotificationsPagestableState);
            }
        });
    };

    $scope.to_trusted = function (html_code) {
        return $sce.trustAsHtml(html_code);
    }
});