//Fixed double click issue on bootstrap dropdown
InitDropdown = function () {
    $(this).dropdown();
};

$(document).on("mouseover", ".dropdownmenu", InitDropdown);

ShowNotification = function (heading, msg, type, url, autoHide, tapToDismiss) {
    if (typeof toastr != 'undefined') {
        var $toast;
        toastr.clear($toast);
        var timeOut = 5000;
        var extendedTimeOut = 1000;
        var onClick = null;
        var onShown = null;

        if (typeof url != "undefined" && url != '') {
            onClick = function () {
                window.location = url;
            }
        }

        if (type == 'info') {
            onShown = function () {
                $('#toast-container').addClass('info');
            }
        }

        if (typeof autoHide != 'undefined' && !autoHide) {
            timeOut = 0;
            extendedTimeOut = 0;
        }

        if (typeof tapToDismiss == 'undefined')
            tapToDismiss = true;

        toastr.options = {
            "closeButton": true,
            "debug": false,
            "newestOnTop": false,
            "progressBar": true,
            "positionClass": "toast-bottom-right",
            "preventDuplicates": true,
            "showDuration": "300",
            "hideDuration": "1000",
            "timeOut": timeOut,
            "extendedTimeOut": extendedTimeOut,
            "showEasing": "swing",
            "hideEasing": "linear",
            "showMethod": "fadeIn",
            "toastClass": "toastr",
            "tapToDismiss": tapToDismiss,
            onclick: onClick,
            onShown: onShown
        }

        $toast = toastr[type](msg, heading);
    }
};

InitAppActionMenu = function () {

    var AppMenusScript = $('[data-actionmid]');

    if ((!isEditPage() || (typeof vjEditorSettings == 'undefined')) && AppMenusScript.length > 0) {
        AppMenusScript.each(function () {
            var mid = $(this).data('actionmid');
            $('.DnnModule-' + mid).vjModuleActions({
                AppMenus: jQuery.parseJSON($(this).html()), mid: mid
            });
        });
    }
};

isEditPage = function () {

    var grapesjsCookie;

    if (typeof getCookie != 'undefined')
        grapesjsCookie = getCookie('vj_InitUX');

    if (grapesjsCookie == null || grapesjsCookie == '' || grapesjsCookie == 'false')
        return false;
    else
        return true;
};

DestroyAppActionMenu = function () {
    $('[data-actionmid]').each(function () {
        $('#moduleActions-' + $(this).data('actionmid')).remove();
    });
};

GetParameterByName = function (Param, Src) {
    var Match = RegExp('/' + Param + '/([^/]*)', 'i').exec(Src);

    if (Match == "" || Match == undefined || Match == null)
        Match = RegExp('' + Param + '=([^&]*)', 'i').exec(Src);

    return Match && decodeURIComponent(Match[1].replace(/\+/g, ' '));
};

GetPopupURL = function (TabUrl, Param) {
    if (TabUrl != null && TabUrl.indexOf('?') != -1) {
        return TabUrl + Param.replace('?', '');
    }
    return TabUrl + Param;
};

ClosePopUp = function (closeall) {
    if (typeof closeall != 'undefined' && closeall)
        $('.uxmanager-modal').find('[data-bs-dismiss="modal"]').click();
    else
        $('.uxmanager-modal').last().find('[data-bs-dismiss="modal"]').click();
};

RedirectPopup = function (iframe) {
    if (iframe.contentWindow.location.href.indexOf('ctl') == -1)
        ClosePopUp();
};

