global.getCookie = function (name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}

$(document).ready(function () {

    $('#iframeHolder iframe').on("load", function () {
        var $iframe = $(this);
        $iframe.prev().hide();
        $iframe.css('display', 'block');
    });

    $('.device-view').click(function () {
        $('.device-view').removeClass('active');
        $(this).addClass('active');
    });

    if ($("#dnn_ContentPane").css("marginTop") == '71px')
        $('body, form').css('height', 'calc(100vh - 71px)');

    //Menu Settings Animation
    $(".MenuItem li,.ToolbarItem li").each(function () {
        var menuitem = $(this).find('> a[target!="_blank"]');

        $(menuitem).click(function (e) {
            var $this = $(this);

            if ($this.siblings("ul.MenuItem").length) {
                $this.parents("#MenuSettings").find("li").removeClass("active");
                $this.parents("#MenuSettings").find("ul").removeClass("left-0 left-100 left-200 left-minus-100");
                $this.parent().addClass("active");

                $("#MenuSettings").find("ul.MenuItem:not(:first-child)").addClass("left-200");

                if ($(".MenuItem li.active").parent().parents("ul.MenuItem").length)
                    $(".MenuItem li.active").parent().parents("ul.MenuItem").addClass("left-minus-100");
                else
                    $(".MenuItem li.active").parent().addClass("left-minus-100");

                $(".MenuItem li.active").parent().removeClass("left-200").addClass("left-0");
                $("li.active").find("ul.MenuItem").removeClass("left-200").addClass("left-100");

                // Add title in back button
                var title = $this.text();
                $this.siblings("ul.MenuItem").find(".backbutton > .back-title").text(title);
            }
            else {
                $('#iframeHolder .loader').show();
                $('#iframeHolder').find('iframe').attr('src', 'about:blank');

                var itemsrc = $(this).attr('href');
                setTimeout(function () {
                    $("#BlockManager").hide();
                    $("#StyleToolManager").hide();
                    $("#iframeHolder").fadeIn();
                }, 300);
                e.preventDefault();
                $('#iframeHolder').find('iframe').attr('src', itemsrc).show();
            }
        });
    });

    var toolitems = $('.ToolbarItem li').length;

    if (toolitems > 6) {
        var toolbar = $('.ToolbarItem').html();
        $('.ToolbarItem li:gt(4)').hide();
        $('.ToolbarItem').prepend("<div class='ntoolbox'></div><ul class='more_icons'><li class='openbtn'><em class='fas fa-chevron-up' data-bs-toggle='tooltip' data-bs-placement='right'  title=\"" + VjLocalized.ShowHiddenIcons + "\"></em></li></ul>");
        $('.ntoolbox').prepend(toolbar);
        $('.ntoolbox  li:lt(5)').hide();
    }

    $(".search-block input").keyup(function () {
        var inputval = $(this).val();
        if (inputval == "") {
            $(".search-block em.fa-times").hide();
        }
        else {
            $(".search-block em.fa-times").show();
        }

    });


    $(".close-searchbtn").click(function () {
        var menuboxitems = $(this).parent().find("#uxm-search").length;
        $(".search-block input").val("");
        $(".search-block em.fa-times").hide();

        if (menuboxitems >= 1) {
            $("#MenuSettings > .Searchresult").hide();
            $("#MenuSettings > .MenuItem").show();
        }
    });


    var Searchpagerender = function () {
        $(".Searchresult li").each(function () {
            var menuitem = $(this).find('> a');

            $(menuitem).click(function (e) {
                var $this = $(this);
                $('#iframeHolder').find('iframe').attr('src', 'about:blank');
                var itemsrc = $(this).attr('href');

                setTimeout(function () {
                    $("#BlockManager").hide();
                    $("#StyleToolManager").hide();
                    $("#iframeHolder").fadeIn();
                }, 300);
                e.preventDefault();
                $('#iframeHolder').find('iframe').attr('src', itemsrc);
            });
        });
    };

    $(".backbutton").click(function () {
        var $this = $(this);
        $this.parent().removeClass("left-0 left-100 left-200 left-minus-100").addClass("left-200");

        if ($this.parent().parents("ul.MenuItem").length > 1)
            $this.parent().parent().parent().removeClass("left-0 left-100 left-200 left-minus-100").addClass("left-100");
        else
            $this.parent().parent().parent().removeClass("left-0 left-100 left-200 left-minus-100").addClass("left-0");
    });

    var Click_ExtensionBack = function () {
        $(this).parent().find('iframe').attr('src', 'about:blank');
        $("#ContentBlocks").fadeOut();
        $("#MenuSettings, .Menupanel-top").fadeIn();
        $('#iframeHolder').fadeOut();
        $("#StyleToolManager").hide();
    };

    $('.openbtn').click(function (e) {
        $('.ntoolbox').toggle();
        $("#LanguageManager,#DeviceManager").hide();
        event.stopPropagation();
    });

    $(window).resize(function () {
        if ($(window).width() < 1000) 
            $(window.parent.document.body).find('.gjs-cv-canvas__frames').addClass('lockcanvas');
        else 
            $(window.parent.document.body).find('.gjs-cv-canvas__frames').removeClass('lockcanvas');
    });


    OpenAbout = function (e, title, url) {
        $("#About").find("iframe").attr("src", url);

        $("#Aboutframe").on("load", function () {
            var Aboutframe = $(this);
            Aboutframe.prev().hide();
            Aboutframe.show();
        });
    }

    // Show-Hide ContentBlocks & MenuSettings
    $(".block-manager #MenuSettings , .Menupanel-top , #About, #Shortcuts").hide();
    $(".block-manager #MenuSettings ul ul").css("left", "100%");

    $(".panelheader .blockItem").click(function (e) {

        e.preventDefault();
        var $this = $(this)
        $this.addClass("active");
        $("#BlockManager").fadeIn();
        $("#iframeHolder").hide();
        $("#StyleToolManager").hide();
        var ID = $this.attr("href");

        if (ID == "#MenuSettings") {
            $(".block-manager > div").not("#MenuSettings,.Menupanel-top").fadeOut();
            $("#StyleToolManager").hide();
            $("#Notification").hide();
            $("#About").hide();
            $("#Shortcuts").hide();
            $(".Searchresult ul").empty();
            setTimeout(function () {
                $(".block-manager").find(ID).fadeIn();
                $(".Menupanel-top").fadeIn();
                $(".Menupanel-top input").focus();
            }, 300);

            $(".box-content").find("ul, li").removeAttr("style");
            $("#MenuSettings").find("ul").removeClass("left-0 left-100 left-200 left-minus-100");

            if ("$('.box-content').is(':visible')")
                $(".box-content .MenuItem ul").css("left", "100%");
        }
        else if (ID == "#ContentBlocks") {
            $(".block-manager #MenuSettings ,.Menupanel-top").hide();
            $("#StyleToolManager").hide();
            $("#Notification").hide();
            $(".Menupanel-top").hide();
            $("#About").hide();
            $("#Shortcuts").hide();
            //setTimeout(function () {
            $(".panel-top , .block-set").fadeIn();
            $("#ContentBlocks").fadeIn();
            ChangeBlockType();
            //}, 300);
        }

        else if (ID == "#About") {
            $(".block-manager #MenuSettings ,.Menupanel-top").hide();
            $("#StyleToolManager").hide();
            $("#Notification").hide();
            $(".Menupanel-top").hide();
            $(".panel-top , .block-set").hide();
            $("#Shortcuts").hide();
            $("#About").show();
        }

        else if (ID == "#Shortcuts") {
            $(".block-manager #MenuSettings ,.Menupanel-top").hide();
            $("#StyleToolManager").hide();
            $("#Notification").hide();
            $(".Menupanel-top").hide();
            $(".panel-top , .block-set").hide();
            $("#About").hide();
            $("#Shortcuts").fadeIn();

        }

        $("#iframeHolder").find('iframe').attr('src', 'about:blank');

        $(".styletab").removeClass('active');
        $(".traitstab").addClass('active');
        $('.stylemanager').hide();
        $('.traitsmanager').show();
    });

    //Start-Menu Search
    var uxmsearch = document.getElementById('uxm-search');
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
    var UXManager_Search = debounce(function () {
        var sf = $.ServicesFramework(-1);
        var Keyword = $('#uxm-search').val();
        var url = window.location.origin + $.ServicesFramework(-1).getServiceRoot("Vanjaro") + "UXManager/Search?" + "Keyword=" + Keyword;
        if (GetParameterByName('m2v', parent.window.location) != null)
            url = url + "&m2v=true&skinsrc=[g]skins/vanjaro/base"

        $.ajax({
            type: "POST",
            url: url,
            headers: {
                'ModuleId': parseInt(sf.getModuleId()),
                'TabId': parseInt(sf.getTabId()),
                'RequestVerificationToken': sf.getAntiForgeryValue()
            },
            success: function (response) {
                //console.log(response);
                if (response.IsSuccess) {
                    $("#MenuSettings > .Searchresult").show();
                    $("#MenuSettings > .MenuItem").hide();
                    $("#MenuSettings > .Searchresult").empty();
                    $("#MenuSettings > .Searchresult").append(response.Data);
                    if ($(response.Data).find('li').length <= 0)
                        $("#MenuSettings > .Searchresult  >.MenuItem").append("<li class='search-info-msg'>" + VjLocalized.NoResultsFound + "</li>");
                    if (Keyword.length == 0) {
                        $("#MenuSettings > .Searchresult").empty();
                        $("#MenuSettings > .Searchresult").hide();
                        $("#MenuSettings > .MenuItem").show();
                    }
                    Searchpagerender();
                }
            }
        });

    }, 500);
    if (uxmsearch != null)
        uxmsearch.addEventListener('input', UXManager_Search);
    //End-Menu Search  

});

global.Click_Back = function () {
    $('#iframeHolder').find('iframe').attr('src', 'about:blank');
    $('#iframeHolder').hide();
    $('#BlockManager').show();
}

global.setCookie = function (name, value, days) {
    var expires = "";
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 500));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}

global.eraseCookie = function (name) {
    this.setCookie(name, null, -1);
}
