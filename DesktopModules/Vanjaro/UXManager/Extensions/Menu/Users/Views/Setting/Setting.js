app.controller('setting_setting', function ($scope, $routeParams, FileUploader, $attrs, $http, CommonSvc, SweetAlert, $filter, $sce) {
    var common = CommonSvc.getData($scope);
    $scope.UserImage = new FileUploader();
    $scope.uid = $routeParams["uid"];
    $scope.ShowUser_accountTab = true;
    $scope.ShowUser_profileTab = false;
    $scope.ShowPermissionsTab = false;
    $scope.ShowAuditTab = false;
    $scope.ShowSearchByRole = false;
    $scope.UserImageDetails = [];
    $scope.notifyUser = false;
    $scope.UpdateRole = [];
    $scope.Show_UserName = false;
    $scope.Regions = [];
    $scope.onInit = function () {
        if ($scope.ui.data.IsAdmin.Value == 'False')
            $scope.Click_ShowTab('User_profile');
        $scope.Click_ShowTab('User_account');
        // Show UserName Option        
        $scope.Show_UserName = ($scope.ui.data.UserDetails.Options.userName != $scope.ui.data.UserDetails.Options.email);
        $.each($('input[type="file"]'), function (key, element) {
            if ($(element).attr('uploader') != undefined) {
                var data = {
                    ControlName: $(element).attr('uploader'),
                    UserID: $scope.uid
                }
                setTimeout(function () {
                    $.extend($scope[data.ControlName].formData[0], data);
                }, 3);
            }
        });

        $(window.parent.document.body).find('.uxmanager-modal').on('hidden.bs.modal', function () {
            $(window.parent.document.body).find(".Gravitar").remove();
        })

        $(window.parent.document.body).find(".Gravitar").remove();
        $('.Gravitar').prependTo($(window.parent.document.body).find('#defaultModalLabel'));

        //$scope.RoleName = $.extend(false, {}, $scope.ui.data.RoleName.Options);

        $.each($scope.ui.data.ProfileProperties.Options, function (key, value) {
            if (value.ControlType == "Country") {
                $scope.Change_Country(value.ProfilePropertyDefinition, 'false');
            }
            if (value.ControlType == "TrueFalse") {
                if (typeof value.ProfilePropertyDefinition.PropertyValue != 'undefined' && value.ProfilePropertyDefinition.PropertyValue != null)
                    value.ProfilePropertyDefinition.PropertyValue = Boolean.parse(value.ProfilePropertyDefinition.PropertyValue);
                else
                    value.ProfilePropertyDefinition.PropertyValue = false;
            }
        });
        $('.Date .date_picker input').datepicker({
            autoclose: true,
            clearBtn: true,
            todayHighlight: true
        });
    };

    $scope.InjectDatetTimePicker = function () {
        $('.user-roles .date_picker input').datepicker({
            autoclose: true,
            clearBtn: true,
            todayHighlight: true
        });
        $('.user-roles .date_picker input').each(function () {
            if (this.value == '0001-01-01T00:00:00' || this.value == '1-01-01T00:00:00')
                $(this).datepicker('clearDates');
            else
                this.value = GetRoleDateFormat(this.value);
        });
    };

    var GetRoleDateFormat = function (date) {
        if (date == '0001-01-01T00:00:00' || date == '1-01-01T00:00:00')
            date = "";
        else
            date = $filter('date')(date, 'MM/dd/yyyy');
        return date;
    };

    $scope.Pipe_UserRolePagging = function (tableState) {
        //Pipe_(event) is a Method of Smart table(Grid) and using for Pagging
        var SearchKeys = {};
        if (Boolean.parse($scope.ui.data.IsAdmin.Value)) {
            SearchKeys.Search_Key = $('.UserRole .input-sm.form-control').val();

            if (tableState != null && tableState != 'undefiend' && tableState != '') {
                tableState.pagination.numberOfPages = 0;
                $scope.pagginationData = tableState;
            }
            if (tableState != null && tableState != 'undefiend' && tableState != '') {
                SearchKeys.skip = tableState.pagination.start;
                if ($scope.ui.data.UserRoles.Options.length - 1 == 0)
                    SearchKeys.skip = 0;
                SearchKeys.pagesize = tableState.pagination.number;
            }
            else {
                SearchKeys.skip = $scope.pagginationData.pagination.start,
                    SearchKeys.pagesize = $scope.pagginationData.pagination.number;
            }

            common.webApi.get('user/getuserroles', 'keyword=' + SearchKeys.Search_Key + '&userId=' + $scope.uid + '&pageindex=' + SearchKeys.skip / SearchKeys.pagesize + '&pagesize=' + SearchKeys.pagesize).then(function (data) {
                if (data.data != null && data.data.Data != null && data.data.IsSuccess && !data.data.HasErrors) {
                    if (tableState != null && tableState != 'undefiend' && tableState != '') {
                        tableState.pagination.numberOfPages = Math.ceil(data.data.Data.totalRecords / SearchKeys.pagesize);
                    }
                    else {
                        $scope.pagginationData.pagination.numberOfPages = Math.ceil(data.data.Data.totalRecords / SearchKeys.pagesize);
                        $scope.pagginationData.pagination.start = 0;
                    }
                    $scope.ui.data.UserRoles.Options = data.data.Data.UserRoles;
                }
            });

            setTimeout(function () {
                $scope.InjectDatetTimePicker();
            }, 100);
        }
    };

    $scope.Click_ShowTab = function (type) {
        if (type == 'User_account') {
            $('#User_account a').addClass("active");
            $('#User_profile a').removeClass("active");
            $('#Permissions a').removeClass("active");
            $('#Audit a').removeClass("active");
            $scope.ShowUser_accountTab = true;
            $scope.ShowUser_profileTab = false;
            $scope.ShowPermissionsTab = false;
            $scope.ShowAuditTab = false;
        }
        else if (type == 'User_profile') {
            $('#User_account a').removeClass("active");
            $('#User_profile a').addClass("active");
            $('#Permissions a').removeClass("active");
            $('#Audit a').removeClass("active");
            $scope.ShowUser_accountTab = false;
            $scope.ShowUser_profileTab = true;
            $scope.ShowPermissionsTab = false;
            $scope.ShowAuditTab = false;
        }
        else if (type == 'Permissions') {
            $('#User_account a').removeClass("active");
            $('#User_profile a').removeClass("active");
            $('#Permissions a').addClass("active");
            $('#Audit a').removeClass("active");
            $scope.ShowUser_accountTab = false;
            $scope.ShowUser_profileTab = false;
            $scope.ShowPermissionsTab = true;
            $scope.ShowAuditTab = false;
        }
        else if (type == 'Audit') {
            $('#User_account a').removeClass("active");
            $('#User_profile a').removeClass("active");
            $('#Permissions a').removeClass("active");
            $('#Audit a').addClass("active");
            $scope.ShowUser_accountTab = false;
            $scope.ShowUser_profileTab = false;
            $scope.ShowPermissionsTab = false;
            $scope.ShowAuditTab = true;
        }
    };

    $scope.Click_CancelSetting = function (type) {
        window.parent.document.callbacktype = type;
        $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
    };

    $scope.Change_Country = function (option, changed) {
        var regionChange = Boolean.parse(changed);
        if (!isNaN(option.PropertyValue) && option.PropertyValue != null) {
            var Id = parseInt(option.PropertyValue);
            if (Id != null) {
                common.webApi.get('user/Regions', 'country=' + Id).then(function (data) {
                    $scope.Regions = data.data;
                    $.each($scope.ui.data.ProfileProperties.Options, function (key, value) {
                        if (regionChange || data.data.length == 1) {
                            if (value.ControlType == "Region") {
                                value.ProfilePropertyDefinition.PropertyValue = "-1";
                            }
                        }
                    });
                });
            }
        }
    };

    $scope.Click_Update = function (Sender, userdata) {
        var Data =
        {
            "UserBasicDto": userdata,
            "ProfilePropertyDefinitionCollection": $scope.ui.data.ProfileProperties.Options
        };
        $('.vj-ux-manager.user-info .col-md-9.left_border.uiengine-wrapper.scrollbar').find('.alert.alert-danger.summary').remove();
        if (mnValidationService.DoValidationAndSubmit('', 'setting_setting')) {
            var fileID = $scope.ui.data.PhotoURL.Options.FileId;
            if (typeof fileID == 'undefined' && fileID == null)
                fileID = -1;

            common.webApi.post('user/updateuserbasicinfo', 'fileid=' + fileID, Data).then(function (data) {
                if (data.data.IsSuccess) {
                    $(window.parent.document.body).find(".Gravitar").remove();
                    $scope.Click_CancelSetting(Sender);
                    window.parent.ShowNotification(userdata.displayName, '[L:UserUpdatedSuccess]', 'success');
                }
                if (data.data.HasErrors) {
                    var errorMessages = "<div class='alert alert-danger summary'>";
                    $.each(data.data.Errors, function (k, v) {
                        if (v.Message != null && v.Message != "") {
                            errorMessages = errorMessages + v.Message + "<br/>";
                        }
                    });
                    errorMessages = errorMessages + "</div>";
                    $('.vj-ux-manager.user-info .col-md-9.left_border.uiengine-wrapper.scrollbar').prepend(errorMessages);
                    parent.ShowNotification('[L:UsersError]', data.Message, 'error');
                }
            });
        }
    };

    $scope.Click_RemoveRole = function (row) {
        window.parent.swal({
            title: "[L:Confirm]",
            text: "[L:DeleteRole]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[L:Yes]",
            cancelButtonText: "[L:No]",
            closeOnConfirm: true,
            closeOnCancel: true
        }, function (isConfirm) {
            if (isConfirm) {
                common.webApi.post('user/removeuserrole', '', row).then(function (response) {
                    if (response.data.IsSuccess) {
                        var tablestate = $scope.pagginationData;
                        $scope.Pipe_UserRolePagging(tablestate);
                        parent.ShowNotification(row.displayName, response.data.Message, 'success');
                    }
                    if (response.data.HasErrors) {
                        parent.ShowNotification(row.displayName, response.data.Message, 'error');
                    }
                });
            }
        });
        return false;
    };

    $scope.CountryremoteAPI = function (userInputString, timeoutPromise) {
        return common.webApi.get('user/countries', 'keyword=' + userInputString).then(function (response) { });
    };
    $scope.RoleremoteAPI = function (userInputString, timeoutPromise) {
        return common.webApi.get('user/getsuggestionroles', 'keyword=' + userInputString).then(function (response) { });
    };

    $scope.Click_UserRoleAdd = function (selectedRole) {
        if (selectedRole != undefined && selectedRole.originalObject != undefined && selectedRole.originalObject.Value != undefined) {
            var Role = $.extend(true, {}, $scope.ui.data.RoleName.Options);
            Role.roleId = selectedRole.originalObject.Value;
            Role.roleName = selectedRole.originalObject.Label;
            common.webApi.post('user/saveuserrole', 'notifyuser=' + $scope.notifyUser + '&isowner=' + false + '&userid=' + $scope.ui.data.UserDetails.Options.userId + '&action=add', Role).then(function (data) {
                if (data.data.IsSuccess) {
                    var tablestate = $scope.pagginationData;
                    $scope.Pipe_UserRolePagging(tablestate);
                    parent.ShowNotification(data.data.Data.displayName, data.data.Message, 'success');
                }
                if (data.data.HasErrors) {
                    parent.ShowNotification($scope.ui.data.UserDetails.Options.displayName, data.data.Message, 'error');
                }
            });
            selectedRole.originalObject.Value == undefined;
            $scope.selectedRole = '';
            $scope.$broadcast('angucomplete-alt:clearInput');
        }
    };

    function twoDigits(d) {
        if (0 <= d && d < 10) return "0" + d.toString();
        if (-10 < d && d < 0) return "-0" + (-1 * d).toString();
        return d.toString();
    };

    $scope.Click_SetUserRoleDate = function (row) {
        $scope.UpdateRole = row;
        $('.user-roles .date_picker input').on('changeDate', function (e) {
            if ($(e.target).hasClass('start-date')) {
                $scope.UpdateRole.startTime = e.target.value;
            }

            if ($(e.target).hasClass('end-date')) {
                $scope.UpdateRole.expiresTime = e.target.value;
            }
            $scope.Click_UpdateUserRole($scope.UpdateRole);
        });
    };

    $scope.Click_UpdateUserRole = function (row) {
        if (typeof (row.startTime) != 'string') {
            var sd = row.startTime;
            row.startTime = sd.getFullYear() + "-" + twoDigits(1 + sd.getMonth()) + "-" + twoDigits(sd.getDate()) + "T" + twoDigits(sd.getHours()) + ":" + twoDigits(sd.getMinutes()) + ":" + twoDigits(sd.getSeconds());
        }
        if (typeof (row.expiresTime) != 'string') {
            var ed = row.expiresTime;
            row.expiresTime = ed.getFullYear() + "-" + twoDigits(1 + ed.getMonth()) + "-" + twoDigits(ed.getDate()) + "T" + twoDigits(ed.getHours()) + ":" + twoDigits(ed.getMinutes()) + ":" + twoDigits(ed.getSeconds());
        }
        var valid = true;
        if (typeof row.startTime != 'undefined' && typeof row.expiresTime != 'undefined' && row.startTime != "" && row.expiresTime != "") {
            if ((row.startTime != "0001-01-01T00:00:00" && row.startTime != "1-01-01T00:00:00") && (row.expiresTime != "0001-01-01T00:00:00" && row.expiresTime != "1-01-01T00:00:00")) {
                valid = new Date(row.startTime) < new Date(row.expiresTime);
            }
        }
        if (valid) {
            common.webApi.post('user/saveuserrole', 'notifyuser=' + false + '&isowner=' + true + '&userid=' + $scope.ui.data.UserDetails.Options.userId + '&action=update', row).then(function (data) {
                if (data.data != null && data.data.Data != null && data.data.IsSuccess && !data.data.HasErrors) {
                    var tablestate = $scope.pagginationData;
                    $scope.Pipe_UserRolePagging(tablestate);
                }
                if (data.data.HasErrors) {
                    parent.ShowNotification('[L:UsersError]', data.data.Message, 'error');
                }
            });
        }
        else {
            window.parent.ShowNotification('[L:UsersError]', '[L:InvalidDate]', 'error');
        }

    };

    $scope.UserImage.onCompleteAll = function () {
        if ($scope.UserImage.progress == 100) {
            var FileIds = [];
            $.each($scope.UserImage.queue, function (key, value) {
                if (value.file.name != "File/s not uploaded successfully!")
                    FileIds.push(parseInt(value.file.name.split('fileid')[1]));
            });
            if (FileIds.length > 0) {
                common.webApi.get('Upload/GetMultipleFileDetails', 'fileids=' + FileIds.join()).then(function (response) {
                    $.each(response.data, function (key, value) {
                        if (value.Name != null) {
                            var Title = (value.Name.split('/').pop()).split('.')[0];
                        }
                        var data = {
                            "Name": value.Name,
                            "FileUrl": value.FileUrl,
                            "FileId": value.FileId,
                            "Title": Title,
                            "KBSize": value.Size,
                            "Size": (value.Size / 1024).toFixed(2) + " KB",
                            "SortOrder": key
                        };
                        $scope.ui.data.PhotoURL.Options = [];
                        $scope.ui.data.PhotoURL.Value = null;
                        $scope.ui.data.PhotoURL.Options = data;
                        $scope.ui.data.UserDetails.Options.avatar = data.FileUrl;
                    });
                    $scope.UserImage.queue = [];
                });
            }
        }
    };

    $scope.$watch('UserImage .selectqueue', function (newValue, oldValue) {
        if (newValue != undefined && newValue.length > 0) {
            $.each(newValue, function (key, value) {
                var FileId = parseInt(value.fileid);
                if (FileId > 0) {
                    common.webApi.get('Upload/GetFile', 'fileid=' + FileId).then(function (response) {
                        if (response.data.Name != null) {
                            var Title = (response.data.Name.split('/').pop()).split('.')[0];
                        }
                        var data = {
                            "Name": response.data.Name,
                            "FileUrl": response.data.FileUrl,
                            "FileId": FileId,
                            "Title": Title,
                            "KBSize": 0,
                            "Size": (0 / 1024).toFixed(2) + " KB",
                            "SortOrder": key
                        };
                        $scope.ui.data.PhotoURL.Options = [];
                        $scope.ui.data.PhotoURL.Value = null;
                        $scope.ui.data.PhotoURL.Options.push(data);
                        $("#uploaded_image").attr("src", response.data.FileUrl);
                        $scope.ui.data.PhotoURL.Value = data.FileUrl;

                    });
                    $scope.UserImage.selectqueue = [];

                }
            });
        }
    });

    $scope.UserImage.onErrorItem = function (item, response, status, headers) {
        CommonSvc.SweetAlert.swal(response.ExceptionMessage);
        $scope.UserImage.progress = 0;
    };

    $scope.Click_ShowSearchByRole = function () {
        $scope.ShowSearchByRole = true;
        setTimeout(function () {
            $('input#Role_value').focus();
        }, 200);
    };

    $scope.Parse = function (option) {
        var markup = option.ProfilePropertyDefinition.PropertyValue == null && option.ProfilePropertyDefinition.PropertyValue == '' ? option.ProfilePropertyDefinition.DefaultValue : option.ProfilePropertyDefinition.PropertyValue;
        return $sce.trustAsHtml(markup);
    }
    $scope.OnLoaded = function () {
        $('.editorcontrol label').hide();
    }
});
app.filter('removeSpacesThenLowercase', function () {
    return function (text) {
        if (typeof text == 'undefined')
            return "";
        var str = text.replace(/\s+/g, '_');
        return str;
    };
});