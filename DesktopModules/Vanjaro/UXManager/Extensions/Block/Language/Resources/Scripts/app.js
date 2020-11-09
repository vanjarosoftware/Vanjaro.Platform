var vjselectlanguage = function () {
    if (!isEditPage())
        window.location.href = $(event.target).find(":selected").attr("url");
};