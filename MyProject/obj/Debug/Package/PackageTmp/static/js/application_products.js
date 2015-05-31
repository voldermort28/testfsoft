jQuery.noConflict();
jQuery(document).ready(function($){
  /*------------------ Exams ------------------*/
  function register_products_form(){
    $('.register-products-link').fancybox({
      maxWidth    : 900,
      maxHeight   : 900,
      padding     : 0,
      html: $('#register-products-form'),
      afterShow: function () {
          // jQuery('.chosen-select').trigger('chosen:updated');
          jQuery(".chosen-select").chosen({
              disable_search_threshold: 10,
              no_results_text: "Không thấy kết quả",
              search_contains: true
          }); //WTF
      },
      helpers     : {
        title     : {
          type    : 'outside',
          position: 'top'
        },
        media: {} 
      }
    });
  }
  //jQuery('.chosen-select').chosen();
  register_products_form();
});