// This is for a modal on a modal
(function ($) {
    let placeHolderElement = $('#modal-placeholder-trades');
    let trade_button = $('button[data-toggle="ajax-modal-trade"]')
    trade_button.on("click", tradeAction)

    function tradeAction(event){
        // add or amend trade
        let url = $(this).data('url');
        $.get(url).done(getPartialView)
    }

    function getPartialView(data) {
        // get the edit or add partial view
        placeHolderElement.html(data);
        placeHolderElement.find('.modal').modal('show');
    }

    placeHolderElement.on('click', '[data-save="modal-trade"]', saveInformation)
    function saveInformation(event) {
        event.preventDefault();
        var form = $(this).parents('.modal').find('form');
        var actionUrl = form.attr('action');
        var sendData = form.serialize();
        $.post(actionUrl, sendData).done(reflectModal)
    }

    function reflectModal(data) {
        //Replace the previous modalbody with the new body that will reflect the errors if any.
        var newModalBody = $('.modal-body', data);
        placeHolderElement.find('.modal-body').replaceWith(newModalBody)
        var isValid = newModalBody.find('[name="IsValid"]').val() == 'True';
        if (isValid) {
            placeHolderElement.find('.modal').modal('hide');
        }
    }
})($);