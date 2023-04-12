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
    while (e.lastChild) e.lastChild.remove()
}

function is_localhost(hostname) {
    return hostname === "localhost" || hostname === "127.0.0.1";
}
const api_path = (is_localhost(location.hostname) ? "https://localhost:7110" : location.origin) + "/api";

console.log("using API path", api_path)

const api_post = async (path, data) => fetch(api_path + path, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify((data instanceof FormData) ? Object.fromEntries(data) : data),
});

function set_ul_content(ul, item_or_arr) {
    remove_all_children(ul)
    if (item_or_arr) {
        if (item_or_arr instanceof Array) {
            for (const item of item_or_arr) {
                const li = document.createElement("li")
                li.innerText = item;
                ul.append(li)
            }
        } else {
            const li = document.createElement("li")
            li.innerText = item_or_arr;
            ul.append(li)
        }
    }
    hide_if_empty(ul)
}
function hide_if_empty(ul) {
    if (ul.childElementCount > 0) {
        ul.classList.remove("hide")
    } else {
        ul.classList.add("hide")    
    }
}

function bind_inner_text(binding, value) {
    const elems = document.querySelectorAll(`[data-bind=${binding}]`);
    for (let i = 0; i < elems.length; i++) {
        elems[i].innerText = value;
    }
}

async function get_error_messages_from_response(response, result_specific_action) {

	const response_body = await response.text();

	if (response.status >= 500) {
		console.error(response.status, response_body)
		return `(${response.status}) Server side error.`;
	}
	
	if (response.status !== 400) {
		console.error(response.status, response_body)
		return `(${response.status}) Unexpected server response status.`;
	}
	
	let result;
	try {
		result = JSON.parse(response_body)
	} catch (e) {
		console.error(response.status, response_body)
		return `(${response.status}) Failed to parse server response.`
	}
	
	const error_value = result_specific_action(result);

	if (error_value)
		return error_value;

	console.error(response.status, response_body)
	return `(${response.status}) Something went wrong, but couldn't figure out what`
}

export { 
    Template, 
    Array2D, 
    remove_all_children, 
    api_path, 
    api_post, 
    set_ul_content, 
    bind_inner_text, 
    get_error_messages_from_response,
}