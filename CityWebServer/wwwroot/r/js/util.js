function merge(target, source, mutable = false) {
    //https://stackoverflow.com/a/54904499/1160471
    const newObj = typeof target == 'object' ? (mutable ? target : Object.assign({}, target)) : {};
    for (const prop in source) {
        if (target[prop] == null || typeof target[prop] === 'undefined') {
            newObj[prop] = source[prop];
        } else if (Array.isArray(target[prop])) {
            newObj[prop] = source[prop] || target[prop];
        } else if (target[prop] instanceof RegExp) {
            newObj[prop] = source[prop] || target[prop];
        } else {
            newObj[prop] = typeof source[prop] === 'object' ? this.merge(target[prop], source[prop]) : source[prop];
        }
    }
    return newObj;
}
