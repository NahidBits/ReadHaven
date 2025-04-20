function loadOrderData() {
    $.ajax({
        url: '/BookOrder/GetUserOrder',
        type: 'GET',
        success: function (data) {
            if (data) {
                $('#totalAmount').text(`$${data.totalAmount}`);
                $('#status').text(data.status);
                $('#orderDate').text(new Date(data.orderDate).toLocaleString());

            } else {
                console.error('Error fetching Order data:', error);
            }
        }
    });
}

// On page load, fetch and display the Order data
window.onload = function () {
    loadOrderData();
};