OpenPopUp = function (e, width, position, title, url, height, showtogglebtn, removemodals, ModuleId, scrollbars, titleposition) {

    var id = 'vjModal' + (new Date()).getTime();
    var edit = '';
    var fullwidth = '';
    var scrolling;
    var modalclass = '';
    var modalstyle = ' style="';

    if (typeof scrollbars != 'undefined') {
        if (scrollbars)
            scrolling = 'yes';
        else
            scrolling = 'no';
    }

    if (typeof titleposition == 'undefined')
        titleposition = '';

    if (typeof showtogglebtn == 'undefined')
        showtogglebtn = false

    if (width == "100%") {

        fullwidth = 'fullwidth';

        if (typeof scrollbars == 'undefined')
            scrolling = 'yes';

        modalstyle += 'width: 100%;';
    }
    else
        modalstyle += 'width:' + width + 'px;';

    if (typeof height != 'undefined' && height != null && height != '' && height != '100%')
        modalstyle += 'height:' + height + 'px;';
    else
        modalclass += ' fullheight';

    if (position == 'right')
        modalclass += ' modal-right'

    if (typeof ModuleId != 'undefined')
        edit = 'data-edit="edit_module" data-mid="' + ModuleId + '"';

    if (modalstyle == ' style="')
        modalstyle = '';
    else
        modalstyle += '"';

    var modal = `<div id="` + id + `"  class="uxmanager-modal modal fade ` + fullwidth + `" tabindex="-1" ` + edit + ` role="dialog" aria-labelledby="defaultModalLabel" aria-hidden="true">
        <div class="modal-dialog` + modalclass + `"` + modalstyle + `>
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title ` + titleposition + `" id="defaultModalLabel"></h4>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-hidden="true"></button>
                </div>
                <div class="modal-body" id="UXRender">
<img class="loader" alt="Loading" src="` + VjDefaultPath + `loading.svg" />
                    <iframe id="UXpagerender" scrolling="` + scrolling + `" onload="RedirectPopup(this);"></iframe>
                </div>
            </div>
        </div>
    </div>`;

    if (typeof removemodals != 'undefined' && removemodals)
        $("body div[id*='vjModal'], .modal-backdrop").remove();

    if ($('body').find('#' + id).length <= 0)
        $('body').append(modal);

    var $modal = $('#' + id);

    if (url != '') {
        $modal.find('#UXpagerender').on("load", function () {
            var $iframe = $(this);
            $iframe.prev().hide();
            $iframe.show();
        });
    }

    if (position == 'right' && showtogglebtn) {

        if ($modal.find(".modal-toggle").length == 0)
            $modal.append('<button type="button" class="modal-toggle" style="right: ' + (width - 1) + 'px;"><em class="fas fa-chevron-right"></em></a>');

        $modal.find(".modal-toggle").click(function () {
            var $this = $(this);
            var modalwidth = $modal.find('.modal-dialog').width();
            var togglepos = parseInt($this.css('right'));

            if (togglepos == 0) {
                $this.animate({ 'right': modalwidth - 1 }, 'slow').removeClass('modalclosebtn');
                $modal.find('.modal-dialog').animate({ 'right': '0' }, 'slow', function () {
                    $this.find("em").addClass("fa-chevron-right").removeClass("fa-chevron-left");
                    $modal.removeClass("modal-close").addClass('modal-open').css('width', '100%');
                    $(".modal-backdrop").removeClass("hide").addClass('show').css('width', '100vw');
                    $('.gjs-cv-canvas__frames').css('pointer-events', 'all');
                });

            }
            else {
                $this.animate({ 'right': "0" }, "slow").addClass('modalclosebtn');
                $modal.find('.modal-dialog').animate({ 'right': "-" + modalwidth }, 'slow', function () {
                    $this.find("em").addClass("fa-chevron-left").removeClass("fa-chevron-right");
                    $modal.addClass("modal-close").removeClass('modal-open');
                    $(".modal-backdrop").removeClass("show").addClass('hide').css('width', '0');
                    $('.gjs-cv-canvas__frames').css('pointer-events', 'none');
                });
            }
        });
    }
    else
        $modal.find('.modal-toggle').remove();

    if (url != '') {

        var existingurl = window.parent.$('#iframe').attr('src');

        if (url.startsWith('~'))
            url = url.replace('~', '');
        else if (!url.startsWith('http')) {
            if (existingurl !== 'about:blank') {
                if (existingurl.indexOf("?") == -1)
                    existingurl = existingurl + "?v=" + (new Date()).getTime();
                else
                    existingurl = existingurl + "&v=" + (new Date()).getTime();
                url = existingurl + url;
            }
        }
        else {
            var urlArr = url.split('#');
            if (urlArr[0].indexOf("?") == -1)
                url = urlArr[0] + "?v=" + (new Date()).getTime();
            else
                url = urlArr[0] + "&v=" + (new Date()).getTime();

            if (urlArr[1] != undefined) {
                url = url + '#' + urlArr[1];
            }
        }
    }

    $modal.find('#defaultModalLabel').text(title);

    $.fn.modal.Constructor.prototype._enforceFocus = function () { }

    $modal.modal({
        backdrop: 'static', keyboard: true
    });

    var $backdrop = $modal.prev('.modal-backdrop');

    if ($backdrop.length && $backdrop.prev('div[id*="vjModal"]').length) {
        var zindex = parseInt($backdrop.prev('div[id*="vjModal"]').css('z-index'));
        $modal.css('z-index', zindex + 2);
        $modal.next(".modal-backdrop").css('z-index', zindex + 1);
    }

    window.document.callbacktype = '';

    $modal.on("hidden.bs.modal", function (e) {
        if (url != '') {
            $modal.find('#UXpagerender').attr('src', 'about:blank');
            if (window.document.callbacktype === "update") {
                var iframe = window.parent.$('#iframe');
                if (typeof iframe[0] != 'undefined' && iframe != null) {
                    iframe[0].src = iframe[0].src;
                }
            }
            var appid = $(window.document.body).find('.uxmanager-modal').data('mid')
            var appdiv = "#dnn_vj_" + appid;
            var $appframe = $('.gjs-frame').contents().find(appdiv).find("#Appframe");

            if ($appframe.length)
                $appframe.prev().hide();

        }
        $modal.remove();
    });

    $modal.on("shown.bs.modal", function (e) {
        $(document).off('focusin.modal');
    });

    if (url != '')
        $modal.find('#UXpagerender').attr('src', url);

    $modal.modal('show');

    $(window.parent.document.body).find('[data-bs-dismiss="modal"]').on("click", function (e, reload) {
        if ($(window.document.body).find('.uxmanager-modal').data('edit') == 'edit_module') {
            var mid = $(window.document.body).find('.uxmanager-modal').data('mid');
            if ($('.gjs-frame').contents().find('#dnn_vj_' + mid).length > 0) {
                var framesrc = CurrentTabUrl;
                if (framesrc.indexOf("?") == -1)
                    framesrc = framesrc + "?mid=" + mid + "&icp=true";
                else
                    framesrc = framesrc + "&mid=" + mid + "&icp=true";
                $('.gjs-frame').contents().find('#dnn_vj_' + mid).html("<img class=\"centerloader moduleloader\" src='" + VjDefaultPath + "loading.svg'><iframe id=\"Appframe\" scrolling=\"no\" onload=\"window.parent.RenderApp(this);\" src='" + framesrc + "' style=\"width:100%;height:auto;\"></iframe>");
            }
            else if (typeof reload == 'undefined' || reload)
                window.parent.location.reload();
        }
    });


    if (e != null && e.stopImmediatePropagation != undefined)
        e.stopImmediatePropagation();

    return false;
};

