//Pass desktop modules scrips and styles to grapejs iframe
global.VjLinks = [];
global.VjScriptTags = [];
global.VjScript = '';
global.VjStyle = '';
global.BindLinksAndScripts = function () {
    global.VjLinks = [];
    global.VjScriptTags = [];
    global.VjScript = '';
    global.VjStyle = '';
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
    $('style').each(function () {
        if (typeof this.attributes.vj == 'undefined' && this.innerHTML != undefined && this.innerHTML.length > 0)
            VjStyle += this.innerHTML;
    });
};

global.GetMultiScripts = function (arr, path) {

    var _arr = $.map(arr, function (obj) {
        if (obj.type == 'external')
            return $.getScript((path || "") + obj.script);
        else
            $('body').append(obj.script);
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
    var head_script_arr = [];
    var body_script_arr = [];
    var inlineStyleMarkup = '';
    $.each(dom, function (i, v) {
        if (v.tagName != undefined && v.tagName.toLowerCase() == "script") {
            var exist = false;
            $.each(existingScripts, function (key, value) {
                if (value.src != '' && v.src != '') {
                    if (value.src == v.src) {
                        exist = true;
                        return false;
                    }
                }
                else if (value.innerHTML != '' && v.innerHTML != '') {
                    if (value.innerHTML.replace('//<![CDATA[', '').replace('//]]>', '').replace(/\s/g, '') == v.innerHTML.replace('//<![CDATA[', '').replace('//]]>', '').replace(/\s/g, '')) {
                        exist = true;
                        return false;
                    }
                }
            });
            if (!exist) {
                if (v.src != undefined && v.src != '') {
                    if (v.parentNode.nodeName.toLowerCase() == 'head')
                        head_script_arr.push({ type: 'external', script: v.src });
                    else
                        body_script_arr.push({ type: 'external', script: v.src });
                }
                else if (v.innerHTML.indexOf('Sys.WebForms.PageRequestManager') <= 0)
                    body_script_arr.push({ type: 'inline', script: v.outerHTML.replace('//<![CDATA[', '').replace('//]]>', '') });
            }
        }
        else if (v.tagName != undefined && v.tagName.toLowerCase() == "link") {
            if (v.href != undefined && v.href != '') {
                var exist = false;
                $.each(existingLinks, function (key, value) {
                    if (value.href == v.href)
                        exist = true;
                });
                if (!exist) {
                    if (v.parentNode.nodeName.toLowerCase() == 'head')
                        document.head.appendChild(v);
                    else
                        document.body.prepend(v);
                }
            }
        }
        else if (v.tagName != undefined && v.tagName.toLowerCase() == "style") {
            var exist = false;
            $.each(existingStyles, function (key, value) {
                if (value.innerHTML == v.innerHTML)
                    exist = true;
            });
            if (!exist)
                inlineStyleMarkup += v.innerHTML;
        }
    });

    if (head_script_arr.length)
        GetMultiScripts(head_script_arr);

    if (body_script_arr.length)
        GetMultiScripts(body_script_arr);

    if (inlineStyleMarkup != undefined && inlineStyleMarkup != '') {
        var newStyle = document.createElement("style");
        var inlineStyle = document.createTextNode(inlineStyleMarkup);
        newStyle.appendChild(inlineStyle);
        document.body.prepend(newStyle);
    }
};