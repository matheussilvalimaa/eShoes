const accountLink = document.getElementById('account-link');
accountLink.addEventListener('click', async (e) => {
    e.preventDefault();

    const token = await fetch('https://localhost:5032/api/user/me', { credentials: 'include' })

    if (token) {
        window.location.href = "/Pages/Account/account.html";
    } else {
        window.location.href = "/Pages/Account/Login/login.html"
    }
});

//Function to load the products's data
async function loadProduct() {
    try {
        //Get the product ID from the URL
        const urlParams = new URLSearchParams(window.location.search);
        const productId = urlParams.get('id');

        //Load the selected product's data
        const response = await fetch(`https://localhost:5032/api/products/${productId}`);
        const product = await response.json();

        let imageUrl = product.imagePath;

        //Force HTTPS if the endpoint is HTTP
        if (imageUrl.startsWith('http://')) {
            imageUrl = imageUrl.replace('http://', 'https://');
        }

        const finalImageUrl = imageUrl.startsWith('/')
            ? new URL(imageUrl, 'https://localhost:5032').href
            : imageUrl;

        if (product) {
            //Update the HTML elements
            document.title = product.name;
            document.getElementById('product-name').textContent = product.name;
            document.getElementById('product-price').textContent = `R$ ${product.price.toFixed(2)}`;
            document.getElementById('product-description').textContent = product.description;
            document.getElementById('product-image').src = finalImageUrl;

            //Create size buttons
            const sizeContainer = document.getElementById('size-options');
            sizeContainer.innerHTML = '';
            (product.availableSizes || '').split(',')
                .map(s => s.trim())
                .filter(s => s)
                .forEach(size => {
                    const btn = document.createElement('button');
                    btn.className = 'size-option';
                    btn.textContent = size;
                    btn.addEventListener('click', () => selectSize(btn));
                    sizeContainer.appendChild(btn);
            });
        }
    } catch (error) {
        console.error("Error loading the product", error);
        document.title = "Error to load";
    }
}

//Function to select the user selected size
let selectedSize = null;
function selectSize(btnElement) {
    document.querySelectorAll('.size-option').forEach(btn => btn.classList.remove('selected'));
    btnElement.classList.add('selected');
    selectedSize = btnElement.textContent.trim();
}

//Reviews
const reviewsListEl = document.getElementById('reviews-list');
const formContainer  = document.getElementById('review-form');
const starInputEl   = document.getElementById('star-input');
const reviewTextEl  = document.getElementById('review-text');
const btnSubmit     = document.getElementById('btn-submit-review');
let selectedStars = 0;
let reviews = [
    { author: 'John Nakamura',   stars: 5, text: 'Great, i loved it!.' },
    { author: 'Jhow Developer',  stars: 4, text: 'I liked it, but i have to ask. Is there a library to buy it?' },
    { author: 'Keven Abraham', stars: 1, text: 'I hated. I hate this website, i hate the owner.' }
];

//Verify the login and get the username
async function checkLogin() {
    try {
      const res = await fetch('https://localhost:5032/api/user/profile', {
        credentials: 'include'
      });
      if (!res.ok) throw new Error('Não autenticado');
      const profile = await res.json();
      currentUser = profile.username;
      formContainer.style.display = 'block';
    } catch {
      formContainer.style.display = 'none';
    }
}

//Render the review list
function renderReviews() {
    reviewsListEl.innerHTML = reviews.map(r => `
      <div class="review-card">
        <div class="review-header">
          <div class="author">${r.author}</div>
          <div class="review-stars">${'★'.repeat(r.stars)}${'☆'.repeat(5-r.stars)}</div>
        </div>
        <div class="review-text">${r.text}</div>
      </div>
    `).join('');
}

//Stars choose input
starInputEl.querySelectorAll('span').forEach(star => {
    star.addEventListener('click', () => {
      selectedStars = +star.dataset.value;
      starInputEl.querySelectorAll('span').forEach(s => {
        s.classList.toggle('selected', +s.dataset.value <= selectedStars);
      });
    });
});

//Review sender button
btnSubmit.addEventListener('click', () => {
    const text = reviewTextEl.value.trim();
    if (!selectedStars || !text) {
      return alert('Please, select the stars and send your review!');
    }
    reviews.unshift({
        author: currentUser,
        stars: selectedStars,
        text
    });

    //CLear the form
    selectedStars = 0;
    starInputEl.querySelectorAll('span').forEach(s => s.classList.remove('selected'));
    reviewTextEl.value = '';
    renderReviews();
});


//Add to Cart button 
document.getElementById('add-to-cart').addEventListener('click', async () => {
    if (!selectedSize) {
        alert('Please, select a size!');
        return;
    }

    try {
        const productId = new URLSearchParams(window.location.search).get('id');

        const response = await fetch(`https://localhost:5032/api/cart/add/${productId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include',
            body: JSON.stringify({
                quantity: 1,
                size: selectedSize
            })
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message);
        }
        alert('Product added to cart!');
    } catch (error) {
        alert(`No user logged in, please login.`);
    }
});

document.addEventListener('DOMContentLoaded', async () => {
    await checkLogin();
    loadProduct();
    renderReviews();
})