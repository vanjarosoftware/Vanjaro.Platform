app.controller('setting_resources', function ($scope, $attrs, $routeParams, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.ResourceMode = 'Host';

    $scope.onInit = function () {
        $scope.ResourceFileName = '';
        $scope.ResourceFile = '';
        
        if (!$scope.ui.data.IsSuperUser.Options) {
            $scope.ResourceMode = 'Portal';
        }
    };
    $scope.Click_ResourceMode = function (Mode) {
        $scope.ResourceMode = Mode;
        if ($scope.ResourceFileName !== '' && $scope.ResourceFile !== '')
            $scope.Click_GetFileFields($scope.ResourceFileName, $scope.ResourceFile);
    };
    $scope.Click_Update = function () {
        $scope.ui.data.UpdateTransaltionsRequest.Options.ResourceFile = $scope.ResourceFile;
        $scope.ui.data.UpdateTransaltionsRequest.Options.Mode = $scope.ResourceMode;
        common.webApi.post('Resources/SaveResxEntries', 'lid=' + $scope.ui.data.LanguageID.Options, $scope.ui.data.UpdateTransaltionsRequest.Options).then(function (Response) {
            window.parent.ShowNotification('', '[L:SavedSuccessfully]', 'success');
        });
    };
    $scope.Click_GetFoldersFils = function (event, folder) {
        var $this = $(event.currentTarget);
        if ($this.hasClass("fas fa-caret-right") && $this.parent().parent().find('> .rootfolder li').length <= 0) {
            common.webApi.get('Resources/GetSubRootResources', 'currentFolder=' + folder.Value, '').then(function (Response) {
                if (Response.data.IsSuccess) {
                    folder.children = Response.data.Data;
                    setTimeout(function () {
                        $this.toggleClass('fas fa-caret-right fas fa-caret-down');
                        if ($this.hasClass('fas fa-caret-down'))
                            $this.parent().parent().find('> .rootfolder').show();
                        else
                            $this.parent().parent().find('> .rootfolder').hide();
                    }, 2)
                }
                else if (Response.data.HasErrors)
                    CommonSvc.SweetAlert.swal(Response.data.Message);
            });
        }
        else {
            if ($this.parent().parent().find('> .rootfolder li').length <= 0)
                return;
            $this.toggleClass('fas fa-caret-right fas fa-caret-down');
            if ($this.hasClass('fas fa-caret-down'))
                $this.parent().parent().find('> .rootfolder').show();
            else
                $this.parent().parent().find('> .rootfolder').hide();
        }
    };
    $scope.Click_GetFileFields = function (resourceFileName, resourceFile) {
        var Data = {
            resourceFile: resourceFile
        };
        $scope.ResourceFileName = resourceFileName;
        $scope.ResourceFile = resourceFile;


        var targetfile = event.target;
        if ($(targetfile).hasClass("files")) {
            $(".files").removeClass("active");
            $(targetfile).addClass("active");
        }

        common.webApi.post('Resources/GetResxEntries', 'mode=' + $scope.ResourceMode + '&lid=' + $scope.ui.data.LanguageID.Options, Data).then(function (Response) {
            if (Response.data.IsSuccess) {
                $scope.ui.data.UpdateTransaltionsRequest.Options.Entries = Response.data.Data;
            }
            else if (Response.data.HasErrors)
                CommonSvc.SweetAlert.swal(Response.data.Message);
        });
    };
    $scope.Click_Cancel = function () {
        $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
    };

});