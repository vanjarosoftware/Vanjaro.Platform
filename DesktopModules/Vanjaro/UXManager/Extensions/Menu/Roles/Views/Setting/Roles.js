app.controller('setting_roles', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);

    $scope.onInit = function () {
        $scope.HeaderText = '[LS:Header]';
        $scope.FilterGroupOption = null;
        $scope.Roles = [];
        $scope.ShowGroup_Roles = false;
        if ($scope.ui.data.RoleGroup !== null && $scope.ui.data.RoleGroup.Value !== null)
            $scope.ui.data.RoleGroup.Value = parseInt($scope.ui.data.RoleGroup.Value);
    };

    $scope.filterGroup = function (option) {
        if (option !== undefined) {
            if (typeof $scope.ui.data.GroupName.Value === "number" && $scope.ui.data.GroupName.Value !== option.GroupId) {
                $scope.pagginationData.pagination.start = 0;
            }
            $scope.FilterGroupOption = option;
            $scope.ui.data.GroupName.Value = option.GroupId;
            $scope.Pipe_RolesPaging($scope.pagginationData);
            $scope.HeaderText = option.Name;
        }
    };

    $scope.Pipe_RolesPaging = function (tableState) {
        //Pipe_(event) is a Method of Smart table(Grid) and using for Pagging
        var GroupRoles = {
            groupId: $scope.ui.data.GroupName.Value,
            startIndex: 0,
            pageSize: 0,
            keyword: null,
        };
        GroupRoles.keyword = $('.input-sm.form-control').val();

        if (tableState !== null && tableState !== 'undefiend' && tableState !== '') {
            tableState.pagination.numberOfPages = 0;
            $scope.pagginationData = tableState;
        }
        if (tableState !== null && tableState !== 'undefiend' && tableState !== '') {
            GroupRoles.startIndex = tableState.pagination.start;
            GroupRoles.pageSize = tableState.pagination.number;
        }
        else {
            GroupRoles.startIndex = $scope.pagginationData.pagination.start;
            GroupRoles.pageSize = $scope.pagginationData.pagination.number;
        }
        if ($scope.ui.data.GroupName.Value !== '' || typeof $scope.ui.data.GroupName.Value === "number") {
            common.webApi.post('rolegroup/getGroupRoles', 'keyword=' + GroupRoles.keyword + '&startIndex=' + GroupRoles.startIndex + '&pageSize=' + GroupRoles.pageSize, GroupRoles).then(function (data) {
                if (data !== null && data.data.Roles !== null) {
                    if (tableState !== null && tableState !== 'undefiend' && tableState !== '') {
                        tableState.pagination.numberOfPages = Math.ceil(data.data.total / GroupRoles.pageSize);
                    }
                    else {
                        $scope.pagginationData.pagination.numberOfPages = Math.ceil(data.data.total / GroupRoles.pageSize);
                        $scope.pagginationData.pagination.start = 0;
                    }
                    $scope.Roles = data.data.Roles;
                    $scope.ShowGroup_Roles = true;
                }
            });
        }
    };

    $scope.Click_EditRole = function (row) {
        parent.OpenPopUp(null, 700, 'right', '[L:EditRoles]', '#!/add?rid=' + row.Id);
    };

    $scope.Click_EditGroup = function (row) {
        parent.OpenPopUp(null, 700, 'right', '[L:EditGroups]', '#!/addgroup?gid=' + row.GroupId);
    };

    $scope.Click_AddUser = function (row) {
        parent.OpenPopUp(null, 700, 'right', '[L:AddUser]', '#!/adduser?rid=' + row.Id);
    };

    $scope.Click_DeleteRole = function (row, type) {
        $scope.ui.data.Working_RoleDto.Options = row;
        window.parent.swal({
            title: "[L:Confirm]",
            text: "[L:DeleteRole]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[L:Delete]",
            cancelButtonText: "[L:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('role/deleterole', '', $scope.ui.data.Working_RoleDto.Options).then(function (data) {
                        if (data.data.Message !== null && data.data.Message.length > 0) {
                            window.parent.ShowNotification('Error!', data.Message, 'error');
                        }
                        if (data.data.IsSuccess) {
                            $scope.Pipe_RolesPaging($scope.pagginationData);
                            $scope.ui.data.RoleGroup.Options = data.data.Data.RoleGroups;
                        }
                    });
                }
            });
    };

    $scope.Click_DeleteGroup = function (row, type) {
        $scope.ui.data.Working_RoleGroupDto.Options.id = row.GroupId;
        window.parent.swal({
            title: "[L:Confirm]",
            text: "[L:DeleteGroup]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[L:Delete]",
            cancelButtonText: "[L:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('rolegroup/DeleteRoleGroup', '', $scope.ui.data.Working_RoleGroupDto.Options).then(function (data) {
                        if (data.data.Message !== null && data.data.Message.length > 0) {
                            window.parent.ShowNotification('Error!', data.data.Message, 'error');
                        }
                        if (data.IsSuccess) {
                            $scope.ui.data.RoleGroup.Options = data.data.Data.AllRoleGroup;
                        }
                    });
                }
            });
    };

    $scope.Click_Back = function () {
        if ($scope.ShowGroup_Roles) {
            $scope.ShowGroup_Roles = false;
            $scope.HeaderText = '[LS:Header]';
            $('.roles .input-sm.form-control').val('');
            $scope.FilterGroupOption = null;
        }
        else
            parent.Click_Back();
    };
});