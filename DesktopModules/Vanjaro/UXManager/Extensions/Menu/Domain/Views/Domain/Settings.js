app.controller('domain_settings', function ($scope, $routeParams, CommonSvc, SweetAlert) {
    $scope.sid = $routeParams["sid"] ? $routeParams["sid"] : 0;
    var common = CommonSvc.getData($scope);
    //Init Scope
    $scope.onInit = function () {
    };

    $scope.Click_Save = function () {
        if (mnValidationService.DoValidationAndSubmit('', 'domain_settings')) {
            $scope.ui.data.UpdateSiteAliasRequest.Options.CultureCode = $scope.ui.data.Languages.Value == "Default" ? null : $scope.ui.data.Languages.Value;
            common.webApi.post('domain/AddUpdateSiteAlias', '', $scope.ui.data.UpdateSiteAliasRequest.Options).then(function (Response) {
                if (Response.data.IsSuccess) {
                    var ParentScope = parent.document.getElementById("iframe").contentWindow.angular;
                    if (ParentScope != undefined && ParentScope.element(".menuextension").scope() != undefined && ParentScope.element(".menuextension").scope().PortalAliases != undefined) {
                        ParentScope.element(".menuextension").scope().PortalAliases = Response.data.Data.SiteAliases.PortalAliases;
                        ParentScope.element(".menuextension").scope().Search_PortalAliases = Response.data.Data.SiteAliases.PortalAliases;
                        ParentScope.element(".menuextension").scope().$apply();
                    }
                    $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
                    window.parent.ShowNotification($scope.ui.data.UpdateSiteAliasRequest.Options.HTTPAlias, $scope.sid > 0 ? '[L:DomainUpdated]' : '[L:DomainAdded]', 'success');
                }
                else {
                    window.parent.ShowNotification($scope.ui.data.UpdateSiteAliasRequest.Options.HTTPAlias, Response.data.Message, 'error');
                }
            });
        }
    };
});