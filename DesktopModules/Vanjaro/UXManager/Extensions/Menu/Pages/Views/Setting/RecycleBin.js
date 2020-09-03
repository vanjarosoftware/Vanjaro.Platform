app.controller('setting_recyclebin', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    //Init Scope
    $scope.onInit = function () {        
    };    
    $scope.Restore_Pages = function (row, event) {
        event.preventDefault();
        $scope.ui.data.List_PageItem.Options = [];
        $scope.ui.data.List_PageItem.Options = GetSelectedPages();
        if ($scope.ui.data.List_PageItem.Options.length < 1 && row != null) {
            $scope.ui.data.PageItem.Options.Id = row.TabID;
            $scope.ui.data.PageItem.Options.Name = row.TabName.trim();;
            $scope.ui.data.List_PageItem.Options = [];
            $scope.ui.data.List_PageItem.Options.push($scope.ui.data.PageItem.Options);
        }

        if ($scope.ui.data.List_PageItem.Options.length > 0) {
            window.parent.swal({
                title: "[L:RestorePageTitle]",
                text: "[L:RestorePageText]",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55", confirmButtonText: "[L:RestoreButton]",
                cancelButtonText: "[L:CancelButton]",
                closeOnConfirm: true,
                closeOnCancel: true
            },
                function (isConfirm) {
                    if (isConfirm) {
                        common.webApi.post('pages/restorepage', '', $scope.ui.data.List_PageItem.Options).success(function (Response) {
                            if (Response.IsSuccess) {
                                $scope.ui.data.PagesTree.Options = [];
                                $scope.ui.data.PagesTree.Options = Response.Data.PagesTree;

                                $scope.ui.data.DeletedPages.Options = [];
                                $scope.ui.data.DeletedPages.Options = Response.Data.DeletedPages;
                            }
                            if (Response.HasErrors && Response.Message != null) {
                                window.parent.ShowNotification('[LS:Pages]', Response.Message, 'error');                                  
                            }
                        });
                    }
                });
        }
        return false;
    };
    $scope.Remove_Pages = function (row, event) {
        event.preventDefault()
        $scope.ui.data.List_PageItem.Options = [];
        $scope.ui.data.List_PageItem.Options = GetSelectedPages();
        if ($scope.ui.data.List_PageItem.Options.length < 1 && row != null) {
            $scope.ui.data.PageItem.Options.Id = row.TabID;
            $scope.ui.data.PageItem.Options.Name = row.TabName.trim();;
            $scope.ui.data.List_PageItem.Options = [];
            $scope.ui.data.List_PageItem.Options.push($scope.ui.data.PageItem.Options);
        }

        if ($scope.ui.data.List_PageItem.Options.length > 0) {
            window.parent.swal({
                title: "[L:DeletePageTitle]",
                text: "[L:RemovePageText]",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55", confirmButtonText: "[L:DeleteButton]",
                cancelButtonText: "[L:CancelButton]",
                closeOnConfirm: true,
                closeOnCancel: true
            },
                function (isConfirm) {
                    if (isConfirm) {
                        if ($scope.ui.data.List_PageItem.Options.length > 0) {
                            common.webApi.post('pages/removepage', '', $scope.ui.data.List_PageItem.Options).success(function (Response) {
                                if (Response.IsSuccess) {
                                    $scope.ui.data.DeletedPages.Options = [];
                                    $scope.ui.data.DeletedPages.Options = Response.Data.DeletedPages;
                                }
                                if (Response.HasErrors && Response.Message != null) {
                                    window.parent.ShowNotification('[LS:Pages]', Response.Message, 'error');                                   
                                }
                            });
                        }
                    }
                });
        }
        return false;
    };
    var GetSelectedPages = function () {
        $.each($scope.ui.data.DeletedPages.Options, function (key, value) {
            if ($('.deletedpages .checkbox' + value.TabID).is(":checked")) {
                $scope.ui.data.PageItem.Options.Id = value.TabID;
                $scope.ui.data.PageItem.Options.Name = value.TabName.trim();
                $scope.ui.data.List_PageItem.Options.push($.extend(true, new Object, $scope.ui.data.PageItem.Options));
            }
        });
        return $scope.ui.data.List_PageItem.Options;
    };
});