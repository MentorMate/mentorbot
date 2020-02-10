interface DictionaryNum<T> {
  [key: number]: T;
}

interface Dictionary<T> extends DictionaryNum<T> {
  [key: string]: T;
}

interface Array<T> {
  toDictionary<F>(map: (items: Dictionary<F>, item: T) => void): Dictionary<F>;
}

Array.prototype.toDictionary = function (map) {
  return this.reduce((items, item) => {
    map(items, item);
    return items;
  }, {});
};