$(document).keyup(function (e) {
    var code = e.keyCode || e.which;
    if (code == 27 && $('[ng-show=ShowFileManager]:not(".ng-hide")').length <= 0) {
        var $modal = $(".uxmanager-modal");

        if ($modal.length <= 0)
            $modal = $(window.parent.document.body).find('.uxmanager-modal');

        if ($modal.length > 0)
            $modal.find('[data-bs-dismiss="modal"]').click();
    }
});

OpenImagePopup = function (img) {

    if (window.parent.VjEditor == undefined || window.parent.VjEditor == null || (window.parent.VjEditor != undefined && window.parent.VjEditor.getComponents().length <= 0)) {
        img = $(img);
        var src = img.attr("src");
        var title = '';

        if (typeof img.attr("title") != 'undefined')
            title = img.attr("title");

        var $Modal = $("body").find("#ImgModal");

        $Modal.css({
            "max-height": "100%",
            "height": "auto"
        });

        $Modal.find(".modal-title").text("").text(title);
        $Modal.find("img").attr("src", img.attr("src"));
        $Modal.modal();

        windowWidth = $(window).width();
        windowHeight = $(window).height();

        if (windowWidth > 992) {

            var orignalwidth = img.prop("naturalWidth");
            var orignalheight = img.prop("naturalHeight");

            if (orignalheight > windowHeight - 200) {

                var aspectRatio = orignalwidth / orignalheight;
                var fixedHeight = windowHeight - 200;
                var fixedWidth = aspectRatio * fixedHeight;

                $Modal.find(".modal-dialog").css("max-width", fixedWidth).find(".modal-content").css("max-height", fixedHeight + 100).find(".modal-body").css("max-height", fixedHeight + 100).find("img").css("max-height", fixedHeight);
            }
            else
                $Modal.find(".modal-dialog").css("max-width", orignalwidth).find(".modal-content").css("max-height", orignalheight + 100).find(".modal-body").css("max-height", orignalheight + 100).find("img").css("max-height", orignalheight);
        }
    }
};

