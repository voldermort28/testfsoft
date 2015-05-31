jQuery.noConflict();
jQuery(document).ready(function($){ 
  /*------------------ Benefits ------------------*/
  $('.sony-member-benefits h2').click(function(e){
    if($(window).width() < 768 && !$(e.target).hasClass('benefit-title')){
      e.preventDefault();
      $('.sony-member-benefits').find('.benefit-desc').hide();
      $(this).parent().find('.benefit-desc').slideToggle();
      $(this).has('a');
      $('html, body').animate({
        scrollTop: $($(this)).offset().top
      });
    }        
  });
});