document.getElementById("send-otp-btn").addEventListener("click", function () {
    const email = document.getElementById("email").value;

    if (!email) {
        displayMessage("Please enter a valid email address.", "error");
        return;
    }

    $.ajax({
        url: '/sendEmail',
        type: 'POST',
        contentType: 'application/json', 
        data: JSON.stringify({ email: email }), 
           success: function (data) {
            if (data.success) {
                displayMessage("OTP sent successfully! Please check your email.", "success");
                $("#hidenEmail").val(email); // Use .val() to set the value
                $("#email-section").hide();
                $("#otp-input-section").show();
            } else {
                displayMessage(data.message || "Failed to send OTP. Please try again.", "error");
            }
        },
        error: function (xhr, status, error) {
            displayMessage("An error occurred while sending OTP. Please try again.", "error");
        }
    });
});

document.getElementById("validate-otp-btn").addEventListener("click", function () {
    const otp = document.getElementById("otp").value;
    const email = $("#hidenEmail").val(); // Retrieve email from hidden input

    if (!otp) {
        displayMessage("Please enter the OTP.", "error");
        return;
    }

    if (!email) {
        displayMessage("Email is missing. Please try again.", "error");
        return;
    }

    // Validate OTP request to the server
    $.ajax({
        url: '/ValidateOtp',
        type: 'POST',
        data: { email: email, otp: otp },
        success: function (data) {
            if (data.success) {
                displayMessage("OTP validated successfully!", "success");
            } else {
                displayMessage(data.message || "Invalid OTP. Please try again.", "error");
            }
        },
        error: function (xhr, status, error) {
            displayMessage("An error occurred while validating OTP. Please try again.", "error");
        }
    });
});

function displayMessage(message, type) {
    const messageElement = document.getElementById("message");
    messageElement.textContent = message;
    messageElement.style.color = type === "success" ? "green" : "red";
}