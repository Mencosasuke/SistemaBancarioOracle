$(document).ready(function () {

    // Global configurations for jquery noty
    var notyConfig = {
        text: "",
        type: "information",
        dismissQueue: true,
        layout: 'topRight',
        closeWith: ['click'],
        theme: 'relax',
        template: "<div class='noty_message'><span class='noty_text'></span><div class='noty_close'></div></div>",
        maxVisible: 5,
        timeout: 3000,
        force: true,
        modal: false,
        buttons: false,
        animation: {
            open: 'animated bounceInRight',
            close: 'animated bounceOutRight',
            easing: 'swing',
            speed: 500
        },
        callback: {
            onShow: function () { },
            afterShow: function () { },
            onClose: function () { },
            afterClose: function () { }
        }
    };

    $.noty.defaults = notyConfig;
});