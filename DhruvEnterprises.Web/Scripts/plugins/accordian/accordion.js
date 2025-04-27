$.fn.accordian = function(){
        this.bind( "click", function() {
            if(!$(this).hasClass('active')){
                if($(this).closest('.acc_cont').siblings('.acc_handle').hasClass('active')){
                    $(this).closest('.acc_cont').find('.acc_cont').slideUp();   
                    $(this).closest('.acc_cont').find('.acc_handle.active').removeClass('active'); 
                }else{
                    $(this).closest('.acc_container').children('.acc_loop').children('.acc_handle.active').parent().children('.acc_cont').slideUp();   
                    $(this).closest('.acc_container').children('.acc_loop').children('.acc_handle.active').removeClass('active'); 
                }   
                $(this).parent().children('.acc_handle').first().addClass('active');
                $(this).parent().children('.acc_cont').first().slideDown();
            }else{
                if($(this).closest('.acc_cont').siblings('.acc_handle').hasClass('active')){
                    $(this).closest('.acc_cont').find('.acc_handle.active').removeClass('active');    
                    $(this).closest('.acc_cont').find('.acc_cont').slideUp();    
                }else{
                    $(this).closest('.acc_container').children('.acc_loop').children('.acc_handle.active').removeClass('active');
                    $(this).closest('.acc_container').children('.acc_loop').children('.acc_cont').slideUp();
                }

            }
        });        
    }