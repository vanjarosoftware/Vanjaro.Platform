var vjselectlanguage = function () {
    if (parent.window.location.href != undefined && parent.window.location.href != '')
        parent.window.location.href = $(event.target).find(":selected").attr("url");
    if (!isEditPage())
        window.location.href = $(event.target).find(":selected").attr("url");
};