$(document).ready(function () {

    onFailTestClick();
    onAckTestClick();

    countTests();

    handleFailDetailsModal();

    notifyIfPrescriptionErrors();

})

var testState = {
    testName: "",
    rowRef: null,
    testDetailsModal: null,
    testComments: null,
    ackTextareaRef: null,
    ackDetailsModal: null
}

var notifyIfPrescriptionErrors = function () {

    const url = "/prescriptionAlert";
    const method = "GET";
    var async = true;
    var postData = "";

    var request = new XMLHttpRequest();

    request.open(method, url, async);

    request.onreadystatechange = function () {
        if (request.readyState === 4 && request.status === 200) {
            console.log("inside request onload");

            var status = request.status; // HTTP response status, e.g., 200 for "200 OK"
            var data = request.responseText; // Returned data, e.g., an HTML document.

            var prescriptionAlert = new jBox('Modal', {
                title: '<h4>Attention!</h4>',
                animation: 'pulse',
                content: data,
                overlay: false,
                draggable: 'title',
                closeButton: false,
                onInit: function () { if (data) { this.open(); } }
            });

            closePresAlertButton(prescriptionAlert);
            console.log(status);
        }
    };
    request.setRequestHeader("Content-type", "text/html; charset=utf-8");

    request.send(postData);

}

var handleFailDetailsModal = function () {
    const url = "/failDetails";
    const method = "GET";
    var async = true;
    var request = new XMLHttpRequest();

    request.open(method, url, async);

    request.onreadystatechange = function () {
        if (request.readyState === 4 && request.status === 200) {
            console.log("inside request onload");
            // show a message that tells user test has been updated successfully;
            var status = request.status; // HTTP response status, e.g., 200 for "200 OK"
            var data = request.responseText; // Returned data, e.g., an HTML document.

            testState.testDetailsModal = new jBox('Modal', {
                attach: '.fail',
                title: 'Failed Test Details',
                content: data,
                overlay: false,
                draggable: 'title',
                onCreated: function () { acknowledgeTest(); }
            });


        }
    };

    request.setRequestHeader("Content-type", "text/html; charset=utf-8");
    request.send();

}

var handleAckDetailsModal = function () {
    const url = "/ackDetails?testName=" + testState.testName;
    const method = "GET";
    var async = true;
    var request = new XMLHttpRequest();

    request.open(method, url, async)

    request.onreadystatechange = function () {
        if (request.readyState === 4 && request.status === 200) {
            console.log("inside request onload");
            // show a message that tells user test has been updated successfully;
            var status = request.status; // HTTP response status, e.g., 200 for "200 OK"
            var data = request.responseText; // Returned data, e.g., an HTML document.

            if (testState.ackDetailsModal == null) {
                testState.ackDetailsModal = new jBox('Modal', {
                    title: '<h4>Comments</h4>',
                    maxWidth: 700,
                    content: data,
                    overlay: false,
                    draggable: 'title',
                    onInit: function () { this.open(); }
                });
            } else {
                testState.ackDetailsModal.setContent(data);
                testState.ackDetailsModal.open();
            }
            handleDismiss()

        }
    };

    request.setRequestHeader("Content-type", "text/html; charset=utf-8");
    request.send();

}

var handleDismiss = function () {
    $("#ackDismissButton").click(function () {
        flushCommentsModal();
    });
}

var closePresAlertButton = function (prescriptionAlert) {

    $("#prescriptionAlertButton").click(function () {
        prescriptionAlert.close();
    })
    console.log("presAlert");
}

var countTests = function () {

    var nFailed = $(".fail").length;
    $("#failedTestIndex").text("Count: " + nFailed);

    var nPassed = $(".pass").length;
    $("#passedTestIndex").text("Count: " + nPassed);

    var nAck = $(".ack").length;
    $("#ackTestIndex").text("Count: " + nAck);

    var nWarn = $(".warn").length;
    $("#warningsIndex").text("Count: " + nWarn);

}

var acknowledgeTest = function () {

    const method = "GET";
    var postData = "";
    var async = true;
    var request = new XMLHttpRequest();

    $("#detailsButton").click(function () {

        // get textarea comments and update state
        testState.ackTextareaRef = $("#acknowledgeModalComments");
        testState.testComments = testState.ackTextareaRef.val();

        const url = "/acknowledge?testName=" + testState.testName + "&testComments=" + testState.testComments;

        request.open(method, url, async);

        request.onreadystatechange = function () {

            if (request.readyState === 4 && request.status === 200) {
                console.log("inside request onload");
                // show a message that tells user test has been updated successfully;
                var status = request.status; // HTTP response status, e.g., 200 for "200 OK"
                var data = request.responseText; // Returned data, e.g., an HTML document.
                console.log(testState.testName);
                console.log("test: " + testState.testName + " acknowledge received");

                flushAckModal();


                updateTables();

            }

        };

        request.setRequestHeader("Content-type", "text/html; charset=utf-8");
        request.send(postData);

    })
}

var flushAckModal = function () {
    testState.ackTextareaRef.val("");
    testState.testDetailsModal.close();
}

var flushCommentsModal = function () {
    // to make sure there isn't more than one listener 
    $("#ackDismissButton").unbind('click');

    testState.ackDetailsModal.close();
}

var updateTables = function () {
    testState.rowRef.remove();
    testState.rowRef.removeClass("fail");
    testState.rowRef.addClass("ack");
    testState.rowRef.find("td:nth-child(3)").text("ACK");
    $(".acknowledged-tests").prepend(testState.rowRef);
    countTests();
    onAckTestClick();
}

var onAckTestClick = function () {

    //To prevent duplicate events every time this function is called;
    $(".ack").unbind('click');

    $(".ack").click(function () {
        console.log("here");
        testState.testName = $(this).find("td:first").text();
        handleAckDetailsModal();
    })
}

var onFailTestClick = function() {
    $(".fail").click(function () {
        testState.testName = $(this).find("td:first").text();
        testState.rowRef = $(this);
    })
}

function openNav() {
    document.getElementById("mySidenav").style.width = "250px";
    document.getElementById("main").style.marginLeft = "250px";
}

function closeNav() {
    document.getElementById("mySidenav").style.width = "0";
    document.getElementById("main").style.marginLeft = "0";
}