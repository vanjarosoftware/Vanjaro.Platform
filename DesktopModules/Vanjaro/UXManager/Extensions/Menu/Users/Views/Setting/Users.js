app.controller('setting_users', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.onInit = function () {
        $scope.WorkingUser = [];
        $scope.IsDeleted = false;
        $scope.ShowRecyclebin = false;
        $scope.DeletedUsers = [];
        $scope.RestoreUsers = [];
        $scope.ui.data.UserFilters.Value = parseInt($scope.ui.data.UserFilters.Value);
        $scope.ui.data.CurrentUserId.Value = parseInt($scope.ui.data.CurrentUserId.Value);
        $scope.ui.data.DeletedUsers.Value = parseInt($scope.ui.data.DeletedUsers.Value);

        $scope.HeaderText = "[L:Users]";
        $scope.Show_RecycleBin = false;
        $('[rel=popup]').click(function (event, width, title, url) {
            width = $(this).data('width');
            title = $(this).data('title');
            url = $(this).attr('href');
            parent.OpenPopUp(event, width, 'right', title, url);
            return false;
        });
    };

    $scope.Open_MemberProfile = function () {
        event.preventDefault();
        window.location.href = $scope.ui.data.MemberProfileUrl.Value;
    };

    $scope.Click_LoginAs = function (userId) {
        common.webApi.post('impersonation/handleimpersonation', 'iuserid=' + userId, '').then(function (data) {

            if (data.data != null && data.data.Data != null && data.data.IsSuccess) {
                window.parent.location.href = data.data.Data;
            }
            else {
                window.parent.ShowNotification('[L:Users]', data.data.Message, 'error');
            }

        });
    };

    $scope.Click_Back = function () {
        if ($scope.Show_RecycleBin) {
            $scope.Show_RecycleBin = false;
            $scope.HeaderText = "[L:Users]";
        }
        else {
            parent.Click_Back();
            $('.users .input-sm.form-control').val('');
        }
    };

    $scope.Pipe_AllUserPagging = function (tableState) {
        //Pipe_(event) is a Method of Smart table(Grid) and using for Pagging
        var SearchKeys = {};
        SearchKeys.Search_Key = $('.input-sm.form-control').val();

        if (tableState != null && tableState != 'undefiend' && tableState != '') {
            tableState.pagination.numberOfPages = 0;
            $scope.allUserPagginationData = tableState;
        }
        if (tableState != null && tableState != 'undefiend' && tableState != '') {
            SearchKeys.skip = tableState.pagination.start,
                SearchKeys.pagesize = tableState.pagination.number;
        }
        else {
            SearchKeys.skip = $scope.allUserPagginationData.pagination.start,
                SearchKeys.pagesize = $scope.allUserPagginationData.pagination.number;
        }
        SearchKeys.pagesize = parseInt($('#setting_users1grid0').attr('pagesize'));

        common.webApi.get('user/getusers', 'searchtext=' + SearchKeys.Search_Key + '&filter=' + $scope.ui.data.UserFilters.Value + '&pageindex=' + SearchKeys.skip / SearchKeys.pagesize + '&pagesize=' + SearchKeys.pagesize + '&sortcolumn=&sortascending=false').then(function (data) {
            if (data.data != null && data.data.Data != null && data.data.IsSuccess && !data.data.HasErrors) {
                if (tableState != null && tableState != 'undefiend' && tableState != '') {
                    tableState.pagination.numberOfPages = Math.ceil(data.data.Data.TotalResults / SearchKeys.pagesize);
                }
                else {
                    $scope.allUserPagginationData.pagination.numberOfPages = Math.ceil(data.data.Data.TotalResults / SearchKeys.pagesize);
                    $scope.allUserPagginationData.pagination.start = 0;
                }
                $scope.ui.data.AllUsers.Options = data.data.Data.Results;
            }
        });
    };

    $scope.Change_UserFilter = function (option) {
        $scope.ui.data.Select_UserFilters.Options = option;
        $scope.ui.data.UserFilters.Value = option.Value;
        $scope.allUserPagginationData.pagination.start = 0;
        $scope.Pipe_AllUserPagging($scope.allUserPagginationData);
    };

    //$scope.Click_Recyclebin = function () {
    //    $scope.ShowRecyclebin = true;
    //    $scope.pagginationData.pagination.start = 0;
    //    var tablestate = $scope.pagginationData;
    //    $scope.Pipe_DeletedUserPagging(tablestate);
    //};

    $scope.Pipe_DeletedUserPagging = function (tableState) {
        var SearchKeys = {};
        SearchKeys.Search_Key = '';
        if (tableState != null && tableState != 'undefiend' && tableState != '') {
            tableState.pagination.numberOfPages = 0;
            $scope.pagginationData = tableState;
            SearchKeys.Search_Key = $('.deletedusers .input-sm.form-control').val();
        }
        if (tableState != null && tableState != 'undefiend' && tableState != '') {
            SearchKeys.skip = tableState.pagination.start;
            if ($scope.DeletedUsers.length - 1 == 0)
                SearchKeys.skip = 0;
            SearchKeys.pagesize = tableState.pagination.number;
        }
        else {
            SearchKeys.skip = $scope.pagginationData.pagination.start,
                SearchKeys.pagesize = $scope.pagginationData.pagination.number;
        }
        SearchKeys.pagesize = parseInt($('#setting_users2grid0').attr('pagesize'));
        common.webApi.get('user/getdeleteduserlist', 'searchtext=' + SearchKeys.Search_Key + '&pageindex=' + SearchKeys.skip / SearchKeys.pagesize + '&pagesize=' + SearchKeys.pagesize).then(function (data) {
            if (data.data != null && data.data.Data != null && data.data.IsSuccess && !data.data.HasErrors) {
                if (tableState != null && tableState != 'undefiend' && tableState != '') {
                    tableState.pagination.numberOfPages = Math.ceil(data.data.Data.TotalResults / SearchKeys.pagesize);
                }
                else {
                    $scope.pagginationData.pagination.numberOfPages = Math.ceil(data.data.Data.TotalResults / SearchKeys.pagesize);
                    $scope.pagginationData.pagination.start = 0;
                }
                $scope.DeletedUsers = data.data.Data.Results;
                $scope.ui.data.DeletedUsers.Value = data.data.Data.TotalResults;
            }
        });
    };

    $scope.Click_ForceChangePassword = function (row) {
        common.webApi.post('user/forcechangepassword', 'userid=' + row.userId, '').then(function (data) {
            if (data.data != null && data.data.IsSuccess && !data.data.HasErrors) {
                window.parent.ShowNotification(row.displayName, '[L:Success_ForceChangePasswordMessage]', 'success');
            }
            else {
                window.parent.ShowNotification(row.displayName, data.data.Message, 'error');
            }
        });
    };

    $scope.Click_SendPasswordResetLink = function (row) {
        common.webApi.post('user/sendpasswordresetlink', 'userid=' + row.userId, '').then(function (data) {
            if (data.data != null && data.data.IsSuccess && !data.data.HasErrors) {
                window.parent.ShowNotification(row.displayName, '[L:PasswordSent]', 'success');
            }
            else {
                window.parent.ShowNotification(row.displayName, data.data.Message, 'error');
            }
        });
    };

    $scope.Click_SoftDelete = function (row) {
        window.parent.swal({
            title: "[L:Confirm]",
            text: "[L:DeleteUser]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[L:Yes]",
            cancelButtonText: "[L:No]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('user/softdeleteuser', 'userid=' + row.userId, '').then(function (data) {
                        if (data.data != null && data.data.IsSuccess && !data.data.HasErrors) {
                            row.isDeleted = true;
                            var index = $scope.ui.data.AllUsers.Options.indexOf(row);
                            $scope.ui.data.AllUsers.Options.splice(index, 1);
                            if ($scope.ui.data.AllUsers.Options.length == 0 && $scope.pagginationData.pagination.start != 0)
                                $scope.pagginationData.pagination.start = $scope.pagginationData.pagination.start - parseInt($('#setting_users1grid0').attr('pagesize'));
                            $scope.Pipe_AllUserPagging($scope.pagginationData);
                            //$scope.ui.data.DeletedUsers.Options.push(row);
                            $scope.pagginationData.pagination.start = 0;
                            $scope.Pipe_DeletedUserPagging($scope.pagginationData);
                            window.parent.ShowNotification(row.displayName, '[L:Success_SoftDeleteMessage]', 'success');
                        }
                        else {
                            window.parent.ShowNotification(row.displayName, data.data.Message, 'error');
                        }
                    });
                }
            });
        return false;
    };

    $scope.Click_HardDelete = function (row) {
        window.parent.swal({
            title: "[L:Confirm]",
            text: "[L:DeleteUserPermanently]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "Yes",
            cancelButtonText: "No",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('user/harddeleteuser', 'userid=' + row.userId, '').then(function (data) {
                        if (data.data != null && data.data.IsSuccess && !data.data.HasErrors) {
                            var index = $scope.ui.data.AllUsers.Options.indexOf(row);
                            $scope.ui.data.AllUsers.Options.splice(index, 1);
                            if ($scope.ui.data.AllUsers.Options.length == 0 && $scope.pagginationData.pagination.start != 0)
                                $scope.pagginationData.pagination.start = $scope.pagginationData.pagination.start - parseInt($('#setting_users1grid0').attr('pagesize'));
                            $scope.Pipe_AllUserPagging($scope.pagginationData);
                            $scope.pagginationData.pagination.start = 0;
                            $scope.Pipe_DeletedUserPagging($scope.pagginationData);
                        }
                        else {
                            window.parent.ShowNotification(row.displayName, data.data.Message, 'error');
                        }
                    });
                }
            });
    };


    $scope.Remove_Users = function () {
        window.parent.swal({
            title: "[L:Confirm]",
            text: "[L:PermanentlyRemoveUsersMessage]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "Yes",
            cancelButtonText: "No",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('user/removeusers', '').then(function (data) {
                        if (data.data != null && data.data.IsSuccess) {
                            $scope.pagginationData.pagination.start = 0;
                            $scope.Pipe_DeletedUserPagging($scope.pagginationData);
                        }
                        else {
                            window.parent.ShowNotification('[L:Users]', data.data.Message, 'error');
                        }
                    });
                }
            });

    };

    $scope.Remove_User = function (row) {
        $scope.RemoveUsers = [];
        $scope.RemoveUsers.push(row);
        common.webApi.post('user/removeuser', '', $scope.RemoveUsers).then(function (data) {
            if (data.data != null && data.data.IsSuccess && !data.data.HasErrors && !data.data.Data.Status) {
                //var index = $scope.ui.data.DeletedUsers.Options.indexOf(row);
                //$scope.ui.data.DeletedUsers.Options.splice(index, 1);
                //var index = $scope.DeletedUsers.indexOf(row);
                //$scope.DeletedUsers.splice(index, 1);
                if ($scope.DeletedUsers.length == 0 && $scope.pagginationData.pagination.start != 0)
                    $scope.pagginationData.pagination.start = $scope.pagginationData.pagination.start - parseInt($('#setting_users2grid0').attr('pagesize'));
                $scope.Pipe_DeletedUserPagging($scope.pagginationData);
                $scope.Pipe_AllUserPagging($scope.allUserPagginationData);
                window.parent.ShowNotification(row.DisplayName, '[L:Success_RemoveUserMessage]', 'success');
                if ($scope.Show_RecycleBin)
                    $scope.Click_Back();
            }
            else {
                window.parent.ShowNotification(row.DisplayName, data.data.Message, 'error');
            }
        });
    };



    $scope.Restore_User = function (row) {
        $scope.RestoreUsers = [];
        $scope.RestoreUsers.push(row);
        common.webApi.post('user/restoreuser', '', $scope.RestoreUsers).then(function (data) {
            if (data.data != null && data.data.IsSuccess && !data.data.HasErrors && !data.data.Data.Status) {
                //var index = $scope.ui.data.DeletedUsers.Options.indexOf(row);
                //$scope.ui.data.DeletedUsers.Options.splice(index, 1);
                //var index = $scope.DeletedUsers.indexOf(row);
                //$scope.ui.data.DeletedUsers.Options.splice(index, 1);
                if ($scope.DeletedUsers.length == 0 && $scope.pagginationData.pagination.start != 0)
                    $scope.pagginationData.pagination.start = $scope.pagginationData.pagination.start - parseInt($('#setting_users2grid0').attr('pagesize'));
                $scope.Pipe_DeletedUserPagging($scope.pagginationData);
                $scope.pagginationData.pagination.start = 0;
                $scope.Pipe_AllUserPagging($scope.pagginationData);
                $scope.Show_RecycleBin = false;
                $scope.HeaderText = "[L:Users]";
                window.parent.ShowNotification(row.DisplayName, '[L:Success_RestoreUserMessage]', 'success');
            }
            else {
                window.parent.ShowNotification(row.DisplayName, data.data.Message, 'error');
            }
        });
    };

    $scope.Click_Restore = function (row) {
        common.webApi.post('user/restoredeleteduser', 'userid=' + row.userId, '').then(function (data) {
            if (data.data != null && data.data.IsSuccess && !data.data.HasErrors) {
                row.isDeleted = false;
                if ($scope.ui.data.UserFilters.Value == 2) {
                    var index = $scope.ui.data.AllUsers.Options.indexOf(row);
                    $scope.ui.data.AllUsers.Options.splice(index, 1);
                }
                if ($scope.ui.data.AllUsers.Options.length == 0 && $scope.pagginationData.pagination.start != 0)
                    $scope.pagginationData.pagination.start = $scope.pagginationData.pagination.start - parseInt($('#setting_users1grid0').attr('pagesize'));
                $scope.Pipe_AllUserPagging($scope.pagginationData);
                $scope.pagginationData.pagination.start = 0;
                $scope.Pipe_DeletedUserPagging($scope.pagginationData);
                window.parent.ShowNotification(row.displayName, '[L:Success_RestoreUserMessage]', 'success');
            }
            else {
                window.parent.ShowNotification(row.displayName, data.data.Message, 'error');
            }
        });
    };


    $scope.Click_UpdateAuthorizeStatus = function (row) {

        if (row.authorized) {
            window.parent.swal({
                title: "[L:Confirm]",
                text: "[L:UserDeauthorize]",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55", confirmButtonText: "[L:ConfirmYes]",
                cancelButtonText: "[L:ConfirmNo]",
                closeOnConfirm: true,
                closeOnCancel: true
            },
                function (isConfirm) {
                    if (isConfirm) {
                        UpdateAuthorizeStatus(row);
                    }
                });
        }
        else {
            UpdateAuthorizeStatus(row);
        }
    };

    var UpdateAuthorizeStatus = function (row) {
        var setAuthorized = null;
        if (row.authorized)
            setAuthorized = false;
        if (!row.authorized)
            setAuthorized = true;
        common.webApi.post('user/updateauthorizestatus', 'userid=' + row.userId + '&authorized=' + setAuthorized, '').then(function (data) {
            if (data.data != null && data.data.IsSuccess && !data.data.HasErrors) {
                row.authorized = setAuthorized;
                if (row.authorized) {
                    if ($scope.ui.data.UserFilters.Value == 1) {
                        var index = $scope.ui.data.AllUsers.Options.indexOf(row);
                        $scope.ui.data.AllUsers.Options.splice(index, 1);
                    }
                }
                if (!row.authorized) {
                    if ($scope.ui.data.UserFilters.Value == 0) {
                        var index = $scope.ui.data.AllUsers.Options.indexOf(row);
                        $scope.ui.data.AllUsers.Options.splice(index, 1);
                    }
                }
                if ($scope.ui.data.AllUsers.Options.length == 0 && $scope.pagginationData.pagination.start != 0)
                    $scope.pagginationData.pagination.start = $scope.pagginationData.pagination.start - parseInt($('#setting_users1grid0').attr('pagesize'));
                $scope.Pipe_AllUserPagging($scope.pagginationData);
                $scope.pagginationData.pagination.start = 0;
                $scope.Pipe_DeletedUserPagging($scope.pagginationData);
                window.parent.ShowNotification(row.displayName, data.data.Message, 'success');
            }
            else {
                window.parent.ShowNotification(row.displayName, data.data.Message, 'error');
            }
        });
    };

    $scope.Click_UpdateSuperUserStatus = function (row) {

        if (!row.isSuperUser) {
            window.parent.swal({
                title: "[L:Confirm]",
                text: "[L:UserGranted]",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55", confirmButtonText: "[L:ConfirmYes]",
                cancelButtonText: "[L:ConfirmNo]",
                closeOnConfirm: true,
                closeOnCancel: true
            },
                function (isConfirm) {
                    if (isConfirm) {
                        UpdateGrantedStatus(row);
                    }
                });
        }
        else {
            UpdateGrantedStatus(row);
        }
    };

    var UpdateGrantedStatus = function (row) {
        var setSuperUser = null;
        if (row.isSuperUser)
            setSuperUser = false;
        if (!row.isSuperUser)
            setSuperUser = true;
        common.webApi.post('user/updatesuperuserstatus', 'userid=' + row.userId + '&setsuperuser=' + setSuperUser, '').then(function (data) {
            if (data.data != null && data.data.IsSuccess && !data.data.HasErrors) {
                row.isSuperUser = setSuperUser;
                if (row.isSuperUser) {
                    if ($scope.ui.data.UserFilters.Value != 3 && $scope.ui.data.UserFilters.Value != 5) {
                        var index = $scope.ui.data.AllUsers.Options.indexOf(row);
                        $scope.ui.data.AllUsers.Options.splice(index, 1);
                    }
                }
                if (!row.isSuperUser) {
                    if ($scope.ui.data.UserFilters.Value == 3) {
                        var index = $scope.ui.data.AllUsers.Options.indexOf(row);
                        $scope.ui.data.AllUsers.Options.splice(index, 1);
                    }
                }
                if ($scope.ui.data.AllUsers.Options.length == 0 && $scope.pagginationData.pagination.start != 0)
                    $scope.pagginationData.pagination.start = $scope.pagginationData.pagination.start - parseInt($('#setting_users1grid0').attr('pagesize'));
                $scope.Pipe_AllUserPagging($scope.pagginationData);
                $scope.pagginationData.pagination.start = 0;
                $scope.Pipe_DeletedUserPagging($scope.pagginationData);
                if (row.isSuperUser)
                    window.parent.ShowNotification(row.displayName, '[L:Success_StatusChangeGrant]', 'success');
                else
                    window.parent.ShowNotification(row.displayName, '[L:Success_StatusChangeRevoke]', 'success');
            }
            else {
                window.parent.ShowNotification(row.displayName, data.data.Message, 'error');
            }
        });
    };

    $scope.Click_UserSettings = function (row) {
        parent.OpenPopUp(null, 800, 'right', null, '#!/setting?uid=' + row.userId);
    };

    $scope.Click_ManagePassword = function (row) {
        parent.OpenPopUp(null, 400, 'center', '[L:ChangePassword]', '#!/changepassword?uid=' + row.userId + '&uname=' + row.displayName, 350);
    };

});