function loadOrderData() {
    $.ajax({
        url: '/BookOrder/GetMyOrdersPending',
        type: 'GET',
        success: function (orders) {
            let rowsHtml = '';

            if (orders && orders.length > 0) {
                orders.forEach(order => {
                    rowsHtml += `
                        <tr>
                            <td>$${order.totalAmount}</td>
                            <td>${new Date(order.orderDate).toLocaleString()}</td>
                            <td>
                                <button class="btn btn-success btn-sm" onclick="completeOrder('${order.id}')">Done</button>
                            </td>
                        </tr>
                    `;
                });
            } else {
                rowsHtml = `
                    <tr>
                        <td colspan="4" class="text-center">No orders found.</td>
                    </tr>
                `;
            }

            $('#orderTableBody').html(rowsHtml);
        },
        error: function () {
            $('#orderTableBody').html(`
                <tr>
                    <td colspan="4" class="text-center text-danger">Failed to load orders.</td>
                </tr>
            `);
        }
    });
}
function completeOrder(orderId) {
    if (!confirm("Are you sure you want to mark this order as done?")) return;

    $.ajax({
        url: '/BookOrder/ConfirmOrder',
        type: 'POST',
        data: { orderId: orderId },
        success: function () {
            showToastMessage("Order marked as done successfully!", "success");
            loadOrderData();
        },
        error: function () {
            showToastMessage("Failed to complete the order.", "danger");
        }
    });
}

window.onload = function () {
    loadOrderData();
};
