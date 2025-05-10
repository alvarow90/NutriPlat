// NutriPlat.WebApp/wwwroot/js/login.js
console.log("login.js SCRIPT CARGADO Y EJECUTÁNDOSE"); // Mensaje #1

document.addEventListener('DOMContentLoaded', function () {
    console.log("DOMContentLoaded evento disparado en login.js"); // Mensaje #2

    const loginForm = document.getElementById('loginForm');
    const messageDiv = document.getElementById('message');

    // MUY IMPORTANTE: Reemplaza esta URL con la URL base donde tu NutriPlat.Api está corriendo.
    const apiBaseUrl = 'https://localhost:7260'; // Reemplaza 7260 con el puerto real de tu API

    if (loginForm) {
        console.log("Formulario 'loginForm' ENCONTRADO en el DOM."); // Mensaje #3
        loginForm.addEventListener('submit', async function (event) {
            console.log("Evento SUBMIT del formulario 'loginForm' capturado."); // Mensaje #4
            event.preventDefault();

            const emailInput = document.getElementById('email');
            const passwordInput = document.getElementById('password');

            if (!emailInput || !passwordInput) {
                console.error("No se encontraron los campos de email o contraseña en el DOM dentro del submit.");
                if (messageDiv) showMessage('Error interno del formulario. Contacta al administrador.', 'error');
                return;
            }

            const email = emailInput.value;
            const password = passwordInput.value;

            if (messageDiv) {
                messageDiv.textContent = '';
                messageDiv.className = 'mt-4 text-center text-sm';
            }

            if (!email || !password) {
                if (messageDiv) showMessage('Por favor, ingresa tu correo y contraseña.', 'error');
                return;
            }
            console.log(`Intentando login para: ${email} con URL base API: ${apiBaseUrl}`); // Mensaje #5

            try {
                const response = await fetch(`${apiBaseUrl}/api/auth/login`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ email, password }),
                });
                console.log("Respuesta recibida de la API, status:", response.status); // Mensaje #6

                const responseData = await response.json();

                if (response.ok && responseData.accessToken) {
                    if (messageDiv) showMessage('¡Inicio de sesión exitoso!', 'success');

                    localStorage.setItem('authToken', responseData.accessToken);
                    localStorage.setItem('userRole', responseData.role);
                    localStorage.setItem('userName', responseData.fullName);
                    localStorage.setItem('userId', responseData.userId);

                    alert(`Bienvenido ${responseData.fullName}! Rol: ${responseData.role}. Redirigiendo...`);

                    let dashboardUrl = '/Account/Dashboard';
                    switch (responseData.role.toLowerCase()) {
                        case 'admin':
                            dashboardUrl = '/AdminDashboard/Index';
                            break;
                        case 'nutritionist':
                            dashboardUrl = '/NutritionistDashboard/Index';
                            break;
                        case 'trainer':
                            dashboardUrl = '/TrainerDashboard/Index';
                            break;
                        case 'client':
                            if (messageDiv) showMessage('Los clientes deben usar la aplicación móvil.', 'error');
                            localStorage.clear();
                            return;
                        default:
                            if (messageDiv) showMessage('Rol de usuario no reconocido.', 'error');
                            localStorage.clear();
                            return;
                    }
                    window.location.href = dashboardUrl;

                } else {
                    let errorMessage = 'Error al iniciar sesión.';
                    if (responseData && responseData.message) {
                        errorMessage = responseData.message;
                    } else if (responseData && responseData.title) {
                        errorMessage = responseData.title;
                        if (responseData.errors) {
                            const errors = Object.values(responseData.errors).flat();
                            errorMessage += ": " + errors.join(" ");
                        }
                    } else if (response.status === 401) {
                        errorMessage = "Credenciales incorrectas.";
                    }
                    if (messageDiv) showMessage(errorMessage, 'error');
                    console.log("Error en login, respuesta API:", responseData); // Mensaje #7
                }
            } catch (error) {
                console.error('Error en la solicitud de login (catch):', error); // Mensaje #8
                if (messageDiv) showMessage('Ocurrió un error de red o la API no está disponible.', 'error');
            }
        });
    } else {
        console.error("El formulario con id 'loginForm' NO fue encontrado en el DOM."); // Mensaje #9
    }

    function showMessage(message, type) {
        if (messageDiv) {
            messageDiv.textContent = message;
            if (type === 'success') {
                messageDiv.classList.add('text-green-600');
                messageDiv.classList.remove('text-red-600');
            } else if (type === 'error') {
                messageDiv.classList.add('text-red-600');
                messageDiv.classList.remove('text-green-600');
            }
        }
    }
});


