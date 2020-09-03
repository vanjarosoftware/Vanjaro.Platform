app.controller('memberprofile_memberprofilesettings', function ($scope, $routeParams, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.mpid = $routeParams["mpid"] ? $routeParams["mpid"] : 0;
    $scope.IsUpdate = $scope.mpid > 0 ? true : false;
    $scope.ShowUserProfileFields = true;
    $scope.ShowPropertyLocalization = false;
    $scope.Show_Tab = true;
    $scope.Loaded = false;
    $scope.Show_ListSettingTab = false;
    $scope.Show_ListEntryTab = false;
    //Init Scope
    $scope.onInit = function () {
        if (!$scope.IsUpdate)
            $scope.ui.data.ProfileProperty.Options.ProfileProperty.Length = '';
        $scope.Loaded = true;
    };

    $scope.InitGridSortingEvent = function () {
        $("#memberprofile_memberprofilesettings3grid0 tbody").sortable({
            handle: '.drag', cancel: false, helper: function (e, tr) { tr.children().each(function () { $(this).width($(this).width()); }); return tr; }, axis: 'y', update: function (event, ui) {
                var Seed; $.each(ui.item.closest('tbody').find('tr'), function (index, tr) { var $scope = angular.element($(tr)).scope(); $scope.$apply(function () { if (!isNaN($scope.row["SortOrder"])) { if (isNaN(Seed) || $scope.row["SortOrder"] < Seed) Seed = $scope.row["SortOrder"]; } }); }); if (!isNaN(Seed)) Seed = 0; $.each(ui.item.closest('tbody').find('tr'), function (index, tr) { var $scope = angular.element($(tr)).scope(); $scope.$apply(function () { $scope.row["SortOrder"] = Seed; Seed++; }); });
                $scope.onGridSorted();
            }
        });
        window.setTimeout(function () { $('.drag').css('cursor', 'move'); }, 2000);
    };

    $scope.Click_ShowTab = function (type) {
        if (type == 'UserProfileFields') {
            $('#UserProfileFields a.nav-link').addClass("active");
            $scope.ShowUserProfileFields = true;
        }
    };

    $scope.Click_Next = function (type) {
        $('.vj-ux-manager.memberprofile-settings .datatype-group .input-group').find('.datatype.error').remove();
        var IsDatatypeSelected = parseInt($scope.ui.data.ProfileProperty.Options.ProfileProperty.DataType) > 0;
        if (!IsDatatypeSelected)
            $('.vj-ux-manager.memberprofile-settings').find('.datatype-group .input-group').append('<label id="datatype_error" class="datatype error">This field is required.</label>');
        if (mnValidationService.DoValidationAndSubmit('', 'memberprofile_memberprofilesettings') && IsDatatypeSelected) {
            common.webApi.post('memberprofile/AddUpdateMemberProfile', '', $scope.ui.data.ProfileProperty.Options.ProfileProperty).success(function (Response) {
                if (Response.IsSuccess) {
                    if (parent.document.getElementById("iframe") != null) {
                        var ParentScope = parent.document.getElementById("iframe").contentWindow.angular;
                        if (ParentScope != undefined && ParentScope.element(".menuextension").scope() != undefined && ParentScope.element(".menuextension").scope().ui.data.MemberProfile != undefined) {
                            ParentScope.element(".menuextension").scope().ui.data.MemberProfile.Options = Response.Data.MemberProfile;
                            ParentScope.element(".menuextension").scope().$apply();
                        }
                    }
                    $scope.ui.data.ProfilePropertyLocalization.Options = Response.Data.PropertyLocalization;
                    $scope.ui.data.Entries.Options = Response.Data.Entries;
                    $scope.ShowUserProfileFields = false;
                    if ($scope.ui.data.ProfileProperty.Options.ProfileProperty.DataType == "358") {//358 is List Control
                        $scope.ShowPropertyLocalization = false;
                        $scope.Show_ListEntryTab = true;
                        $scope.Show_ListSettingTab = false;
                        window.setTimeout(function () { $scope.InitGridSortingEvent(); }, 2000);
                    }
                    else {
                        $scope.Show_ListEntryTab = false;
                        $scope.ShowPropertyLocalization = true;
                    }
                }
                else {
                    window.parent.ShowNotification($scope.ui.data.ProfileProperty.Options.ProfileProperty.PropertyName, Response.Message, 'error');
                }
            });
        };
    }

    $scope.Click_Save = function (type) {
        if (mnValidationService.DoValidationAndSubmit('', 'memberprofile_memberprofilesettings')) {
            common.webApi.post('memberprofile/UpdateProfilePropertyLocalization', '', $scope.ui.data.ProfilePropertyLocalization.Options).success(function (Response) {
                if (Response.IsSuccess) {
                    $(window.parent.document.body).find('[data-dismiss="modal"]').click();
                    //window.parent.ShowNotification($scope.ui.data.ProfileProperty.Options.ProfileProperty.PropertyName, Response.Message, 'success');
                }
                else {
                    window.parent.ShowNotification($scope.ui.data.ProfileProperty.Options.ProfileProperty.PropertyName, Response.Message, 'error');
                }
            });
        };
    };

    $scope.Change_Language = function () {
        var cultureCode = $scope.ui.data.ProfilePropertyLocalization.Options.Language;
        var propertyName = $scope.ui.data.ProfilePropertyLocalization.Options.PropertyName;
        var propertyCategory = $scope.ui.data.ProfilePropertyLocalization.Options.PropertyCategory;
        common.webApi.get('memberprofile/GetProfilePropertyLocalization', 'cultureCode=' + cultureCode + '&propertyName=' + propertyName + '&propertyCategory=' + propertyCategory).success(function (Response) {
            if (Response.IsSuccess) {
                $scope.ui.data.ProfilePropertyLocalization.Options = Response.Data;
            }
        });
    };

    $scope.ShowAdd_ListEntry = function () {
        $scope.ui.data.ListEntryRequest.Options = $.extend(true, {}, $scope.ui.data.TemplateListEntryRequest.Options);
        $scope.Show_ListSettingTab = true;
    };

    $scope.Click_Settings = function (row) {
        if (row != undefined) {
            $scope.ui.data.ListEntryRequest.Options.EntryId = row.EntryID;
            $scope.ui.data.ListEntryRequest.Options.Text = row.Text;
            $scope.ui.data.ListEntryRequest.Options.Value = row.Value;
            $scope.ui.data.ListEntryRequest.Options.EnableSortOrder = row.EnableSortOrder;
            $scope.Show_ListSettingTab = true;
        }
    };

    $scope.AddUpdate_ListEntry = function (row) {
        if (mnValidationService.DoValidationAndSubmit('', 'memberprofile_memberprofilesettings')) {
            $scope.ui.data.ListEntryRequest.Options.ListName = $scope.ui.data.ProfileProperty.Options.ProfileProperty.PropertyName;
            common.webApi.post('memberprofile/updatelistentry', '', $scope.ui.data.ListEntryRequest.Options).success(function (Response) {
                if (Response.IsSuccess) {
                    $scope.ui.data.Entries.Options = Response.Data.Entries;
                    $scope.Show_ListSettingTab = false;
                    if ($scope.ui.data.Entries.Options.length > 1)
                        window.setTimeout(function () { $scope.InitGridSortingEvent(); }, 2000);
                }
                else {
                    window.parent.ShowNotification($scope.ui.data.ProfileProperty.Options.ProfileProperty.PropertyName, Response.Message, 'error');
                }
            });
        };
    };
    $scope.Click_Delete = function (row) {
        window.parent.swal({
            title: "[L:DeleteTitle]",
            text: "[L:DeleteText]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[L:DeleteButton]",
            cancelButtonText: "[L:CancelButton]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('memberprofile/DeleteListEntry', 'entryId=' + row.EntryID + '&propertyName=' + $scope.ui.data.ProfileProperty.Options.ProfileProperty.PropertyName, '').success(function (Response) {
                        if (Response.IsSuccess) {
                            $scope.ui.data.Entries.Options = Response.Data.Entries;
                        }
                        if (Response.HasErrors) {
                            window.parent.ShowNotification(row.PropertyName, Response.Message, 'error');
                        }
                    });
                }
            });
        return false;
    };

    $scope.ListNext = function () {
        $scope.ShowUserProfileFields = false;
        $scope.Show_ListSettingTab = false;
        $scope.Show_ListEntryTab = false;
        $scope.ShowPropertyLocalization = true;
    };

    $scope.Click_Cancel = function () {
        $scope.Show_ListSettingTab = false;
    };
    $scope.onGridSorted = function () {
        $scope.ui.data.TemplateListEntryOrdersRequest.Options.Entries = $scope.ui.data.Entries.Options;
        common.webApi.post('memberprofile/UpdateListEntryOrders', '', $scope.ui.data.TemplateListEntryOrdersRequest.Options).success(function (Response) {
        });
    };

    $scope.$watch('ui.data.ProfileProperty.Options.ProfileProperty.Required', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            if (newValue) {
                $scope.ui.data.ProfileProperty.Options.ProfileProperty.Visible = true;
                $scope.ui.data.ProfileProperty.Options.ProfileProperty.ReadOnly = false;
            }
            else {
                $scope.ui.data.ProfileProperty.Options.ProfileProperty.Visible = true;
                $scope.ui.data.ProfileProperty.Options.ProfileProperty.ReadOnly = false;
            }
        }
    });

    $scope.$watch('ui.data.ProfileProperty.Options.ProfileProperty.Visible', function (newValue, oldValue) {
        if (newValue != undefined && oldValue != undefined) {
            if (!newValue) {
                $scope.ui.data.ProfileProperty.Options.ProfileProperty.ReadOnly = false;
            }
        }
    });
});