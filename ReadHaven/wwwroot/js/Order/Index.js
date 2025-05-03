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
const previousAddress = document.getElementById("usePreviousAddress");

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
    
    const orderId = localStorage.getItem("orderId");
    var order = {
        ShippingAddress: shippingAddress.value,
        ShippingCity: parseInt(shippingCity.value),
        ShippingContact: shippingContact.value,
        ShippingPostalCode: shippingPostalCode.value,
        ShippingCountry: parseInt(shippingCountry.value)
    };

    if (orderId) {
        order.Id = orderId;
    }

    $.ajax({
        url: '/BookOrder/ConfirmOrder',
        type: 'POST',
        contentType: 'application/json', 
        data: JSON.stringify(order), 
        success: function (response) {
            if (response.success) {
                usePreviousAddress.style.display = 'none';
                personalInfoForm.style.display = 'none';
                paymentInfoForm.style.display = 'block';
                localStorage.setItem("orderId", response.orderId);
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

function sendWithPreviousAddress()
{
    const orderId = localStorage.getItem("orderId");

    $.ajax({
        url: '/BookOrder/OrderWithPreviousAddress',
        type: 'POST',
        contentType: 'application/json',
        data: { orderId : orderId },
        success: function (response) {
            if (response.success) {
                usePreviousAddress.style.display = 'none';
                personalInfoForm.style.display = 'none';
                paymentInfoForm.style.display = 'block';
                localStorage.setItem("orderId", response.orderId);
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
    var orderId = localStorage.getItem("orderId") || 0; 
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
                localStorage.removeItem("orderId");
                showToastMessage('OTP verified successfully and Payment is Successed!', 'success', 20000);
                const leftContent = document.getElementById("leftContent");
                if (leftContent) {
                    leftContent.innerHTML = `
                        <div class="card border-0 shadow mb-4 text-center p-5">
                            <i class="fas fa-check-circle fa-3x text-success mb-3"></i>
                            <h3 class="fw-bold text-success">Thank You for Your Purchase!</h3>
                            <p class="mt-3">Your order has been confirmed and will be processed shortly.</p>
                            <p class="text-muted">A receipt has been sent to <strong>${email}</strong>.</p>
                            <a href="/Book" class="btn btn-outline-primary mt-4">
                                Browse More Books <i class="fas fa-book ms-2"></i>
                            </a>
                        </div>
                    `;
                }

                // Optional: disable or blur the summary panel
                const summaryPanel = document.querySelector(".col-lg-4 .card");
                if (summaryPanel) {
                    summaryPanel.classList.add("opacity-50");
                    summaryPanel.style.pointerEvents = "none";
                }

                $.ajax({
                    url: '/Payment/SendOrderSummary',
                    type: 'POST',
                    data: { orderId: orderId}
                });
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