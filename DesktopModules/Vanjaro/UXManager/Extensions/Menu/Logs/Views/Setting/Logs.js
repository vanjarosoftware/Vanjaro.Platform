app.controller('setting_logs', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {

    var common = CommonSvc.getData($scope);
    $scope.SelectedLogItems = [];

    $scope.onInit = function () {
        setTimeout(function () {
            $('#vjLogTypes').find($('[id="*"]')).addClass('active');
        }, 100);
    };

    $scope.RefreshGrid = function () {
        if ($scope.LogItemsTableState !== undefined) {
            $scope.LogItemsTableState.pagination.start = 0;
            $scope.LogItemsTableState.pagination.numberOfPages = 10;
            $scope.Pipe_LogItemsPagging($scope.LogItemsTableState);
        };
    };

    $scope.Click_ClearLog = function () {
        swal({
            title: "[LS:AreYouSure]",
            text: "[L:ClearLogsMsg]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[L:ClearLogs]",
            cancelButtonText: "[LS:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.get('Logs/ClearLog').success(function (data) {
                        if (data === "Success") {
                            $scope.RefreshGrid();
                        }
                    })
                }
            }
        );
    };


    $scope.Click_ShowSiteLogs = function () {
        event.preventDefault();
        window.location.href = $scope.ui.data.LogSettingsUrl.Value;
    };

    $scope.Click_DeleteSelected = function () {
        if ($scope.SelectedLogItems.length > 0) {
            swal({
                title: "[LS:AreYouSure]",
                text: "[L:DeleteLogs]",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55", confirmButtonText: "[LS:Delete]",
                cancelButtonText: "[LS:Cancel]",
                closeOnConfirm: true,
                closeOnCancel: true
            },
                function (isConfirm) {
                    if (isConfirm) {
                        common.webApi.post('Logs/DeleteLogItems', '', $scope.SelectedLogItems).success(function (data) {
                            if (data === "success") {
                                $scope.SelectedLogItems = [];
                                $scope.RefreshGrid();
                            }
                        });
                    }
                }
            );
        }
        else
            window.parent.ShowNotification('[LS:Warning]', '[L:SelectAtleastOneLog]', 'warning');
    };

    $scope.Click_SelectLog = function (logid) {
        if ($scope.SelectedLogItems.indexOf(logid) < 0) {
            $scope.SelectedLogItems.push(logid);
        }
        else {
            var arrlength = $scope.SelectedLogItems.length;
            for (var i = 0; i < arrlength; i++) {
                if ($scope.SelectedLogItems[i] === logid) {
                    $scope.SelectedLogItems.splice(i, 1);
                }
            }
        }
    };

    $scope.ShowLogProperties = function (row) {
        $('p#p' + row.LogGUID).toggle();
    }

    $scope.EmailSelected = function (event) {
        event.preventDefault();
        if ($scope.SelectedLogItems.length > 0) {
            swal({
                title: '',
                text: "<iframe class='border-0' style='width: 100%; height: 450px;' src='" + window.location.href + "#email'></iframe>",
                html: true,
                showConfirmButton: false
            });
        }
        else
            window.parent.ShowNotification('[LS:Warning]', '[L:SelectAtleastOneLog]', 'warning');
    };

    $scope.ChangeGetLogTypes = function (e, LogType) {
        $('#vjLogTypes').find('.active').removeClass('active');
        $(e.currentTarget).addClass('active');
        $scope.ui.data.GetLogTypes.Value = LogType;
        $scope.RefreshGrid();
    };

    $scope.Pipe_LogItemsPagging = function (tableState) {
        $scope.LogItemsTableState = tableState;
        common.webApi.get('Logs/GetLogItems', 'logType=' + $scope.ui.data.GetLogTypes.Value + '&pageSize=' + tableState.pagination.number + '&pageIndex=' + (tableState.pagination.start / tableState.pagination.number)).success(function (data) {
            if (data) {
                $scope.LogItems = data.Items;
                $scope.LogItemsTableState.pagination.numberOfPages = data.NumberOfPages;
            }
        })
    };
});