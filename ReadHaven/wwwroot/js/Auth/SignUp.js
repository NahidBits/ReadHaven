document.addEventListener('DOMContentLoaded', function () {
        $('#signUpForm').on('submit', function (e) {
            e.preventDefault();

            const username = $('#Username').val().trim();
            const email = $('#Email').val().trim();
            const password = $('#PasswordHash').val();
            const confirmPassword = $('#ConfirmPassword').val();

            if (password !== confirmPassword) {
                showToastMessage('Passwords do not match.', 'error');
                return;
            } 

            const formData = {
                Username: username,
                Email: email,
                PasswordHash: password,
            };

            $.ajax({
                url: '/Auth/SignUp',
                type: 'POST',
                data: { user: formData, confirmPassword: confirmPassword },
                success: function (response) {
                    if (response.success) {
                        showToastMessage('Registration successful!', 'success');
                        const returnUrl = localStorage.getItem('returnUrl') || '/Book/Index';
                            window.location.href = returnUrl;
                    } else {
                        showToastMessage(response.message, 'error');
                    }
                },
                error: function () {
                    showToastMessage('An error occurred during sign-up.', 'error');
                }
            });
        });
});