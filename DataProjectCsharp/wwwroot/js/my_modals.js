// This is the modal to add a portfolio
(function ($) {
    let placeHolderElement = $('#modal-placeholder-portfolio');
    let add_portfolio_button = $('button[data-toggle="ajax-modal-portfolio"]')
    add_portfolio_button.on("click", showAddPortfolioModal)

    function showAddPortfolioModal(event) {
        let url = $(this).data('url');
        $.get(url).done(getPartialView)
    }
    function getPartialView(data) {
        placeHolderElement.html(data);
        placeHolderElement.find('.modal').modal('show');
    }

    placeHolderElement.on('click', '[data-save="modal-portfolio"]', savePortfolio)
    function savePortfolio(event) {
        event.preventDefault();
        let form = $(this).parents('.modal').find('form');
        let actionUrl = form.attr('action');
        let sendData = form.serialize();
        $.post(actionUrl, sendData).done(reflectModal)
    }
    
    function reflectModal(data) {
        //Replace the previous modalbody with the new body that will reflect the errors if any.
        var newModalBody = $('.modal-body', data);
        placeHolderElement.find('.modal-body').replaceWith(newModalBody);
        var isValid = newModalBody.find('[name="IsValid"]').val() == 'True';
        if (isValid) {
            placeHolderElement.find('.modal').modal('hide');
            location.reload()
        }
    }
})($);

// This is the modal to view all of the user trades

(function ($) {
    let placeHolderElement = $('#modal-placeholder-trade-view');
    let view_trades_button = $('button[data-toggle="ajax-modal-view-trades"]')
    view_trades_button.on("click", getTrades)

    function getTrades(event) {
        let url = $(this).data('url');
        $.get(url).done(showTradesModal)
    }

    function showTradesModal(data) {
        placeHolderElement.html(data);
        placeHolderElement.find('.modal').modal('show');
    }

    placeHolderElement.on('click', '[data-dismiss="modal"]', hideTradesModal)
    function hideTradesModal(data) {
        placeHolderElement.find('.modal').modal('hide');
    }
    
    placeHolderElement.on('hidden.bs.modal', '.modal', function () {
        $('.modal:visible').length && $(document.body).addClass('modal-open');
    });
})($);


// this is the modal to view all tradeable securities
(function ($) {
    let placeHolderElement = $('#modal-placeholder-available-securities');
    let view_tradeable_button = $('button[data-toggle="ajax-modal-view-available-securities"]')
    view_tradeable_button.on("click", showTradeableSecurities)

    function showTradeableSecurities(event) {
        let url = $(this).data('url');
        $.get(url).done(showTradeableSecuritiesModal)
    }
    
    function showTradeableSecuritiesModal(data) {
        placeHolderElement.html(data);
        placeHolderElement.find('.modal').modal('show');
    }

    placeHolderElement.on('click', '[data-dismiss="modal"]', hideTradeableSecuritiesModal)
    function hideTradeableSecuritiesModal(data) {
        placeHolderElement.find('.modal').modal('hide');
    }
})($);
