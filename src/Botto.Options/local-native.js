const sleep = msec => new Promise(resolve => {
    setTimeout(() => {
        resolve()
    }, msec);
});

export const localGet = key =>
    sleep(1000) // dummy
        .then(() => {
            return new Promise(resolve => {
                resolve(localStorage.getItem(key));
            });
        });

export const localSet = (key, value) =>
    sleep(1000) // dummy
        .then(() => {
            return new Promise(resolve => {
                resolve(localStorage.setItem(key, value));
            });
        });
