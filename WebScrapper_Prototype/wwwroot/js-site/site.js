﻿(function ($) { // Begin jQuery
    $(function () { // DOM ready
        // If a link has a dropdown, add sub menu toggle.
        $('nav ul li a:not(:only-child)').click(function (e) {
            $(this).siblings('.nav-dropdown').toggle();
            // Close one dropdown when selecting another
            $('.nav-dropdown').not($(this).siblings()).hide();
            e.stopPropagation();
        });
        // Clicking away from dropdown will remove the dropdown class
        $('html').click(function () {
            $('.nav-dropdown').hide();
        });
        // Toggle open and close nav styles on click
        $('#nav-toggle').click(function () {
            $('nav ul').slideToggle();
        });
        // Hamburger to X toggle
        $('#nav-toggle').on('click', function () {
            this.classList.toggle('active');
        });
    }); // end DOM ready
})(jQuery); // end jQuery

$(document).ready(function () {

    $('#mylinks a').click(function () {
        $('#mylinks a').removeClass('highlight');
        $(this).addClass('highlight');
    });

});
$(document).ready(function () {
    $('#reloadDiv').click(function () {
        $.ajax({
            type: "GET",
            url: "/Home/FeaturedProducts",
            data: { brand: "amd", location: "AMD Products" },
            success: function (data) {
                $('#divToReload').html(data);
            }
        });
    });
});