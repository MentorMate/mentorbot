const routes: Record<string, string | string[] | null> = {
  '/app/dashboard': ['user', 'administrator'],
  '/app/settings': 'administrator',
  '/app/users': 'administrator',
  '/app/about': null,
  '/app/questions': 'administrator',
};

export interface UserInfo {
  sub: string;
  given_name: string;
  family_name?: string;
  name?: string;
  email?: string;
  gender?: string;
}

export function userInfo(value?: UserInfo | null): UserInfo | null {
  const key = 'mm-bot-usr';
  if (value === null) {
    sessionStorage.removeItem(key);
    return null;
  }

  if (typeof value === 'object') {
    sessionStorage.setItem(key, JSON.stringify(value));
  }

  const storedValue = sessionStorage.getItem(key);
  return storedValue === null ? null : JSON.parse(storedValue);
}

// Get or set the role name. The server checks for access again. Note that multiple logins are not supported with this.
export function userRole(value?: string | null): string | null {
  const user = userInfo();
  const key = 'mm-bot-' + user?.sub + '-role';

  if (user === null) {
    return null;
  }

  if (value === null) {
    sessionStorage.removeItem(key);
    return null;
  }

  if (typeof value === 'string') {
    sessionStorage.setItem(key, value);
  }

  return sessionStorage.getItem(key) || null;
}

export function checkPath(url: string): boolean {
  const roleName = routes[url];
  if (roleName === null) {
    return true;
  }

  if (typeof roleName === 'undefined') {
    return false;
  }

  const userRoleName = userRole();
  return (
    userRoleName !== null &&
    ((Array.isArray(roleName) && roleName.indexOf(userRoleName) > -1) || (typeof roleName === 'string' && userRoleName === roleName))
  );
}
