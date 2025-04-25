// First map numeric code to status string
const orderStatusCodeMap = {
    0: "Pending",
    1: "Confirmed",
    2: "Shipped",
    3: "Delivered",
    4: "Cancelled"
};

// Then map status string to badge HTML
const orderStatusBadgeMap = {
    "Pending": '<span class="badge bg-warning text-dark">Pending</span>',
    "Confirmed": '<span class="badge bg-info text-dark">Confirmed</span>',
    "Shipped": '<span class="badge bg-primary">Shipped</span>',
    "Delivered": '<span class="badge bg-success">Delivered</span>',
    "Cancelled": '<span class="badge bg-danger">Cancelled</span>'
};

function loadProfileData() {
    $.ajax({
        url: "/Profile/GetUserProfile",
        type: "GET",
        success: function (data) {
            $("#profileName").text(data.name);
            $("#profileEmail").text(data.email);
        },
        error: function (error) {
            console.error("Error loading profile data:", error);
        }
    });
}

function loadMyOrderData() {
    $.ajax({
        url: "/BookOrder/GetMyOrders",
        type: "GET",
        success: function (orders) {
            let rowsHtml = '';
            if (orders && orders.length > 0) {
                orders.forEach(order => {
                    const statusText = orderStatusCodeMap[order.status];
                    const statusBadge = orderStatusBadgeMap[statusText];

                    rowsHtml += `
                        <tr>
                            <td>#${order.id.substring(0, 8).toUpperCase()}</td>
                            <td>${formatDate(order.orderDate)}</td>
                            <td>$${order.totalAmount.toFixed(2)}</td>
                            <td>${statusBadge}</td>
                        </tr>
                    `;
                });
            } else {
                rowsHtml = `
                    <tr>
                        <td colspan="3" class="text-center">No orders found.</td>
                    </tr>
                `;
            }
            $('#myOrderTableBody').html(rowsHtml);
        },
        error: function () {
            $('#myOrderTableBody').html(`
                <tr>
                    <td colspan="3" class="text-center text-danger">Failed to load orders.</td>
                </tr>
            `);
        }
    });
}
function loadUserOrderData() {
    $.ajax({
        url: "/BookOrder/GetAllUserOrders",
        type: "GET",
        success: function (orders) {
            let rowsHtml = '';
            if (orders && orders.length > 0) {
                orders.forEach(order => {
                    const amount = parseFloat(order.totalAmount);
                    const status = parseInt(order.status);
                    const statusText = orderStatusCodeMap[status];

                    rowsHtml += `
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
                <li>
                    <a class="dropdown-item ${status === parseInt(val) ? 'active' : ''}" href="#" onclick="changeOrderStatus('${order.id}', ${val}, event)">
    ${label}
</a>
                </li>
            `).join('')}
        </ul>
    </div>
</td>

                        </tr>
                    `;
                });
            } else {
                rowsHtml = `
                    <tr>
                        <td colspan="5" class="text-center">No user orders found.</td>
                    </tr>
                `;
            }
            $('#userOrderTableBody').html(rowsHtml);
        },
        error: function () {
            $('#userOrderTableBody').html(`
                <tr>
                    <td colspan="5" class="text-center text-danger">Failed to load user orders. Please try again later.</td>
                </tr>
            `);
        }
    });
}
function loadBookSalesData()
{
    $.get('/GetBookSales', function (data) {
        const tbody = $('#salesTableBody');
        data.forEach(book => {
            tbody.append(`
                <tr>
                     <td>
                    <img src=${book.imageUrl} alt="Book Image" style="width: 50px; height: auto;" />
                    </td>
                    <td>${book.title}</td>
                    <td class="text-center">${book.quantitySold}</td>
                </tr>
            `);
        });
    });
}
function changeOrderStatus(orderId, newStatus, event) {
    // Prevent the dropdown from closing immediately
    event.preventDefault();

    $.ajax({
        url: '/BookOrder/ChangeOrderStatus',
        type: 'POST',
        data: {
            orderId: orderId,
            status: newStatus
        },
        success: function () {
            const statusText = orderStatusCodeMap[newStatus];

            // Update button text
            $(`#dropdownMenuButton-${orderId}`).text(statusText);

            // Remove 'active' from all dropdown items of this order
            $(`#dropdownMenuButton-${orderId}`).siblings('.dropdown-menu').find('.dropdown-item').removeClass('active');

            // Add 'active' to the selected one
            $(event.target).addClass('active');

            showToastMessage("Order status updated successfully!");
        },
        error: function () {
            // Show error toast
            showToastMessage("Failed to update order status.", "danger");
        }
    });
}



window.onload = function () {
    loadProfileData();
    loadMyOrderData();
    loadBookSalesData();

    if (userRole === "Admin") {
        loadUserOrderData();
    }
};
