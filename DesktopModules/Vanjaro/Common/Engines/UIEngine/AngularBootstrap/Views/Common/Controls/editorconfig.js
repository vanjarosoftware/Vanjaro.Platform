app.controller('common_controls_editorconfig', function ($scope, $attrs, $http, $location, CommonSvc, SweetAlert, FileUploader) {
    var common = CommonSvc.getData($scope);
    $scope.ShowLink = true;
    $scope.ShowSetting = false;
    $scope.SelectEditorProfile = true;
    $scope.Profile = null;
    $scope.NewProfile = false;
    $scope.ProfileName = 'Basic';
    $scope.DefaultProfiles = [];
    $scope.inputTextFields = {};
    $scope.PluginsCount = 0;
    $scope.CurrentProfileValue = 0;

    $scope.onInit = function () {

        if ($scope.ui.data.IsSuperUser.Value == "True")
            $scope.ui.data.IsSuperUser.Value = true;
        else
            $scope.ui.data.IsSuperUser.Value = false;

        $scope.ui.data.ModuleID.Value = parseInt($scope.ui.data.ModuleID.Value);

        if ($scope.ui.data.Profiles != undefined && $scope.ui.data.Profiles != null) {
            $scope.ui.data.Profiles.Value = parseInt($scope.ui.data.Profiles.Value);
            $.each($scope.ui.data.Profiles.Options, function (k, v) {
                if (v.ProfileID == $scope.ui.data.Profiles.Value)
                    $scope.ProfileName = v.Name;
                if (v.Name == "Basic" || v.Name == "Standard" || v.Name == "Full" || v.Name == "Minimal")
                    $scope.DefaultProfiles.push(v);
            });
            $.each($scope.ui.data.EditorOptions.Options, function (k, v) {
                if (k != 'Plugins' && k != 'ToolbarDependencies' && (k == 'UiColor' || k == 'Height' || k == 'Width')) {
                    $scope.inputTextFields[k] = v;
                }
            });
            $.each($scope.ui.data.EditorOptions.Options.Plugins, function (k, v) {
                $scope.PluginsCount += 1;
            });
            $scope.CurrentProfileValue = $scope.ui.data.Profiles.Value;
        }
    };

    $scope.ToggleLinkSetting = function (link, setting) {
        $scope.ShowLink = link;
        $scope.ShowSetting = setting;
    };

    $scope.CloseWindow = function () {
        dnnModal.closePopUp(false);
        window.close();
    };


    $scope.SaveEditorProfileConfirm = function (apply) {
        CommonSvc.SweetAlert.swal({
            title: "[L:DeleteProfileTitle]",
            text: "[L:ActionProfileText]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#428bca", confirmButtonText: "[L:OK]",
            cancelButtonText: "[LS:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    $scope.SaveEditorProfile(apply);
                }
            });
    };


    $scope.SaveEditorProfile = function (apply) {
        common.webApi.post('~EditorConfig/SaveEditorProfile', 'uid=' + $scope.ui.data.UID.Value + '&profileid=' + $scope.ui.data.Profiles.Value + '&applyto=' + apply, $scope.ui.data.Settings.Options).success(function (success) {
            if (success) {
                dnnModal.closePopUp(false);
                window.close();
            }
        });
    };

    $scope.ManageProfile = function () {
        if ($scope.ui.data.IsSuperUser.Value == false) {
            $scope.ui.data.Profiles.Options = $scope.ui.data.Profiles.Options.filter(function (item) {
                return item.ProfileID > 0;
            });
        }
        if ($scope.ui.data.Profiles.Options.length == 0) {
            $scope.CreateNewProfile();
        }
        else if ($scope.ui.data.Profiles.Value <= 0 && $scope.ui.data.IsSuperUser.Value == false) {
            $scope.ui.data.Profiles.Value = $scope.ui.data.Profiles.Options[0].ProfileID;
        }
        $scope.SelectEditorProfile = false;
        $scope.changeProfile();
    };

    $scope.showSelectEditorProfile = function () {
        if ($scope.DefaultProfiles != null && $scope.ui.data.IsSuperUser.Value == false) {
            $.each($scope.DefaultProfiles, function (key, value) {
                $scope.ui.data.Profiles.Options.push(value);
            })
        }
        $scope.ui.data.Profiles.Value = $scope.CurrentProfileValue;
        $scope.SelectEditorProfile = true;
        $scope.NewProfile = false;
    };

    $scope.Update = function (sender, ProfileName, Apply) {
        if ($('#txtEditorConfigNewProfile').val() == "") {
            SweetAlert.swal("Please enter a profile name");
            $('#txtNewProfile').focus();
        }
        else {
            $scope.ProfileName = $('#txtEditorConfigNewProfile').val();
            common.webApi.post('~EditorConfig/SaveProfile', 'profileid=' + $scope.ui.data.Profiles.Value + '&profileName=' + $scope.ProfileName + '&uid=' + $scope.ui.data.UID.Value, $scope.ui.data.EditorOptions.Options).success(function (success) {
                if (success) {
                    if (success.Data != undefined && success.Data != null) {
                        $scope.ui.data.UID = success.Data[0];
                        $scope.ui.data.Profiles = success.Data[2];
                        $scope.ui.data.EditorOptions = success.Data[3];
                        if ($scope.ui.data.Profiles != undefined && $scope.ui.data.Profiles != null) {
                            $scope.ui.data.Profiles.Value = parseInt($scope.ui.data.Profiles.Value);
                            $.each($scope.ui.data.Profiles.Options, function (k, v) {
                                if (v.ProfileID == $scope.ui.data.Profiles.Value) {
                                    $scope.ProfileName = v.Name;
                                    $scope.SelectEditorProfile = true;
                                    $scope.NewProfile = false;
                                }
                            });
                        }
                        if (Apply) {
                            if (success.Profile != undefined && success.Profile != null) {
                                $scope.ui.data.Profiles.Value = success.Profile.ProfileID;
                                $scope.CurrentProfileValue = $scope.ui.data.Profiles.Value;
                            }
                            $scope.SaveEditorProfile('CurrentModule');
                        }
                    }
                    else if (success.Message != undefined && success.Message != null && success.Message == "Profile Name Exists") {
                        $scope.ProfileName = '';
                        SweetAlert.swal("Profile name already exists.");
                    }
                }
            });
        }
    };

    $scope.EditorOptions_ngClick = function (isPlugin, key, value) {
        if (isPlugin) {
            $.each($scope.ui.data.EditorOptions.Options.Plugins, function (k, v) {
                if (k == key)
                    $scope.ui.data.EditorOptions.Options.Plugins[k] = value;
            });
        }
        else {
            $.each($scope.ui.data.EditorOptions.Options, function (k, v) {
                if (k == key)
                    $scope.ui.data.EditorOptions.Options[k] = value;
            });
        }
    };

    $scope.CreateNewProfile = function () {
        $scope.NewProfile = true;
        common.webApi.get('~EditorConfig/GetNewProfile').success(function (success) {
            if (success != undefined && success != null) {
                $scope.ui.data.EditorOptions.Options = success.EditorOptions;
                $.each(success.FullPlugins, function (key, value) {
                    if ($scope.ui.data.EditorOptions.Options.Plugins[key] == undefined) {
                        Object.defineProperty($scope.ui.data.EditorOptions.Options.Plugins, key, {
                            value: value,
                            writable: true,
                            enumerable: true,
                            configurable: true
                        });
                    }
                });
                $('#Height').val(success.EditorOptions.Height);
                $('#UiColor').val(success.EditorOptions.UiColor);
                $('#Width').val(success.EditorOptions.Width);
                $scope.ProfileName = "";
                $('#txtEditorConfigNewProfile').val("");
                $scope.ui.data.Profiles.Value = 0;
            }
        });
    };

    $scope.changeProfile = function () {
        $.each($scope.ui.data.Profiles.Options, function (k, v) {
            if (v.ProfileID == $scope.ui.data.Profiles.Value) {
                $scope.ui.data.EditorOptions.Options = JSON.parse(v.Value);
                $scope.ProfileName = v.Name;
                $scope.NewProfile = true;
            }
        });
    };

    $scope.getLocalizedValue = function (key) {
        if (key != undefined)
            return $scope.ui.data.LocalizationKeyValue.Options['' + key + ''];
    };

    $scope.EditorOptions_ngAllClick = function () {
        var IsChecked = $('#EditorOptionsAllPlugins').prop('checked');
        if (IsChecked) {
            $.each($scope.ui.data.EditorOptions.Options.Plugins, function (key, value) {
                $scope.ui.data.EditorOptions.Options.Plugins[key] = true;
            });
        }
        else {
            $.each($scope.ui.data.EditorOptions.Options.Plugins, function (key, value) {
                $scope.ui.data.EditorOptions.Options.Plugins[key] = false;
            });
        }
    };

    $scope.DeleteProfile = function () {
        CommonSvc.SweetAlert.swal({
            title: "[L:DeleteProfileTitle]",
            text: "[L:DeleteProfileText]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#428bca", confirmButtonText: "[LS:Delete]",
            cancelButtonText: "[LS:Cancel]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.delete('~EditorConfig/DeleteProfile', 'profileid=' + $scope.ui.data.Profiles.Value).success(function (success) {
                        if (success != null && success != undefined) {
                            if (success == "deleted")
                                window.location.reload();
                            else if (success == "inuse")
                                setTimeout(function () { CommonSvc.SweetAlert.swal("[L:NotDeleted]") }, 100);
                        }
                    });

                }
            });
    };




});