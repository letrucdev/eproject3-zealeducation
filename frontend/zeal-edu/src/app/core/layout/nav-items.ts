import { UserRole } from '@core/models/user-role';

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
  {
    title: 'Manages',
    roles: [UserRole.Incharge],
    items: [
      {
        label: 'Courses',
        route: '/app/incharge/courses',
        icon: 'lucideBookOpen',
        roles: [UserRole.Incharge],
      },
      {
        label: 'Batches',
        route: '/app/incharge/batches',
        icon: 'lucideGraduationCap',
        roles: [UserRole.Incharge],
      },
      {
        label: 'Candidates',
        route: '/app/incharge/candidates',
        icon: 'lucideUsers',
        roles: [UserRole.Incharge],
      },
      {
        label: 'Materials',
        route: '/app/incharge/materials',
        icon: 'lucideFolderOpen',
        roles: [UserRole.Incharge],
      },
      {
        label: 'Feedback',
        route: '/app/incharge/feedback',
        icon: 'lucideMessageSquare',
        roles: [UserRole.Incharge],
      },
      {
        label: 'Certificates',
        route: '/app/incharge/certificates',
        icon: 'lucideAward',
        roles: [UserRole.Incharge],
      },
    ],
  },
  {
    title: 'Faculty',
    roles: [UserRole.Faculty],
    items: [
      {
        label: 'Schedule',
        route: '/app/faculty/schedule',
        icon: 'lucideCalendarDays',
        roles: [UserRole.Faculty],
      },
      {
        label: 'My Batches',
        route: '/app/faculty/batches',
        icon: 'lucideGraduationCap',
        roles: [UserRole.Faculty],
      },
      {
        label: 'Examinations',
        route: '/app/faculty/examinations',
        icon: 'lucideClipboardCheck',
        roles: [UserRole.Faculty],
      },
    ],
  },
  {
    title: 'Finance',
    roles: [UserRole.AccountsStaff],
    items: [
      {
        label: 'Payments',
        route: '/app/accounts/payments',
        icon: 'lucideWallet',
        roles: [UserRole.AccountsStaff],
      },
      {
        label: 'Financial Report',
        route: '/app/accounts/financial-report',
        icon: 'lucideChartColumn',
        roles: [UserRole.AccountsStaff],
      },
    ],
  },
  {
    title: 'My Learning',
    roles: [UserRole.Candidate],
    items: [
      {
        label: 'My Batches',
        route: '/app/candidate/batches',
        icon: 'lucideGraduationCap',
        roles: [UserRole.Candidate],
      },
      {
        label: 'My Certificates',
        route: '/app/candidate/certificates',
        icon: 'lucideAward',
        roles: [UserRole.Candidate],
      },
      {
        label: 'Profile',
        route: '/app/candidate/profile',
        icon: 'lucideUser',
        roles: [UserRole.Candidate],
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
