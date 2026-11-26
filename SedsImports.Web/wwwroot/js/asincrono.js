/*
 * Comunicación asíncrona con Fetch API (Fase 4).
 * Las vistas marcan con atributos data-* los elementos que se actualizan sin recargar la página:
 *   - data-busqueda-vivo  → búsqueda de vehículos mientras se escribe (fragmento HTML).
 *   - data-resumen-vivo   → indicadores del panel de inicio (JSON), cada minuto o con el botón.
 *   - data-nota-async     → agregar una anotación de revisión (JSON) y mostrarla al instante.
 * Si el JavaScript está desactivado, los formularios siguen funcionando de la forma normal.
 * Todo el texto que llega del servidor se inserta con textContent (nunca innerHTML), para evitar XSS.
 */
(function () {
    'use strict';

    const tokenCsrf = document.querySelector('meta[name="csrf-token"]')?.content ?? '';

    /** fetch con el encabezado antifalsificación, cookies de la sesión y lectura del error del servidor. */
    async function pedir(url, opciones = {}) {
        const respuesta = await fetch(url, {
            credentials: 'same-origin',
            ...opciones,
            headers: {
                'X-Requested-With': 'fetch',
                'X-CSRF-TOKEN': tokenCsrf,
                ...(opciones.headers || {})
            }
        });

        if (respuesta.status === 401) {
            window.location.href = '/Acceso?returnUrl=' + encodeURIComponent(location.pathname + location.search);
            throw new Error('Tu sesión venció.');
        }

        if (!respuesta.ok) {
            let mensaje = 'No se pudo completar la operación.';
            try { mensaje = (await respuesta.json()).mensaje || mensaje; } catch { /* sin JSON */ }
            throw new Error(mensaje);
        }
        return respuesta;
    }

    // ───────────── 1. Búsqueda de vehículos en vivo ─────────────
    document.querySelectorAll('[data-busqueda-vivo]').forEach(function (campo) {
        const destino = document.querySelector(campo.dataset.destino);
        const vacio = document.querySelector(campo.dataset.vacio);
        let espera = null;
        let controlador = null;

        campo.form?.addEventListener('submit', function (e) { e.preventDefault(); });

        campo.addEventListener('input', function () {
            clearTimeout(espera);
            // Espera 300 ms después de la última tecla para no hacer una petición por cada letra.
            espera = setTimeout(async function () {
                controlador?.abort();            // Cancela la búsqueda anterior si aún no terminó.
                controlador = new AbortController();

                const parametros = new URLSearchParams({ busqueda: campo.value.trim() });
                if (campo.dataset.estado) parametros.set('estado', campo.dataset.estado);

                try {
                    destino.setAttribute('aria-busy', 'true');
                    const respuesta = await pedir(campo.dataset.url + '?' + parametros, { signal: controlador.signal });
                    destino.innerHTML = await respuesta.text();   // Fragmento generado por Razor (ya codificado).
                    const total = Number(respuesta.headers.get('X-Total-Resultados') || 0);
                    if (vacio) vacio.hidden = total > 0;
                    history.replaceState(null, '', '?' + parametros);
                } catch (error) {
                    if (error.name !== 'AbortError') mostrarAviso(error.message, true);
                } finally {
                    destino.removeAttribute('aria-busy');
                }
            }, 300);
        });
    });

    // ───────────── 2. Indicadores del panel de inicio ─────────────
    document.querySelectorAll('[data-resumen-vivo]').forEach(function (panel) {
        const estado = document.querySelector('[data-resumen-estado]');
        const boton = document.querySelector('[data-resumen-actualizar]');
        const dinero = { format: v => '$' + Number(v).toLocaleString('en-US', { maximumFractionDigits: 2 }) };

        async function actualizar() {
            try {
                boton && (boton.disabled = true);
                const datos = await (await pedir(panel.dataset.url)).json();
                const valores = {
                    total: datos.vehiculos.total,
                    vendidosMes: datos.vendidosMes,
                    ventasMes: datos.ventasMes,
                    ...datos.vehiculos.porEstado
                };

                panel.querySelectorAll('[data-kpi]').forEach(function (kpi) {
                    const valor = valores[kpi.dataset.kpi];
                    if (valor === undefined) return;
                    const texto = kpi.dataset.formato === 'dinero' ? dinero.format(valor) : String(valor);
                    if (kpi.textContent !== texto) {
                        kpi.textContent = texto;
                        kpi.classList.add('kpi-cambio');
                        setTimeout(() => kpi.classList.remove('kpi-cambio'), 1200);
                    }
                });

                const hora = new Date(datos.generadoEn).toLocaleTimeString('es-SV', { hour: '2-digit', minute: '2-digit' });
                if (estado) estado.textContent = 'Actualizado a las ' + hora;
            } catch (error) {
                if (estado) estado.textContent = error.message;
            } finally {
                boton && (boton.disabled = false);
            }
        }

        boton?.addEventListener('click', actualizar);
        const segundos = Number(panel.dataset.segundos || 60);
        setInterval(function () { if (!document.hidden) actualizar(); }, segundos * 1000);
    });

    // ───────────── 3. Anotaciones de revisión sin recargar ─────────────
    document.querySelectorAll('form[data-nota-async]').forEach(function (formulario) {
        const campo = formulario.querySelector('textarea[name="Texto"]');
        const boton = formulario.querySelector('button[type="submit"]');
        const estado = formulario.querySelector('[data-estado-envio]');
        const lista = document.querySelector(formulario.dataset.destino);

        formulario.addEventListener('submit', async function (e) {
            e.preventDefault();

            // Validación en el cliente (el servidor y la API vuelven a validar).
            const texto = campo.value.trim();
            if (texto.length < 3 || texto.length > 500) {
                estado.textContent = 'La anotación debe tener entre 3 y 500 caracteres.';
                estado.classList.add('texto-error');
                campo.focus();
                return;
            }

            boton.disabled = true;
            estado.classList.remove('texto-error');
            estado.textContent = 'Guardando…';

            try {
                const respuesta = await pedir(formulario.dataset.url, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ texto: texto })
                });
                const nota = await respuesta.json();

                // Se arma el elemento con textContent: el texto del usuario nunca se interpreta como HTML.
                const div = document.createElement('div');
                div.className = 'nota nota-nueva';
                const meta = document.createElement('div');
                meta.className = 'nota-meta';
                meta.textContent = nota.fecha + ' · ' + nota.usuario;
                const cuerpo = document.createElement('div');
                cuerpo.className = 'nota-texto';
                cuerpo.textContent = nota.texto;
                div.append(meta, cuerpo);
                lista.prepend(div);

                document.querySelector('[data-sin-notas]')?.remove();
                campo.value = '';
                estado.textContent = 'Anotación agregada.';
            } catch (error) {
                estado.textContent = error.message;
                estado.classList.add('texto-error');
            } finally {
                boton.disabled = false;
            }
        });
    });

    /** Aviso breve en la esquina (errores de las funciones asíncronas). */
    function mostrarAviso(mensaje, esError) {
        const aviso = document.createElement('div');
        aviso.className = 'aviso-flotante' + (esError ? ' aviso-error' : '');
        aviso.setAttribute('role', 'alert');
        aviso.textContent = mensaje;
        document.body.append(aviso);
        setTimeout(() => aviso.remove(), 4000);
    }
})();