$(document).ready(function () {

    $("#navbar-menu li.dropdown").hover(function () {
        if ($(window).width() < 767)
            return;
        var menuItem = $(this);
        var subMenu = $(this).find("> ul.dropdown-menu");
        var screenWidth = $(window).width()
            , menuItemWidth = menuItem.width()
            , subMenuWidth = subMenu.width()
            , menuItemPosRight = subMenu.offset().left + subMenu.width();
        if (menuItemPosRight > screenWidth) {
            subMenu.css("margin-left", "-" + ((subMenuWidth - menuItemWidth) + 10) + "px");
            subMenu.css('border-radius', '0');
        } else
            subMenu.css("margin-left", '2px');
    });

    $("#navbar-menu li.dropdown-submenu").hover(function () {
        if ($(window).width() < 767)
            return;
        var subMenu = $(this).find("ul.dropdown-menu");
        if (!subMenu.get(0))
            return;
        var screenWidth = $(window).width()
            , subMenuWidth = subMenu.width()
            , subMenuParentWidth = $(this).parent().width()
            , subMenuPosRight = subMenu.offset().left + subMenu.width();
        if (subMenuPosRight > screenWidth)
            subMenu.css("margin-left", "-" + (subMenuParentWidth + subMenuWidth + 10) + "px");
        else
            subMenu.css("margin-left", 0);
    });

    if ($("body").find(".vj-image-gallery").length) {

        var modal = `
		<div class="modal fade" id="ImgModal">
			<div class="modal-dialog">
				<div class="modal-content">
					<div class ="modal-header">
						<h4 class="modal-title"></h4>
						<button type="button" class="close" data-bs-dismiss="modal" aria-hidden="true">&times;</button>
					</div>
					<div class="modal-body">
						<img class="img-fluid" src=""/>
					</div>
				</div>
			</div>
		</div>
		`
        $("body").append(modal);
    }

    $('.uxmanager-modal [data-bs-dismiss="modal"]').on("click", function (e) {
        $('#UXpagerender').show().siblings().remove();
    });

    $('.DefaultMenu').find('.dropdown-toggle[href="javascript:void(0)"], .dropdown-submenu [href="javascript:void(0)"], .dropdown-toggle[href!="#"] svg, .dropdown-submenu a[href!="#"] svg').on('click', function (e) {

        e.preventDefault();

        if ($(window).width() > 991)
            return this;

        $(this).parent().parent().toggleClass('resp-active');

        return this;
    });

    InitAppActionMenu();
});

InitHamburgerMenu = function ($this) {

    if ($this.parent().find(".DefaultMenu").length) {
        var thismenu = $this.parent().find(".DefaultMenu");
        var posoff = $this.offset().top;
        $(thismenu).appendTo("body");
        $(thismenu).css("top", posoff + 30);
    }
    else {
        var thismenu = $this;
        var menuid = thismenu.attr("data-target");
        var posoff = $this.parent();
        setTimeout(function () {
            $(menuid).appendTo(posoff);
            $(menuid).css("top", "auto");
        }, 500);
    }
};

$(window).resize(function () {
    if ($(window).width() > 991) {
        $(".navbar-toggler").each(function () {
            var thislength = $(this).hasClass("collapsed");
            if (!thislength) {
                var thismenu = $(this);
                $(thismenu).trigger("click");
                var menuid = thismenu.attr("data-target");
                var posoff = $(this).parent();
                $(menuid).appendTo(posoff);
                $(menuid).css("top", "auto");
            }
        });
    };
});

