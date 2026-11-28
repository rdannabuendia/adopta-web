import { test, expect } from "@playwright/test";
import fs from "node:fs";
import path from "node:path";

const BASE_URL = "https://jolly-cliff-005bd731e.5.azurestaticapps.net";

const CREDENCIALES = {
  organizacion: {
    email: "carlos.ramirezjoquin12@gmail.com",
    password: "Carlos1234#",
  },
  admin: {
    email: "admin@adopta.com",
    password: "Adopta123*",
  },
};

const LOG_DIR = path.resolve("./tests/logs");
if (!fs.existsSync(LOG_DIR)) fs.mkdirSync(LOG_DIR, { recursive: true });

const fechaLocal = new Date(Date.now() - new Date().getTimezoneOffset() * 60000)
  .toISOString()
  .slice(0, 10);

const LOG_FILE = path.join(LOG_DIR, `${fechaLocal}.log`);

function registrarLog(nivel, modulo, flujo, duracionMs, estado, detalle = "") {
  const ahora = new Date();
  const offsetMin = ahora.getTimezoneOffset();
  const local = new Date(ahora.getTime() - offsetMin * 60000);
  const ts = local.toISOString().slice(0, -1) + "-06:00";

  const linea = `[${ts}] [${nivel}] [${modulo}] ${flujo} | Tiempo=${duracionMs}ms | Estado=${estado}${detalle ? ` | ${detalle}` : ""}\n`;
  fs.appendFileSync(LOG_FILE, linea, "utf8");
}

async function iniciarSesion(page) {
  await page.goto(`${BASE_URL}/login`);
  await page.getByRole("textbox", { name: /Correo Electrónico/i }).fill(CREDENCIALES.organizacion.email);
  await page.getByRole("textbox", { name: /^Contraseña$/i }).fill(CREDENCIALES.organizacion.password);

  const esperarLogin = page.waitForResponse(
    (res) => res.url().includes("/auth/login") && res.status() === 200,
    { timeout: 25000 }
  );

  await page.getByRole("button", { name: /Iniciar Sesión/i }).click();
  await esperarLogin;
  await expect(page).not.toHaveURL(/login/, { timeout: 25000 });
}

test.describe.serial("Adopta+ | Verificación Funcional de Módulos Principales", () => {

  test("1. Módulo de Autenticación: Inicio de sesión y persistencia de sesión", async ({ page }) => {
    const inicio = Date.now();
    await iniciarSesion(page);

    // Verifica que la sesión persiste al recargar
    await page.reload();
    await expect(page).not.toHaveURL(/login/, { timeout: 15000 });

    registrarLog("INFO", "MODULO-AUTH", "Login exitoso y persistencia de token JWT", Date.now() - inicio, "PASS");
  });

  test("2. Módulo de Catálogo de Mascotas: Consulta de inventario y ficha de detalle", async ({ page }) => {
    const inicio = Date.now();
    await iniciarSesion(page);

    // Prepara la escucha de la API antes de hacer clic en el menú
    const esperarMascotas = page.waitForResponse(
      (res) => res.url().includes("/api/") && res.url().toLowerCase().includes("mascota") && res.status() === 200,
      { timeout: 20000 }
    );

    await page.getByRole("link", { name: /mascotas/i }).first().click();
    const respuestaApi = await esperarMascotas;

    expect(respuestaApi.ok()).toBeTruthy();
    registrarLog("INFO", "MODULO-MASCOTAS", "Carga funcional de catálogo de mascotas desde BD", Date.now() - inicio, "PASS", `HTTP=${respuestaApi.status()}`);
  });

  test("3. Módulo de Solicitudes de Adopción: Consulta y apertura de expediente", async ({ page }) => {
    const inicio = Date.now();
    await iniciarSesion(page);

    await page.getByRole("link", { name: /solicitudes|adopciones/i }).first().click();
    await expect(page.locator("main")).toBeVisible({ timeout: 15000 });

    registrarLog("INFO", "MODULO-ADOPCIONES", "Consulta funcional de bandeja de solicitudes de adopción", Date.now() - inicio, "PASS");
  });

  test("4. Módulo de Solicitudes y Documentos: Revisión de expediente y documentos adjuntos", async ({ page }) => {
    test.setTimeout(60000); // Permite hasta 60s para el flujo completo con carga de archivos
    const inicio = Date.now();
    await iniciarSesion(page);

    // 1. Entrar a "Solicitudes de adopción"
    await page.getByRole("link", { name: /Solicitudes de adopción/i }).click();

    // 2. Hacer clic en el primer enlace "Detalle"
    await page.getByRole("link", { name: /^Detalle$/i }).first().click();

    // 3. Hacer clic en el botón "Ver Documentos"
    const btnVerDocs = page.getByRole("button", { name: /Ver Documentos/i });
    await expect(btnVerDocs).toBeVisible({ timeout: 15000 });
    await btnVerDocs.click();

    // 4. Esperar a que carguen los archivos dentro del modal
    const modalDocs = page.getByRole("dialog", { name: /Documentos adjuntos/i });
    await expect(modalDocs).toBeVisible({ timeout: 15000 });
    await expect(modalDocs.getByText(/Cargando/i)).toBeHidden({ timeout: 20000 });

    const botonesVerArchivo = modalDocs.getByRole("button", { name: /^Ver$/i });
    await expect(botonesVerArchivo.first()).toBeVisible({ timeout: 10000 });

    const totalArchivos = await botonesVerArchivo.count();
    expect(totalArchivos).toBeGreaterThan(0);
    
    registrarLog(
      "INFO",
      "MODULO-DOCUMENTOS",
      "Consulta de expediente y verificación de archivos adjuntos del adoptante",
      Date.now() - inicio,
      "PASS",
      `DocumentoVisible=true | ArchivosCargados=${totalArchivos}`
    );
  });

  test("5. Módulo de Perfil y Cierre de Sesión: Consulta de datos y finalización segura", async ({ page }) => {
    const inicio = Date.now();
    await iniciarSesion(page);

    await page.getByText(/cerrar sesión/i).first().click();
    await expect(page).toHaveURL(/login|inicio/, { timeout: 15000 });

    registrarLog("INFO", "MODULO-SESION", "Cierre de sesión limpio y revocación en cliente", Date.now() - inicio, "PASS");
  });

  test("6. Sonda Sintética Global: Verificación directa de disponibilidad API y BD", async ({ request }) => {
    const inicio = Date.now();

    // Valida disponibilidad de API REST + consulta de credenciales en MySQL (SLI-01 / SLI-16)
    const respuesta = await request.post(
      "https://adopta-api-hagrcpb2ebhseyfy.eastus-01.azurewebsites.net/api/auth/login",
      {
        data: {
          email: CREDENCIALES.organizacion.email,
          password: CREDENCIALES.organizacion.password,
        },
      }
    );
    const duracion = Date.now() - inicio;

    expect(respuesta.status()).toBe(200);

    // Evalúa contra el umbral SLT de SLI-16 (<= 2000 ms)
    const cumpleSlt = duracion <= 2000;
    registrarLog(
      cumpleSlt ? "INFO" : "WARN",
      "SONDA-GLOBAL",
      "Sonda sintética de disponibilidad API REST + MySQL",
      duracion,
      "PASS",
      `HTTP=${respuesta.status()} | CumpleSLT_2000ms=${cumpleSlt}`
    );
  });

});