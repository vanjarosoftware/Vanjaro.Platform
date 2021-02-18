app.controller('settings_permission', function ($scope, $attrs, $http, CommonSvc) {

    var common = CommonSvc.getData($scope);

    $scope.Click_Save = function () {
        var data = {
            PermissionsRoles: $scope.PermissionsRoles,
            PermissionsUsers: $scope.PermissionsUsers,
            PermissionsInherit: $scope.PermissionsInherit
        };
        common.webApi.post('permission/update', 'entityid=' + $scope.ui.data.EntityID.Value + '&entity=' + $scope.ui.data.Entity.Value, data).success(function (Response) {
            if (Response.IsSuccess) {
                var attrAdded = false;
                var target = window.parent.document.vj_personalization_target;
                if (target != undefined) {
                    if (Response.Data <= 0) {
                        const attr = target.getAttributes();
                        delete attr.perm;
                        target.setAttributes(attr);
                    }
                    else if (Response.Data > 0 && target.attributes.attributes.perm == undefined) {
                        target.addAttributes({ perm: Response.Data });
                        attrAdded = true;
                    }
                }
                $scope.Click_Cancel('update', attrAdded);
            }
            else {
                window.parent.ShowNotification('', Response.Message, 'error');
            }
        });
    };

    $scope.Click_Cancel = function (type, attrAdded) {
        if (attrAdded)
            window.parent.ShowNotification('[L:Section]', '[L:PermissionAddedSuccess]', 'warning');
        else
            window.parent.ShowNotification('[L:Section]', '[L:PermissionUpdatedSuccess]', 'success');
        window.parent.document.callbacktype = type;
        $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
    };

    $scope.$watch('PermissionsInherit', function (newValue, oldValue) {
        if (newValue != undefined) {
            $scope.ShowHideGrid(newValue);
        }
    });

    $scope.ShowHideGrid = function (type) {
        setTimeout(function () {
            if (type) {
                $('.addroles').hide();
                $('.rolesdiv').hide();
                $('.addusers').hide();
                $('.usersdiv').hide();
                $('.empty_msg').hide();
                $('.permission_grid hr').hide();
            }
            else {
                $('.addroles').show();
                $('.rolesdiv').show();
                $('.addusers').show();
                $('.usersdiv').show();
                $('.empty_msg').show();
                $('.permission_grid hr').show();
            }
        }, 10);
    };
});