export const chromeGet = key =>
    new Promise(resolve => {
        console.log("chromeGet");
        chrome.storage.sync.get(key, items => {
            console.log(`resolve: ${items[key]}`);
            resolve(items[key]);
        })
    });

export const chromeSet = (key, value) =>
    new Promise(resolve => {
        console.log("chromeSet")
        let v = {};
        v[key] = value;
        chrome.storage.sync.set(v, resolve);
    });

    chrome.storage.sync.get(['key'], function(result) {
        console.log('Value currently is ' + result.key);
      });

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