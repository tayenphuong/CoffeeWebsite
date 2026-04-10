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
function updateQuantity(drinkId, size, change) {
    // 1. Lấy ô input dựa trên ID và Size
    var input = $(`#qty-${drinkId}-${size}`);
    var currentQty = parseInt(input.val());
    var newQty = currentQty + change;

    // 2. Không cho giảm xuống dưới 1
    if (newQty < 1) return;

    $.ajax({
        url: '/Cart/UpdateCart',
        type: 'POST',
        data: {
            drinkId: drinkId,
            size: size,
            quantity: newQty // Gửi số lượng MỚI thực tế
        },
        success: function (response) {
            if (response.success) {
                // 3. Cập nhật số lượng vào ô input
                input.val(newQty);

                // 4. Cập nhật các con số khác trên giao diện (Ajax mượt mà)
                $(`#subtotal-${drinkId}-${size}`).text(response.itemSubtotal.toLocaleString() + 'đ');
                $('#cart-subtotal').text(response.newTotal.toLocaleString() + 'đ');
                $('#cart-discount').text('-' + response.discountAmount.toLocaleString() + 'đ');
                $('#cart-total').text(response.finalTotal.toLocaleString() + 'đ');
                $('.cart-count').text(response.cartCount + ' items');
            } else {
                alert(response.message);
            }
        },
        error: function () {
            alert("Lỗi kết nối server");
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



