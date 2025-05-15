const menuButton = document.querySelector(".menu-button");
const navbar = document.querySelector(".navbar");
menuButton.addEventListener('click', () => {
  navbar.classList.toggle("show-menu");
});

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

//Sort the product's rating
document.addEventListener('DOMContentLoaded', function() {
    const productGrid = document.querySelector('.product-grid');
    const products = Array.from(document.querySelectorAll('.product-card'));
  
    //Sort in descending order
    products.sort((a, b) => {
      const ratingA = parseInt(a.getAttribute('data-rating'));
      const ratingB = parseInt(b.getAttribute('data-rating'));
      return ratingB - ratingA;
    });
  
    //Clear the container and puts the selected products
    productGrid.innerHTML = '';
    products.forEach(product => {
      productGrid.appendChild(product);
    });
}); 

//Sort the product's brands
const brandLinks = document.querySelectorAll('.brand-options a');

brandLinks.forEach(link => {
  link.addEventListener('click', function(e) {
    e.preventDefault();
    const selectedBrand = this.id; 
    const products = Array.from(document.querySelectorAll('.product-card'));

    products.forEach(product => {
      // Shows data if the data-brand attribute equals the selected brand
      if (product.getAttribute('data-brand') === selectedBrand) {
        product.style.display = 'block';
      } else {
        product.style.display = 'none';
      }
    });
  });
});

//Sort the product's price
function sortProducts(criteria, order) {
    const grid = document.querySelector('.product-grid');
    const products = Array.from(grid.children);

    //Order the list based on the chosen attribute
    products.sort((a, b) => {
        let aVal = parseFloat(a.getAttribute('data-' + criteria));
        let bVal = parseFloat(b.getAttribute('data-' + criteria));
        return order === 'asc' ? aVal - bVal : bVal - aVal;
    });

    //Update the grid removing and readding the products in new order
    products.forEach(product => grid.appendChild(product));
}

document.getElementById('price-high').addEventListener('click', function(e) {
    e.preventDefault();
    sortProducts('price', 'desc');
});

document.getElementById('price-low').addEventListener('click', function(e) {
    e.preventDefault();
    sortProducts('price', 'asc');
});

document.getElementById('rating-high').addEventListener('click', function(e) {
    e.preventDefault();
    sortProducts('rating', 'desc');
});

document.getElementById('rating-low').addEventListener('click', function(e) {
    e.preventDefault();
    sortProducts('rating', 'asc');
});

//Function to create the product
function createProductCard(product) {
  const article = document.createElement('article');
  article.className = 'product-card';
  article.setAttribute('data-brand', product.brand);
  article.setAttribute('data-price', product.price);
  article.setAttribute('data-rating', product.rating);

  article.addEventListener('click', () => {
    window.location.href = `/Pages/Products/product.html?id=${product.id}`;
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
  const grid = document.querySelector('.product-grid');
  fetch('https://localhost:5032/api/products')
    .then(response => {
      if (!response.ok) {
        throw new Error('Error loading the products.');
      }
      return response.json();
    })
    .then(products => {
      grid.innerHTML = '';

      //Iterate on the product's list
      products.forEach(product => {
        const card = createProductCard(product);
        grid.appendChild(card);
      });
    })
    .catch(error => {
      console.error('Error', error);
      const grid = document.querySelector('.product-grid');
      grid.innerHTML = '<p>Unable to load the products. Try again later.</p>'
    });
});
