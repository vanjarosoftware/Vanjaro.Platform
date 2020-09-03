var vjselectlanguage = function () {
    if (window.parent.VjEditor == undefined || window.parent.VjEditor == null || (window.parent.VjEditor != undefined && window.parent.VjEditor.getComponents().length <= 0)) {
        window.location.href = $(event.target).find(":selected").attr("url");
    }
};