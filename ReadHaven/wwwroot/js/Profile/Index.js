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

                // Define meaningful icons and small text next to them based on status
                let statusIcon = '';
                if (statusText === 'Confirmed') {
                    statusIcon = `<i class="bi bi-check-circle text-success" title="Confirmed"></i> <span class="ms-2">Confirmed</span>`;
                } else if (statusText === 'Pending') {
                    statusIcon = `<i class="bi bi-clock text-warning" title="Pending"></i> <span class="ms-2">Pending</span>`;
                } else if (statusText === 'Shipped') {
                    statusIcon = `<i class="bi bi-truck text-primary" title="Shipped"></i> <span class="ms-2">Shipped</span>`;
                } else if (statusText === 'Delivered') {
                    statusIcon = `<i class="bi bi-check2-circle text-info" title="Delivered"></i> <span class="ms-2">Delivered</span>`;
                } else if (statusText === 'Cancelled') {
                    statusIcon = `<i class="bi bi-x-circle text-danger" title="Cancelled"></i> <span class="ms-2">Cancelled</span>`;
                } else {
                    statusIcon = `<i class="bi bi-question-circle text-muted" title="Unknown"></i> <span class="ms-2">Unknown</span>`;
                }

                return `
                    <tr>
                        <td>#${order.id.substring(0, 8).toUpperCase()}</td>
                        <td>${formatDate(order.orderDate)}</td>
                        <td>$${order.totalAmount.toFixed(2)}</td>
                        <td class="text-center">${statusIcon}</td>
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
        tbody.empty(); // Clear previous data

        if (data?.length > 0) {
            data.forEach(book => {
                const imageUrl = book.imageUrl || '/images/default-book.png'; // Fallback image
                const title = book.title || 'Untitled';
                const quantity = book.quantitySold ?? 0;

                tbody.append(`
                    <tr>
                        <td>
                            <img src="${imageUrl}" alt="Book Image"
                                 class="rounded shadow-sm"
                                 style="width: 45px; height: 60px; object-fit: cover;" />
                        </td>
                        <td class="fw-semibold text-start">${title}</td>
                        <td>
                            <span class="badge bg-success-subtle text-success fw-semibold px-2 py-1">
                                ${quantity}
                            </span>
                        </td>
                    </tr>
                `);
            });
        } else {
            tbody.append(`
                <tr>
                    <td colspan="3" class="text-center text-muted">No sales data found.</td>
                </tr>
            `);
        }
    }).fail(function () {
        $('#salesTableBody').html(`
            <tr>
                <td colspan="3" class="text-center text-danger">❌ Failed to load sales data.</td>
            </tr>
        `);
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
        mainContent.style.marginLeft = '140px';
        sidebarToggle.style.left = '210px';
    }
    sidebarOpen = !sidebarOpen;
});


// When page loads
window.onload = function () {
    updateCartCountBadge();
    loadMyOrderSection();
    localStorage.setItem("returnUrl", window.location.href);
};