/*
// NutriPlat.WebApp/wwwroot/js/login.js
console.log("login.js SCRIPT CARGADO Y EJECUTÁNDOSE"); // Mensaje #1

document.addEventListener('DOMContentLoaded', function () {
console.log("DOMContentLoaded evento disparado en login.js"); // Mensaje #2

const loginForm = document.getElementById('loginForm');
const messageDiv = document.getElementById('message');
 
// MUY IMPORTANTE: Reemplaza esta URL con la URL base donde tu NutriPlat.Api está corriendo.
const apiBaseUrl = 'https://localhost:7260'; // Reemplaza 7260 con el puerto real de tu API

if (loginForm) {
    console.log("Formulario 'loginForm' ENCONTRADO en el DOM."); // Mensaje #3
    loginForm.addEventListener('submit', async function (event) {
        console.log("Evento SUBMIT del formulario 'loginForm' capturado."); // Mensaje #4
        event.preventDefault(); 

        const emailInput = document.getElementById('email');
        const passwordInput = document.getElementById('password');
        
        if (!emailInput || !passwordInput) {
            console.error("No se encontraron los campos de email o contraseña en el DOM dentro del submit.");
            if (messageDiv) showMessage('Error interno del formulario. Contacta al administrador.', 'error');
            return;
        }

        const email = emailInput.value;
        const password = passwordInput.value;

        if (messageDiv) {
            messageDiv.textContent = ''; 
            messageDiv.className = 'mt-4 text-center text-sm'; 
        }

        if (!email || !password) {
            if (messageDiv) showMessage('Por favor, ingresa tu correo y contraseña.', 'error');
            return;
        }
        console.log(`Intentando login para: ${email} con URL base API: ${apiBaseUrl}`); // Mensaje #5

        try {
            const response = await fetch(`${apiBaseUrl}/api/auth/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ email, password }),
            });
            console.log("Respuesta recibida de la API, status:", response.status); // Mensaje #6

            const responseData = await response.json();

            if (response.ok && responseData.accessToken) {
                if (messageDiv) showMessage('¡Inicio de sesión exitoso!', 'success');
                
                localStorage.setItem('authToken', responseData.accessToken);
                localStorage.setItem('userRole', responseData.role);
                localStorage.setItem('userName', responseData.fullName);
                localStorage.setItem('userId', responseData.userId);

                alert(`Bienvenido ${responseData.fullName}! Rol: ${responseData.role}. Redirigiendo...`);
                
                let dashboardUrl = '/Account/Dashboard'; 
                switch (responseData.role.toLowerCase()) {
                    case 'admin':
                        dashboardUrl = '/AdminDashboard/Index';
                        break;
                    case 'nutritionist':
                        dashboardUrl = '/NutritionistDashboard/Index';
                        break;
                    case 'trainer':
                        dashboardUrl = '/TrainerDashboard/Index';
                        break;
                    case 'client':
                        if (messageDiv) showMessage('Los clientes deben usar la aplicación móvil.', 'error');
                        localStorage.clear(); 
                        return; 
                    default:
                        if (messageDiv) showMessage('Rol de usuario no reconocido.', 'error');
                        localStorage.clear();
                        return; 
                }
                window.location.href = dashboardUrl;

            } else {
                let errorMessage = 'Error al iniciar sesión.';
                if (responseData && responseData.message) {
                    errorMessage = responseData.message;
                } else if (responseData && responseData.title) { 
                    errorMessage = responseData.title;
                    if(responseData.errors) {
                        const errors = Object.values(responseData.errors).flat();
                        errorMessage += ": " + errors.join(" ");
                    }
                } else if (response.status === 401) {
                    errorMessage = "Credenciales incorrectas.";
                }
                if (messageDiv) showMessage(errorMessage, 'error');
                console.log("Error en login, respuesta API:", responseData); // Mensaje #7
            }
        } catch (error) {
            console.error('Error en la solicitud de login (catch):', error); // Mensaje #8
            if (messageDiv) showMessage('Ocurrió un error de red o la API no está disponible.', 'error');
        }
    });
} else {
    console.error("El formulario con id 'loginForm' NO fue encontrado en el DOM."); // Mensaje #9
}

function showMessage(message, type) {
    if (messageDiv) {
        messageDiv.textContent = message;
        if (type === 'success') {
            messageDiv.classList.add('text-green-600');
            messageDiv.classList.remove('text-red-600');
        } else if (type === 'error') {
            messageDiv.classList.add('text-red-600');
            messageDiv.classList.remove('text-green-600');
        }
    }
}
});
*/
