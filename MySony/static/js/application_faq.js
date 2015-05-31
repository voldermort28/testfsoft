jQuery.noConflict();
jQuery(document).ready(function($){   
  /*------------------ Show faq ------------------*/
  $('.view-faq-list .faq-list h2').click(function(e){
    e.preventDefault();
    var ctop = $($(this)).offset().top;
    if($(window).width() < 768){
      $('.view-faq-list .faq-list').find('.faq-answer').hide();
      $('.view-faq-list .faq-list .item').removeClass('active');
      $(this).parent().parent().addClass('active');
      $(this).parent().parent().find('.faq-answer').slideToggle();
    }else{
      if($(this).parent().parent().hasClass('active')){
        $(this).parent().parent().removeClass('active');
        $(this).parent().parent().find('.faq-answer').removeAttr("style");
        ctop = ctop - 55;
      }else{
        $(this).parent().parent().addClass('active');
        $(this).parent().parent().find('.faq-answer').css({
          display: 'table-cell'
        });
        ctop = ctop - 15;
      }
    }
    
    $('html, body').animate({
      scrollTop: ctop
    });        
  });
});