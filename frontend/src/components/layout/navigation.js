import {
  BuildingIcon,
  ChartIcon,
  ClipboardIcon,
  HomeIcon,
  PawIcon,
  TimelineIcon,
  UserIcon,
} from "../ui/Icons";

export const ROLES = {
  ADMIN: "Administrador",
  ORG: "Organizacion"
};

const TODOS = [ROLES.ADMIN, ROLES.ORG, ROLES.ADOPTANTE];
const ADMIN_ORG = [ROLES.ADMIN, ROLES.ORG];

export const NAV_ITEMS = [
  {
    label: "Inicio",
    to: "/inicio",
    icon: HomeIcon,
    roles: TODOS,
  },
  {
    label: "Reportes y estadísticas",
    icon: ChartIcon,
    roles: [ROLES.ADMIN],
    description: "Estadísticas generales, reportes y exportación.",
    children: [
      { label: "Panel general", to: "/reportes", end: true },
      { label: "Reporte de adopciones", to: "/reportes/adopciones" },
      { label: "Reporte de solicitudes", to: "/reportes/solicitudes" },
      { label: "Estadísticas de mascotas", to: "/reportes/mascotas" },
      { label: "Generar y exportar", to: "/reportes/exportar" },
    ],
  },
  {
    label: "Organizaciones",
    icon: BuildingIcon,
    roles: [ROLES.ADMIN],
    description: "Listado, solicitudes de registro y bajas.",
    children: [
      { label: "Listado", to: "/organizaciones", end: true },
      { label: "Solicitudes de registro", to: "/organizaciones/solicitudes" },
    ],
  },
  {
    label: "Mascotas",
    to: "/mascotas",
    icon: PawIcon,
    roles: ADMIN_ORG,
    description: "Consulta y administra las mascotas registradas.",
  },
  {
    label: "Seguimiento de mascotas",
    icon: TimelineIcon,
    roles: ADMIN_ORG,
    description: "Adaptación de mascotas adoptadas y devoluciones.",
    children: [
      { label: "Gestión de seguimiento", to: "/seguimiento", end: true },
      { label: "Solicitudes de devolución", to: "/seguimiento/devoluciones" },
    ],
  },
  {
    label: "Solicitudes de adopción",
    to: "/solicitudes",
    icon: ClipboardIcon,
    roles: ADMIN_ORG,
    description: "Revisa, resuelve y pide información adicional.",
  },
  {
    label: "Ver perfil",
    to: "/perfil",
    icon: UserIcon,
    roles: TODOS,
  },
];

export const matchesPath = (pathname, to) =>
  pathname === to || pathname.startsWith(`${to}/`);

export function getVisibleNav(rol) {
  return NAV_ITEMS.filter((i) => i.roles.includes(rol))
    .map((i) =>
      i.children
        ? { ...i, children: i.children.filter((c) => !c.roles || c.roles.includes(rol)) }
        : i
    )
    .filter((i) => !i.children || i.children.length > 0);
}

export function getPageTitle(pathname, items) {
  const links = items.flatMap((i) => (i.children ? i.children : [i]));
  const match = links
    .filter((l) => matchesPath(pathname, l.to))
    .sort((a, b) => b.to.length - a.to.length)[0];
  return match?.label ?? "Adopta+";
}