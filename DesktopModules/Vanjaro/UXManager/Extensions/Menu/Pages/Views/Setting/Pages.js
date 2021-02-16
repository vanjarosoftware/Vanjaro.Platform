app.controller('setting_pages', function ($scope, $attrs, $http, CommonSvc, SweetAlert) {
    var common = CommonSvc.getData($scope);
    //Init Scope
    $scope.onInit = function () {
        $scope.SearchKey = '';
        $scope.HeaderText = "[L:Pages]";
        $scope.Show_RecycleBin = false;
        $('[rel=popup]').click(function (event, width, title, url) {
            width = $(this).data('width');
            title = $(this).data('title');
            url = $(this).attr('href');
            parent.OpenPopUp(event, width, 'right', title, url);
            return false;
        });

        $(".close-pagebtn").click(function () {
            $(".page-search input").val("");
            $(".page-search em.fa-times").hide();
            $scope.SearchKey = '';
            $scope.FetchPages();
        });

    };

    $scope.init = function () {
        $(".dropdown-menu .submenu").parent().find('.dropdown-item').not(".back-icon").click(function () {
            var $this = $(this);
            $this.parent().addClass("active");
            $this.parents(".dropdown.open").css("position", "unset");
            $this.siblings("ul").show();
            $this.parents(".dropdown-menu").find("li:not(.active)").not(".submenu li").addClass("animate-menu");
            $this.parent().parent().addClass("slide-parent");
            $this.siblings("ul").addClass("slide-child");

            setTimeout(function () {
                $(".slide-parent").find("li:not(.active)").not(".submenu li").hide();
            }, 200);
            return false;
        });

        $(".dropdown-menu .submenu .back-icon").click(function () {
            var $this = $(this);
            $(".slide-parent").find("li:not(.active)").not(".submenu li").show();
            $this.parents(".dropdown-menu").removeClass("slide-parent");
            $this.parents(".dropdown-menu").find("li").removeClass("animate-menu");
            $this.parent().parent().removeClass("slide-child");
            $(".submenu").hide();
            return false;
        });
        $('[data-toggle="tooltip"]').tooltip();
    };


    $scope.LoadCompleted = function () {
        $scope.init();
        if ($scope.ui != undefined && $scope.ui.data.HasTabPermission.Value == "false" || $scope.SearchKey != undefined && $scope.SearchKey.length > 0) {
            $(".treeview").find("li").attr("data-nodrag", "true");
            $('.angular-ui-tree-handle').css("cursor", "default").css("user-select", "none");
            if ($scope.ui.data.HasTabPermission.Value == "false")
                $('.Menu-Pages .dropdown-menu .setaspage').css("display", "none");
        }
    }

    $scope.Edit_PagesTreeNode = function (node) {
        parent.OpenPopUp(null, 800, 'right', node.label, '#/detail?pid=' + node.Value);
    };

    $scope.SaveTemplateAs = function (node, event) {
        parent.OpenPopUp(null, 550, 'center', '[L:SaveTemplateAs]', '#/savetemplateas/' + node.Value, 350);
    };

    $scope.View_Page = function (node, event) {
        event.preventDefault();
        if (node.PageUrl != null && node.LinkNewWindow)
            window.open(node.PageUrl, '_blank');
        else
            parent.document.location.href = node.PageUrl;
    };

    $scope.Copy_Page = function (node, event) {
        parent.OpenPopUp(null, 800, 'right', '[L:CopyPages]' + node.label, '#/detail?pid=' + node.Value + '&copy=true');
    };

    $scope.gettreeicon = function (node) {
        if (node.Value == $scope.ui.data.PageSetting.Options.SplashTabId)
            return { 'fas fa-tint': true };
        else if (node.Value == $scope.ui.data.PageSetting.Options.HomeTabId)
            return { 'fas fa-home': true };
        else if (node.Value == $scope.ui.data.PageSetting.Options.LoginTabId)
            return { 'fas fa-user-circle': true };
        else if (node.Value == $scope.ui.data.PageSetting.Options.RegisterTabId)
            return { 'fas fa-user-plus': true };
        else if (node.Value == $scope.ui.data.PageSetting.Options.UserTabId)
            return { 'fas fa-user-tag': true };
        else if (node.Value == $scope.ui.data.PageSetting.Options.SearchTabId)
            return { 'fas fa-search': true };
        else if (node.Value == $scope.ui.data.PageSetting.Options.Custom404TabId)
            return { 'fas fa-exclamation': true };
        else if (node.Value == $scope.ui.data.PageSetting.Options.Custom500TabId)
            return { 'fas fa-exclamation-triangle': true };
        else if (node.FolderPage)
            return { 'far fa-folder': true };
        else if (node.IsRedirectPage)
            return { 'fas fa-link': true };
        else
            return { 'fas fa-file': true };
    };

    $scope.gettreeicontitle = function (node) {
        if (node.Value == $scope.ui.data.PageSetting.Options.SplashTabId)
            return '[L:SplashPage]';
        else if (node.Value == $scope.ui.data.PageSetting.Options.HomeTabId)
            return '[L:HomePage]';
        else if (node.Value == $scope.ui.data.PageSetting.Options.LoginTabId)
            return '[L:LoginPage]';
        else if (node.Value == $scope.ui.data.PageSetting.Options.RegisterTabId)
            return '[L:RegistrationPage]';
        else if (node.Value == $scope.ui.data.PageSetting.Options.UserTabId)
            return '[L:UserProfilePage]';
        else if (node.Value == $scope.ui.data.PageSetting.Options.SearchTabId)
            return '[L:SearchResultsPage]';
        else if (node.Value == $scope.ui.data.PageSetting.Options.Custom404TabId)
            return '[L:404ErrorPage]';
        else if (node.Value == $scope.ui.data.PageSetting.Options.Custom500TabId)
            return '[L:500ErrorPage]';
        else if (node.FolderPage)
            return '[L:FolderPage]';
        else if (node.IsRedirectPage)
            return '[L:RedirectPage]';
        else
            return '[L:Page]';
    };
    $scope.hidedeleteicon = function (node) {
        if (node.Value == $scope.ui.data.PageSetting.Options.SplashTabId)
            return false;
        else if (node.Value == $scope.ui.data.PageSetting.Options.HomeTabId)
            return false;
        else if (node.Value == $scope.ui.data.PageSetting.Options.LoginTabId)
            return false;
        else if (node.Value == $scope.ui.data.PageSetting.Options.UserTabId)
            return false;
        else
            return true;
    };

    $scope.Remove_PagesTreeNode = function (node, event) {
        event.preventDefault();
        window.parent.swal({
            title: "[L:DeletePageTitle]",
            text: "[L:DeletePageText]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[L:DeleteButton]",
            cancelButtonText: "[L:CancelButton]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    $scope.ui.data.PageItem.Options.Id = node.Value;
                    $scope.ui.data.PageItem.Options.Name = node.label.trim();;
                    $scope.ui.data.PageItem.Options.ChildrenCount = node.children.length;

                    common.webApi.post('pages/deletepage', '', $scope.ui.data.PageItem.Options).success(function (Response) {
                        if (Response.IsSuccess) {
                            if (Response.IsRedirect) {
                                window.parent.location.href = Response.RedirectURL;
                            }
                            $scope.RemovenodeFind($scope.ui.data.PagesTree.Options, node.Value);

                            $scope.ui.data.DeletedPages.Options = [];
                            $scope.ui.data.DeletedPages.Options = Response.Data.DeletedPages;
                            $scope.ui.data.DeletedPagesCount.Options = Response.Data.DeletedPages.length;
                            $scope.init();
                            $scope.RenderMarkup();
                        }
                        if (Response.HasErrors) {
                            window.parent.ShowNotification('[LS:Pages]', Response.Message, 'error');
                        }
                    });
                }
            });
        return false;
    };

    $scope.Update_DefaultPagesSetting = function (node, event, type) {
        event.preventDefault();
        $scope.ui.data.DefaultPagesSettingsRequest.Options.PortalId = $scope.ui.data.PageSetting.Options.PortalId;
        $scope.ui.data.DefaultPagesSettingsRequest.Options.CultureCode = $scope.ui.data.PageSetting.Options.CultureCode;
        $scope.ui.data.DefaultPagesSettingsRequest.Options.SplashTabId = $scope.ui.data.PageSetting.Options.SplashTabId;
        $scope.ui.data.DefaultPagesSettingsRequest.Options.HomeTabId = $scope.ui.data.PageSetting.Options.HomeTabId;
        $scope.ui.data.DefaultPagesSettingsRequest.Options.LoginTabId = $scope.ui.data.PageSetting.Options.LoginTabId;
        $scope.ui.data.DefaultPagesSettingsRequest.Options.RegisterTabId = $scope.ui.data.PageSetting.Options.RegisterTabId;
        $scope.ui.data.DefaultPagesSettingsRequest.Options.UserTabId = $scope.ui.data.PageSetting.Options.UserTabId;
        $scope.ui.data.DefaultPagesSettingsRequest.Options.SearchTabId = $scope.ui.data.PageSetting.Options.SearchTabId;
        $scope.ui.data.DefaultPagesSettingsRequest.Options.Custom404TabId = $scope.ui.data.PageSetting.Options.Custom404TabId;
        $scope.ui.data.DefaultPagesSettingsRequest.Options.Custom500TabId = $scope.ui.data.PageSetting.Options.Custom500TabId;
        $scope.ui.data.DefaultPagesSettingsRequest.Options.TermsTabId = $scope.ui.data.PageSetting.Options.TermsTabId;
        $scope.ui.data.DefaultPagesSettingsRequest.Options.PrivacyTabId = $scope.ui.data.PageSetting.Options.PrivacyTabId;
        $scope.ui.data.DefaultPagesSettingsRequest.Options.PageHeadText = $scope.ui.data.PageSetting.Options.PageHeadText;
        $scope.ui.data.DefaultPagesSettingsRequest.Options.RedirectAfterLoginTabId = $scope.ui.data.PageSetting.Options.RedirectAfterLoginTabId;
        $scope.ui.data.DefaultPagesSettingsRequest.Options.RedirectAfterLogoutTabId = $scope.ui.data.PageSetting.Options.RedirectAfterLogoutTabId;
        $scope.ui.data.DefaultPagesSettingsRequest.Options.RedirectAfterRegistrationTabId = $scope.ui.data.PageSetting.Options.RedirectAfterRegistrationTabId;
        switch (type) {
            case "splashpage":
                if ($scope.ui.data.DefaultPagesSettingsRequest.Options.SplashTabId == node.Value) {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.SplashTabId = null;
                    type = 'RemoveFrom_' + type;
                }
                else {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.SplashTabId = node.Value;
                    type = 'SetAs_' + type;
                }
                break;
            case "homepage":
                if ($scope.ui.data.DefaultPagesSettingsRequest.Options.HomeTabId == node.Value) {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.HomeTabId = null;
                    type = 'RemoveFrom_' + type;
                }
                else {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.HomeTabId = node.Value;
                    type = 'SetAs_' + type;
                }
                break;
            case "loginpage":
                if ($scope.ui.data.DefaultPagesSettingsRequest.Options.LoginTabId == node.Value) {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.LoginTabId = null;
                    type = 'RemoveFrom_' + type;
                }
                else {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.LoginTabId = node.Value;
                    type = 'SetAs_' + type;
                }
                break;
            case "registrationpage":
                if ($scope.ui.data.DefaultPagesSettingsRequest.Options.RegisterTabId == node.Value) {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.RegisterTabId = null;
                    type = 'RemoveFrom_' + type;
                }
                else {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.RegisterTabId = node.Value;
                    type = 'SetAs_' + type;
                }
                break;
            case "userprofilepage":
                if ($scope.ui.data.DefaultPagesSettingsRequest.Options.UserTabId == node.Value) {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.UserTabId = null;
                    type = 'RemoveFrom_' + type;
                }
                else {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.UserTabId = node.Value;
                    type = 'SetAs_' + type;
                }
                break;
            case "searchresultspage":
                if ($scope.ui.data.DefaultPagesSettingsRequest.Options.SearchTabId == node.Value) {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.SearchTabId = null;
                    type = 'RemoveFrom_' + type;
                }
                else {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.SearchTabId = node.Value;
                    type = 'SetAs_' + type;
                }
                break;
            case "404errorpage":
                if ($scope.ui.data.DefaultPagesSettingsRequest.Options.Custom404TabId == node.Value) {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.Custom404TabId = null;
                    type = 'RemoveFrom_' + type;
                }
                else {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.Custom404TabId = node.Value;
                    type = 'SetAs_' + type;
                }
                break;
            case "500errorpage":
                if ($scope.ui.data.DefaultPagesSettingsRequest.Options.Custom500TabId == node.Value) {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.Custom500TabId = null;
                    type = 'RemoveFrom_' + type;
                }
                else {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.Custom500TabId = node.Value;
                    type = 'SetAs_' + type;
                }
                break;
            case "PrivacyTabId":
                if ($scope.ui.data.DefaultPagesSettingsRequest.Options.PrivacyTabId == node.Value) {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.PrivacyTabId = null;
                    type = 'RemoveFrom_' + type;
                }
                else {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.PrivacyTabId = node.Value;
                    type = 'SetAs_' + type;
                }
                break;
            case "TermsTabId":
                if ($scope.ui.data.DefaultPagesSettingsRequest.Options.TermsTabId == node.Value) {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.TermsTabId = null;
                    type = 'RemoveFrom_' + type;
                }
                else {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.TermsTabId = node.Value;
                    type = 'SetAs_' + type;
                }
                break;
            case "RedirectAfterRegistration":
                if ($scope.ui.data.DefaultPagesSettingsRequest.Options.RedirectAfterRegistrationTabId == node.Value) {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.RedirectAfterRegistrationTabId = null;
                    type = 'RemoveFrom_' + type;
                }
                else {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.RedirectAfterRegistrationTabId = node.Value;
                    type = 'SetAs_' + type;
                }
                break;
            case "RedirectAfterLogin":
                if ($scope.ui.data.DefaultPagesSettingsRequest.Options.RedirectAfterLoginTabId == node.Value) {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.RedirectAfterLoginTabId = null;
                    type = 'RemoveFrom_' + type;
                }
                else {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.RedirectAfterLoginTabId = node.Value;
                    type = 'SetAs_' + type;
                }
                break;
            case "RedirectAfterLogout":
                if ($scope.ui.data.DefaultPagesSettingsRequest.Options.RedirectAfterLogoutTabId == node.Value) {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.RedirectAfterLogoutTabId = null;
                    type = 'RemoveFrom_' + type;
                }
                else {
                    $scope.ui.data.DefaultPagesSettingsRequest.Options.RedirectAfterLogoutTabId = node.Value;
                    type = 'SetAs_' + type;
                }
                break;
            default:
                $scope.ui.data.DefaultPagesSettingsRequest.Options.HomeTabId = node.Value;
                break;
        }
        common.webApi.post('pages/updatedefaultpagessettings', 'key=' + type, $scope.ui.data.DefaultPagesSettingsRequest.Options).success(function (Response) {
            if (Response.IsSuccess) {
                $scope.ui.data.PageSetting.Options = Response.Data;
                window.parent.ShowNotification(node.label, Response.Message, 'success');
            }
            if (Response.HasErrors) {
                window.parent.ShowNotification(node.label, Response.Message, 'error');
            }
        });
        return false;
    };

    $scope.PagesTreetreeOptions = {
        accept: function (sourceNodeScope, destNodesScope, destIndex) {
            return true;
        },

        dragStart: function () {
            $(".angular-ui-tree-handle").addClass('Dropablepage');
        },

        dragStop: function (event) {
            var ParentId = null;
            var children = null;
            $(".angular-ui-tree-handle").removeClass('Dropablepage');
            try { ParentId = event.dest.nodesScope.$nodeScope.$modelValue.Value; } catch (ex) { };
            try { children = event.dest.nodesScope.$modelValue; } catch (ex) { };
            if (ParentId == null)
                ParentId = 0;
            try {
                $scope.ui.data.PageMoveRequest.Options.ParentId = ParentId;
                $scope.ui.data.PageMoveRequest.Options.PageId = event.dest.nodesScope.$modelValue[event.dest.index].Value;
                if (event.dest.index == children.length - 1) {
                    try { $scope.ui.data.PageMoveRequest.Options.RelatedPageId = event.dest.nodesScope.$modelValue[event.dest.index - 1].Value; } catch (ex) { };
                    if ($scope.ui.data.PageMoveRequest.Options.RelatedPageId <= 0 && ParentId > 0)
                        $scope.ui.data.PageMoveRequest.Options.Action = "parent";
                    else
                        $scope.ui.data.PageMoveRequest.Options.Action = "after";
                }
                else {
                    $scope.ui.data.PageMoveRequest.Options.RelatedPageId = event.dest.nodesScope.$modelValue[event.dest.index + 1].Value;
                    $scope.ui.data.PageMoveRequest.Options.Action = "before";
                }

            } catch (ex) { };

            var dynamicdata =
            {
                Children: JSON.stringify(children),
                Pagemove: JSON.stringify($scope.ui.data.PageMoveRequest.Options),
            };
            if (children != undefined && children.length > 0 && dynamicdata.Pagemove.length > 0) {
                common.webApi.post('pages/updatehireracy', 'ParentId=' + ParentId, dynamicdata).success(function (response) {
                    if (response.HasErrors) {
                        $scope.ui.data.PagesTree.Options = [];
                        $scope.ui.data.PagesTree.Options = response.Data.PagesTree;
                        $scope.SearchKey = '';
                        $scope.init();
                        window.parent.ShowNotification('[L:UpdateHireracyError]', response.Message, 'error');
                    }
                    else {
                        $scope.RenderMarkup();
                    }
                });
            }
        }
    };

    $scope.Restore_Pages = function (row, event) {
        event.preventDefault();
        $scope.ui.data.List_PageItem.Options = [];
        $scope.ui.data.List_PageItem.Options = GetSelectedPages();
        if ($scope.ui.data.List_PageItem.Options.length < 1 && row != null) {
            $scope.ui.data.PageItem.Options.Id = row.id;
            $scope.ui.data.PageItem.Options.Name = row.name.trim();
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
                                $scope.ui.data.DeletedPagesCount.Options = Response.Data.DeletedPages.length;
                                $scope.Show_RecycleBin = false;
                                $scope.HeaderText = "[L:Pages]";
                                $scope.SearchKey = '';
                                $scope.init();
                                $scope.RenderMarkup();
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
            $scope.ui.data.PageItem.Options.Id = row.id;
            $scope.ui.data.PageItem.Options.Name = row.name.trim();
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
                                    $scope.ui.data.DeletedPages.Options = Response.Data.DeletedPages
                                    $scope.ui.data.DeletedPagesCount.Options = Response.Data.DeletedPages.length;
                                    if (Response.Data.DeletedPages.length <= 0) {
                                        $scope.Show_RecycleBin = false;
                                        $scope.HeaderText = "[L:Pages]";
                                    }
                                }
                                if (Response.HasErrors) {
                                    window.parent.ShowNotification('[LS:Pages]', Response.Message, 'error');
                                }
                            });
                        }
                    }
                });
        }
        return false;
    };

    $scope.Remove_All_Pages = function () {
        window.parent.swal({
            title: "[L:DeletePageTitle]",
            text: "[L:DeleteAllPagesText]",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55", confirmButtonText: "[L:DeleteButton]",
            cancelButtonText: "[L:CancelButton]",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (isConfirm) {
                if (isConfirm) {
                    common.webApi.post('pages/removeallpages', '', '').success(function (Response) {
                        if (Response.IsSuccess) {
                            $scope.ui.data.DeletedPages.Options = [];
                            $scope.ui.data.DeletedPagesCount.Options = [];
                            $scope.Show_RecycleBin = false;
                            $scope.HeaderText = "[L:Pages]";
                        }
                        if (Response.HasErrors) {
                            window.parent.ShowNotification('[L:DeleteAllPagesError]', Response.Message, 'error');
                        }
                    });

                }
            });

        return false;
    };

    $scope.Click_Back = function () {
        if ($scope.Show_RecycleBin) {
            $scope.Show_RecycleBin = false;
            $scope.HeaderText = "[L:Pages]";
        }
        else
            parent.Click_Back();
    };

    var GetSelectedPages = function () {
        $.each($scope.ui.data.DeletedPages.Options, function (key, value) {
            if ($('.deletedpages .checkbox' + value.id).is(":checked")) {
                $scope.ui.data.PageItem.Options.Id = value.id;
                $scope.ui.data.PageItem.Options.Name = value.name.trim();;

                $scope.ui.data.List_PageItem.Options.push($.extend(true, new Object, $scope.ui.data.PageItem.Options));
            }
        });
        return $scope.ui.data.List_PageItem.Options;
    };

    $scope.Update_HideShow_NavMenu = function (node, event, type, value) {
        event.preventDefault()
        var request = new Object();
        request = {
            TabId: node.Value,
            Key: type,
            Value: value
        };
        common.webApi.post('pages/updatesettings', 'key=' + type, request).success(function (Response) {
            if (Response.IsSuccess) {
                $scope.Findnode(Response.Data.PagesTree, $scope.ui.data.PagesTree.Options, node.Value);
                $scope.init();
                window.parent.ShowNotification(node.label, Response.Message, 'success');
            }
            if (Response.HasErrors) {
                window.parent.ShowNotification(node.label, Response.Message, 'error');
            }
            $scope.RenderMarkup();
        });
    };

    $scope.Findnode = function (newPages, oldPages, nodeId) {
        if (newPages != null) {
            $.each(newPages, function (key, v) {
                if (nodeId == v.Value) {
                    oldPages[key] = v;
                    return false;
                }
                if (v.children)
                    $scope.Findnode(v.children, oldPages[key].children, nodeId);
            });
        }
    };

    $scope.RemovenodeFind = function (oldPages, nodeId) {
        if (oldPages != null) {
            $.each(oldPages, function (key, v) {
                if (nodeId == v.Value) {
                    oldPages.splice(key, 1);
                    return false;
                }
                if (v.children)
                    $scope.RemovenodeFind(oldPages[key].children, nodeId);
            });
        }
    };

    $scope.RenderMarkup = function () {
        $.each(window.parent.getAllComponents().filter(function (component) {
            return (component.attributes.name == "Menu" || component.attributes["custom-name"] != undefined && component.attributes["custom-name"] == "Menu");
        }), function (index, value) {
            window.parent.RenderBlock(value);
        });
    }

    ////Start-Menu Search
    function debounce(func, wait, immediate) {
        var timeout;
        return function () {
            var context = this, args = arguments;
            var later = function () {
                timeout = null;
                if (!immediate) func.apply(context, args);
            };
            var callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
            if (callNow) func.apply(context, args);
        };
    };

    var Page_Search = debounce(function () {
        var inputval = $(".page-search input").val();
        if (inputval == "") {
            $(".page-search em.fa-times").hide();
        }
        else {
            $(".page-search em.fa-times").show();
        }
        $scope.FetchPages();
    }, 500);

    $scope.FetchPages = function () {
        common.webApi.post('pages/SearchPages', 'searchKey=' + $scope.SearchKey, '').success(function (Response) {
            if (Response.IsSuccess) {
                $scope.ui.data.PagesTree.Options = [];
                $scope.ui.data.PagesTree.Options = Response.Data.PagesTree;
            }
        });
    };

    window.addEventListener('keyup', Page_Search);
    ////End-Menu Search  
});