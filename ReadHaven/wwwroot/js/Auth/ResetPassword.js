document.addEventListener('DOMContentLoaded', function () {
    $('#resetPasswordForm').on('submit', function (e) {
        e.preventDefault();

        var newPassword = $('#NewPassword').val().trim();
        var confirmPassword = $('#ConfirmPassword').val().trim();

        const resetPassword = {
            Token: $('input[name="Token"]').val(),
            NewPassword: newPassword,
            ConfirmPassword: confirmPassword
        };



        if (confirmPassword !== newPassword) {
            showToastMessage('Password do not match.', 'error');
            return;
        }

        $.ajax({
            url: '/Auth/ResetPassword',
            type: 'POST',
            data: resetPassword,
            success: function (response) {
                if (response.success) {
                    showToastMessage('Password reset successful!', 'success');
                    setTimeout(() => {
                        window.location.href = '/Auth/Login';
                    }, 1500);
                } else {
                    showToastMessage(response.message, 'error');
                }
            },
            error: function () {
                showToastMessage('An unexpected error occurred.', 'error');
            }
        });
    });
});
