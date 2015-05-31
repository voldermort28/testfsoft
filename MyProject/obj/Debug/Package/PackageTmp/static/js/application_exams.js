jQuery.noConflict();
jQuery(document).ready(function($){ 
  /*------------------ Exams ------------------*/
  function exams_details_view(){
    $('.page-exams-list .page-exams-item a').fancybox({
      maxWidth	: 770,
      maxHeight	: 400,
      prevEffect : 'none',
      nextEffect : 'none',
      arrows : false,
      padding     : 0,
      afterLoad   : function() {
       // this.inner.append('<ul class="socials-links"><li>Chia sẻ:</li><li class="facebook"><a href="' + $(this.element).attr('data-fb') +  '">f</a></li><li class="zing"><a href="' + $(this.element).attr('data-zing') +  '">Z</a></li></ul>');
      },
      helpers : {
        title     : {
          type    : 'outside',
          position: 'top'
        },
        media : {}
      }
    });
  }
  exams_details_view();
});