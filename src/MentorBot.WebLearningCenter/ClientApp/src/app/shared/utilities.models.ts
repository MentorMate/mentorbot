export interface Data<T> {
  [childPropertyName: string]: T[] | unknown;
}
