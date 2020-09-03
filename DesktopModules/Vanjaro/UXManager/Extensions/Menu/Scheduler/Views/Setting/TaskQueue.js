var ScheduleStatus;
var StopSchedule;
app.controller('setting_taskqueue', function ($scope, $attrs, $http, CommonSvc, SweetAlert, $compile) {
    var common = CommonSvc.getData($scope);

    $scope.ScheduleMode;
    $scope.DelayAtAppStart;
    $scope.onInit = function () {
        $('.task').addClass('active');
        $('#taskqueue').addClass("active");
        $('#settings').removeClass("active");
        $('#scheduler').removeClass("active");
        $('#history').removeClass("active");
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

    $scope.GetScheduleStatus = function () {
        common.webApi.get('Scheduler/GetScheduleStatus', '').success(function (data) {
            if (data) {
                $scope.ui.data.ScheduleStatus.Options.Data.ScheduleQueue = data.Data.Data.ScheduleQueue;
                $scope.ui.data.ScheduleStatus.Options.Data.Status = data.Data.Data.Status;
                $scope.ui.data.ScheduleStatus.Options.Data.MaxThreadCount = data.Data.Data.MaxThreadCount;
                $scope.ui.data.ScheduleStatus.Options.Data.ActiveThreadCount = data.Data.Data.ActiveThreadCount;
                $scope.ui.data.ScheduleStatus.Options.Data.FreeThreadCount = data.Data.Data.FreeThreadCount;
                $scope.ui.data.ScheduleStatus.Options.Data.ServerTime = data.Data.Data.ServerTime;
                $scope.ui.data.ScheduleStatus.Options.Data.ScheduleProcessing = data.Data.Data.ScheduleProcessing;
            }
        });
        ScheduleStatus = setTimeout($scope.GetScheduleStatus, 5000);
    }
});