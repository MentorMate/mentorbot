import { Injectable } from '@angular/core';
import { data } from './utilities.models';

@Injectable()
export class UtilitiesService {
  flatTree<T extends data<T>>(nestedObjects: T[], childPropertyName: string): T[] {
    return nestedObjects.reduce((resultArray: T[], nestedObject: T): T[] => {
      resultArray.push(nestedObject);
      const children = nestedObject[childPropertyName] as T[];
      if (children && children.length != 0) {
        resultArray.push(...this.flatTree<T>(children, childPropertyName));
      }

      return resultArray;
    }, []);
  }
}
