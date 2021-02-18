app.controller('review_moderator', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    $scope.Click_Moderate = function (Action) {
        var Title = "";
        var Commentlable = "";
        switch (Action) {
            case "approve":
                Title = '[L:ApproveAreyousure]';
                Commentlable = $scope.ui.data.NextState.Options != null ? '[L:ApproveCommentlable]' + '<span class="SweetAlertBold">' + $scope.ui.data.NextState.Options.Name + '</span>' : '';
                break;
            case "reject":
                Title = '[L:RejectAreyousure]';
                Commentlable = $scope.ui.data.PreviousState.Options != null ? '[L:RejectCommentlable]' + '<span class="SweetAlertBold">' + $scope.ui.data.PreviousState.Options.Name + '</span>' : '';
                break;
            case "publish":
                Title = '[L:PublishAreyousure]';
                Commentlable = $scope.ui.data.NextState.Options != null ? '[L:PublishCommentlable]' + '<span class="SweetAlertBold">' + $scope.ui.data.NextState.Options.Name + '</span>' : '';
                break;
        }
        parent.swal(
            {
                text: Commentlable + ' <textarea class="form-control" id="VJReviewComment" rows="4" style="width: 100%;margin-top: 20px;" type="text" tabindex="3" placeholder="Comments" autofocus></textarea>', title: Title, confirmButtonText: "[L:Submit]", cancelButtonText: "[L:Cancel]", showCancelButton: true, closeOnConfirm: false, animation: "slide-from-top", html: true

                //title: Title, text: Commentlable, html: true, type: "input", confirmButtonText: "[L:Submit]", cancelButtonText: "[L:Cancel]", showCancelButton: true, closeOnConfirm: false, animation: "slide-from-top", inputPlaceholder: "[L:EnterComment]"
            },
            function (inputValue) {
                if (inputValue != false) {
                    var Comment = $('#VJReviewComment', parent.document).val();
                    var Data = {
                        Action: Action,
                        EntityID: $scope.ui.data.ReviewContentInfo.Options.EntityID,
                        Comment: Comment,
                        Entity: $scope.ui.data.ReviewContentInfo.Options.Entity,
                        Version: $scope.ui.data.ReviewContentInfo.Options.Version,
                    };
                    if (Comment != undefined && Comment != '') {
                        common.webApi.post('moderator/addcomment', '', Data).success(function (data) {
                            if (data != null) {
                                $scope.ui.data = data.Data;
                                $("#VJnotifycount", parent.document).text(data.NotifyCount);
                                $(window.parent.document.body).find('[data-bs-dismiss="modal"]').click();
                                parent.swal.close();
                                parent.toastr.clear();
                                if (data.Data.NextState.Options == null)
                                    $('.gjs-cv-canvas__frames', parent.document).removeClass('lockcanvas')
                                if ($scope.getCookie("vj_InitUX") == "" || $scope.getCookie("vj_InitUX") == "true") {
                                    $scope.RenderMarkup('Register Link');
                                }
                                else {
                                    $('.registerlink-notification > sup > strong', parent.document).text(data.NotifyCount);
                                }
                                window.parent.VJIsContentApproval = data.ReviewVariable.IsContentApproval ? "True" : "False";
                                window.parent.VJNextStateName = data.ReviewVariable.NextStateName;
                                window.parent.VJIsPageDraft = data.ReviewVariable.IsPageDraft;
                                window.parent.VJIsLocked = data.ReviewVariable.IsLocked ? "True" : "False";
                                window.parent.VJIsModeratorEditPermission = data.ReviewVariable.IsModeratorEditPermission;

                                if ($(window.parent.document.body).find('.VersionManagement').length > 0)
                                    $(window.parent.document.body).find('.fa.fa-history').trigger('click');
                            }
                        });
                    }
                    else {
                        parent.swal.showInputError('[L:Commentisrequired]');
                        return false;
                    }
                }

            });
        setTimeout(function () {
            $(window.parent.document.body).find('#VJReviewComment').focus();
        }, 100);
    };


    $scope.getCookie = function (name) {
        var nameEQ = name + "=";
        var ca = document.cookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ') c = c.substring(1, c.length);
            if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
        }
        return null;
    }

    $scope.RenderMarkup = function (name) {
        if (window.parent.getAllComponents != undefined) {
            $.each(window.parent.getAllComponents().filter(function (component) {
                return (component.attributes.name == name || component.attributes["custom-name"] != undefined && component.attributes["custom-name"] == name);
            }), function (index, value) {
                window.parent.RenderBlock(value);
            });
        }
    }
});