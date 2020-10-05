app.controller('notification_notifications', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);

    $scope.onInit = function () {
        $('.Texttab').removeClass('active');
        $('.Messagetab').addClass('active');
    };

    $scope.Pipe_NotificationsPages = function (tableState) {
        $scope.NotificationsPagestableState = tableState;

        common.webApi.get('Notification/GetNotificationList', 'Page=' + tableState.pagination.start + '&PageSize=' + tableState.pagination.number).success(function (data) {
            if (data) {
                tableState.pagination.numberOfPages = data.TotalNotifications;
                $scope.Notifications = [];
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
});