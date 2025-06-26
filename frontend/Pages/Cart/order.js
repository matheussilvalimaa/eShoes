const stripeKey = "pk_test_51QfRWZKdeDlDaNy8mbIdylH19E3oLhAQkPmz9tKiEaAVQoMyBsQkm0eD1kHe6YKonulgRfVb58GJgHW1NFukmD7B00d3FCVSRy";

document.addEventListener('DOMContentLoaded', () => {
  // Sections & radios
  const sections = {
    address: document.querySelector('.address-box'),
    payment: document.querySelector('.payment-box'),
    confirmation: document.querySelector('.confirmation-box')
  };
  const radios = {
    address: document.getElementById('address'),
    payment: document.getElementById('payment'),
    confirmation: document.getElementById('confirmation'),
  };
  function showSection() {
    Object.keys(sections).forEach(k => {
      if (radios[k].checked) {
        sections[k].classList.add('active');
      } else {
        sections[k].classList.remove('active');
      }
    });
    document.querySelector('.add-card-form').classList.remove('active');
  }
  Object.values(radios).forEach(r => r.addEventListener('change', showSection));
  showSection();

  // Stripe Elements
  const stripe = Stripe(stripeKey);
  const elements = stripe.elements();
  let stripePaymentMethodId = null;
  const cardElement = elements.create('card', {
    hidePostalCode: true
  });

  // State
  let selectedPaymentType = null;
  let shippingCost = 0;
  let subtotal = 0;

  // Summary containers
  const itemListEl       = document.querySelector('.item-list');
  const paymentChoiceEl  = document.getElementById('payment-choice');
  const addressChoiceEl  = document.getElementById('address-choice');
  const shippingCostEl   = document.getElementById('shipping-cost');
  const totalCostEl      = document.getElementById('total-cost');

  //Address Form
  const pcodeEl = document.getElementById('pcode');
  const streetEl = document.getElementById('street');
  const numberEl = document.getElementById('house-number');
  const cityEl = document.getElementById('city');
  const stateEl = document.getElementById('state');

  //ViaCep autofill
  pcodeEl.addEventListener('blur', async () => {
    const c = pcodeEl.value.replace(/\D/g, '');
    if (c.length === 8) {
      const r = await fetch(`https://viacep.com.br/ws/${c}/json/`);
      const d = await r.json();
      if (!d.erro) {
        streetEl.value = d.logradouro;
        cityEl.value   = d.localidade;
        stateEl.value  = d.uf;
      }
    }
  });

  document.getElementById('btn-address-continue').addEventListener('click', async () => {
    const cartRes = await fetch('https://localhost:5032/api/cart/items', { credentials: 'include' });
    const cartData = await cartRes.json();
    const items = cartData.items;

    subtotal = items.reduce((s,i) => s + i.price * i.quantity, 0);

    itemListEl.innerHTML = items.map(i =>
      `<div class="order-item">
         <img src="https://localhost:5032${i.image}" class="item-image" />
         <div class="item-details">${i.name}</div>
         <div class="item-quantity">x${i.quantity}</div>
         <div class="item-price">R$${(i.price*i.quantity).toFixed(2)}</div>
       </div>`
    ).join('');

    //Calculate Shipping Cost
    const shipRes = await fetch(`https://localhost:5032/api/checkout/shipping/${stateEl.value}`, { credentials:'include' });
    const { shippingCost: sc } = await shipRes.json();
    shippingCost = sc;

    //Show in OrderSummary
    addressChoiceEl.innerHTML = `
      <div>${streetEl.value}, ${numberEl.value}</div>
      <div>${cityEl.value}</div>
    `;
    shippingCostEl.textContent = `R$${sc.toFixed(2)}`;
    totalCostEl.textContent    = `R$${(subtotal + sc).toFixed(2)}`;

    // advance
    radios.payment.checked = true;
    showSection();
  });

  //Payment Form
  const addCardBtn = document.getElementById('add-card');
  const cardForm = document.querySelector('.add-card-form');
  const cancelCardBtn = document.getElementById('btn-card-return');

  addCardBtn.addEventListener('click', () => {
    cardForm.classList.toggle('active');
    document.querySelectorAll('#add-card, #btn-pix').forEach(b => b.classList.remove('selected'));
    document.getElementById('add-card').classList.add('selected');

  
    if (cardForm.classList.contains('active') &&
      !document.getElementById('card-element').childElementCount) {
      cardElement.mount('#card-element');
    }
  });

  document.querySelectorAll('.card-button').forEach(btn => {
    btn.addEventListener('click', () => {
      document.getElementById('selectedBrand').value = btn.dataset.brand;
      document.querySelectorAll('.card-button.selected')
        .forEach(b=>b.classList.remove('selected'));
      btn.classList.add('selected');
    });
  });

  document.getElementById('btn-save-card').addEventListener('click', async () => {
    // create Stripe PM
    const { paymentMethod, error } = await stripe.createPaymentMethod({
      type: 'card',
      card: cardElement
    });
    if (error) return alert(error.message);

    stripePaymentMethodId = paymentMethod.id;
    selectedPaymentType  = 'card';

    paymentChoiceEl.innerHTML = `
    <img src="/Pages/assets/images/${paymentMethod.card.brand.toLowerCase()}.svg" 
         class="card-logo-small" alt="${paymentMethod.card.brand}">
    <span>•••• •••• •••• ${paymentMethod.card.last4}</span>
    `;

    document.querySelector('.add-card-form').classList.remove('active');
    radios.confirmation.checked = true;
    showSection();
  });

  cancelCardBtn.addEventListener('click', () => {
    cardForm.classList.remove('active');
  });

  document.querySelector('#btn-pix').addEventListener('click', () => {
    selectedPaymentType = 'pix';
    document.querySelectorAll('#add-card, #btn-pix').forEach(b => b.classList.remove('selected'));
    document.getElementById('btn-pix').classList.add('selected');
    paymentChoiceEl.innerHTML = `<img src="/Pages/assets/images/pix.svg" class="card-logo-small" alt="PIX"><span>PIX</span>`;
  });


  document.getElementById('btn-payment-continue').addEventListener('click', () => {
    if (document.querySelector('.add-card-form').classList.contains('active')) return;
    if (!selectedPaymentType) return alert('Select a payment method');
    radios.confirmation.checked = true;
    showSection();
  });

  document.getElementById('btn-payment-return').addEventListener('click', () => {
    radios.address.checked = true;
    showSection();
  });

  document.getElementById('btn-payment-final').addEventListener('click', async () => {
    const modal = createModal();
    modal.showSpinner();
  
    try {
      const address = {
        PostalCode: pcodeEl.value,
        Street: streetEl.value,
        Number: numberEl.value,
        City: cityEl.value,
        State: stateEl.value
      };
  
      const paymentData = {
        Address: address,
        Payment: {
          Method: selectedPaymentType,
          PaymentMethodId: stripePaymentMethodId
        }
      };
  
      const res = await fetch('https://localhost:5032/api/checkout', {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(paymentData)
      });
  
      const data = await res.json();
      if (!res.ok) throw new Error(data.message || 'Payment failed');
  
      switch(selectedPaymentType) {
        case 'card':
          modal.showSuccess(`Payment with card **** ${data.last4} approved!`);
          setTimeout(() => {
            window.location.href = '/Pages/Account/account.html';
          }, 3000);
          break;
  
          case 'pix':
            modal.showPix(data.qrCode, data.pixCode);
            const checkStatus = async () => {
              const res = await fetch(`https://localhost:5032/api/payments/${paymentId}/status`);
              const { status } = await res.json();
              
              if (status === 'succeeded') {
                alert('Payment approved');
                window.location.href = '/Pages/Account/account.html';
              }
            };
            break;
      }
  
    } catch (error) {
      modal.showError(error.message);
      setTimeout(() => {
        modal.hide();
      }, 5000);
    }
  });

  document.getElementById('btn-confirm-return').addEventListener('click', () => {
    radios.payment.checked = true;
    showSection();
  });

  // Modal factory
  function createModal() {
    let m = document.getElementById('payment-modal');
    if (!m) {
      m = document.createElement('div');
      m.id = 'payment-modal';
      m.innerHTML = `
        <div class="modal-backdrop"></div>
        <div class="modal-content">
          <div class="checkmark"></div>
          <div class="spinner" id="modal-spinner"></div>
          <div id="modal-body"></div>
        </div>`;
      document.body.appendChild(m);
    }

    const checkmarkEl = m.querySelector('.checkmark');

    return {
      showSpinner: () => {
        m.style.display = 'flex';
        checkmarkEl.style.display = 'none';
        m.querySelector('#modal-spinner').style.display = 'block';
        m.querySelector('#modal-body').innerHTML = '';
      },
      showPix: (qr, code) => {
        checkmarkEl.style.display = 'none';
        m.querySelector('#modal-spinner').style.display = 'none';
        m.querySelector('#modal-body').innerHTML = `<img src="${qr}" /><div>${code}</div>`;
      },
      showSuccess: msg => {
        checkmarkEl.style.display = 'flex';
        m.querySelector('#modal-spinner').style.display = 'none';
        m.querySelector('#modal-body').innerHTML = `<p class="success">${msg}</p>`;
      },
      showError: msg => {
        checkmarkEl.style.display = 'none';
        m.querySelector('#modal-spinner').style.display = 'none';
        m.querySelector('#modal-body').innerHTML = `<p class="error">${msg}</p>`;
      }
    };
  }
});
