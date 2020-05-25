(function ($) {
    var placeHolderElement = $('#modal-placeholder');
    $('button[data-toggle="ajax-modal-portfolio"]').click(function (event) {
        var url = $(this).data('url');
        $.get(url).done(function (data) {
            placeHolderElement.html(data);
            placeHolderElement.find('.modal').modal('show');
        });
    });
    placeHolderElement.on('click', '[data-save="modal"]', function (event) {
        event.preventDefault();
        var form = $(this).parents('.modal').find('form');
        var actionUrl = form.attr('action');
        var sendData = form.serialize();
        $.post(actionUrl, sendData).done(function (data) {
            //Replace the previous modalbody with the new body that will reflect the errors if any.
            var newModalBody = $('.modal-body', data);
            placeHolderElement.find('.modal-body').replaceWith(newModalBody)

            var isValid = newModalBody.find('[name="IsValid"]').val() == 'True';
            if (isValid) {
                placeHolderElement.find('.modal').modal('hide');
            }
        });
    });
})($);


(function ($) {
    var placeHolderElement = $('#modal-placeholder2');
    $('button[data-toggle="ajax-modal-view-trades"]').click(function (event) {
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


