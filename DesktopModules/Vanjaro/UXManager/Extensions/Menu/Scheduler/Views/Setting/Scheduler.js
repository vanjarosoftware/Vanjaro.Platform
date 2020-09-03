app.controller('setting_scheduler', function ($scope, $attrs, $http, CommonSvc, SweetAlert, $compile) {
    var common = CommonSvc.getData($scope);
    $scope.AddTaskSchedule = false;
    $scope.ShowHistoryTask = false;
    //$scope.SchedulerObject = [];
    $scope.ScheduleID = "0";
    $scope.EnabledScheduling = false;
    $scope.ScheduleMode;
    $scope.DelayAtAppStart;
    $scope.ShowAddTask = false;

    $scope.onInit = function () {
        $scope.ShowAddTask = true;
        $('.task').removeClass('active');
        $('#scheduler').addClass("active");
        $('#taskqueue').removeClass("active");
        $('#history').removeClass("active");
        $('#settings').removeClass("active");
        if ($scope.ui.data !== undefined) {
            clearTimeout(ScheduleStatus);
            $scope.GetScheduleStatus();
            $scope.ui.data.ScheduleStatus.Options.Data.ScheduleMode = parseInt($scope.ui.data.ScheduleStatus.Options.Data.ScheduleMode);
            $scope.ScheduleMode = $scope.GetSchuduleMode($scope.ui.data.ScheduleStatus.Options.Data.ScheduleMode);
            $scope.DelayAtAppStart = parseInt($scope.ui.data.ScheduleStatus.Options.Data.DelayAtAppStart);
        }

    };

    $scope.Click_AddTask = function () {
        $scope.ShowAddTask = false;
        $scope.AddTaskClick();
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
    }

    $scope.RefreshGrid = function () {
        $scope.HistoryItemTableState.pagination.start = 0;
        $scope.HistoryItemTableState.pagination.numberOfPages = 5;
        $scope.Pipe_ScheduleHistoryItemPagging($scope.HistoryItemTableState);
    };

    $scope.Click_Cancel = function () {
        $scope.ui.data.RunOnEvent.Value = "0";
        $scope.ui.data.EnabledScheduling.Value = false,
            $scope.ui.data.ScheduleStartDate.Value = "";
        $scope.ui.data.Frequency.Value = ""; //Frequency
        $scope.ui.data.FrequencyPeriod.Value = "s";
        $scope.ui.data.RetainScheduleHistory.Value = "0";
        $scope.ui.data.RetryTimeLapse.Value = "";
        $scope.ui.data.RunTimeLapsePeriod.Value = "s";
        $scope.ui.data.Server.Value = "";
        $scope.ui.data.ObjectDependencies.Value = "";
        $scope.ui.data.ClassNameAssembly.Value = "";
        $scope.ui.data.CatchUpTasks.Value = "false";
        $scope.ui.data.FriendlyName.Value = "";
        $scope.ScheduleID = "0";
        $scope.AddTaskSchedule = false;
        $scope.ShowAddTask = true;
    };

    $scope.Click_deleteTask = function (scheduleid) {
        swal({
            title: "[LS:AreYouSure]",
            text: "[L:DeleteTaskMessage]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[L:DeleteSchedule]",
            cancelButtonText: "[LS:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('Scheduler/DeleteSchedule', 'ScheduleID=' + scheduleid).success(function (data) {
                        if (data.IsSuccess) {
                            $scope.pagginationData.pagination.numberOfPages = data.Data.ScheduleItems.numberOfPages;
                            $scope.ScheduleItems = data.Data.ScheduleItems.ScheduledItems;
                            window.parent.ShowNotification('', data.Message, 'success');
                        }
                        else {
                            window.parent.ShowNotification('', data.Message, 'error');
                        }
                    });
                }
            })
    };

    $scope.AddTaskClick = function () {
        $scope.ScheduleID = "0";
        $scope.AddTaskSchedule = true;
    };

    $scope.RunSchedule = function (sender) {
        if (mnValidationService.DoValidationAndSubmit(sender)) {
            var scheduleDto = {};
            scheduleDto.ScheduleID = $scope.ScheduleID;
            scheduleDto.FriendlyName = $scope.ui.data.FriendlyName.Value;
            scheduleDto.AttachToEvent = $scope.ui.data.RunOnEvent.Value === "0" ? "" : $scope.ui.data.RunOnEvent.Value;
            scheduleDto.Enabled = $scope.ui.data.EnabledScheduling.Value;
            scheduleDto.ScheduleStartDate = $scope.ui.data.ScheduleStartDate.Value;
            scheduleDto.TimeLapse = $scope.ui.data.Frequency.Value; //scheduleDto.Frequency
            scheduleDto.TimeLapseMeasurement = $scope.ui.data.FrequencyPeriod.Value;
            scheduleDto.RetainHistoryNum = $scope.ui.data.RetainScheduleHistory.Value;
            scheduleDto.RetryTimeLapse = $scope.ui.data.RetryTimeLapse.Value === "" ? -1 : $scope.ui.data.RetryTimeLapse.Value;
            scheduleDto.RetryTimeLapseMeasurement = $scope.ui.data.RunTimeLapsePeriod.Value;
            scheduleDto.Servers = $scope.ui.data.Server.Value;
            scheduleDto.ObjectDependencies = $scope.ui.data.ObjectDependencies.Value;
            scheduleDto.TypeFullName = $scope.ui.data.ClassNameAssembly.Value;
            scheduleDto.CatchUpEnabled = $scope.ui.data.CatchUpTasks.Value;
            common.webApi.post('Scheduler/RunSchedule', '', scheduleDto).success(function (data) {
                if (data.IsSuccess) {
                    $scope.Click_Cancel();
                    window.parent.ShowNotification('', data.Message, 'success');
                }
                else {
                    window.parent.ShowNotification('', data.Message, 'error');
                }
            });
        };
    };

    $scope.DirectRunTask = function (scheduleid) {
    common.webApi.post('Scheduler/DirectRunSchedule', 'ScheduleID=' + scheduleid).success(function (data) {
        if (data.IsSuccess) {
            window.parent.ShowNotification('', data.Message, 'success');
        }
        else {
            window.parent.ShowNotification('', data.Message, 'error');
        }
    });
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

    $scope.EditTask = function (scheduleid) {
        common.webApi.get('Scheduler/GetScheduleItem', 'scheduleId=' + scheduleid).success(function (data) {
            if (data.Data.Status === 'Success') {
                $scope.ui.data.RunOnEvent.Value = data.Data.Data.AttachToEvent === "" ? "0" : data.Data.Data.AttachToEvent;
                if (data.Data.Data.Enabled === true) {
                    $scope.ui.data.EnabledScheduling.Value = data.Data.Data.Enabled;
                    $scope.EnabledScheduling = true;
                }
                $scope.ui.data.ScheduleStartDate.Value = data.Data.Data.ScheduleStartDate; //change
                $scope.ui.data.Frequency.Value = data.Data.Data.TimeLapse.toString();
                $scope.ui.data.FrequencyPeriod.Value = data.Data.Data.TimeLapseMeasurement === "" ? "s" : data.Data.Data.TimeLapseMeasurement;
                $scope.ui.data.RetainScheduleHistory.Value = data.Data.Data.RetainHistoryNum.toString() === "" ? "0" : data.Data.Data.RetainHistoryNum.toString();
                $scope.ui.data.RetryTimeLapse.Value = data.Data.Data.RetryTimeLapse.toString() === "-1" ? "" : data.Data.Data.RetryTimeLapse.toString();
                $scope.ui.data.RunTimeLapsePeriod.Value = data.Data.Data.RetryTimeLapseMeasurement === "" ? "s" : data.Data.Data.RetryTimeLapseMeasurement;
                $scope.ui.data.Server.Value = data.Data.Data.Servers;
                $scope.ui.data.ObjectDependencies.Value = data.Data.Data.ObjectDependencies;
                $scope.ui.data.ClassNameAssembly.Value = data.Data.Data.TypeFullName;
                $scope.ui.data.CatchUpTasks.Value = data.Data.Data.CatchUpEnabled.toString();
                $scope.ui.data.FriendlyName.Value = data.Data.Data.FriendlyName;
                $scope.ScheduleID = data.Data.Data.ScheduleID;
            }
        })
        $scope.AddTaskSchedule = true;
    };

    $scope.Click_Update = function (sender) {
        if (mnValidationService.DoValidationAndSubmit(sender)) {
            var scheduleDto = {};

            if ($scope.ui.data.FriendlyName.Value !== undefined && $scope.ui.data.FriendlyName.Value !== "")
                scheduleDto.FriendlyName = $scope.ui.data.FriendlyName.Value;

            if ($scope.ui.data.RunOnEvent.Value === 1) {
                scheduleDto.AttachToEvent = "APPLICATION_START";
            }
            if ($scope.ui.data.EnabledScheduling.Value === true) {
                scheduleDto.Enabled = true;
            }
            if ($scope.ui.data.ScheduleStartDate.Value !== undefined && $scope.ui.data.ScheduleStartDate.Value !== "") {
                scheduleDto.ScheduleStartDate = $scope.ui.data.ScheduleStartDate.Value;
            }

            scheduleDto.TimeLapse = $scope.ui.data.Frequency.Value;

            scheduleDto.TimeLapseMeasurement = $scope.ui.data.FrequencyPeriod.Value;

            if ($scope.ui.data.RetainScheduleHistory.Value !== '0')
                scheduleDto.RetainHistoryNum = $scope.ui.data.RetainScheduleHistory.Value;

            if ($scope.ui.data.RetryTimeLapse.Value !== undefined && $scope.ui.data.RetryTimeLapse.Value !== "") {
                scheduleDto.RetryTimeLapse = $scope.ui.data.RetryTimeLapse.Value;
                scheduleDto.RetryTimeLapseMeasurement = $scope.ui.data.RunTimeLapsePeriod.Value; //issue
            }

            if ($scope.ui.data.Server.Value !== undefined && $scope.ui.data.Server.Value !== "") {
                scheduleDto.Servers = $scope.ui.data.Server.Value;
            }

            if ($scope.ui.data.ObjectDependencies.Value !== undefined && $scope.ui.data.ObjectDependencies.Value !== "")
                scheduleDto.ObjectDependencies = $scope.ui.data.ObjectDependencies.Value;

            scheduleDto.TypeFullName = $scope.ui.data.ClassNameAssembly.Value;

            if ($scope.ui.data.CatchUpTasks.Value !== undefined && $scope.ui.data.CatchUpTasks.Value !== "0") {
                scheduleDto.CatchUpEnabled = $scope.ui.data.CatchUpTasks.Value
            }
            if ($scope.ScheduleID === "0") {
                common.webApi.post('scheduler/CreateScheduleItem', '', scheduleDto).success(function (data) {
                    if (data.IsSuccess) {
                        $scope.pagginationData.pagination.numberOfPages = data.Data.ScheduleItems.numberOfPages;
                        $scope.ScheduleItems = data.Data.ScheduleItems.ScheduledItems;
                        $scope.Click_Cancel();
                        $scope.ShowAddTask = true;
                        //$scope.AddTaskSchedule = false;
                        window.parent.ShowNotification('', data.Message, 'success');
                    }
                    else
                        window.parent.ShowNotification('', data.Message, 'error');
                })
            }
            else {
                scheduleDto.ScheduleID = $scope.ScheduleID;
                common.webApi.post('scheduler/UpdateScheduleItem', '', scheduleDto).success(function (data) {
                    if (data.IsSuccess) {
                        $scope.pagginationData.pagination.numberOfPages = data.Data.ScheduleItems.numberOfPages;
                        $scope.ScheduleItems = data.Data.ScheduleItems.ScheduledItems;
                        $scope.Click_Cancel();
                        $scope.ShowAddTask = true;
                        //$scope.AddTaskSchedule = false;
                        window.parent.ShowNotification('', data.Message, 'success');
                    }
                    else
                        window.parent.ShowNotification('', data.Message, 'error');
                })
            }
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

    $scope.Click_CancelHistory = function () {
        $scope.ScheduleID = "0";
        $scope.ShowHistoryTask = false;
        $scope.ShowAddTask = true;
    };

    $scope.HistoryTask = function (ScheduleID) {
        $scope.ScheduleID = ScheduleID;
        $scope.ShowHistoryTask = true;
        $scope.ShowAddTask = false;
        $scope.RefreshGrid();
    };

    $scope.Pipe_ScheduleHistoryItemPagging = function (tableState) {
        $scope.HistoryItemTableState = tableState;
        if ($scope.ScheduleID !== "0") {
            common.webApi.get('history/GetScheduleItemHistory', 'scheduleId=' + $scope.ScheduleID + '&pageSize=' + tableState.pagination.number + '&pageIndex=' + (tableState.pagination.start / tableState.pagination.number)).success(function (data) {
                if (data.Data.Status === "Success") {
                    $scope.ScheduleHistoryItem = data.Data.Item;
                    $scope.HistoryItemTableState.pagination.numberOfPages = data.Data.NumberOfPages;
                }
                else {
                    window.parent.ShowNotification('', data.Data.Status, 'error');
                }
            });
        }
    };

    $scope.Pipe_SchedulePaging = function (tableState) {
        $scope.pagginationData = tableState;
        var formData = {
            Skip: $scope.pagginationData.pagination.start,
            Take: $scope.pagginationData.pagination.number
        }


        common.webApi.post('Scheduler/GetSchedulerItembyPageing', '', formData).success(function (data) {
            if (data != null) {
                tableState.pagination.numberOfPages = data.Data.ScheduleItems.numberOfPages;
                $scope.ScheduleItems = data.Data.ScheduleItems.ScheduledItems;
            }

        });
    };
});