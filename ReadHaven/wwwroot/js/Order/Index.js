const paymentMethodSelect = document.getElementById('paymentMethod');
const emailForm = document.getElementById('emailForm');
const otpForm = document.getElementById('otpForm');
const sendOtpButton = document.getElementById('sendOtp');
//const completePaymentButton = document.getElementById('completePayment');
const emailInput = document.getElementById('otpEmail');
const otpInput = document.getElementById('otp');

function loadProductDetails() {
    $.ajax({
        url: '/BookOrder/UserProductDetails',
        type: 'GET',
        success: function (data) {
            document.getElementById('totalQuantity').textContent = data.totalQuantity;
            document.getElementById('totalAmount').textContent = parseFloat(data.totalAmount).toFixed(2);
            document.getElementById('taxAmount').textContent = parseFloat(data.tax).toFixed(2) + '%';
            document.getElementById('discountAmount').textContent = parseFloat(data.discount).toFixed(2) + '%';

            let totalAmountAllCutting = (data.totalAmount + (data.totalAmount * (data.tax / 100)) - (data.totalAmount * (data.discount / 100)));
            document.getElementById('totalAmountAllCutting').textContent = totalAmountAllCutting.toFixed(2);
        },
        error: function (error) {
            console.error('Error loading product details:', error);
            showToastMessage('Error loading product details. Please try again later.', 'danger', 3000);
        }
    });
}

function orderForm() {
    var shippingAddress = document.getElementById("shippingAddress").value;
    var shippingCity = parseInt(document.getElementById("shippingCity").value);
    var shippingPostalCode = document.getElementById("shippingPostalCode").value;
    var shippingContact = document.getElementById("contactNumber").value;
    var shippingCountry = document.getElementById("shippingCountry").value;
    var email = document.getElementById("email").value;
    var currency = parseInt(document.getElementById("currency").value);
    var paymentMethod = parseInt(document.getElementById("paymentMethod").value);
    var amount = parseFloat(document.getElementById("totalAmountAllCutting").textContent);

    var order = {
        ShippingAddress: shippingAddress,
        ShippingCity: shippingCity,
        ShippingContact: shippingContact,
        Email: email,
        PaymentMethod: paymentMethod,
        Amount: amount,
        Currency: currency,
        ShippingPostalCode: shippingPostalCode,
        ShippingCountry: shippingCountry
    };

    $.ajax({
        url: '/BookOrder/ConfirmOrder',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(order),
        success: function (response) {
            if (response.success) {
                window.location.href = "/Book";
            } else {
                showToastMessage('Error confirming order. Please try again.', 'danger', 3000);
            }
        },
        error: function (error) {
            console.error('Error Form Submit:', error);
            showToastMessage('Error during form submission. Please try again later.', 'danger', 3000);
        }
    });
}

function loadFormSubmit() {
    

    paymentMethodSelect.addEventListener('change', function () {
        if (paymentMethodSelect.value === "0") {
            emailForm.style.display = 'block';
            otpForm.style.display = 'none';
        } else {
            emailForm.style.display = 'none';
            otpForm.style.display = 'none';
        }
    });
}

function sendOtpToEmail() {
    const email = document.getElementById('otpEmail').value;
    const otpForm = document.getElementById('otpForm');

    if (!email) {
        showToastMessage('Please enter your email address.', 'warning', 3000);
        return;
    }

    $.ajax({
        url: '/Payment/SendOtp',
        type: 'POST',
        data: { email: email },
        success: function (response) {
            otpForm.style.display = 'block';
            showToastMessage('OTP has been sent to your email. Please check your inbox.', 'info', 3000);
        },
        error: function (error) {
            console.error('Error sending OTP:', error);
            showToastMessage('Error while sending OTP. Please try again later.', 'danger', 3000);
        }
    });
}
function completePayment() {
    const otp = otpInput.value;
    const email = emailInput.value;
    var shippingAddress = document.getElementById("shippingAddress").value;
    var shippingCity = parseInt(document.getElementById("shippingCity").value);
    var shippingPostalCode = document.getElementById("shippingPostalCode").value;
    var shippingContact = document.getElementById("contactNumber").value;
    var shippingCountry = document.getElementById("shippingCountry").value;
    var emailAddress = document.getElementById("email").value;
    var currency = parseInt(document.getElementById("currency").value);
    var paymentMethod = parseInt(document.getElementById("paymentMethod").value);
    var amount = parseFloat(document.getElementById("totalAmountAllCutting").textContent);

    var order = {
        ShippingAddress: shippingAddress,
        ShippingCity: shippingCity,
        ShippingContact: shippingContact,
        Email: emailAddress,
        PaymentMethod: paymentMethod,
        Amount: amount,
        Currency: currency,
        ShippingPostalCode: shippingPostalCode,
        ShippingCountry: shippingCountry
    };

    

    if (!otp) {
        showToastMessage('Please enter your OTP.', 'warning', 3000);
        return;
    }

    if (!isVerifiedOtp(email, otp))
    {
        return;
    }
        

            $.ajax({
                url: '/BookOrder/ConfirmOrder',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(order),
                success: function (response) {
                    if (response.success) {
                        window.location.href = "/Book";
                    } else {
                        showToastMessage('Error confirming order. Please try again.', 'danger', 3000);
                    }
                },
                error: function (error) {
                    console.error('Error Form Submit:', error);
                    showToastMessage('Error during form submission. Please try again later.', 'danger', 3000);
                }
            });
}

function isVerifiedOtp(email, otp, callback) {
    $.ajax({
        url: '/Payment/VerifyOtp',
        type: 'POST',
        data: { email: email, otp: otp },
        success: function (response) {
            if (response) {
                callback(true);  
            } else {
                showToastMessage('Invalid OTP. Please try again.', 'danger', 3000);
                callback(false);  
            }
        },
        error: function (error) {
            console.error('Error during OTP verification:', error);
            showToastMessage('Error during OTP verification. Please try again later.', 'danger', 3000);
            callback(false);  
        }
    });
}



window.onload = function () {
    loadProductDetails();
    loadFormSubmit();
};