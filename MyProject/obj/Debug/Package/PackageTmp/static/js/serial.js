$(function () {
   
    $(document).ready(function () {
       // jQuery(".chosen-select").chosen(); //WTF
        $('#slProduct').html('');
         
        $.post('/HeThong/GetProductsOfCategory/',
           {
               'categoryId': $('#slCategory option:selected').val()
           }, function (a) {               
               $('#slProduct').append('<option value="">Chọn sản phẩm</option>');
               $(a).each(function (index, e) {
                   $('#slProduct').append('<option value="' + e + '">' + e + '</option>');
               });
               jQuery("#slProduct").trigger("chosen:updated");
           });

    });

    $(".chosen-select").chosen({
        allow_single_deselect: true,
        disable_search_threshold: 10,
        no_results_text: 'Không có sản phẩm',
        placeholder_text_single: 'Lựa chọn 1 sản phẩm',
        placeholder_text_multiple: 'Lựa chọn 1 sản phẩm',
        search_contains:true
    });

    $('.chosen-results').css({ "max-height": "300px" });


    $("#slCategory").change(function () {
        
        $('#slProduct').html('');
        $.post('/HeThong/GetProductsOfCategory/',
            {
                'categoryId': $(this).val()
            }, function (a) {            
                $('#slProduct').append('<option value="">Chọn sản phẩm</option>');                
                $(a).each(function (index, e) {
                    $('#slProduct').append('<option value="' + e + '">' + e + '</option>');
                });
                jQuery("#slProduct").trigger("chosen:updated");
            });
    });

    
});