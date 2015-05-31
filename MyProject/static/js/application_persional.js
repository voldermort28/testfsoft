jQuery.noConflict();
jQuery(document).ready(function($){
  /*------------------ Exams ------------------*/
  function change_password_form(){
    $('.form-group-change-password a').fancybox({
      maxWidth    : 700,
      maxHeight   : 700,
      padding     : 0,
      html        : $('#change-password-form'),
      helpers     : {
        title     : {
          type    : 'outside',
          position: 'top'
        },
        media : {}
      }
    });
  }
  change_password_form();
});