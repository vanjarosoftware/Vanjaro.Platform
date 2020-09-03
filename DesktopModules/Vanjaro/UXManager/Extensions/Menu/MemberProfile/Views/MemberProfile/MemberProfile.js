app.controller('memberprofile_memberprofile', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.Loaded = false;
    $scope.HeaderText = "[L:MemberProfile]";
    //Init Scope
    $scope.onInit = function () {
        $scope.Loaded = true;
    };

    $scope.Add_MemberProfile = function () {
        parent.OpenPopUp(event, 900, 'right', '[L:AddMemberProfileTitle]', $scope.ui.data.MemberProfileUrl.Value + '#memberprofilesettings');
    };
    $scope.Click_Settings = function (row) {
        parent.OpenPopUp(event, 900, 'right', '[L:UpdateMemberProfile]', $scope.ui.data.MemberProfileUrl.Value + '#memberprofilesettings?mpid=' + row.PropertyDefinitionId);
    };
    $scope.Click_MemberProfileSettings = function () {
        parent.OpenPopUp(event, 700, 'right', '[L:UserProfileSettings]', $scope.ui.data.MemberProfileUrl.Value + '#settings');
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
                    common.webApi.post('memberprofile/DeleteProfileProperty', 'propertyId=' + row.PropertyDefinitionId, '').success(function (Response) {
                        if (Response.IsSuccess) {
                            $scope.ui.data.MemberProfile.Options = Response.Data.MemberProfile;
                        }
                        if (Response.HasErrors) {
                            window.parent.ShowNotification(row.PropertyName, Response.Message, 'error');
                        }
                    });
                }
            });
        return false;
    };

    $scope.onGridSorted = function () {
        $scope.ui.data.TemplateProfilePropertyOrders.Options.Properties = $scope.ui.data.MemberProfile.Options;
        common.webApi.post('memberprofile/UpdateProfilePropertyOrders', '', $scope.ui.data.TemplateProfilePropertyOrders.Options).success(function (Response) {
        });
    };

    $scope.RegsiterTooltip = function () {
        if ($('[data-toggle="tooltip"]').tooltip != undefined) { }
        $('[data-toggle="tooltip"]').tooltip()
    };
    $scope.getDefaultVisibilityIcon = function (row) {
        if (row != undefined && row.DefaultVisibility != undefined) {
            if (row.DefaultVisibility == "AllUsers")
                return { '': true };
            else if (row.DefaultVisibility == "MembersOnly")
                return { 'fas fa-user-shield': true };
            else if (row.DefaultVisibility == "AdminOnly")
                return { 'fas fa-user-lock': true };
            else if (row.DefaultVisibility == "FriendsAndGroups")
                return { 'fas fa-users': true };
        }
    };

    $scope.getDefaultVisibilitytitle = function (row) {
        if (row != undefined && row.DefaultVisibility != undefined) {
            if (row.DefaultVisibility == "AllUsers")
                return '[L:DefaultVisibilityAllUsers]';
            else if (row.DefaultVisibility == "MembersOnly")
                return '[L:DefaultVisibilityMembersOnly]';
            else if (row.DefaultVisibility == "AdminOnly")
                return '[L:DefaultVisibilityAdminOnly]';
            else if (row.DefaultVisibility == "FriendsAndGroups")
                return '[L:DefaultVisibilityFriendsAndGroups]';
        }
    };

    $scope.Click_Back = function () {
        event.preventDefault();
        window.location.href = $scope.ui.data.UsersUrl.Value;
    };
});