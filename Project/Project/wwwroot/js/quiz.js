"use strict";

// Create connection with signalR QuizHub.
var connection = new signalR.HubConnectionBuilder().withUrl("/QuizHub").build();

document.getElementById("submitAnswer").disabled = true;
document.getElementById("playAgain").disabled = true;

document.getElementById("option1").style.display = 'none';
document.getElementById("option2").style.display = 'none';
document.getElementById("option3").style.display = 'none';
document.getElementById("option4").style.display = 'none';

// Receive prompts sent by admin/server.
connection.on("ReceiveMessage", function (user, message) {
    document.getElementById("adminMessage").innerHTML = `${user}: ${message}`;
    $('#adminMessage').fadeIn(1000);
    
    console.log("3");
    setTimeout(function () {
        $('#adminMessage').fadeOut(1000);
    }, 4000);
    

});

var timerId = null;
var timeLeft = null;
var elem = null;

// Receive questions from admin/server.
connection.on("ReceiveQuestion", function (message) {
    const myArr = message.split("|");

    var question = myArr[0];
    if (question.charAt(0) == '1') {
        // Resetting score.
        document.getElementById('score').innerHTML = "";

        // Displaying radio buttons.
        document.getElementById("option1").style.display = 'block';
        document.getElementById("option2").style.display = 'block';
        document.getElementById("option3").style.display = 'block';
        document.getElementById("option4").style.display = 'block';
    }

    // Updating/setting questions.
    document.getElementById('question').innerText = question;
    document.getElementById("option1").value = myArr[1];
    document.getElementById("option1Text").innerHTML = myArr[1];

    document.getElementById("option2").value = myArr[2];
    document.getElementById("option2Text").innerHTML = myArr[2];

    document.getElementById("option3").value = myArr[3];
    document.getElementById("option3Text").innerHTML = myArr[3];

    document.getElementById("option4").value = myArr[4];
    document.getElementById("option4Text").innerHTML = myArr[4];

    // Configuring timer.
    timeLeft = 30;
    elem = document.getElementById('question_time');
    timerId = setInterval(countdown, 1000);

    document.getElementById("submitAnswer").disabled = false;
    document.getElementById("groupNameInput").disabled = true;
    document.getElementById("playAgain").disabled = true;
    document.getElementById('result').innerHTML = "";
});

// Receive score from admin/server.
connection.on("ReceiveScore", function (message) {
    document.getElementById('score').innerHTML = message;
});

// Receive game result from admin/server.
connection.on("RecieveResult", function (message) {
    document.getElementById('result').innerHTML = message;
    document.getElementById("groupNameInput").disabled = false;
    document.getElementById("playAgain").disabled = false;
});

// Event to abruptly stooping the game if one user leave the game.
connection.on("StopGame", function (message) {
    document.getElementById('result').innerHTML = message;
    document.getElementById("groupNameInput").disabled = false;
    document.getElementById("submitAnswer").disabled = true;
    clearTimeout(timerId);
});

// Enable play again button again.
connection.on("EnablePlayAgainButton", function () {
    document.getElementById("playAgain").disabled = false;
});

// Starting connection with signalR. 
connection.start().then(function () {
    //document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

// Adding to group on click of join button.
document.getElementById("joinButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var groupName = document.getElementById("groupNameInput").value;

    // Invoking method in QuizHub to add the user to group.
    connection.invoke("AddToGroup", user, groupName).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

// Leave group on click of leave button.
document.getElementById("leaveButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var groupName = document.getElementById("groupNameInput").value;

    // Invoking method in QuizHub to remove the user from group.
    connection.invoke("RemoveFromGroup", user, groupName).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

// To maintain countdown of question.
function countdown() {
    if (timeLeft == -1) {
        clearTimeout(timerId);
        submitAnswer();
    } else {
        if (timeLeft > 10 && timeLeft <= 20) {
            elem.style.color = "orange";
        } else if (timeLeft <= 10) {
            elem.style.color = "red";
        } else {
            elem.style.color = "black";
        }
        elem.innerHTML = timeLeft + ' seconds remaining';
        timeLeft--;
    }
}

// To submit answer of the user.
function submitAnswer() {
    document.getElementById("submitAnswer").disabled = true;

    var ele = document.getElementsByName('options');
    var groupName = document.getElementById("groupNameInput").value;
    var elem = document.getElementById('question_time');

    // To get the checked answer.
    for (var i = 0; i < ele.length; i++) {
        if (ele[i].checked) {
            clearTimeout(timerId);
            elem.innerHTML = "";

            // Invoking method in QuizHub to send answer of the user.
            connection.invoke("SendAnswer", groupName, ele[i].value).catch(function (err) {
                return console.error(err.toString());
            });
        }
    }


    event.preventDefault();
}

// To submit answer on click of submit answer button.
document.getElementById("submitAnswer").addEventListener("click", function (event) {
    submitAnswer();
});

// To send wish to play again on click of play again button.
document.getElementById("playAgain").addEventListener("click", function (event) {
    document.getElementById("playAgain").disabled = true;
    var groupName = document.getElementById("groupNameInput").value;

    // Invoking method in QuizHub to send play again request of the user.
    connection.invoke("PlayAgain", groupName).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});