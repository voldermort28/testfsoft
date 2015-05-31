jQuery.noConflict();
jQuery(document).ready(function($){
  /**------Init slider------------------*/
  var news_slider_start = function(){
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
        $('.sony-filter-tabs').iosSlider('goToSlide', args.currentSlideNumber);
        $('.sony-filter-tabs li').removeClass('active');
        $('.sony-filter-tabs li:eq(' + (args.currentSlideNumber-1) + ')').addClass('active');
      },
      onSliderLoaded: function (args) {
        sliderResize(args);
      }
    });

    $('.sony-filter-tabs').iosSlider({
      desktopClickDrag: true,
      snapToChildren: true,
      infiniteSlider: true,
      onSliderResize: function (args) {
        $('.sony-filter-tabs').height( args.currentSlideObject.actual('outerHeight'));
      },
      onSliderUpdate: function (args) {
        $('.sony-filter-tabs').height( args.currentSlideObject.actual('outerHeight'));
      },
      onSlideChange: function (args) {
        $('.sony-filter-tabs').height( args.currentSlideObject.actual('outerHeight'));
        $('.view-hot-article-wrapper').iosSlider('goToSlide', args.currentSlideNumber);
      },
      onSliderLoaded: function (args) {
        $('.sony-filter-tabs').height( args.currentSlideObject.actual('outerHeight'));
      }
    });
  };
  
  var news_slider_destroy = function(){
    $('.view-hot-article-wrapper').iosSlider('destroy');
    $('.sony-filter-tabs').iosSlider('destroy');
  };
  
  $('.sony-filter-tabs li').each(function(i) {				
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
  $('.sony-filter-tabs li a').click(function(e){
    e.preventDefault();
    $('.hot-article-wrapper .view-hot-article').hide();
    $(this).parent().parent().find('li').removeClass('active');
    $(this).parent().addClass('active');
    $($(this).attr('href')).show();
  });
  
  
  /**-------if is not mobile: stop slider---------*/
  if($(window).width() > 767){
    news_slider_destroy();
  }else{
    news_slider_start();
  }
  
  /**------Window resize-------------*/
  $(window).resize(function() {
    if($(window).width() > 767){
      news_slider_destroy();
    }else{
      news_slider_start();
    }
  });
  
});


