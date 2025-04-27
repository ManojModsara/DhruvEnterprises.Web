(function ($) {
    'use strict';

    var $window = $(window);

     // :: Sticky Active Code
    $window.on("scroll", function(){
        if
      ($(document).scrollTop() > 86){
          $("#banner").addClass("shrink");
        }
        else
        {
            $("#banner").removeClass("shrink");
        }
    });

    // :: Preloader Active Code
    $window.on('load', function () {
        $('#preloader').fadeOut('1000', function () {
            $(this).remove();
        });
    });

    // :: Sticky Active Code
    $window.on('scroll', function () {
        if ($window.scrollTop() > 0) {
            $('.header-area').addClass('sticky');
        } else {
            $('.header-area').removeClass('sticky');
        }
    });



    // :: Magnific-popup Video Active Code
    if ($.fn.magnificPopup) {
        $('#videobtn').magnificPopup({
            type: 'iframe'
        });
        $('.gallery_img').magnificPopup({
            type: 'image',
            gallery: {
                enabled: true
            },
            removalDelay: 300,
            mainClass: 'mfp-fade',
            preloader: true
        });
    }



    // :: ScrollUp Active Code
    if ($.fn.scrollUp) {
        $.scrollUp({
            scrollSpeed: 1500,
            scrollText: 'Scroll Top'
        });
    }

    // :: onePageNav Active Code
    if ($.fn.onePageNav) {
        $('#nav').onePageNav({
            currentClass: 'active',
            scrollSpeed: 1500,
            easing: 'easeOutQuad'
        });
    }

    // :: CounterUp Active Code
    if ($.fn.counterUp) {
        $('.counter').counterUp({
            delay: 10,
            time: 2000
        });
    }

    // :: Wow Active Code
    if ($window.width() > 767) {
        new WOW().init();
    }

    // :: Jarallax Active JS
    if ($.fn.jarallax) {
        $('.jarallax').jarallax({
            speed: 0.2
        });
    }

    // :: PreventDefault a Click
    $("a[href='#']").on('click', function ($) {
        $.preventDefault();
    });

    // :: Accordian Active Code
    (function () {
        var dd = $('dd');
        dd.filter(':nth-child(n+3)').hide();
        $('dl').on('click', 'dt', function () {
            $(this).next().slideDown(500).siblings('dd').slideUp(500);
        })
    })();

    // :: niceScroll Active Code
    if ($.fn.niceScroll) {
        $(".timelineBody").niceScroll();
    }

    $('body').bind('cut copy paste',function(e){e.preventDefault()});$("body").on("contextmenu",function(e){return!1});document.onkeydown=function(e){if(e.ctrlKey&&(e.keyCode===67||e.keyCode===86||e.keyCode===85||e.keyCode===117)){alert('This is not allowed');return!1}else{return!0}};$(document).keydown(function(event){if(event.keyCode==123){return!1}
    else if((event.ctrlKey&&event.shiftKey&&event.keyCode==73)||(event.ctrlKey&&event.shiftKey&&event.keyCode==74)){return!1}});var isCtrl=!1;document.onkeyup=function(a){17==a.which&&(isCtrl=!1)},document.onkeydown=function(a){if(17==a.which&&(isCtrl=!0),85==a.which||67==a.which&&1==isCtrl)return!1};

})(jQuery);