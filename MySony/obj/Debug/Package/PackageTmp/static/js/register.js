$(function () {
    // page load
    $('.ui-datepicker').attr('style', 'width: 21em !important');
    var isAddSP = false;
    $('#addSP').click(function () {
        isAddSP = !isAddSP;
        if (isAddSP) {
            $(this).html('+ Không thêm nữa');
            $('#form-add-product').addClass('RegisProduct');
            $('#form-add-product .txtProductSerial').attr('data-parsley-required', 'true');//timePurchase
            $('#form-add-product .timePurchase').attr('data-parsley-required', 'true');//
            $('#form-add-product .chosen-select').attr('data-parsley-required', 'true');//
        } else {
            $(this).html('+ Thêm sản phẩm');
            $('#form-add-product').removeClass('RegisProduct');
            $('#form-add-product .txtProductSerial').attr('data-parsley-required', 'false');
            $('#form-add-product .timePurchase').attr('data-parsley-required', 'false');
            $('#form-add-product .chosen-select').attr('data-parsley-required', 'false');//
        }
    });

    $(document).ready(function () {
        jQuery(".chosen-select").chosen({
            disable_search_threshold: 10,
            no_results_text: "Không thấy kết quả",
            search_contains: true
        }); //WTF


        // set datepicker
        $("#slDOB").datepicker({
            changeMonth: true,
            changeYear: true,
            dateFormat: "dd/mm/yy",
            yearRange: '1940:2020'
        });
        $(".timePurchase").datepicker({
            changeMonth: true,
            changeYear: true,
            //minDate:'',
            maxDate: 0,
            dateFormat: "dd/mm/yy",
        });


        //var el = $(this).closest('.RegisProduct');
        $('.slProductID').html('');

        //$("#productImage").html('');
        //$("#serialImage").html('');
        $('#modelImage').val($('.category', this).val());
        $('#modelSerialImage').val($('.category', this).val());

        //$("#productImage").append($('#modelImage option:selected').text());
        //$("#serialImage").append($('#modelSerialImage option:selected').text());
        //
        $("#productImage").attr("src", $('#modelImage option:selected').text());
        $("#serialImage").attr("src", $('#modelSerialImage option:selected').text());

    });

    // add product
    $('#btnAddProduct').click(function () {
        ShowProducts();
    });

    // load district
    //$.get('/Register/GetDistrictOfCity/', { 'cityID': $('#slCity').val() }, function (a) {
    //    $('#slDistrict').html('');
    //    $(a).each(function (index, e) {
    //        if (index == 0) {
    //          //  $('#slDistrict').parent().find('span').html(e.name);
    //            $('#slDistrict').append('<option value="' + e.district_id + '">' + e.name + '</option>');
    //        } else {
    //            $('#slDistrict').append('<option value="' + e.district_id + '">' + e.name + '</option>');
    //        }
    //    });
    //    jQuery("#slDistrict").trigger("chosen:updated");
    //});

    jQuery("#slCity").chosen().change(function () {
        var that = this;
        $.get('/Register/GetDistrictOfCity/', { 'cityID': $('#slCity').val() }, function (a) {

            $('#slDistrict').html('');
            $(a).each(function (index, e) {
                //$('#slDistrict').append('<option value="' + e.district_id + '">' + e.name + '</option>')
                if (index == 0) {
                    //     $('#slDistrict').parent().find('span').html(e.name);
                    $('#slDistrict').append('<option value="' + e.district_id + '">' + e.name + '</option>');
                } else {
                    $('#slDistrict').append('<option value="' + e.district_id + '">' + e.name + '</option>');
                }
            });
            jQuery("#slDistrict").trigger("chosen:updated");
            //$('#slDistrict option:first').attr("selected", "selected");
        });
    });

    //$('#slCity').change(function () {
    //    var that = this;
    //    $.get('/Register/GetDistrictOfCity/', { 'cityID': $('#slCity').val() }, function (a) {

    //        $('#slDistrict').html('');
    //        $(a).each(function (index,e) {
    //            //$('#slDistrict').append('<option value="' + e.district_id + '">' + e.name + '</option>')
    //            if (index == 0) {
    //                $('#slDistrict').parent().find('span').html(e.name);
    //                $('#slDistrict').append('<option value="' + e.district_id + '">' + e.name + '</option>');
    //            } else {
    //                $('#slDistrict').append('<option value="' + e.district_id + '">' + e.name + '</option>');
    //            }
    //        });
    //       // $('#slDistrict option:first').attr("selected", "selected");
    //    });
    //});hin

    // load san pham
    jQuery('#panelRegistProduct').on('change', '.RegisProduct .category', function () {
        //ShowPopup(1);
        var el = $(this).closest('.RegisProduct');
        // set image
        $('#modelImage').val($(this).val());
        $('#modelSerialImage').val($(this).val());

        //$("#productImage").html('');
        //$("#serialImage").html('');
        $("#productImage").append($('#modelImage option:selected').text());
        $("#serialImage").append($('#modelSerialImage option:selected').text());
        // comment
        $("#productImage").attr("src", $('#modelImage option:selected').text());
        $("#serialImage").attr("src", $('#modelSerialImage option:selected').text());


        $('.slProductID', el).html('');
        $.get('/Register/GetProductsOfCategory/',
            {
                'cartegoryID': $(this).val()
            }, function (a) {
                $(a).each(function (index, e) {
                    if (index == 0) {
                        //    $('.slProductID', el).parent().find('span').html(e.name);
                        $('.slProductID', el).append('<option value=' + e.product_id + ' selected="selected">' + e.name + '</option>');
                    } else {
                        $('.slProductID', el).append('<option value="' + e.product_id + '">' + e.name + '</option>');
                    }
                });
                // alert($('.slProductID', el).html());
                jQuery(".slProductID").trigger("chosen:updated");
            });
    });

    //$('#panelRegistProduct .RegisProduct .category').on('change', function () {
    //    //ShowPopup(1);
    //    var el = $(this).closest('.RegisProduct');
    //    $('.slProductID', el).html('');
    //    $.get('/Register/GetProductsOfCategory/', { 'cartegoryID': $(this).val() }, function (a) {
    //        $(a).each(function (index, e) {
    //            if (index == 0) {
    //                $('.slProductID',el).parent().find('span').html(e.name);
    //                $('.slProductID',el).append('<option value=' + e.product_id + ' selected="selected">' + e.name + '</option>');
    //            } else {
    //                $('.slProductID',el).append('<option value="' + e.product_id + '">' + e.name + '</option>');
    //            }
    //        });
    //       // alert($('.slProductID', el).html());
    //        jQuery(".slProductID").trigger("chosen:updated");
    //    });
    //});

    // load shops
    jQuery('#panelRegistProduct').on('change', '.RegisProduct .slCity', function () {
        //ShowPopup(1);
        var el = $(this).closest('.RegisProduct');
        $('.slShop', el).html('');
        $.get('/Register/GetShopOfCity/', { 'cityID': $('.slCity', el).val() }, function (a) {
            $(a).each(function (index, e) {
                //$('.slShop', el).append('<option value="' + e.shop_id + '">' + e.name + '</option>');
                if (index == 0) {
                    //  $('.slShop',el).parent().find('span').html(e.name);
                    $('.slShop', el).append('<option value=' + e.shop_id + ' selected="selected">' + e.name + '</option>');
                } else {
                    $('.slShop', el).append('<option value="' + e.shop_id + '">' + e.name + '</option>');
                }
            });
            jQuery(".slShop").trigger("chosen:updated");
        });
    });

    // validate form before submit
    $('#frmReg').parsley().subscribe('parsley:form:validate', function (formInstance) {
        if (formInstance.isValid() && checkCharactersValid()) {
            products = [];
            $('#panelRegistProduct .RegisProduct').each(function () {
                var prod = {
                    'BuyDate': $('.timePurchase', this).val(),
                    'CategoryID': $('.category', this).val(),
                    'ProductID': $('.slProductID', this).val(),
                    'Serial': $('.txtProductSerial', this).val(),
                    'ShopID': $('.slShop', this).val(),
                    'CityID': $('.slCity', this).val(),
                    'ProductName': $('.slProductID option:selected', this).text(),
                };

                // if (isAddSP) {
                products.push(prod);
                // }

            });

            var sex;
            if ($('#slSex').val() == 1) {
                sex = true;
            } else {
                sex = false;
            }
            var customer = {
                'email': $('#txtEmailRegister').val(),
                'mobilephone': $('#txtTelRegister').val(),
                'password': $('#password').val(),
                'lastname': $('#txtLastNameRegister').val(),
                'firstname': $('#txtFirstNameRegister').val(),
                'sex': sex,
                'birthday': $('#slDOB').val(),
                'address': $('#txtAddressRegister').val(),
                'district_id': $('#slDistrict').val(),
                'city_id': $('#slCity').val()
            };
            //$('#btnSubmit').attr('disabled', 'disabled');   
            $.post('/Register/RegisterProcess/',
                    {
                        cus0: customer,
                        listCusPro: products,
                        recaptcha: $('#g-recaptcha-response').val()
                    },
                    function (a) {
                        if (a.result != "OK") {
                            ShowPopup(a.msg, function () {
                                grecaptcha.reset();
                                //  location.href = "/UserCP/MyProduct/"
                            });
                            $('#frmReg').clearForm();
                        } else {
                            ShowPopup(a.msg, function () {
                                location.href = "/";
                            });
                        }
                    });
        }
        else {
            //ShowPopup('Lỗi');
        }
    });

    if (!($('#chkPolicy1').prop('checked'))) {
        $('#btnSubmit').attr('disabled', 'disabled');
    }

    $('#chkPolicy1').click(function () {
        if ($('#chkPolicy1').prop('checked')) {
            $('#btnSubmit').prop('disabled', false);
        } else {
            $('#btnSubmit').attr('disabled', 'disabled');
        }
    });
});