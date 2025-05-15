let currentSlide = 0;
const slides = document.querySelectorAll('.slide');
const totalSlides = slides.length;
const navbar = document.querySelector(".navbar");
const menuButton = document.querySelector(".menu-button");
const sliderContainer = document.querySelector('.slider-container');
const button = document.querySelector(".button-click")

//Event to show the menu (to minor screens)
menuButton.addEventListener('click', () => {
    navbar.classList.toggle("show-menu");
});

button.addEventListener('click', () => {
    window.location.href = "/Pages/Products/products.html"
})

function nextSlide() {
    currentSlide++;
    if (currentSlide >= totalSlides) {
        currentSlide = 0;
    }
    updateSlider();
}

function previousSlide(){
    currentSlide--;
    if (currentSlide < 0) {
        currentSlide = totalSlides - 1;
    }
    updateSlider();
}
function updateSlider() {
    sliderContainer.style.transform = `translateX(-${currentSlide * 100}vw)`;
}

setInterval(nextSlide, 6000);

const accountLink = document.getElementById('account-link');
accountLink.addEventListener('click', async (e) => {
    const token = await fetch('https://localhost:5032/api/user/me', { credentials: 'include' })

    if (token) {
        window.location.href = "/Pages/Account/account.html";
    } else {
        window.location.href = "/Pages/Account/Login/login.html"
    }
});

//Function to create the product
function createProductCard(product) {
    const article = document.createElement('article');
    article.className = 'product-card';
    article.setAttribute('data-brand', product.brand);
    article.setAttribute('data-price', product.price);
    article.setAttribute('data-rating', product.rating);

    article.addEventListener('click', () => {
       window.location.href =  `/Pages/Products/product.html?id=${product.id}`;
    });
  
    //Product image
    const img = document.createElement('img');
    let path = product.imagePath;
    if (/^http:\/\//i.test(path)) {
        path = path.replace(/^http:/i, 'https:');
    }
    const src = /^https?:\/\//i.test(path)
        ? path
        : `https://localhost:5032${path}`;
    console.log('🚀 IMAGE SRC:', src);
    img.src = src;
    img.alt = product.name;
    article.appendChild(img);
  
    //Product name
    const h3 = document.createElement('h3');
    h3.textContent = product.name;
    article.appendChild(h3);
  
    //Product price
    const price = document.createElement('p');
    price.className = 'price';
    price.textContent = `R$${product.price.toFixed(2)}`;
    article.appendChild(price);
  
    //Product Ratings
    const rating = document.createElement('p');
    rating.className = 'rating';
    let stars = '';
    for (let i = 0; i < product.rating; i++) {
      stars += '★';
    }
    for (let i = product.rating; i < 5; i++) {
      stars += '☆';
    }
    rating.textContent = stars;
    article.appendChild(rating);
  
    return article;
}

document.addEventListener('DOMContentLoaded', () => {
    const container = document.querySelector('.product-grid');
    fetch('https://localhost:5032/api/products/featured', { method: 'GET' })
        .then(res => res.json())
        .then(products => {
            container.innerHTML = '';

            products.forEach(product => {
                const card = createProductCard(product);
                container.appendChild(card);
            });
        })
        .catch(error => {
            console.error('Error loading the featured products', error);
            const grid = document.querySelector('.product-grid');
            grid.innerHTML = '<p>Unable to load the products. Try again later.</p>'
        });
});