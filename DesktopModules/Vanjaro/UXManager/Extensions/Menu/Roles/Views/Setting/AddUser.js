app.controller('setting_adduser', function ($scope, $routeParams, $attrs, $http, CommonSvc, SweetAlert, $filter) {
    $scope.rid = $routeParams["rid"];
    var common = CommonSvc.getData($scope);
    $scope.ShowSearchByUser = false;
    $scope.onInit = function () {
    };
    $scope.InjectDatetTimePicker = function () {
        $('.date_picker input').datepicker({
            autoclose: true,
            clearBtn: true,
            todayHighlight: true
        });
        $('.date_picker input').each(function () {
            if (this.value == '0001-01-01T00:00:00' || this.value == '1-01-01T00:00:00')
                $(this).datepicker('clearDates');
            else
                this.value = GetRoleDateFormat(this.value);
        });
    };
    $scope.Pipe_UserRolePagging = function (tableState) {
        //Pipe_(event) is a Method of Smart table(Grid) and using for Pagging
        var SearchKeys = {};
        SearchKeys.Search_Key = $('.UserRole .input-sm.form-control').val();

        if (tableState != null && tableState != 'undefiend' && tableState != '') {
            tableState.pagination.numberOfPages = 0;
            $scope.pagginationData = tableState;
        }
        if (tableState != null && tableState != 'undefiend' && tableState != '') {
            SearchKeys.skip = tableState.pagination.start;
            if ($scope.ui.data.UserRole.Options.length - 1 == 0)
                SearchKeys.skip = 0;
            SearchKeys.pagesize = tableState.pagination.number;
        }
        else {
            SearchKeys.skip = $scope.pagginationData.pagination.start;
            SearchKeys.pagesize = $scope.pagginationData.pagination.number;
        }

        common.webApi.get('role/getroleusers', 'keyword=' + SearchKeys.Search_Key + '&roleid=' + $scope.rid + '&pageindex=' + SearchKeys.skip / SearchKeys.pagesize + '&pagesize=' + SearchKeys.pagesize).then(function (data) {
            if (data != null && data.data.Data != null && data.data.IsSuccess && !data.data.HasErrors) {
                if (tableState != null && tableState != 'undefiend' && tableState != '') {
                    tableState.pagination.numberOfPages = Math.ceil(data.data.Data.totalRecords / SearchKeys.pagesize);
                }
                else {
                    $scope.pagginationData.pagination.numberOfPages = Math.ceil(data.data.Data.totalRecords / SearchKeys.pagesize);
                    $scope.pagginationData.pagination.start = 0;
                }
                $scope.ui.data.UserRole.Options = data.data.Data.users;
            }
        });
        setTimeout(function () {
            $scope.InjectDatetTimePicker();
        }, 100);
    };

    var GetRoleDateFormat = function (date) {
        if (date == '0001-01-01T00:00:00' || date == '1-01-01T00:00:00')
            date = "";
        else
            date = $filter('date')(date, 'MM/dd/yyyy');
        return date;
    };

    $scope.Click_SetUserRoleDate = function (row) {
        $scope.ui.data.Working_UserRole.Options = row;
        $('.date_picker input').on('changeDate', function (e) {
            if ($(e.target).hasClass('start-date')) {
                $scope.ui.data.Working_UserRole.Options.StartTime = e.target.value;
            }

            if ($(e.target).hasClass('end-date')) {
                $scope.ui.data.Working_UserRole.Options.ExpiresTime = e.target.value;
            }
            $scope.Click_UpdateUserRole($scope.ui.data.Working_UserRole.Options);
        });
    };

    $scope.Click_ShowSearchByUser = function () {
        $scope.ShowSearchByUser = true;
        setTimeout(function () {
            $('input#User_value').focus();
        }, 200);
    };

    $scope.Click_RoleUserAdd = function (selectedUser) {
        if (selectedUser != undefined && selectedUser.originalObject != undefined && selectedUser.originalObject.Value != undefined) {
            var User = $.extend(true, {}, $scope.ui.data.Working_UserRole.Options);
            User.UserId = selectedUser.originalObject.Value;
            User.DisplayName = selectedUser.originalObject.Label;
            User.UserName = selectedUser.originalObject.UserName;
            User.Email = selectedUser.originalObject.Email;
            User.AvatarUrl = selectedUser.originalObject.AvatarUrl;
            User.RoleId = $scope.ui.data.Working_RoleDto.Options.id;
            common.webApi.post('role/addusertorole', '', User).then(function (data) {
                if (data.data.IsSuccess) {
                    var tablestate = $scope.pagginationData;
                    $scope.Pipe_UserRolePagging(tablestate);
                }
                if (data.HasErrors) {
                    window.parent.ShowNotification(User.DisplayName, data.data.Message, 'error');
                }
            });
            selectedUser.originalObject.Value == undefined;
            $scope.SelectedUser = '';
            $scope.$broadcast('angucomplete-alt:clearInput');
        }
    };

    $scope.UserremoteAPI = function (userInputString, timeoutPromise) {
        return common.webApi.get('role/getsuggestionusers', 'keyword=' + userInputString + '&count=' + 10).then(function (response) { });
    };

    $scope.Click_UpdateUserRole = function (row) {
        if (typeof (row.StartTime) != 'string') {
            var sd = row.StartTime;
            row.StartTime = sd.getFullYear() + "-" + twoDigits(1 + sd.getMonth()) + "-" + twoDigits(sd.getDate()) + "T" + twoDigits(sd.getHours()) + ":" + twoDigits(sd.getMinutes()) + ":" + twoDigits(sd.getSeconds());
        }
        if (typeof (row.ExpiresTime) != 'string') {
            var ed = row.ExpiresTime;
            row.ExpiresTime = ed.getFullYear() + "-" + twoDigits(1 + ed.getMonth()) + "-" + twoDigits(ed.getDate()) + "T" + twoDigits(ed.getHours()) + ":" + twoDigits(ed.getMinutes()) + ":" + twoDigits(ed.getSeconds());
        }
        var valid = true;
        if (typeof row.StartTime != 'undefined' && typeof row.ExpiresTime != 'undefined' && row.StartTime != "" && row.ExpiresTime != "") {
            if ((row.StartTime != "0001-01-01T00:00:00" && row.StartTime != "1-01-01T00:00:00") && (row.ExpiresTime != "0001-01-01T00:00:00" && row.ExpiresTime != "1-01-01T00:00:00")) {
                valid = new Date(row.StartTime) < new Date(row.ExpiresTime);
            }
        }
        if (valid) {
            common.webApi.post('role/saveuserrole', 'notifyuser=' + false + '&isowner=' + true + '&userid=' + row.UserId, row).then(function (data) {
                if (data.data.IsSuccess) {
                    var tablestate = $scope.pagginationData;
                    $scope.Pipe_UserRolePagging(tablestate);
                }
                if (data.HasErrors) {
                    window.parent.ShowNotification('[LS:RolesError]', data.data.Message, 'error');
                }
            });
        }
        else {
            window.parent.ShowNotification('[LS:RolesError]', '[LS:InvalidDate]', 'error');
        }
    };

    $scope.Click_RemoveUser = function (row) {
        window.parent.swal({
            title: "[L:Confirm]",
            text: "[L:DeleteUser]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[L:Delete]",
            cancelButtonText: "[L:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('role/removeUserFromRole', '', row).then(function (response) {
                        if (response.data.IsSuccess) {
                            var tablestate = $scope.pagginationData;
                            $scope.Pipe_UserRolePagging(tablestate);
                        }
                        if (response.data.HasErrors) {
                            window.parent.ShowNotification('[LS:RolesError]', response.data.Message, 'error');
                        }
                    });
                }
            });
    };

});

