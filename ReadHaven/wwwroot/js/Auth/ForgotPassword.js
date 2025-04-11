@{
    ViewData["Title"] = "Forgot Password";
}

<div class="container mt-5">
    <h2 class="text-center">Forgot Password</h2>
    <p class="text-center">Enter your email address to receive a password reset link.</p>

    <form asp-action="ForgotPassword" method="post" class="w-50 mx-auto">
        @if (ViewBag.Message != null)
        {
            <div class="alert alert-info">
                @ViewBag.Message
            </div>
        }

        <div class="form-group">
            <label for="email">Email Address</label>
            <input type="email" name="email" class="form-control" required placeholder="Enter your email" />
        </div>

        <button type="submit" class="btn btn-primary btn-block mt-3">Send Reset Link</button>
    </form>

    <div class="text-center mt-4">
        <p>Remembered your password? <a href="@Url.Action("Index", "Auth")">Login here</a></p>
    </div>
</div>
