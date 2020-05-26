(function ($) {
    var placeHolderElement = $('#modal-placeholder-portfolio');
    $('button[data-toggle="ajax-modal-portfolio"]').click(function (event) {
        var url = $(this).data('url');
        $.get(url).done(function (data) {
            placeHolderElement.html(data);
            placeHolderElement.find('.modal').modal('show');
        });
    });
    placeHolderElement.on('click', '[data-save="modal-portfolio"]', function (event) {
        event.preventDefault();
        var form = $(this).parents('.modal').find('form');
        var actionUrl = form.attr('action');
        var sendData = form.serialize();
        $.post(actionUrl, sendData).done(function (data) {
            //Replace the previous modalbody with the new body that will reflect the errors if any.
            var newModalBody = $('.modal-body', data);
            placeHolderElement.find('.modal-body').replaceWith(newModalBody);

            // #TO DO (minor bug) - if successful the page is redirected and the redirected page doesn't have a IsValid key so it stay undefined.
            var isValid = newModalBody.find('[name="IsValid"]').val() == 'True';
            if (isValid) {
                placeHolderElement.find('.modal').modal('hide');
            }
        });
    });
})($);


(function ($) {
    var placeHolderElement = $('#modal-placeholder-trade-view');
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


