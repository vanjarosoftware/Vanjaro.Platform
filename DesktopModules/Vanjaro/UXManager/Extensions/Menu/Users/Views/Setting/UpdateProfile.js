app.controller('setting_updateprofile', function ($scope, $routeParams, FileUploader, $attrs, $http, CommonSvc, SweetAlert, $filter, $sce) {
    var common = CommonSvc.getData($scope);
    $scope.uid = $routeParams["uid"];
    $scope.ShowUser_profileTab = true;
    $scope.Show_UserName = false;
    $scope.Regions = [];
    $scope.onInit = function () {
        var formData = {
            userId: $scope.uid,
            username: $('#UserId', window.parent.document).val(),
            password: $('#Password', window.parent.document).val()
        };
        common.webApi.post('updateprofile/getsettings', '', formData).then(function (data) {
            if (data.data != null && data.data.Data != null && data.data.IsSuccess) {
                $.each(JSON.parse(data.data.Data), function (key, value) {
                    $scope.ui.data[value.Name] = value;
                });
                $scope.Click_ShowTab('User_profile');
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
                $(window.parent.document.body).find(".Gravitar").css('display', 'block')
                if (data.data.Message != null && data.data.Message != '')
                    $('.vj-ux-manager.user-info .col-md-9.left_border.uiengine-wrapper.scrollbar').prepend('<div class="alert alert-warning" role="alert">' + data.data.Message + '</div>');
            }
            else {
                if (data.data.IsRedirect && data.data.RedirectURL != null) {
                    $(window.parent.document.body).find(".Gravitar").remove();
                    window.location.href = data.data.RedirectURL;
                }
            }
        });

        $(window.parent.document.body).find('.uxmanager-modal').on('hidden.bs.modal', function () {
            $(window.parent.document.body).find(".Gravitar").remove();
        })

        $(window.parent.document.body).find(".Gravitar").remove();
        $('.Gravitar').prependTo($(window.parent.document.body).find('#defaultModalLabel'));

        $('.Date .date_picker input').datepicker({
            autoclose: true,
            clearBtn: true,
            todayHighlight: true
        });
    };

    $scope.Click_ShowTab = function (type) {
        if (type == 'User_profile') {
            $('#User_profile a').addClass("active");
            $scope.ShowUser_profileTab = true;
        }
    };

    $scope.Change_Country = function (option, changed) {
        var regionChange = Boolean.parse(changed);
        if (!isNaN(option.PropertyValue) && option.PropertyValue != null) {
            var Id = parseInt(option.PropertyValue);
            if (Id != null) {
                common.webApi.get('updateprofile/Regions', 'country=' + Id).success(function (data) {
                    $scope.Regions = data;
                    $.each($scope.ui.data.ProfileProperties.Options, function (key, value) {
                        if (regionChange || data.length == 1) {
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
            "ProfilePropertyDefinitionCollection": $scope.ui.data.ProfileProperties.Options,
            "UserCredential": {
                "Username": $('#UserId', window.parent.document).val(),
                "Password": $('#Password', window.parent.document).val()
            }
        };
        $('.vj-ux-manager.user-info .col-md-9.left_border.uiengine-wrapper.scrollbar .vj-alert-message').find('.alert.alert-danger.summary').remove();
        if (mnValidationService.DoValidationAndSubmit('', 'setting_setting')) {
            var fileID = $scope.ui.data.PhotoURL.Options.FileId;
            if (typeof fileID == 'undefined' && fileID == null)
                fileID = -1;

            common.webApi.post('updateprofile/updateuserbasicinfo', 'fileid=' + fileID, Data).then(function (data) {
                if (data.data.IsSuccess) {
                    if (data.IsRedirect) {
                        window.parent.location.href = data.data.RedirectURL;
                    }
                    else {
                        $(window.parent.document.body).find(".Gravitar").remove();
                        $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
                    }
                }
                if (data.data.HasErrors) {
                    var errorMessages = "<div class='alert alert-danger summary'>";
                    $.each(data.data.Errors, function (k, v) {
                        if (v.Message != null && v.Message != "") {
                            errorMessages = errorMessages + v.Message + "<br/>";
                        }
                    });
                    errorMessages = errorMessages + "</div>";
                    $('.vj-ux-manager.user-info .col-md-9.left_border.uiengine-wrapper.scrollbar .vj-alert-message').prepend(errorMessages);
                    parent.ShowNotification('[L:UsersError]', data.data.Message, 'error');
                }
            });
        }
    };

    $scope.CountryremoteAPI = function (userInputString, timeoutPromise) {
        return common.webApi.get('updateprofile/countries', 'keyword=' + userInputString).success(function (response) { });
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