export abstract class AuthService {
  abstract get isLoggedIn(): boolean;
  abstract get name(): string;
  abstract startAuthentication(): void;
  abstract completeAuthentication(): Promise<boolean>;
  abstract signout(): void;
};
