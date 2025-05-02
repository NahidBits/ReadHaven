const paymentMethodSelect = document.getElementById('paymentMethod');
const emailForm = document.getElementById('emailForm');
const otpForm = document.getElementById('otpForm');
const sendOtpButton = document.getElementById('sendOtp');
const emailInput = document.getElementById('otpEmail');
const otpInput = document.getElementById('otp');
const personalInfoForm = document.getElementById('personalInfoForm');
const paymentInfoForm = document.getElementById('paymentInfoForm');
const shippingAddress = document.getElementById("shippingAddress");
const shippingCity = document.getElementById("shippingCity");
const shippingPostalCode = document.getElementById("shippingPostalCode");
const shippingContact = document.getElementById("contactNumber");
const shippingCountry = document.getElementById("shippingCountry");
const email = document.getElementById("email");
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

function submitShippingAddress() {
    

    var order = {
        ShippingAddress: shippingAddress.value,
        ShippingCity: parseInt(shippingCity.value),
        ShippingContact: shippingContact.value,
        Email: email.value,
        ShippingPostalCode: shippingPostalCode.value,
        ShippingCountry: parseInt(shippingCountry.value)
    };

    $.ajax({
        url: '/BookOrder/ConfirmOrder',
        type: 'POST',
        contentType: 'application/json', 
        data: JSON.stringify(order), 
        success: function (response) {
            if (response.success) {
                personalInfoForm.style.display = 'none';
                paymentInfoForm.style.display = 'block';
                document.getElementById("orderId").value = response.orderId;
                showToastMessage('Shipping address Submitted.', 'success', 3000);

            } else {
                showToastMessage(response.message, 'danger', 3000);
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
function submitPayment() {
    var otp = otpInput.value;
    var email = emailInput.value;
    var orderId = document.getElementById("orderId").value;  
    var currency = parseInt(document.getElementById("currency").value);
    var paymentMethod = parseInt(document.getElementById("paymentMethod").value);
    var amount = parseFloat(document.getElementById("totalAmountAllCutting").textContent);

    if (!otp) {
        showToastMessage('Please enter your OTP.', 'warning', 3000);
        return Promise.resolve(false);
    }

    var payment = {
        Otp: otp,
        Email: email,
        OrderId: orderId,
        Email: email,
        Currency: currency,
        PaymentMethod: paymentMethod,
        Amount: amount
    };

    $.ajax({
        url: '/Payment/VerifyOtp',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payment),
        success: function (response) {
            if (response) {
                showToastMessage('OTP verified successfully and Payment is Successed!', 'success', 20000);
                setTimeout(function () {
                    window.location.assign("/Book");
                }, 3000);
            } else {
                showToastMessage(response.message || 'OTP verification failed.', 'danger', 3000);
            }
        },
        error: function (error) {
            console.error('Error during OTP verification:', error);
            showToastMessage('Error during OTP verification. Please try again later.', 'danger', 3000);
        }
    });
}

window.onload = function () {
    loadProductDetails();
    loadFormSubmit();
    paymentInfoForm.style.display = 'none';
};