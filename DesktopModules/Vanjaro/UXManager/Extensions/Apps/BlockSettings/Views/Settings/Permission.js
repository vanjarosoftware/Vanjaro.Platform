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
                var target = window.parent.document.vj_personalization_target;
                if (target != undefined) {
                    if (Response.Data <= 0) {
                        const attr = target.getAttributes();
                        delete attr.perm;
                        target.setAttributes(attr);
                    }
                    else if (Response.Data > 0 && target.attributes.attributes.perm == undefined)
                        target.addAttributes({ perm: Response.Data });
                }
                $scope.Click_Cancel('update');
            }
            else {
                window.parent.ShowNotification('', Response.Message, 'error');
            }
        });
    };

    $scope.Click_Cancel = function (type) {
        window.parent.document.callbacktype = type;
        $(window.parent.document.body).find('[data-dismiss="modal"]').click();
    };
});