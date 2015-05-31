// for ajax loading
$(document).ajaxStop(function () {
    $('#ajax_loader').hide();
});
$(document).ajaxStart(function () {
    $('#ajax_loader').show();
});


// menu active
var local = location.pathname + location.search;
var a = $('#nav-main').find('a').each(function () {
    var href = $(this).attr("href");
    if (local == href) {
        $(this).addClass('active');
    }

});

// Check special characters in textbox and textarea
function checkCharactersValid() {
    //var lstSpecialCharacters = ['"', "'", ';', ':', '<', '>', ')', '(', '*', '^', '%', '$', '#', '!', '~', '&', '*', '+', '=', '{', '}', '[', ']', ',', '|'];
    var lstSpecialCharacters = ['<', '>'];
    var ok = true;
    $('input[type=text]').each(function (idx1, item1) {
        var a = $(this).val();
        $.each(lstSpecialCharacters, function (idx, item) {
            if (a.indexOf(item) >= 0) {
                ok = false;                
            } else {
                //  alert(item + '|' + );
            }
        });
    });
    $('textarea').each(function () {
        var b = $(this).val();
        $.each(lstSpecialCharacters, function (idx, item) {
            if (b.indexOf(item) >= 0) {
                ok = false;                
            }
        });
    });
    if (!ok) {
        alert("Vui lòng không sử dụng ký tự đặc biệt");
        return false;
    }
    return ok;
}

// Disable button submit when submit form
$('form').submit(function () {
    // checkCharactersValid();
    $('input[type=submit]', this).attr('disabled', 'disabled');
});

// Allow only numberic input
$('.numberic').keypress(function (e) {
    if (e.which != 8 && e.which != 0 && (e.which < 48 || e.which > 57)) return false;
});

// Turn off autocomplete
$('form,input,select,textarea').attr("autocomplete", "off");

// Setup CSRF safety for AJAX:
$.ajaxPrefilter(function (options, originalOptions, jqXHR) {
    if (options.type.toUpperCase() === "POST") {
        var token = $("input[name^=__RequestVerificationToken]").first();
        if (!token.length) return;
        var tokenName = token.attr("name");
        if (options.contentType.indexOf('application/json') > 0) {
            options.url += ((options.url.indexOf("?") === -1) ? "?" : "&") + token.serialize();
        } else if (typeof options.data === 'string' && options.data.indexOf(tokenName) === -1) {
            options.data += (options.data ? "&" : "") + token.serialize();
        }
    }
});

// Clear form
$.fn.clearForm = function () {
    return this.each(function () {
        try {
            var type = this.type, tag = this.tagName.toLowerCase();
            if (tag == 'form')
                return $(':input', this).clearForm();
            //    if (type == 'checkbox' || type == 'radio')
            //        this.checked = false;
            //  else if (tag == 'select')
            //      this.selectedIndex = -1;
            //    else
            //       this.value = '';
        } catch (e) {

        }
    });
};

