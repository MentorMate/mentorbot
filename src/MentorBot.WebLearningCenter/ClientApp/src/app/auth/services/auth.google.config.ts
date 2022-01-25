import { AuthConfig } from 'angular-oauth2-oidc';
import { environment } from '../../../environments/environment';

export const authConfig: AuthConfig = {
  issuer: 'https://accounts.google.com',
  redirectUri: environment.webPath + 'app/signin-google',
  clientId: environment.googleClientId as unknown as string,
  scope: 'openid profile email',
  sessionChecksEnabled: true,
  strictDiscoveryDocumentValidation: false,
};
