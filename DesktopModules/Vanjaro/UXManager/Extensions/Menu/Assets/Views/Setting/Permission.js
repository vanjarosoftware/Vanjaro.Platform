app.controller('setting_permission', function ($scope, $attrs, $http, CommonSvc, SweetAlert, $compile) {

    var common = CommonSvc.getData($scope);

    $scope.CopyPermissionSubfolder = false;

    $scope.Click_Save = function () {
        var PermissionsUsers = [];
        var PermissionsRoles = [];
        var data = {
            PermissionsRoles: $scope.PermissionsRoles,
            PermissionsUsers: $scope.PermissionsUsers,
            PermissionsInherit: $scope.PermissionsInherit,
            FolderIds: $scope.GetFolderIds($scope.CopyPermissionSubfolder)
        };
        common.webApi.post('permission/save', 'folderid=' + $scope.ui.data.FolderID.Value + '&Copyfolder=' + $scope.CopyPermissionSubfolder, data).then(function (Response) {
            if (Response.data.IsSuccess && Response.data.Data !== null) {

                var allFolders = $($(window.parent.$('.uxmanager-modal').find('iframe')[0].contentDocument).find('.Iconsfoldersdiv')).find('.folders');
                var objectkeys = Object.keys(Response.data.Data);

                $.each(allFolders, function (k, v) {
                    var foid = $(v).attr('id').split("folders")[1];
                    if (objectkeys.includes(foid)) {
                        var dom = $(v).parent();
                        if (Response.data.Data[foid]) {
                            if ($(dom).find('.arrowicon .fa-lock').length <= 0)
                                $(dom).append('<span class="action-icon float-end arrowicon"><em class="fas fa-lock"></em></span>');
                        }
                        else {
                            $(dom).find('.arrowicon .fa-lock').remove();
                        }
                    }
                });

                setTimeout(function () {
                    $(window.parent.document.body).find('.uxmanager-modal [data-bs-dismiss="modal"]').click();
                }, 100);
            }
            else if (Response.data.HasErrors)
                window.parent.ShowNotification('', Response.data.Message, 'error');
        });
    };

    $scope.GetFolderIds = function (CopyPermissionSubfolder) {
        var result = [];
        result.push($scope.ui.data.FolderID.Value.toString());
        if (CopyPermissionSubfolder) {
            var nestedli = $(window.parent.$('.uxmanager-modal').find('iframe')[0].contentDocument).find('#folders' + $scope.ui.data.FolderID.Value.toString()).parent().next('ul').find('li');
            $.each(nestedli, function (k, v) {
                result.push($(v).find('.folders').attr('id').split("folders")[1]);
            });
        }
        return result;
    };
});