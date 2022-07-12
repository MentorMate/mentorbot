import { Injectable } from '@angular/core';
import { Data } from './utilities.models';

@Injectable()
export class UtilitiesService {
  flatTree<T extends Data<T>>(nestedObjects: T[], childPropertyName: string): T[] {
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
