document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('loginForm');

    if (form) {
        form.addEventListener('submit', function (e) {
            e.preventDefault();

            const formData = {
                Email: document.getElementById('Email').value.trim(),
                PasswordHash: document.getElementById('PasswordHash').value
            };

            $.ajax({
                url: '/Auth/Login',
                method: 'POST',
                data: { user: formData },
                success: function (response) {
                    if (response.success) {
                        showToastMessage('Login successful!', 'success');
                        const returnUrl = localStorage.getItem('returnUrl') || '/Book/Index';
                        window.location.href = returnUrl;
                    } else {
                        showToastMessage(response.message || 'Login failed.', 'error');
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    console.error("Error submitting login:", textStatus, errorThrown);
                    showToastMessage("Something went wrong! Please try again.", "error");
                }
            });
        });
    }
});
