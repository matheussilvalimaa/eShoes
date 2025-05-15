let cartItems = [];

const accountLink = document.getElementById('account-link');
accountLink.addEventListener('click', async () => {
    const response = await fetch('https://localhost:5032/api/user/me', {
        credentials: 'include'
    });
  
    if (response.ok) {
      window.location.href = '/Pages/Account/account.html';
    } else {
        window.location.href = '/Pages/Account/Login/login.html'
    }
});

//Function to load the cart
async function loadCart() {
    try {
        const response = await fetch('https://localhost:5032/api/cart/items', { credentials: 'include' });
        const data = await response.json();
        cartItems = data.items;
        renderCart();
        updateTotals();
    } catch (error) {
        console.error('Error loading cart:', error);
    }
}

//Function to render the cart in UI
function renderCart() {
    const container = document.getElementById('items');

    if (cartItems.length === 0 ) {
        container.innerHTML = `<p class="empty-message">Add something to the cart</p>`;
        return;
    }
    
    container.innerHTML = cartItems.map(item => {
        let imageSrc = item.image;
        if (imageSrc.startsWith('http://')) {
            imageSrc = imageSrc.replace('http://', 'https://');
        } else if (!imageSrc.startsWith('http') && !imageSrc.startsWith('/')) {
            imageSrc = `https://localhost:5032/${imageSrc}`;
        } else if (!imageSrc.startsWith('http')) {
            imageSrc = `https://localhost:5032${imageSrc}`;
        }

        return `
        <article class="cart-item" data-id="${item.id}">
            <img src="${imageSrc}" alt="${item.name}" class="item-image" 
                 onerror="this.src='https://localhost:5032/images/default-product.png'">
            <div class="item-details">
                <h3>${item.name}</h3>
                <p>Tamanho: ${item.size}</p>
                <div class="quantity-control">
                    <button class="quantity-btn" onclick="updateQuantity(${item.id}, -1)">-</button>
                    <span>${item.quantity}</span>
                    <button class="quantity-btn" onclick="updateQuantity(${item.id}, 1)">+</button>
                </div>
            </div>
            <div class="item-price">
                <p>R$ ${item.price.toFixed(2)}</p>
                <button class="remove-btn" onclick="removeItem(${item.id})">Remover</button>
            </div>
        </article>`;
    }).join('');
}

//Function to update the quantity of the cart items
async function updateQuantity(itemId, delta) {
    try {
        await fetch(`https://localhost:5032/api/cart/${itemId}`, {
            method: 'PATCH',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ Delta: delta })
            
        });
        loadCart();
    } catch (error) {
        console.error('Error updating quantity:', error);
    }
}

//Function to remove the item
async function removeItem(itemId) {
    try {
        await fetch(`https://localhost:5032/api/cart/${itemId}`, { credentials: 'include', method: 'DELETE' });
        loadCart();
    } catch (error) {
        console.error('Error removing item:', error);
    }
}

//Function to update the price
function updateTotals() {
    const subtotal = cartItems.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    document.getElementById('total').textContent = `R$ ${subtotal.toFixed(2)}`;
}

document.getElementById('btn-continue').addEventListener('click', () => {
    window.location.href = '/Pages/Cart/order.html';
});

loadCart();