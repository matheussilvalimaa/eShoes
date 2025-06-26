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