(function ($) {
    $.fn.vjModuleActions = function (opts) {
        var $self = this;
        var moduleId = opts.mid;
        var customActions = opts.AppMenus;
        var customCount = customActions.length;

        function closeMenu(ul) {
            var $menuroot = $('#moduleActions-' + moduleId + ' ul.dnn_mact');
            if (ul && ul.position()) {
                if (ul.position().top > 0) {
                    ul.hide();
                } else {
                    ul.hide();
                }
            }
        }

        function showMenu(ul) {
            // detect position
            var $self = ul.parent();
            var windowHeight = $(window).height();
            var windowScroll = $(window).scrollTop();
            var thisTop = $self.offset().top;
            var atViewPortTop = (thisTop - windowScroll) < windowHeight / 2;

            var ulHeight = ul.height();

            if (!atViewPortTop) {

                ul.prev().css({
                    'border-radius': '0 0 5px 5px'
                });

                ul.css({
                    top: -ulHeight,
                    right: 1,
                    'border-radius': '5px 5px 0 5px'
                }).show();
            }
            else {

                ul.prev().css({
                    'border-radius': '5px 5px 0 0'
                });

                ul.css({
                    top: 30,
                    right: 1,
                    'border-radius': '5px 0 5px 5px'
                }).show();
            }
        }

        function buildMenuRoot(root, rootText, rootClass, rootIcon) {
            root.append("<li class=\"" + rootClass + "\"><a href='javascript:void(0)' aria-label=\"" + rootText + "\">" + rootIcon + "</a><ul></ul>");
            var parent = root.find("li." + rootClass + " > ul");

            return parent;
        }

        function buildMenu(root, rootText, rootClass, rootIcon, actions, actionCount) {
            var $parent = buildMenuRoot(root, rootText, rootClass, rootIcon);

            for (var i = 0; i < actionCount; i++) {
                var action = actions[i];

                if (action.Title !== "~") {
                    if (!action.Url) {
                        action.Url = "javascript: __doPostBack('" + actionButton + "', '" + action.ID + "')";
                    } else {
                        action.Url = decodeURIComponent(action.Url);
                    }

                    var htmlString = "<li class='managemenu'>";

                    switch (action.CommandName) {

                        case "ModuleSettings.Action":
                            htmlString = "<li id=\"moduleActions-" + moduleId + "-Settings\">";
                            break;
                    }

                    if (action.NewWindow == 'False') {
                        var width = '100%';
                        if (action.Width) {
                            width = action.Width;
                        }
                        if (action.Url.indexOf('javascript:') == -1)
                            htmlString += "<a onclick='OpenPopUp(null, \"" + width + "\", \"right\", \"" + action.Title + "\", \"" + action.Url + "\", \"\", \"\", \"\", " + action.ModuleId + ");'>" + action.Title + "</a>";
                        else
                            htmlString += "<a onclick='" + action.Url + "'>" + action.Title + "</a>";
                    }
                    else
                        htmlString += "<a href=\"" + action.Url + "\" target='_blank'>" + action.Title + "</a>";

                    $parent.append(htmlString);
                }
            }
        }

        function position(mId) {
            var container = $(".DnnModule-" + mId);
            var root = $("#moduleActions-" + mId + " > ul");
            var containerPosition = container.offset();
            var containerWidth = container.width();

            root.css({
                position: "absolute",
                marginLeft: 0,
                marginTop: 0,
                top: containerPosition.top + 10,
                left: containerPosition.left + containerWidth - 170,
                visibility: "hidden"
            });
        }

        var $form = $("form#Form");
        if ($form.find("div#moduleActions-" + moduleId).length === 0) {
            $form.append("<div id=\"moduleActions-" + moduleId + "\" class=\"actionMenu\"><ul class=\"dnn_mact\"></ul></div>");
            var menu = $form.find("div:last");
            var menuRoot = menu.find("ul");

            if (customCount > 0) {
                buildMenu(menuRoot, "Edit", "actionMenuEdit", '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 448 512"><path d="M16 132h416c8.837 0 16-7.163 16-16V76c0-8.837-7.163-16-16-16H16C7.163 60 0 67.163 0 76v40c0 8.837 7.163 16 16 16zm0 160h416c8.837 0 16-7.163 16-16v-40c0-8.837-7.163-16-16-16H16c-8.837 0-16 7.163-16 16v40c0 8.837 7.163 16 16 16zm0 160h416c8.837 0 16-7.163 16-16v-40c0-8.837-7.163-16-16-16H16c-8.837 0-16 7.163-16 16v40c0 8.837 7.163 16 16 16z"/></svg>', customActions, customCount);
            }

            $(window).resize(function () {
                position(moduleId);
            });
        }

        $("#moduleActions-" + moduleId + " .dnn_mact > li").on({
            mouseover: function () {
                $("#moduleActions-" + moduleId + " ul").css('visibility', 'visible');
                showMenu($(this).find("ul").first());
            }
        }).on({
            mouseout: function () {
                $("#moduleActions-" + moduleId + " ul").css('visibility', 'hidden');
                $(this).find("> a").css('border-radius', '100%');
                closeMenu($(this).find("ul").first());
            }
        });

        $(".DnnModule-" + moduleId).on({
            mouseover: function () {
                position(moduleId);
                $("#moduleActions-" + moduleId + " ul").css('visibility', 'visible');
            },
            mouseout: function () {
                $("#moduleActions-" + moduleId + " ul").css('visibility', 'hidden');
            }
        });

        return $self;
    };
})(jQuery);

var vj_recaptcha_responsetoken = "";

function validateCaptcha(el, action, callback, input) {
    if (typeof grecaptcha !== "undefined") {
        var sitekey = $('#vjrecaptcha').data('sitekey');
        grecaptcha.ready(function () {
            grecaptcha.execute(sitekey, { action: action }).then(function (token) {
                vj_recaptcha_responsetoken = token;
                $(input).val(token);
                callback(el);
            });
        });
    } else {
        callback(el);
    }
}

$(document).on("ajaxSend", function (event, xhr, settings) {
    xhr.setRequestHeader('vj-recaptcha', vj_recaptcha_responsetoken);
});

$.fn.dnnModuleActions = function (options) {
};