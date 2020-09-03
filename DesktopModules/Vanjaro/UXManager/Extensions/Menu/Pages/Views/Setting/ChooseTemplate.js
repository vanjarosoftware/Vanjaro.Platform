app.controller('setting_choosetemplate', function ($scope, $routeParams, CommonSvc, SweetAlert, FileUploader, LocalizationServices) {
    var common = CommonSvc.getData($scope);
    $scope.onInit = function () {

    };

    $scope.Click_ChooseLayout = function (type, Layout) {
        var formdata = {
            PageSettings: $scope.ui.data.PagesTemplate.Options,
            PageLayout: Layout
        };
        var TabID = $scope.ui.data.PagesTemplate.Options.tabId;
        common.webApi.post('pages/choosetemplate', '', formdata).success(function (Response) {
            if (Response.IsSuccess) {
                window.parent.ShowNotification(Layout.Name, '[L:PageUpdatedSuccess]', 'success');                               
                if (Response.IsRedirect) {
                    window.parent.location.href = window.parent.location.href;
                }
                else
                {
                    $(window.parent.document.body).find('[data-dismiss="modal"]').click();
                }
            }
        });
    };
});