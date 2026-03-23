function addToCart(drinkId, quantity = 1) {
    $.ajax({
        url: '/Cart/AddToCart',
        type: 'POST',
        data: { drinkId: drinkId, quantity: quantity },
        success: function (response) {
            if (response.success) {
                updateCartBadge(response.cartCount);
                
                showNotification('Added to cart successfully!', 'success');
            } else {
                showNotification(response.message || 'Error adding to cart', 'error');
            }
        },
        error: function () {
            showNotification('Error adding to cart', 'error');
        }
    });
}
function updateQuantity(drinkId, size, newQty) {
    if (newQty < 1) return;

    $.ajax({
        url: '/Cart/UpdateCart',
        type: 'POST',
        data: {
            drinkId: drinkId,
            size: size, // Đảm bảo có size gửi lên
            quantity: newQty
        },
        success: function (response) {
            if (response.success) {
                // Cập nhật đúng ô input dựa trên ID kết hợp Size
                $(`#qty-${drinkId}-${size}`).val(newQty);

                // Cập nhật đúng ô Subtotal dựa trên ID kết hợp Size
                $(`#subtotal-${drinkId}-${size}`).text(response.itemSubtotal.toLocaleString() + 'đ');

                // Cập nhật tổng quát giỏ hàng
                $('#cart-subtotal').text(response.newTotal.toLocaleString() + 'đ');
                $('#cart-discount').text('-' + response.discountAmount.toLocaleString() + 'đ');

                // Tính lại Total cuối cùng (bao gồm phí ship 20k)
                var finalWithShip = response.finalTotal + 20000;
                $('#cart-total').text(finalWithShip.toLocaleString() + 'đ');

                // Cập nhật badge giỏ hàng trên Header (nếu có)
                $('.cart-count').text(response.cartCount + ' items');
            }
        }
    });
}

// Hàm format tiền tiện ích nếu bạn chưa có
function formatCurrency(value) {
    return value.toLocaleString('vi-VN') + 'đ';
}

function updateCartBadge(count) {
    $('#cart-count').text(count);
    if (count > 0) {
        $('#cart-count').show();
    } else {
        $('#cart-count').hide();
    }
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN').format(amount) + 'đ';
}

function showNotification(message, type = 'success') {
    const icon = type === 'success' ? '✓' : '✗';
    const alertClass = type === 'success' ? 'alert-success' : 'alert-danger';
    
    const notification = `
        <div class="alert ${alertClass} alert-dismissible fade show position-fixed" 
             style="top: 80px; right: 20px; z-index: 9999; min-width: 300px;" role="alert">
            <strong>${icon}</strong> ${message}
            <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                <span aria-hidden="true">&times;</span>
            </button>
        </div>
    `;
    
    $('body').append(notification);
    
    setTimeout(function() {
        $('.alert').fadeOut('slow', function() {
            $(this).remove();
        });
    }, 3000);
}

$(document).ready(function() {
    $.get('/Cart/GetCartCount', function(data) {
        updateCartBadge(data.count);
    });
});



