const orderStatusMap = {
    0: "Pending",
    1: "Processing",
    2: "Completed",
    3: "Cancelled"
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
                    rowsHtml += `
                        <tr>
                            <td>${new Date(order.orderDate).toLocaleString()}</td>
                            <td>$${order.totalAmount.toFixed(2)}</td>
                            <td>${orderStatusMap[order.status]}</td>
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

                    rowsHtml += `
                        <tr>
                            <td>${new Date(order.orderDate).toLocaleString()}</td>
                            <td>$${amount.toFixed(2)}</td>
                            <td>
                                <select class="form-select form-select-sm" onchange="changeOrderStatus('${order.id}', this.value)">
                                    ${Object.entries(orderStatusMap).map(([val, label]) => `
                                        <option value="${val}" ${status === parseInt(val) ? 'selected' : ''}>${label}</option>
                                    `).join('')}
                                </select>
                            </td>
                        </tr>
                    `;
                });
            } else {
                rowsHtml = `
                    <tr>
                        <td colspan="4" class="text-center">No user orders found.</td>
                    </tr>
                `;
            }
            $('#userOrderTableBody').html(rowsHtml);
        },
        error: function () {
            $('#userOrderTableBody').html(`
                <tr>
                    <td colspan="4" class="text-center text-danger">Failed to load user orders.</td>
                </tr>
            `);
        }
    });
}
function changeOrderStatus(orderId, newStatus) {
    $.ajax({
        url: '/BookOrder/ChangeOrderStatus',
        type: 'POST',
        data: {
            orderId: orderId,
            status: newStatus
        },
        success: function () {
            showToastMessage("Order status updated successfully!");
        },
        error: function () {
            showToastMessage("Failed to update order status.", "danger");
        }
    });
}


window.onload = function () {
    loadProfileData();
    loadMyOrderData();

    if (userRole === "Admin") {
        loadUserOrderData();
    }
};
