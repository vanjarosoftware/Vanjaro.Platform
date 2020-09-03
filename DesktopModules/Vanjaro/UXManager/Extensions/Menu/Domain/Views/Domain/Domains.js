app.controller('domain_domains', function ($scope, $routeParams, CommonSvc, SweetAlert) {
    $scope.sid = $routeParams["sid"] ? $routeParams["sid"] : 0;
    var common = CommonSvc.getData($scope);
    $scope.Loaded = false;
    $scope.HeaderText = "[L:Domain]";
    //Init Scope
    $scope.onInit = function () {
        $scope.PortalAliases = $scope.ui.data.PortalAliases.Options.PortalAliases;
        $scope.Search_PortalAliases = $scope.PortalAliases;
        $scope.Loaded = true;
    };

    $scope.Click_AddDomain = function () {
        parent.OpenPopUp(null, 800, 'right', '[L:DomainTitle]', '#/setting');
    };

    $scope.Register_Tooltip = function () {
        $('[data-toggle="tooltip"]').tooltip();
    };

    $scope.Click_UpdateSetting = function (row) {
        if (row != null && row != undefined) {
            row.IsPrimary = !row.IsPrimary;
            common.webApi.post('domain/updatesitealias', '', row).success(function (Response) {
                if (Response.IsSuccess) {
                    $scope.PortalAliases = Response.Data.SiteAliases.PortalAliases;
                    $scope.Search_PortalAliases = $scope.PortalAliases;
                    window.parent.ShowNotification(row.HTTPAlias, row.IsPrimary ? '[L:SetAsDomainStatus]' : '[L:RemoveFromDomainStatus]', 'success');
                }
                else {
                    $scope.PortalAliases = [];
                    $scope.Search_PortalAliases = $scope.PortalAliases;
                    window.parent.ShowNotification(row.HTTPAlias, Response.Message, 'error');
                }
            });
        }
    };

    $scope.Click_Delete = function (row) {
        if (row != null && row != undefined) {
            window.parent.swal({
                title: "[L:Confirm]",
                text: "[L:DeleteDomain]",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55", confirmButtonText: "[L:Delete]",
                cancelButtonText: "[L:Cancel]",
                closeOnConfirm: true,
                closeOnCancel: true
            },
                function (isConfirm) {
                    if (isConfirm) {
                        common.webApi.post('domain/deletesitealias', 'portalAliasId=' + row.PortalAliasID, '').success(function (Response) {
                            if (Response.IsSuccess) {
                                $scope.PortalAliases = Response.Data.SiteAliases.PortalAliases;
                                $scope.Search_PortalAliases = $scope.PortalAliases;
                                window.parent.ShowNotification(row.HTTPAlias, '[L:DomainDelete]', 'success');
                            }
                            else {
                                $scope.PortalAliases = [];
                                $scope.Search_PortalAliases = $scope.PortalAliases;
                                window.parent.ShowNotification(row.HTTPAlias, Response.Message, 'error');
                            }
                        });
                    }
                });
        }
    };

    $scope.Click_Edit = function (row) {
        parent.OpenPopUp(null, 800, 'right', '[LS:EditDomain]', '#/setting?sid=' + row.PortalAliasID);
    };

    $scope.Click_Back = function () {
        parent.Click_Back();
    };
});