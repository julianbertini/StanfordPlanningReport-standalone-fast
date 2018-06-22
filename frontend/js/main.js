
(function ($) {
    "use strict";
    $('.column100').on('mouseover', function () {
        var table1 = $(this).parent().parent().parent();
        var table2 = $(this).parent().parent();
        var verTable = $(table1).data('vertable') + "";
        var column = $(this).data('column') + "";

        $(table2).find("." + column).addClass('hov-column-' + verTable);
        $(table1).find(".row100.head ." + column).addClass('hov-column-head-' + verTable);
    });

    $('.column100').on('mouseout', function () {
        var table1 = $(this).parent().parent().parent();
        var table2 = $(this).parent().parent();
        var verTable = $(table1).data('vertable') + "";
        var column = $(this).data('column') + "";

        $(table2).find("." + column).removeClass('hov-column-' + verTable);
        $(table1).find(".row100.head ." + column).removeClass('hov-column-head-' + verTable);
    });

})(jQuery);

console.log("before window.onload");

var updateTestSettings = function () {

    console.log("Making post request...");
    
    const url = "/update?name=test123&result=PASS"; 
    const method = "GET";
    var postData = "hello world!";
    var async = true;

    var request = new XMLHttpRequest();

    request.open(method, url, async);

    // when the server respond

    request.onreadystatechange = function () {
        if (request.readyState === 4 && request.status === 200) {
            console.log("inside request onload");
            // show a message that tells user test has been updated successfully;
            var status = request.status; // HTTP response status, e.g., 200 for "200 OK"
            var data = request.responseText; // Returned data, e.g., an HTML document.

            console.log(status);
        }
    };

    request.setRequestHeader("Content-type", "text/html; charset=utf-8");

    request.send(postData);
    console.log("data sent");
    
}

window.onload = updateTestSettings;