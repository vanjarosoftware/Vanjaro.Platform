var VJ_timer = null;
var Search = {
    SearchResult: function () {
        $('.autocomplete-suggestions').remove();
        $('#Search-searchkeywords').autoComplete({
            source: function (keywords, response) {
                keywords = keywords.toLowerCase();
                if (VJ_timer) {
                    clearTimeout(VJ_timer);
                }
                VJ_timer = setTimeout(function () {
                    var sf = $.ServicesFramework(-1);
                    $.ajax({
                        type: "POST",
                        url: window.location.origin + $.ServicesFramework(-1).getServiceRoot("SearchInput") + "Search/Preview?" + "keywords=" + keywords,
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        headers: {
                            'ModuleId': parseInt(sf.getModuleId()),
                            'TabId': parseInt(sf.getTabId()),
                            'RequestVerificationToken': sf.getAntiForgeryValue()
                        },
                        success: function (data) {
                            if (data.IsSuccess && data.Data.length > 0) {
                                $.each(data.Data, function (key, value) {
                                    response(value.Results)
                                });
                            }                            
                        }
                    });
                }, 500);
            },
            renderItem: function (item, search) {
                if (item.Title == "NoSearchResultFound") {
                    return "<div class='" + item.Title.toLowerCase() + "'>" + item.Description + "</div>";
                }
                else {
                    if (item.Snippet != null)
                        return "<a href=\"" + item.DocumentUrl + "\"><div class=\"Search_result\"> <div class=\"s_title\">" + " " + item.Title + "</div><div class=\"search_des\">" + item.Snippet + "</div></div></a>";
                    else
                        return "<a href=\"" + item.DocumentUrl + "\"><div class=\"Search_result\"> <div class=\"s_title\">" + " " + item.Title + "</div></div></a>";
                }
            }
        });
    },
    enterSearch: function (event, $this, href) {
        if (event.keyCode == 13) {
            event.preventDefault();
            if ($this.val() != "" && href != null && href != "") {
                window.location.href = href + "?Search=" + $this.val();
            }
        }
    },

    onclickSearch: function (event, $this, href) {
        event.preventDefault();
        if ($this.parent().find("input").val() != "" && href != null && href != "") {
            window.location.href = href + "?Search=" + $this.parent().find("input").val();
        }
    },
};

$(window).load(function () {
    $('.search_box .ui-autocomplete-input').attr("autofocus", "autofocus");
});





