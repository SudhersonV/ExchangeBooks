$(document).ready(function() {
    $(".external-login-btn").click(function(event) {
        var lilabel = $(event.currentTarget).children('label');
        var lia = $(event.currentTarget).children('a');
        var lia_href = lia.attr('href');
        location.href = lia_href;
    })
});