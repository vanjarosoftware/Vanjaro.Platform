app.controller('setting_settings', function ($scope, $attrs, $http, CommonSvc, SweetAlert, $compile) {
    var common = CommonSvc.getData($scope);
    $scope.ScheduleMode;
    $scope.DelayAtAppStart;

    $scope.onInit = function () {
        $('.task').removeClass('active');
        $('#settings').addClass("active");
        $('#taskqueue').removeClass("active");
        $('#scheduler').removeClass("active");
        $('#history').removeClass("active");
        if ($scope.ui.data !== undefined) {
            clearTimeout(ScheduleStatus);
            $scope.GetScheduleStatus();
            $scope.ui.data.ScheduleStatus.Options.Data.ScheduleMode = parseInt($scope.ui.data.ScheduleStatus.Options.Data.ScheduleMode);
            $scope.ScheduleMode = $scope.ui.data.ScheduleStatus.Options.Data.ScheduleMode === 1 ? true : false;
            $scope.DelayAtAppStart = parseInt($scope.ui.data.ScheduleStatus.Options.Data.DelayAtAppStart);
        }
    };

    $scope.$watch('ScheduleMode', function (newValue, oldValue) {
        if (newValue !== undefined && oldValue !== undefined) {
            $scope.UpdateSchedulerMode();
        }
    });

    var AutoUpdateTimeOutid;
    $scope.UpdateSchedulerMode = function () {
        if (AutoUpdateTimeOutid) {
            clearTimeout(AutoUpdateTimeOutid);
        }
        AutoUpdateTimeOutid = setTimeout(function () {
            var data = {
                "SchedulerMode": $scope.ScheduleMode ? 1 : 0,
                "SchedulerdelayAtAppStart": $scope.ui.data.ScheduleStatus.Options.Data.DelayAtAppStart
            };
            common.webApi.post('TaskQueue/UpdateSchedulerSettings', '', data).then(function (data) {
                if (data.data.IsSuccess) {
                    if ($scope.ScheduleMode)
                        $scope.ui.data.ScheduleStatus.Options.Data.ScheduleMode = 1;
                    else
                        $scope.ui.data.ScheduleStatus.Options.Data.ScheduleMode = 0;
                    $scope.DelayAtAppStart = parseInt($scope.ui.data.ScheduleStatus.Options.Data.DelayAtAppStart);
                    $scope.ui.data.ScheduleStatus.Options.Data.Status = $scope.GetScheduleStatus();
                    if (!$scope.ScheduleMode) {
                        $('.mode').addClass('disable');
                    }
                    window.parent.ShowNotification('[LS:Scheduler]', data.data.Message, 'success');
                }
                else {
                    window.parent.ShowNotification('[LS:Scheduler]', data.data.Message, 'error');
                }
            });
        }, 800)
    };

    $scope.GetScheduleStatus = function () {
        common.webApi.get('Scheduler/GetScheduleStatus', '').then(function (data) {
            if (data.data) {
                $scope.ui.data.ScheduleStatus.Options.Data.Status = data.data.Data.Data.Status;
                $scope.ui.data.ScheduleStatus.Options.Data.MaxThreadCount = data.data.Data.Data.MaxThreadCount;
                $scope.ui.data.ScheduleStatus.Options.Data.ActiveThreadCount = data.data.Data.Data.ActiveThreadCount;
                $scope.ui.data.ScheduleStatus.Options.Data.FreeThreadCount = data.data.Data.Data.FreeThreadCount;
                $scope.ui.data.ScheduleStatus.Options.Data.ServerTime = data.data.Data.Data.ServerTime;
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
                    if (!$scope.ScheduleMode) {
                        $('.mode').addClass('disable');
                    }
                    else {
                        common.webApi.delete('TaskQueue/StopSchedule').then(function (data) {
                            if (data.data.IsSuccess) {
                                $scope.ui.data.ScheduleStatus.Options.Data.Status = $scope.GetScheduleStatus();
                            }
                            else {
                                window.parent.ShowNotification('', data.data.Message, 'error');
                            }
                        });
                    }
                }
            });

    };

    $scope.StartSchedule = function () {
        if (!$scope.ScheduleMode) {
            $('.mode').addClass('disable');
        }
        else {
            common.webApi.post('TaskQueue/StartSchedule').then(function (data) {
                if (data.data.IsSuccess) {
                    $scope.ui.data.ScheduleStatus.Options.Data.Status = $scope.GetScheduleStatus();
                }
                else {
                    window.parent.ShowNotification('', data.data.Message, 'error');
                }
            });
        }
    };
});