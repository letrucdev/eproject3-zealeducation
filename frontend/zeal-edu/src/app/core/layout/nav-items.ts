import { UserRole } from '../models/user-role';

export interface NavigationMenu {
  title: string;
  items: NavItem[];
  roles: readonly UserRole[];
}

export interface NavItem {
  label: string;
  route: string;
  icon: string;
  roles: readonly UserRole[];
  children?: readonly NavItem[];
}

export const NAVIGATION_MENUS: readonly NavigationMenu[] = [
  {
    title: 'Manages',
    roles: [UserRole.SystemAdmin],
    items: [
      {
        label: 'System Maintenance',
        route: '/app/system/maintenance',
        icon: 'lucideWrench',
        roles: [UserRole.SystemAdmin],
      },
      {
        label: 'Staff Accounts',
        route: '/app/system/staff-accounts',
        icon: 'lucideUserPlus',
        roles: [UserRole.SystemAdmin],
      },
      {
        label: 'Audit Log',
        route: '/app/system/audit-log',
        icon: 'lucideScrollText',
        roles: [UserRole.SystemAdmin],
      },
    ],
  },
  {
    title: 'Counselor',
    roles: [UserRole.Counselor],
    items: [
      {
        label: 'Course Enquiries',
        route: '/app/counselor/course-enquiries',
        icon: 'lucideUserSearch',
        roles: [UserRole.Counselor],
      },
    ],
  },
];

function filterNavItemsForRole(items: readonly NavItem[], role: UserRole): NavItem[] {
  return items.reduce<NavItem[]>((acc, item) => {
    if (!item.roles.includes(role)) return acc;
    const children = item.children ? filterNavItemsForRole(item.children, role) : undefined;
    acc.push(children ? { ...item, children } : item);
    return acc;
  }, []);
}

export function navMenusForRole(role: UserRole | undefined): NavigationMenu[] {
  if (!role) return [];

  return NAVIGATION_MENUS.filter((menu) => menu.roles.includes(role))
    .map((menu) => ({
      ...menu,
      items: filterNavItemsForRole(menu.items, role),
    }))
    .filter((menu) => menu.items.length > 0);
}

export const ROLE_LABELS: Record<UserRole, string> = {
  [UserRole.SystemAdmin]: 'System Admin',
  [UserRole.Incharge]: 'Incharge',
  [UserRole.Faculty]: 'Faculty',
  [UserRole.Counselor]: 'Counselor',
  [UserRole.AccountsStaff]: 'Accounts Staff',
  [UserRole.Candidate]: 'Candidate',
};
