// NutriPlat.WebApp/wwwroot/js/site.js

// Coloca aquí cualquier JavaScript global para tu aplicación web.
// Por ejemplo, funciones para manejar la visibilidad de menús de navegación,
// inicialización de componentes comunes, etc.

// Ejemplo de función para manejar la visibilidad de la navegación basada en el token
// (Esto ya está en _Layout.cshtml, pero podría moverse aquí para mayor organización)

// document.addEventListener('DOMContentLoaded', function() {
//     updateNavBasedOnAuth();
// });

// function updateNavBasedOnAuth() {
//     const token = localStorage.getItem('authToken');
//     const navLogin = document.getElementById('navLogin');
//     const navLogout = document.getElementById('navLogout');
//     // const navDashboard = document.getElementById('navDashboard');

//     if (token) {
//         if(navLogin) navLogin.style.display = 'none';
//         if(navLogout) navLogout.style.display = 'block';
//         // if(navDashboard) navDashboard.style.display = 'block';
//     } else {
//         if(navLogin) navLogin.style.display = 'block';
//         if(navLogout) navLogout.style.display = 'none';
//         // if(navDashboard) navDashboard.style.display = 'none';
//     }
// }

// function logoutUser() {
//     localStorage.removeItem('authToken');
//     localStorage.removeItem('userRole');
//     localStorage.removeItem('userName');
//     localStorage.removeItem('userId');
//     updateNavBasedOnAuth(); // Actualizar la navegación inmediatamente
//     window.location.href = '/Account/Login'; // Redirigir
// }

// // Si tienes un botón de logout en _Layout.cshtml con id="navLogout" y onclick="logoutUser()"
// // ya no necesitarías añadir el event listener aquí, pero la función sí.
