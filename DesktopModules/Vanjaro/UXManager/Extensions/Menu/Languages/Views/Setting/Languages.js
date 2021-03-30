app.controller('setting_languages', function ($scope, $attrs, $routeParams, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.onInit = function () {

    }

    $scope.Click_New = function () {
        parent.OpenPopUp(null, 500, 'right', '[L:NewLanguages]', "#!/add");
    }
    $scope.Click_Translator = function (row) {
        parent.OpenPopUp(null, 800, 'right', '[L:Translators]', "#!/translator/lid/" + row.LanguageId);
    }
    $scope.Click_Resources = function (row) {
        parent.OpenPopUp(null, 900, 'right', '[L:TranslateResourceFile]', "#!/resources/lid/" + row.LanguageId);
    }
    $scope.Click_Enabled = function (row) {

        var message = "";
        if (row.Enabled)
            message = "[L:EnableddMessage]";
        else
            message = "[L:DisabledMessage]";
        window.parent.swal({
            title: "[LS:Confirm]",
            text: message + row.EnglishName,
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[L:Yes]",
            cancelButtonText: "[L:No]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.get('languages/Enabled', 'lid=' + row.LanguageId).then(function (Response) {
                        if (Response.data.IsSuccess) {
                            if (Response.data.RedirectURL != null && Response.data.RedirectURL.length > 0)
                                window.parent.location.href = Response.data.RedirectURL;
                                $scope.RenderMarkup();
                            $scope.ui.data.Languages.Options = Response.data.Data;
                        }
                    });
                }
            });
    }

    $scope.RenderMarkup = function () {
        $.each(window.parent.getAllComponents().filter(function (component) {
            return (component.attributes.name == "Language" || component.attributes["custom-name"] != undefined && component.attributes["custom-name"] == "Language");
        }), function (index, value) {
            window.parent.RenderBlock(value);
        });
    }

    $scope.GetLanguages = function () {
        common.webApi.get('languages/getlanguages').then(function (Response) {
            if (Response.data.IsSuccess) {
                $scope.ui.data.Languages.Options = Response.data.Data;
            }
        })
    };
});