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

const LOG_FILE = path.join(LOG_DIR, `sla-funcional-${fechaLocal}.log`);

function registrarLog(quien, modulo, que, duracionMs, estado, detalle = "") {
  const ahora = new Date();
  const offsetMin = ahora.getTimezoneOffset();
  const local = new Date(ahora.getTime() - offsetMin * 60000);
  const ts = local.toISOString().slice(0, -1) + "-06:00";

  const resultado = `${estado} | Tiempo=${duracionMs}ms${detalle ? ` | ${detalle}` : ""}`;
  const linea = `Timestamp=${ts} | Quien=${quien} | Modulo=${modulo} | Que=${que} | Resultado=${resultado}\n`;
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

test.describe.serial("Adopta+ | Trazabilidad Funcional Completa", () => {

  test("1. Log in: Inicio de sesión y persistencia de sesión", async ({ page }) => {
    const inicio = Date.now();
    await iniciarSesion(page);

    await page.reload();
    await expect(page).not.toHaveURL(/login/, { timeout: 15000 });

    registrarLog(
      CREDENCIALES.organizacion.email,
      "MODULO-AUTH",
      "Log in y persistencia de sesión JWT",
      Date.now() - inicio,
      "PASS"
    );
  });

  test("2. Operativa (Catálogo): Consulta de inventario de mascotas", async ({ page }) => {
    const inicio = Date.now();
    await iniciarSesion(page);

    const esperarMascotas = page.waitForResponse(
      (res) => res.url().includes("/api/") && res.url().toLowerCase().includes("mascota") && res.status() === 200,
      { timeout: 20000 }
    );

    await page.getByRole("link", { name: /mascotas/i }).first().click();
    const respuestaApi = await esperarMascotas;

    expect(respuestaApi.ok()).toBeTruthy();
    registrarLog(
      CREDENCIALES.organizacion.email,
      "MODULO-MASCOTAS",
      "Operativa: Consulta funcional de catálogo de mascotas",
      Date.now() - inicio,
      "PASS",
      `HTTP=${respuestaApi.status()}`
    );
  });

  test("3. Operativa (Solicitudes): Consulta de bandeja de adopción", async ({ page }) => {
    const inicio = Date.now();
    await iniciarSesion(page);

    await page.getByRole("link", { name: /solicitudes|adopciones/i }).first().click();
    await expect(page.locator("main")).toBeVisible({ timeout: 15000 });

    registrarLog(
      CREDENCIALES.organizacion.email,
      "MODULO-SOLICITUDES",
      "Operativa: Consulta de bandeja de solicitudes de adopción",
      Date.now() - inicio,
      "PASS"
    );
  });

  test("4. Operativa (Documentos): Revisión de expediente y documentos adjuntos", async ({ page }) => {
    test.setTimeout(60000);
    const inicio = Date.now();
    await iniciarSesion(page);

    await page.getByRole("link", { name: /Solicitudes de adopción/i }).click();
    await page.getByRole("link", { name: /^Detalle$/i }).first().click();

    const btnVerDocs = page.getByRole("button", { name: /Ver Documentos/i });
    await expect(btnVerDocs).toBeVisible({ timeout: 15000 });
    await btnVerDocs.click();

    const modalDocs = page.getByRole("dialog", { name: /Documentos adjuntos/i });
    await expect(modalDocs).toBeVisible({ timeout: 15000 });
    await expect(modalDocs.getByText(/Cargando/i)).toBeHidden({ timeout: 20000 });

    const botonesVerArchivo = modalDocs.getByRole("button", { name: /^Ver$/i });
    await expect(botonesVerArchivo.first()).toBeVisible({ timeout: 10000 });

    const totalArchivos = await botonesVerArchivo.count();
    expect(totalArchivos).toBeGreaterThan(0);

    registrarLog(
      CREDENCIALES.organizacion.email,
      "MODULO-DOCUMENTOS",
      "Operativa: Revisión de expediente y documentos adjuntos",
      Date.now() - inicio,
      "PASS",
      `ArchivosCargados=${totalArchivos}`
    );
  });

  test("5. Log out: Cierre de sesión seguro", async ({ page }) => {
    const inicio = Date.now();
    await iniciarSesion(page);

    await page.getByText(/cerrar sesión/i).first().click();
    await expect(page).toHaveURL(/login|inicio/, { timeout: 15000 });

    registrarLog(
      CREDENCIALES.organizacion.email,
      "MODULO-AUTH",
      "Log out (Cierre de sesión limpio)",
      Date.now() - inicio,
      "PASS"
    );
  });

  test("6. Sonda Sintética Global: Verificación de disponibilidad API + MySQL", async ({ request }) => {
    const inicio = Date.now();

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

    const cumpleSlt = duracion <= 2000;
    registrarLog(
      "Sonda-Playwright",
      "SONDA-GLOBAL",
      "Verificación directa de disponibilidad API REST + MySQL",
      duracion,
      "PASS",
      `HTTP=${respuesta.status()} | CumpleSLT_2000ms=${cumpleSlt}`
    );
  });

});