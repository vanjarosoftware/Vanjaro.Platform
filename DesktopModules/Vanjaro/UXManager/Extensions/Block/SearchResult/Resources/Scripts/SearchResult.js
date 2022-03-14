var SearchResult = {
    Search: function (obj) {
        var sf = $.ServicesFramework(-1);
        var Keyword = window.GetParameterByName('Search', $(location).attr('href'));
        var PageIndex = parseInt($(obj).closest('div[data-block-global]').find('.CurrentPageIndex').attr('attr-searchresult-pageindex')) + 1;
        var dataAttr = $(obj).closest('div[data-block-global]')[0];
        var mappedAttr = new Object();
        var attr = '';
        mappedAttr["data-block-pageindex"] = PageIndex;
        $.each(dataAttr.attributes, function (k, v) {
            if (v.name == "data-block-guid") {
                mappedAttr[v.name] = v.value;
            }
            if (v.name != 'data-block-pageindex' && v.name != 'id')
                attr += '[' + v.name + '=' + v.value + ']';
        });

        $.ajax({
            type: "POST",
            url: window.location.origin + $.ServicesFramework(-1).getServiceRoot("Vanjaro") + "block/rendermarkup?search=" + Keyword,
            dataType: "json",
            data: JSON.stringify(mappedAttr),
            contentType: "application/json; charset=utf-8",
            headers: {
                'ModuleId': parseInt(sf.getModuleId()),
                'TabId': parseInt(sf.getTabId()),
                'RequestVerificationToken': sf.getAntiForgeryValue()
            },
            success: function (response) {
                if (response != null) {                   
                    $.each(response, function (k, v) {
                        var findcurrentElement = '';
                        $.each($(v.Markup)[0].attributes, function (key, value) {
                            if (value.name != 'data-block-pageindex' && value.name != 'id')
                                findcurrentElement += '[' + value.name + '=' + value.value + ']';
                        });

                        if (attr == findcurrentElement) {
                            $(obj).closest('div[data-block-global]').find('.searchresultsmarkup').append($(v.Markup).find('.searchresultsmarkup .conatiner.searchbox'));
                            $(obj).closest('div[data-block-global]').find('.searchresultsmarkup').attr('attr-searchresult-pageindex', PageIndex);
                            if (!Boolean.parse($(v.Markup).find('.searchresultsmarkup').attr('attr-searchresult-more')))
                                $(obj).closest('div[data-block-global]').find(".Searchresultpage #showMore").hide();
                        }
                    });
                }
            }
        });
    }
};