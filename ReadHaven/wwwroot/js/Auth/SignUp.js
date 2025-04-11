@model ReadHaven.Models.User.User

@{
    ViewData["Title"] = "Sign Up";
}

<div class="row justify-content-center mt-5">
    <div class="col-md-4">
        <h2 class="text-center">Sign Up</h2>
        <form asp-action="SignUp" method="post">
            <div class="form-group mb-3">
                <label asp-for="Username" class="form-label">Username</label>
                <input asp-for="Username" name="Username" class="form-control" />
                <span asp-validation-for="Username" class="text-danger"></span>
            </div>

            <div class="form-group mb-3">
                <label asp-for="Email" class="form-label">Email</label>
                <input asp-for="Email" name="Email" class="form-control" />
                <span asp-validation-for="Email" class="text-danger"></span>
            </div>

            <div class="form-group mb-3">
                <label asp-for="PasswordHash" class="form-label">Password</label>
                <input asp-for="PasswordHash" name="PasswordHash" type="password" class="form-control" />
                <span asp-validation-for="PasswordHash" class="text-danger"></span>
            </div>

            <div class="form-group mb-3">
                <label class="form-label">Confirm Password</label>
                <input name="ConfirmPassword" type="password" class="form-control" />
                <span class="text-danger" id="confirmPasswordError"></span>
            </div>

            <button type="submit" class="btn btn-primary w-100">Sign Up</button>
        </form>
    </div>
</div>

@section Scripts {
    @{
        await Html.RenderPartialAsync("_ValidationScriptsPartial");
    }
}
