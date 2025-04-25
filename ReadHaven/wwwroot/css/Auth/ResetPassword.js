@model ReadHaven.Models.User.PasswordResetModel

<h2>Reset Password</h2>

<form method="post">
    <div class="form-group">
        <label for="NewPassword">New Password</label>
        <input type="password" id="NewPassword" name="NewPassword" class="form-control" required />
    </div>
    <div class="form-group">
        <label for="ConfirmPassword">Confirm Password</label>
        <input type="password" id="ConfirmPassword" name="ConfirmPassword" class="form-control" required />
    </div>
    <input type="hidden" name="Token" value="@Model.Token" />
    <button type="submit" class="btn btn-primary">Reset Password</button>
</form>
