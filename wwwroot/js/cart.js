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
function updateQuantity(drinkId, quantity) {
    if (quantity < 1) {
        if (!confirm('Remove this item from cart?')) {
            return;
        }
    }

    $.ajax({
        url: '/Cart/UpdateCart',
        type: 'POST',
        data: { drinkId: drinkId, quantity: quantity },
        success: function (response) {
            if (response.success) {
                if (quantity < 1) {
                    location.reload();
                } else {
                    // 1. Cập nhật số lượng và subtotal của dòng đó
                    $('#qty-' + drinkId).val(quantity);
                    $('#subtotal-' + drinkId).text(formatCurrency(response.itemSubtotal));

                    // 2. Cập nhật Subtotal tổng (Chưa giảm)
                    $('#cart-subtotal').text(formatCurrency(response.newTotal));

                    // 3. CẬP NHẬT GIẢM GIÁ (Strategy)
                    if (response.discountAmount > 0) {
                        $('#discount-row').show();
                        $('#cart-discount').text('-' + formatCurrency(response.discountAmount));
                    } else {
                        $('#discount-row').hide();
                    }

                    // 4. Cập nhật Tổng cuối (FinalTotal + Shipping)
                    var shipping = 20000;
                    var finalWithShipping = response.finalTotal + shipping;
                    $('#cart-total').text(formatCurrency(finalWithShipping));

                    updateCartBadge(response.cartCount);
                }
            }
        },
        error: function () {
            alert('Error updating cart');
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



