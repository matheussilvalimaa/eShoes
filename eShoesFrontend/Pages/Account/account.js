const menuButton = document.querySelector(".menu-button");
const navbar = document.querySelector(".navbar");
menuButton.addEventListener('click', () => {
  navbar.classList.toggle("show-menu");
});

document.addEventListener('DOMContentLoaded', async () => {
  const response = await fetch('https://localhost:5032/api/user/me', {
      credentials: 'include'
  });

  if (!response.ok) {
    window.location.href = '/Pages/Account/Login/login.html';
  }

  //Enable or disable inputs
  function disableProfileInputs(disabled) {
    [usernameInput, emailInput, passwordInput].forEach(input => input.disabled = disabled);
    const style = disabled ? 'grayscale(1)' : 'none';
    [usernameInput, emailInput, passwordInput].forEach(input => input.style.filter = style);
  }

  /***ACCOUNT SECTION ***/
  const usernameInput = document.getElementById('username');
  const emailInput = document.getElementById('email');
  const passwordInput = document.getElementById('password');
  const editBtn = document.querySelector('.btn-change');
  const logoutBtn = document.querySelector('.btn-logout');
  let isEditing = false;

  //Edit and Save buttons
  editBtn.addEventListener('click', async () => {
    if (!isEditing) {
      isEditing = true;
      editBtn.textContent = 'Save';
      disableProfileInputs(false);
      passwordInput.placeholder = 'New Password';
    } else {
      // Save profile changes
      const payload = {
        username: usernameInput.value,
        email: emailInput.value,
        password: passwordInput.value
      };
      try {
        const res = await fetch('https://localhost:5032/api/user/profile', {
          method: 'PUT',
          credentials: 'include',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(payload)
        });
        if (res.ok) {
          isEditing = false;
          editBtn.textContent = 'Edit';
          disableProfileInputs(true);
          passwordInput.value = '';
        } else {
          const err = await res.json();
          alert('Error saving profile: ' + err.message || err);
        }
      } catch (err) {
        alert('Network error while saving profile');
      }
    }
  });

  //Logout button
  logoutBtn.addEventListener('click', async e => {
    e.preventDefault();
    await fetch('https://localhost:5032/api/user/logout', { method: 'POST', credentials: 'include' });
    window.location.href = '/Pages/Home/index.html';
  });

  //Fetch and fill the profile form
  async function loadProfile() {
    try {
      const res = await fetch('https://localhost:5032/api/user/profile', {
        credentials: 'include'
      });
      if (res.ok) {
        const profile = await res.json();
        usernameInput.value = profile.username;
        emailInput.value = profile.email;
        passwordInput.value = '**********';
        disableProfileInputs(true);
      }
    } catch (err) {
      console.error('Profile load error', err);
    }
  }
  await loadProfile();

  const ordersContainer = document.getElementById('orders-list');

  async function loadOrders() {
    try {
      const res = await fetch('https://localhost:5032/api/orders', {
        credentials: 'include'
      });
      if (!res.ok) throw new Error('Not possible to load the orders');
      const orders = await res.json();
      console.log(orders);

      if (orders.length === 0) {
        ordersContainer.innerHTML = '<p>You had not made a order yet.</p>';
        return;
      }

      ordersContainer.innerHTML = orders.map(renderOrderCard).join('');
    } catch (err) {
      ordersContainer.innerHTML = `<p class="error">${err.message}</p>`;
    }
  }

  function renderOrderCard(order) {
    const steps = [
      { key: 'Received', label: 'Order Received' },
      { key: 'Paid',     label: 'Order Confirmed' },
      { key: 'Shipped',  label: 'Order Shipped' },
      { key: 'Delivered',label: 'Order Delivered' }
    ];
   
    const statusIndex = {
      Pending: 0,
      Paid:    1,
      Shipped: 2,
      Delivered: 3
    }[order.status] ?? 0;

    
    const itemsHtml = order.items.map(i => `
      <div class="order-item">
        <div class="name">${i.productName}</div>
        <div class="qty">x${i.quantity}</div>
        <div class="price">R$${i.price.toFixed(2)}</div>
      </div>
    `).join('');

    
    let pmHtml = '';
    if (order.paymentMethodType === 'card') {
      pmHtml = `<img src="/Pages/images/card.svg" alt="Cartão"> Cartão •••• ${order.last4}`;
    } else if (order.PaymentMethodType === 'pix') {
      pmHtml = `<img src="/Pages/images/pix.svg" alt="PIX"> PIX`;
    } else if (order.PaymentMethodType === 'boleto') {
      pmHtml = `<img src="/Pages/images/bankslip.svg" alt="Boleto"> Boleto`;
    }

    // monta o timeline
    const timelineHtml = steps.map((step, idx) => `
      <div class="timeline-step ${idx <= statusIndex ? 'completed' : ''}">
        <div class="dot"></div>
        <div class="label">${step.label}</div>
      </div>
    `).join('');

    return `
    <div class="order-card">
      <div class="order-header">
        <div class="order-status">${order.status}</div>
        <div class="order-date">${new Date(order.orderDate).toLocaleDateString()}</div>
      </div>
      <div class="payment-method">${pmHtml}</div>
      <div class="order-items">${itemsHtml}</div>
      <div class="timeline">${timelineHtml}</div>
    </div>
    `;
  }

  loadOrders();
});

