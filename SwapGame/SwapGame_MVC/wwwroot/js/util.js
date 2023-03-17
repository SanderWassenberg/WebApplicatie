// File containing an assortment of miscellaneous functions and classes which are used thorughout the other modules.

class Template {
    // returns a function that can be used to get a copy of the template.
    static new(template_string) {
        const template = document.createElement('template');
        template.innerHTML = template_string;

        return () => template.content.cloneNode(true);
    }
}

class Array2D {
    #arr;
    #w; #h;
    get w() {return this.#w}
    get h() {return this.#h}

    constructor(w,h) {
        this.#arr = Array(h)
        for (let i = 0; i < h; i++) this.#arr[i] = Array(w)

        this.#w = w;
        this.#h = h;
    }

    get(x, y) { return this.#arr[y][x]; }
    set(x, y, value) { this.#arr[y][x] = value; }
}

const remove_all_children = e => {
    while (e.childNodes[0]) e.childNodes[0].remove()
}

const api_path = (location.hostname === "localhost" ? "https://localhost:7110" : location.origin) + "/api";

console.log("using API path", api_path)

const api_post = (path, object) => fetch(api_path + path, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(object)
});

export { Template, Array2D, remove_all_children, api_path, api_post }