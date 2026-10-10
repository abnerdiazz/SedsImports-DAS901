// SED's Imports & Services — scripts generales (sin dependencias, salvo la validación de jQuery).
(function () {
    "use strict";

    // 1. Menú lateral en celular y tableta
    document.addEventListener("click", function (e) {
        if (e.target.closest("[data-abrir-menu]")) {
            document.body.classList.add("menu-abierto");
        } else if (e.target.closest("[data-cerrar-menu]")) {
            document.body.classList.remove("menu-abierto");
        }
    });

    // 2. Filas de tabla que abren la ficha (data-href)
    document.addEventListener("click", function (e) {
        const fila = e.target.closest("tr[data-href]");
        if (!fila || e.target.closest("a, button, form")) return;
        window.location.href = fila.dataset.href;
    });
    document.addEventListener("keydown", function (e) {
        const fila = e.target.closest && e.target.closest("tr[data-href]");
        if (fila && (e.key === "Enter" || e.key === " ")) {
            e.preventDefault();
            window.location.href = fila.dataset.href;
        }
    });

    // 3. Mensajes que se pueden cerrar
    document.addEventListener("click", function (e) {
        const boton = e.target.closest("[data-cerrar-mensaje]");
        if (boton) boton.closest(".mensaje").remove();
    });

    // 4. Modales con <dialog>
    //    <button data-abrir-modal="modal-id" data-valores='{"VehiculoId":5,"Vehiculo":"Kia Rio"}'>
    //    Los valores llenan los campos del formulario por "name" y los textos marcados con data-texto.
    function limpiarValidacion(form) {
        form.querySelectorAll(".field-validation-error").forEach(function (el) {
            el.textContent = "";
            el.classList.remove("field-validation-error");
            el.classList.add("field-validation-valid");
        });
        form.querySelectorAll(".input-validation-error").forEach(function (el) {
            el.classList.remove("input-validation-error");
        });
    }

    function asignarValores(modal, valores) {
        Object.keys(valores).forEach(function (nombre) {
            const valor = valores[nombre];
            modal.querySelectorAll('[name="' + nombre + '"]').forEach(function (campo) {
                if (campo.type === "hidden" && campo.dataset.casilla !== undefined) return;
                if (campo.type === "checkbox") {
                    campo.checked = valor === true || valor === "true";
                } else if (campo.type !== "hidden" || !modal.querySelector('input[type="checkbox"][name="' + nombre + '"]')) {
                    campo.value = valor === null || valor === undefined ? "" : valor;
                }
            });
            modal.querySelectorAll('[data-texto="' + nombre + '"]').forEach(function (el) {
                el.textContent = valor === null || valor === undefined ? "" : valor;
            });
        });
    }

    document.addEventListener("click", function (e) {
        const abrir = e.target.closest("[data-abrir-modal]");
        if (abrir) {
            const modal = document.getElementById(abrir.dataset.abrirModal);
            if (!modal) return;
            e.preventDefault();
            const form = modal.querySelector("form");
            if (form && abrir.dataset.valores) {
                if (abrir.dataset.reiniciar !== undefined) form.reset();
                asignarValores(modal, JSON.parse(abrir.dataset.valores));
            }
            if (form) limpiarValidacion(form);
            modal.showModal();
            const primero = modal.querySelector("input:not([type=hidden]):not([readonly]), textarea, select");
            if (primero) primero.focus();
            return;
        }

        if (e.target.closest("[data-cerrar-modal]")) {
            e.preventDefault();
            e.target.closest("dialog").close();
            return;
        }

        // Clic fuera del contenido del modal lo cierra
        if (e.target.tagName === "DIALOG") e.target.close();
    });

    // 5. Zona de arrastrar y soltar fotografías
    document.querySelectorAll("[data-zona-archivos]").forEach(function (zona) {
        const input = zona.querySelector("input[type=file]");
        const lista = zona.parentElement.querySelector("[data-lista-archivos]");

        function mostrar() {
            if (!lista) return;
            const nombres = Array.from(input.files).map(function (f) { return f.name; });
            lista.textContent = nombres.length ? nombres.length + " archivo(s): " + nombres.join(", ") : "";
        }

        input.addEventListener("change", mostrar);
        ["dragenter", "dragover"].forEach(function (ev) {
            zona.addEventListener(ev, function (e) { e.preventDefault(); zona.classList.add("arrastrando"); });
        });
        ["dragleave", "drop"].forEach(function (ev) {
            zona.addEventListener(ev, function (e) { e.preventDefault(); zona.classList.remove("arrastrando"); });
        });
        zona.addEventListener("drop", function (e) {
            if (e.dataTransfer && e.dataTransfer.files.length) {
                input.files = e.dataTransfer.files;
                mostrar();
            }
        });
    });

    // 6. Formularios que se envían solos (selects de filtros) y confirmaciones
    document.addEventListener("change", function (e) {
        if (e.target.matches("[data-enviar-al-cambiar]")) e.target.form.submit();
    });
    document.addEventListener("submit", function (e) {
        const mensaje = e.target.dataset.confirmar;
        if (mensaje && !window.confirm(mensaje)) e.preventDefault();
    });

    // 7. Login: llenar credenciales de prueba
    document.addEventListener("click", function (e) {
        const boton = e.target.closest("[data-usuario-prueba]");
        if (!boton) return;
        const form = document.querySelector(".login-form form");
        form.querySelector('[name="Correo"]').value = boton.dataset.usuarioPrueba;
        form.querySelector('[name="Contrasena"]').value = boton.dataset.contrasena;
        form.querySelector('[name="Contrasena"]').focus();
    });
})();
