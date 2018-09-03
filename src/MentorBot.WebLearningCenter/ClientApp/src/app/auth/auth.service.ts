export abstract class AuthService {
  abstract get isLoggedIn(): boolean;
  abstract get name(): string;
  abstract startAuthentication(): void;
  abstract completeAuthentication(): void;
  abstract signout(): void;
};
