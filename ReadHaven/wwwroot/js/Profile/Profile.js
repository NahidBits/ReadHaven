
function loadProfileData() {
    $.ajax({
        url: "/Profile/GetProfileData",
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
        url: "/BookOrder/GetUserOrders",
        type: "GET",
        success: function (orders) {
            let rowsHtml = '';
            if (orders && orders.length > 0) {
                orders.forEach(order => {
                    rowsHtml += `
                        <tr>
                            <td>$${order.totalAmount}</td>
                            <td>${order.status}</td>
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

loadUserOrderData = function () {
   
}


window.onload = function () {
    checkRoleStatus();
    loadProfileData();
    loadMyOrderData();  

    if (userRole === "Admin") {
        loadUserOrderData();
    }
};
