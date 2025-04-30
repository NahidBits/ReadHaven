// Maps numeric status codes to text
const orderStatusCodeMap = {
    0: "Pending",
    1: "Confirmed",
    2: "Shipped",
    3: "Delivered",
    4: "Cancelled"
};

// Maps status text to badge HTML
const orderStatusBadgeMap = {
    "Pending": '<span class="badge bg-warning text-dark">Pending</span>',
    "Confirmed": '<span class="badge bg-info text-dark">Confirmed</span>',
    "Shipped": '<span class="badge bg-primary">Shipped</span>',
    "Delivered": '<span class="badge bg-success">Delivered</span>',
    "Cancelled": '<span class="badge bg-danger">Cancelled</span>'
};

const sidebar = document.getElementById('sidebar');
const mainContent = document.querySelector('.main-content');
const sidebarToggle = document.getElementById('sidebarToggle');
const userOrder = document.getElementById('userOrder');
const myOrder = document.getElementById('myOrder');
const bookSales = document.getElementById('bookSales');
let sidebarOpen = false;

function loadProfileData() {
    $.get("/Profile/GetUserProfile", function (data) {
        $("#profileName").text(data.name);
        $("#profileEmail").text(data.email);
    }).fail(function (error) {
        console.error("Error loading profile data:", error);
    });
}

function loadMyOrderData() {
    $.get("/BookOrder/GetMyOrders", function (orders) {
        let rowsHtml = '';

        if (orders?.length > 0) {
            rowsHtml = orders.map(order => {
                const statusText = orderStatusCodeMap[order.status];
                const statusBadge = orderStatusBadgeMap[statusText];
                return `
                    <tr>
                        <td>#${order.id.substring(0, 8).toUpperCase()}</td>
                        <td>${formatDate(order.orderDate)}</td>
                        <td>$${order.totalAmount.toFixed(2)}</td>
                        <td>${statusBadge}</td>
                    </tr>`;
            }).join('');
        } else {
            rowsHtml = `<tr><td colspan="4" class="text-center">No orders found.</td></tr>`;
        }

        $('#myOrderTableBody').html(rowsHtml);
    }).fail(function () {
        $('#myOrderTableBody').html(`<tr><td colspan="4" class="text-center text-danger">Failed to load orders.</td></tr>`);
    });
}

function loadUserOrderData() {
    $.get("/BookOrder/GetAllUserOrders", function (orders) {
        let rowsHtml = '';

        if (orders?.length > 0) {
            rowsHtml = orders.map(order => {
                const amount = parseFloat(order.totalAmount);
                const status = parseInt(order.status);
                const statusText = orderStatusCodeMap[status];

                return `
                    <tr>
                        <td>#ORD-${order.id.slice(-5).toUpperCase()}</td>
                        <td>${formatDate(order.orderDate)}</td>
                        <td><strong>$${amount.toFixed(2)}</strong></td>
                        <td>
                            <div class="dropdown">
                                <button class="btn btn-secondary dropdown-toggle" type="button" id="dropdownMenuButton-${order.id}" data-bs-toggle="dropdown" aria-expanded="false">
                                    ${statusText}
                                </button>
                                <ul class="dropdown-menu" aria-labelledby="dropdownMenuButton-${order.id}">
                                    ${Object.entries(orderStatusCodeMap).map(([val, label]) => `
                                        <li><a class="dropdown-item ${status === parseInt(val) ? 'active' : ''}" href="#" onclick="changeOrderStatus('${order.id}', ${val}, event)">${label}</a></li>
                                    `).join('')}
                                </ul>
                            </div>
                        </td>
                    </tr>`;
            }).join('');
        } else {
            rowsHtml = `<tr><td colspan="4" class="text-center">No user orders found.</td></tr>`;
        }

        $('#userOrderTableBody').html(rowsHtml);
    }).fail(function () {
        $('#userOrderTableBody').html(`<tr><td colspan="4" class="text-center text-danger">Failed to load user orders.</td></tr>`);
    });
}

function loadBookSalesData() {
    $.get('/GetBookSales', function (data) {
        const tbody = $('#salesTableBody');
        tbody.empty(); // Clear old data before appending
        if (data?.length > 0) {
            data.forEach(book => {
                tbody.append(`
                    <tr>
                        <td><img src="${book.imageUrl}" alt="Book Image" style="width: 50px; height: auto;" /></td>
                        <td>${book.title}</td>
                        <td>${book.quantitySold}</td>
                    </tr>
                `);
            });
        } else {
            tbody.append(`<tr><td colspan="3" class="text-center">No sales data found.</td></tr>`);
        }
    }).fail(function () {
        $('#salesTableBody').html(`<tr><td colspan="3" class="text-center text-danger">Failed to load sales data.</td></tr>`);
    });
}

function changeOrderStatus(orderId, newStatus, event) {
    event.preventDefault();

    $.post('/BookOrder/ChangeOrderStatus', { orderId, status: newStatus }, function () {
        const statusText = orderStatusCodeMap[newStatus];
        const button = $(`#dropdownMenuButton-${orderId}`);

        button.text(statusText);
        button.siblings('.dropdown-menu').find('.dropdown-item').removeClass('active');
        $(event.target).addClass('active');

        showToastMessage("Order status updated successfully!");
    }).fail(function () {
        showToastMessage("Failed to update order status.", "danger");
    });
}
function loadProfileSection() {
    $.get("/Profile/LoadProfileSection", function (data) {
        $("#mainContentArea").html(data); 

        loadProfileData();
    }).fail(function () {
        $("#mainContentArea").html('<div class="alert alert-danger">Failed to load profile section.</div>');
    });
}
function loadMyOrderSection() {
    $.get("/Profile/LoadMyOrderSection", function (data) {
        $("#mainContentArea").html(data);

        loadMyOrderData();
    }).fail(function () {
        $("#mainContentArea").html('<div class="alert alert-danger">Failed to load my order section.</div>');
    });
}
function loadUserOrderSection() {
    $.get("/Profile/LoadUserOrderSection", function (data) {
        $("#mainContentArea").html(data);

        loadUserOrderData();
    }).fail(function () {
        $("#mainContentArea").html('<div class="alert alert-danger">Failed to load user order section.</div>');
    });
}
function loadBookSalesSection() {
    $.get("/Profile/LoadBookSalesSection", function (data) {
        $("#mainContentArea").html(data);

        loadBookSalesData();
    }).fail(function () {
        $("#mainContentArea").html('<div class="alert alert-danger">Failed to load book sales section.</div>');
    });
}

// Sidebar toggle logic
sidebarToggle.addEventListener('click', () => {
    if (sidebarOpen) {
        sidebar.style.transform = 'translateX(-250px)';
        mainContent.style.marginLeft = '0';
        sidebarToggle.style.left = '10px';
    } else {
        sidebar.style.transform = 'translateX(0)';
        mainContent.style.marginLeft = '250px';
        sidebarToggle.style.left = '260px';
    }
    sidebarOpen = !sidebarOpen;
});


// When page loads
window.onload = function () {
    updateCartCountBadge();
    loadMyOrderSection();
};
