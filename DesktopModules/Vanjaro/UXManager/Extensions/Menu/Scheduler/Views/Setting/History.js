app.controller('setting_history', function ($scope, $attrs, $http, CommonSvc, SweetAlert, $compile) {
    var common = CommonSvc.getData($scope);
    $scope.ScheduleMode;
    $scope.DelayAtAppStart;

    $scope.onInit = function () {
        $('.task').removeClass('active');
        $('#history').addClass("active");
        $('#taskqueue').removeClass("active");
        $('#scheduler').removeClass("active");
        $('#settings').removeClass("active");
        if ($scope.ui.data !== undefined) {
            clearTimeout(ScheduleStatus);
            $scope.GetScheduleStatus();
            $scope.ui.data.ScheduleStatus.Options.Data.ScheduleMode = parseInt($scope.ui.data.ScheduleStatus.Options.Data.ScheduleMode);
            $scope.ScheduleMode = $scope.GetSchuduleMode($scope.ui.data.ScheduleStatus.Options.Data.ScheduleMode);
            $scope.DelayAtAppStart = parseInt($scope.ui.data.ScheduleStatus.Options.Data.DelayAtAppStart);
        }
    };

    $scope.GetSchuduleMode = function (Mode) {
        switch (Mode) {
            case 0:
                return $scope.ScheduleMode = "[LS:DISABLED]";
            case 1:
                return $scope.ScheduleMode = "[LS:TIMER_METHOD]";
            case 2:
                return $scope.ScheduleMode = "[LS:REQUEST_METHOD]";
        }
    };

    $scope.GetScheduleStatus = function () {
        common.webApi.get('Scheduler/GetScheduleStatus', '').success(function (data) {
            if (data) {
                $scope.ui.data.ScheduleStatus.Options.Data.Status = data.Data.Data.Status;
                $scope.ui.data.ScheduleStatus.Options.Data.MaxThreadCount = data.Data.Data.MaxThreadCount;
                $scope.ui.data.ScheduleStatus.Options.Data.ActiveThreadCount = data.Data.Data.ActiveThreadCount;
                $scope.ui.data.ScheduleStatus.Options.Data.FreeThreadCount = data.Data.Data.FreeThreadCount;
                $scope.ui.data.ScheduleStatus.Options.Data.ServerTime = data.Data.Data.ServerTime;
            }
        });
        ScheduleStatus = setTimeout($scope.GetScheduleStatus, 5000);
    };

    $scope.StopSchedule = function () {
        CommonSvc.SweetAlert.swal({
            title: "[LS:AreYouSure]",
            text: "",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#8CD4F5", confirmButtonText: "[LS:Yes]",
            cancelButtonText: "[LS:No]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    if ($scope.GetSchuduleMode($scope.ui.data.ScheduleStatus.Options.Data.ScheduleMode) === "[LS:DISABLED]") {
                        $('.mode').addClass('disable');
                    }
                    else {
                        common.webApi.delete('TaskQueue/StopSchedule').success(function (data) {
                            if (data.IsSuccess) {
                                $scope.ui.data.ScheduleStatus.Options.Data.Status = $scope.GetScheduleStatus();
                            }
                            else {
                                window.parent.ShowNotification('', data.Message, 'error');
                            }
                        });
                    }
                }
            });

    };

    $scope.StartSchedule = function () {
        if ($scope.GetSchuduleMode($scope.ui.data.ScheduleStatus.Options.Data.ScheduleMode) === "[LS:DISABLED]") {
            $('.mode').addClass('disable');
        }
        else {
            common.webApi.post('TaskQueue/StartSchedule').success(function (data) {
                if (data.IsSuccess) {
                    $scope.ui.data.ScheduleStatus.Options.Data.Status = $scope.GetScheduleStatus();
                }
                else {
                    window.parent.ShowNotification('', data.Message, 'error');
                }
            });
        }
    };

    $scope.Pipe_ScheduleHistoryItemPagging = function (tableState) {
        $scope.HistoryItemTableState = tableState;
        common.webApi.get('history/GetScheduleItemHistory', 'scheduleId=' + '-1&' + '&pageSize=' + tableState.pagination.number + '&pageIndex=' + (tableState.pagination.start / tableState.pagination.number)).success(function (data) {
            if (data.Data.Status === "Success") {
                $scope.ScheduleHistoryItem = data.Data.Item;
                $scope.HistoryItemTableState.pagination.numberOfPages = data.Data.NumberOfPages;
            }
            else {
                window.parent.ShowNotification('', data.Data.Status, 'error');
            }
        })
    };
});