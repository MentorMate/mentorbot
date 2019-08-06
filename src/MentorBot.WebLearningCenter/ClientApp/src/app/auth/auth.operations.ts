const routes = {
  '/app/dashboard': ['user', 'administrator'],
  '/app/settings': 'administrator',
  '/app/users': 'administrator',
  '/app/about': null
};

export interface UserInfo {
  sub: string;
  given_name: string;
  family_name?: string;
  name?: string;
  email?: string;
  gender?: string;
}

export function userInfo(value?: UserInfo): UserInfo {
  const key = 'mm-bot-usr';
  if (value === null) {
    sessionStorage.removeItem(key);
    return null;
  }

  if (typeof value === 'object') {
    sessionStorage.setItem(key, JSON.stringify(value));
  }

  return JSON.parse(sessionStorage.getItem(key));
}

// Get or set the role name. The server checks for access again. Note that multiple logins are not supported with this.
export function userRole(value?: string): string {
  const user = userInfo();
  const key = 'mm-bot-' + user.sub + '-role';

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
  return userRoleName !== null && (
    (Array.isArray(roleName) && roleName.indexOf(userRoleName) > -1) ||
    (typeof roleName === 'string' && userRoleName === roleName));
}
