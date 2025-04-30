// Function to fetch cart data and update the page
function loadCartData() {
    $.ajax({
        url: '/BookCart/GetCartItems',
        type: 'GET',
        success: function (data) {

            if (data == null) {
                $('#cartTable').hide();
                $('#cartEmptyMessage').show();
                $('#totalSum').text('$0.00');
                return;
            }
            const cartItems = data?.items || data;
            const totalAmount = data?.totalAmount || 0;

            if (!Array.isArray(cartItems) || cartItems.length === 0) {
                $('#cartTable').hide();
                $('#cartEmptyMessage').show();
                $('#totalSum').text('$0.00');
                return;
            }

            $('#cartTable').show();
            $('#cartEmptyMessage').hide();
            $('#cartItemsTableBody').empty();

            let total = 0;

            cartItems.forEach(item => {
                const id = item.id;
                const bookId = item.bookId;
                const title = item.bookTitle ?? 'Unknown';
                const quantity = item.quantity ?? 1;
                const price = item.unitPrice ?? 0;
                const itemTotal = price * quantity;
                const imagePath = item.imagePath ?? '/uploads/book/Default_image.webp'; // Default image if none exists
                total += itemTotal;

                const row = `
                    <tr id="cart-item-${id}">
                        <td class="text-center">
                            <img src="${imagePath}" alt="${title}" class="img-fluid" style="max-width: 50px; height: auto;">
                        </td>
                        <td>
                            ${title}
                        </td>
                        <td class="item-price">$${price.toFixed(2)}</td>
                        <td>
                            <button class="btn btn-sm btn-secondary" onclick="changeQuantity('${id}', -1, ${price})">-</button>
                            <span id="quantity-${id}">${quantity}</span>
                            <button class="btn btn-sm btn-secondary" onclick="changeQuantity('${id}', 1, ${price})">+</button>
                        </td>
                        <td>
                            <button class="btn btn-sm btn-danger" onclick="removeItem('${id}')" title="Remove">
                                <i class="bi bi-trash-fill"></i>
                            </button>
                        </td>
                    </tr>
                `;
                $('#cartItemsTableBody').append(row);
            });

            $('#totalSum').text(`$${total.toFixed(2)}`);
        },
        error: function (xhr, status, error) {
            console.error('Error fetching cart data:', error);
            $('#cartTable').hide();
            $('#cartEmptyMessage').show();
            $('#totalSum').text('$0.00');
        }
    });
}



// Function to change quantity of a cart item
function changeQuantity(id, change, price) {
    $.ajax({
        url: '/BookCart/ChangeCartItemQuantity',
        type: 'POST',
        data: {
            id: id,
            quantity: change
        },
        success: function (response) {
            if (response.success) {
                const quantitySpan = $(`#quantity-${id}`);
                let currentQuantity = parseInt(quantitySpan.text());
                const newQuantity = currentQuantity + change;

                if (newQuantity < 1) return;

                quantitySpan.text(newQuantity);
                changeTotalSum(change * price);
            } else {
                alert('Failed to update quantity.');
            }
        },
        error: function (xhr, status, error) {
            console.error('Error changing quantity:', error);
        }
    });
}

// Function to remove an item from the cart
function removeItem(id) {
    $.ajax({
        url: '/BookCart/RemoveCartItem',
        type: 'POST',
        data: { id: id },
        success: function (response) {
            if (response.success) {
                const itemRow = $('#cart-item-' + id);
                const price = parseFloat(itemRow.find('.item-price').text().replace('$', '').trim());
                const quantity = parseInt(itemRow.find('#quantity-' + id).text());
                $('#cart-item-' + id).remove();

                changeTotalSum(- (quantity * price));

                if (response.cartItemCount === 0) {
                    $('#cartTable').hide();
                    $('#cartEmptyMessage').show();
                }
            } else {
                alert('Failed to remove item.');
            }
        },
        error: function (xhr, status, error) {
            console.error('Error removing item:', error);
        }
    });
}

function changeTotalSum(change) {
    let totalSumText = $('#totalSum').text();
    let totalSum = parseFloat(totalSumText.replace('$', '').trim()) || 0;
    totalSum += parseFloat(change);

    $('#totalSum').text(`$${totalSum.toFixed(2)}`);
}

// On page load, fetch and display the cart data
window.onload = function () {
    loadCartData();
};
