jQuery.noConflict();
jQuery(document).ready(function ($) {
    /*------------------ Add product ------------------*/
    $('.add-product-link a').click(function (e) {
        e.preventDefault();
        $($(this).attr('href')).slideToggle();
    });

    $.fancybox.open({
        padding: 0,
        html: '<div class="change-password-form">gfagadga</div>'
    });

    /*------------------ Exams ------------------*/
    function register_message(element) {
        $(element).fancybox({
            maxWidth: 700,
            maxHeight: 700,
            padding: 0,
            helpers: {
                title: {
                    type: 'outside',
                    position: 'top'
                }
            }
        });
        $(element).trigger('click');
    }
    //register_message('.register-finish');

    /*------------------ User guide ------------------*/
    $('.user-guide a').click(function (e) {
        e.preventDefault();
        register_message($(this).attr('href'));
    });

    /**---------------Tooltip for password-----------------*/
    $('#password').tooltip({
        'trigger': 'focus',
        'title': 'Mật khẩu phải có ít nhất: 6 ký tự và phải bao gồm 1 ký tự thường, 1 chữ số'
    }
    );
    $('#re-password').tooltip({
        'trigger': 'focus',
        'title': 'Mật khẩu xác nhận phải có ít nhất: 6 ký tự và phải bao gồm 1 ký tự thường, 1 chữ số và phải trùng với Mật khẩu'
    }
    );

    /*------------------ Check value is digit ------------------*/
    function isNumber(str) {
        var numberRegex = /^[+-]?\d+(\.\d+)?([eE][+-]?\d+)?$/;
        if (numberRegex.test(str)) {
            return true;
        }
        return false;
    }

    /*------------------ Validate password ------------------*/
    var $passElement = $('#password, #re-password');
    $passElement.on('keyup', function (e) {
        if (isNumber(e.which)) {
            $passElement.trigger('change');
        }
    });

    /*------------------ View password ------------------*/
    $('.form-group-pass').each(function () {
        $(this).append('<span class="pass-hint">Xem mật khẩu</span>');
    });
    $('.form-group-pass').find('.pass-hint').click(function (e) {
        e.preventDefault();
        var type = $(this).parent().find('input')[0].type.toLowerCase();
        switch (type) {
            case "password":
                $(this).text("Ẩn mật khẩu");
                $(this).parent().find('input').attr('type', 'text');
                break;
            case "text":
                $(this).text("Xem mật khẩu");
                $(this).parent().find('input').attr('type', 'password');
                break;
        }
    });
});
