(function ($) {
    var placeHolderElement = $('#modal-placeholder-available-securities');
    $('button[data-toggle="ajax-modal-view-available-securities"]').click(function (event) {
        var url = $(this).data('url');
        $.get(url).done(function (data) {
            placeHolderElement.html(data);
            placeHolderElement.find('.modal').modal('show');
        });
    });
    placeHolderElement.on('click', '[data-dismiss="modal"]', function (event) {
        placeHolderElement.find('.modal').modal('hide');
    });
})($);