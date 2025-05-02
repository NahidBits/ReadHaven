function sendResetLink() {
    const email = document.getElementById("emailInput").value;

    $.ajax({
        url: "/Auth/ForgotPassword",
        type: "POST",
        data: { email: email },
        success: function (response) {
            if (response.success) {
                showToastMessage(response.message);
                window.location.href = "/Auth/ForgotPasswordConfirmation";   
            } else {
                showToastMessage(response.message, "error");
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error("Error submitting email:", textStatus, errorThrown);
            showToastMessage("Something went wrong!", "error");
        }
    });
}