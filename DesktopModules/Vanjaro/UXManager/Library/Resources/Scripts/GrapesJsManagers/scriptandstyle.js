//Pass desktop modules scrips and styles to grapejs iframe
global.VjLinks = [];
global.VjScriptTags = [];
global.VjScript = '';
global.BindLinksAndScripts = function () {
    global.VjLinks = [];
    global.VjScriptTags = [];
    global.VjScript = '';
    $('link').each(function () {
        if (VjLinks.indexOf(this.href) == -1) {
            VjLinks.push(this.href);
        }
    });
    $('script').each(function () {
        if (this.src != '') {
            if (this.src.toLowerCase().indexOf('/desktopmodules/vanjaro/') < 0 && VjScriptTags.indexOf(this.src) == -1)
                VjScriptTags.push(this.src);
        }
        else {
            if (this.attributes.vanjarocore == undefined && this.attributes.grapejs == undefined && this.attributes["data-actionmid"] == undefined && this.innerHTML.indexOf('Sys.WebForms.PageRequestManager') <= 0)
                VjScript += this.innerHTML;
        }
    });
};

global.GetMultiScripts = function (arr, path) {
    var _arr = $.map(arr, function (scr) {
        return $.getScript((path || "") + scr);
    });

    _arr.push($.Deferred(function (deferred) {
        $(deferred.resolve);
    }));

    return $.when.apply($, _arr);
};

global.InjectLinksAndScripts = function (dom, document) {
    var existingScripts = document.getElementsByTagName('script');
    var existingLinks = document.getElementsByTagName('link');
    var existingStyles = document.getElementsByTagName('style');
    var script_arr = [];
    var inlineMarkup = '';
    var inlineStyleMarkup = '';
    $.each(dom, function (i, v) {
        if (v.tagName != undefined && v.tagName.toLowerCase() == "script") {
            if (v.src != undefined && v.src != '') {
                var exist = false;
                $.each(existingScripts, function (key, value) {
                    if (value.src == v.src)
                        exist = true;
                });
                if (!exist)
                    script_arr.push(v.src);
            }
            else {
                var exist = false;
                $.each(existingScripts, function (key, value) {
                    if (value.innerHTML == v.innerHTML)
                        exist = true;
                });
                if (!exist && v.innerHTML.indexOf('Sys.WebForms.PageRequestManager') <= 0)
                    inlineMarkup += v.innerHTML.replace('//<![CDATA[', '').replace('//]]>', '');
            }
        }
        else if (v.tagName != undefined && v.tagName.toLowerCase() == "link") {
            if (v.href != undefined && v.href != '') {
                var exist = false;
                $.each(existingLinks, function (key, value) {
                    if (value.href == v.href)
                        exist = true;
                });
                if (!exist)
                    document.body.appendChild(v);
            }
            else {
                var exist = false;
                $.each(existingLinks, function (key, value) {
                    if (value.innerHTML == v.innerHTML)
                        exist = true;
                });
                if (!exist)
                    inlineMarkup += v.innerHTML;
            }
        }
        else if (v.tagName != undefined && v.tagName.toLowerCase() == "style") {
            var exist = false;
            $.each(existingStyles, function (key, value) {
                if (value.innerHTML == v.innerHTML)
                    exist = true;
            });
            if (!exist) {
                inlineStyleMarkup += v.innerHTML;
            }
        }
    });
    GetMultiScripts(script_arr).done(function () {
        if (inlineMarkup != undefined && inlineMarkup != '') {
            var newScript = document.createElement("script");
            var inlineScript = document.createTextNode(inlineMarkup);
            newScript.appendChild(inlineScript);
            document.body.appendChild(newScript);
        }
        if (inlineStyleMarkup != undefined && inlineStyleMarkup != '') {
            var newStyle = document.createElement("style");
            var inlineStyle = document.createTextNode(inlineStyleMarkup);
            newStyle.appendChild(inlineStyle);
            document.body.appendChild(newStyle);
        }
    });
};