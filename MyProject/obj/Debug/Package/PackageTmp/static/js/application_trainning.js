jQuery.noConflict();
jQuery(document).ready(function($){  
  /*------------------ Trainning ------------------*/  
  function trainning_details_view(){
    $(".sony-trainning-courses a").click(function(e){
      e.preventDefault();
      var trainning_view_port = $(this).parent().parent().parent().find('a');
      if($(window).width() > 767){
        trainning_view_port = $(this).parent().parent().parent().find('.sony-trainning-details > div').not('.visible-mobile').find('a');
      }else{
        trainning_view_port = $(this).parent().parent().parent().find('a');
      }
      
      trainning_view_port.attr('rel', 'gallery').fancybox({
        padding     : 0,
        afterLoad   : function() {
          //this.inner.append('<ul class="socials-links"><li>Chia sẻ:</li><li class="facebook"><a href="' + $(this.element).attr('data-fb') +  '">f</a></li><li class="zing"><a href="' + $(this.element).attr('data-zing') +  '">Z</a></li></ul>');
          var list = $("#trainning-links");
          if (!list.length) {    
            list = $('<ul id="trainning-links">');
            for (var i = 0; i < this.group.length; i++) {
              $('<li data-index="' + i + '">&nbsp;</li>').click(function() { $.fancybox.jumpto( $(this).data('index'));}).appendTo( list );
            }
          }
          this.inner.append(list);
          list.find('li').removeClass('active').eq( this.index ).addClass('active');
        },
        beforeClose : function(){
          $("#trainning-links").remove();    
        },
        helpers     : {
          title     : {
            type    : 'outside',
            position: 'top'
          }
        }
      });
      
      $('#fancybox-thumbs').css({
        background: '#ff0000'
      });
    });
  };
  
  trainning_details_view();
  
  /**------Init slider------------------*/
  var home_slider_start = function(){
    $('.view-hot-article-wrapper').iosSlider({
      snapToChildren: true,
      desktopClickDrag: true,
      responsiveSlideContainer: true,
      responsiveSlides: true,
      infiniteSlider: true,
      snapVelocityThreshold: 0.1,
      onSliderResize: function (args) {
        sliderResize(args);
      },
      onSliderUpdate: function (args) {
        sliderResize(args);
      },
      onSlideChange: function (args) {
        sliderResize(args);
        $('.filter-hot-article').iosSlider('goToSlide', args.currentSlideNumber);
        $('.filter-hot-article .filter-item').removeClass('active');
        $('.filter-hot-article .filter-item:eq(' + (args.currentSlideNumber-1) + ')').addClass('active');
      },
      onSliderLoaded: function (args) {
        sliderResize(args);
      }
    });

    $('.filter-hot-article').iosSlider({
      desktopClickDrag: true,
      snapToChildren: true,
      infiniteSlider: true,
      onSliderResize: function (args) {
        $('.filter-hot-article').height( args.currentSlideObject.actual('outerHeight'));
      },
      onSliderUpdate: function (args) {
        $('.filter-hot-article').height( args.currentSlideObject.actual('outerHeight'));
      },
      onSlideChange: function (args) {
        $('.filter-hot-article').height( args.currentSlideObject.actual('outerHeight'));
        $('.view-hot-article-wrapper').iosSlider('goToSlide', args.currentSlideNumber);
      },
      onSliderLoaded: function (args) {
        $('.filter-hot-article').height( args.currentSlideObject.actual('outerHeight'));
      }
    });
  };
  
  var home_slider_destroy = function(){
    $('.view-hot-article-wrapper').iosSlider('destroy');
    $('.filter-hot-article').iosSlider('destroy');
  };
  
  $('.filter-hot-article .filter-item').each(function(i) {				
    $(this).bind('click', function() {
      $('.view-hot-article-wrapper').iosSlider('goToSlide', i+1);
    });
  });
  
  function sliderResize(args) {
	  var $imgs = args.currentSlideObject.find('img');
    if ($imgs.size() <= 0) console.error('Featured projects must have a header image.');
    $imgs.imagesLoaded(function () {
      $('.view-hot-article-wrapper').height( args.currentSlideObject.actual('outerHeight'));
      $('.view-hot-article-wrapper .hot-article-row').height( args.currentSlideObject.actual('outerHeight'));
    });
	}
  
  /**-------Desktop filter---------*/
  $('.filter-hot-article .filter-list a').click(function(e){
    e.preventDefault();
    $('.hot-article-wrapper .view-hot-article').hide();
    $(this).parent().parent().find('a').removeClass('active');
    $(this).addClass('active');
    $($(this).attr('href')).show();
  });
  
  
  /**-------if is not mobile: stop slider---------*/
  if($(window).width() > 767){
    home_slider_destroy();
  }else{
    home_slider_start();
  }
  
  /**------Window resize-------------*/
  $(window).resize(function() {
    if($(window).width() > 767){
      home_slider_destroy();
    }else{
      home_slider_start();
    }
  });
  
});


