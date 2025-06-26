document.addEventListener("DOMContentLoaded", () => {
    const loginForm = document.querySelector("form.login");
    const signupForm = document.querySelector("form.signup");
    const loginBtn = document.querySelector("label.login");
    const signupBtn = document.querySelector("label.signup");
    const signupLink = document.querySelector("form.login .signup-link a");
  
    signupBtn.onclick = () => {
      loginForm.style.marginLeft = "-50%";
    };
  
    loginBtn.onclick = () => {
      loginForm.style.marginLeft = "0%";
    };
  
    if (signupLink) {
      signupLink.onclick = (e) => {
        e.preventDefault();
        signupBtn.click();
      };
    }
    
    //Login Form
    loginForm.addEventListener("submit", async (e) => {
      e.preventDefault();

      const loginSubmit = document.getElementById("loginSubmit");
      const loginSpinner = document.getElementById("loginSpinner");

      //Hidden the button and shows the spinner
      loginSubmit.style.visibility = "hidden";
      loginSpinner.style.display = "block";
  
      const username = loginForm.querySelector('input[placeholder="Username"]').value;
      const password = loginForm.querySelector('input[placeholder="Password"]').value;
  
      try {
        const response = await fetch("https://localhost:5032/api/user/login", {
          method: "POST",
          credentials: 'include',
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ username, password })
        });
  
        if (response.ok) {
          window.location.href = "/Pages/Home/index.html";
        } else {
          const errorData = await response.json();
          alert("Login failed: " + errorData.message);
        }
      } catch (err) {
        console.error("Error during login:", err);
        alert("An error occurred during login.");
      } finally {
        loginSpinner.style.display = "none";
        loginSubmit.style.visibility = "visible"; 
      }
    });
    
    //Signup Form
    signupForm.addEventListener("submit", async (e) => {
      e.preventDefault();

      const signupSubmit = document.getElementById("signupSubmit");
      const signupSpinner = document.getElementById("signupSpinner");

      //Hidden the button and shows the spinner
      signupSubmit.style.visibility = "hidden";
      signupSpinner.style.display = "block";
  
      const username = signupForm.querySelector('input[placeholder="Username"]').value;
      const email = signupForm.querySelector('input[placeholder="Email"]').value;
      const password = signupForm.querySelector('input[placeholder="Password"]').value;
  
      try {
        const response = await fetch("https://localhost:5032/api/user/register", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ username, email, password })
        });
  
        if (response.ok) {
          window.location.href = "/Pages/Account/account.html";
        } else {
          const errorData = await response.json();
          alert("Signup failed: " + errorData.message);
        }
      } catch (err) {
        console.error("Error during signup:", err);
        alert("An error occurred during signup.");
      } finally {
        signupSpinner.style.display = "none";
        signupSubmit.style.visibility = "visible"; 
      }});
  });
  