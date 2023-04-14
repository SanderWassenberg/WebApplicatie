import { Template } from "./util.js"
import { random_in_range } from "./util.js";


class Captcha extends HTMLElement {

    static tag_name = "sg-captcha"
    static #make_body = Template.new(
`<canvas width="200" height="100"></canvas>
<p></p>
<form>
<input type="text" placeholder="Copy text above">
</form>
<link rel="stylesheet" href="css/captcha.css">`)
        
    static { customElements.define(Captcha.tag_name, Captcha); }

    // elems
    #canvas;
    #input;
    #msg;
    #ctx;

    // internal data
    #string;

    // public interface
    onsubmit;

    constructor() {
        super();

        this.attachShadow({mode: 'open'});
        this.shadowRoot.append(Captcha.#make_body());

        this.#canvas = this.shadowRoot.querySelector("canvas")
        this.#input  = this.shadowRoot.querySelector("input[type='text']")
        this.#msg    = this.shadowRoot.querySelector("p")
        this.#ctx    = this.#canvas.getContext("2d", {alpha: false})

        this.shadowRoot.querySelector("form").addEventListener("submit", e=>{
            e.preventDefault();
            this.onsubmit?.call()
        })

        // this.shadowRoot.querySelector("#reset").addEventListener("click", () => {
        //     this.reset()
        // })

        this.reset()
    }

    reset() {
        this.#msg.innerText = "";
        this.#input.value   = "";

        const str_len = 5;

        {
            this.#string = "";

            // specific set of letters because some of the upper/lower case versions look too much alike
            const characters = 'ABCDEFGHIJKLMNPQRSTUVXYZabdeghijkmnpqrtuy123456789@#$%&'; 
            for (let i = 0; i < str_len; i++) {
                this.#string += characters.charAt(Math.floor(Math.random() * characters.length));
            }
        }

        const styles = ["serif", "sans-serif", "monospace", "cursive"]
        const maxwidth = this.#canvas.width / str_len
        const minwidth = 10
        const minheight = 20

        this.#ctx.fillStyle = "gray"

        this.#ctx.clearRect(0, 0, this.#canvas.width, this.#canvas.height);

        for (let i = 0; i < str_len; i++) {
            const style = styles[random_in_range(0, styles.length)];
            const char_height = random_in_range(minheight, 0.8 * this.#canvas.height);
            const char_width  = random_in_range(minwidth,  maxwidth);

            // times 0.9 to make sure letters that extend beyond the edge, like j and g are fully visible
            const y_pos = random_in_range(char_height, 0.9 * this.#canvas.height) 
            const x_pos = i * maxwidth;

            this.#ctx.font = `${char_height}px ${style}`;
            this.#ctx.fillText(this.#string[i], x_pos, y_pos, char_width)
        }
    }

    focus() {
        this.#input.focus()
    }

    verify() {
        if (this.#input.value === "") {
            this.#msg.innerText = "Fill in the captcha."
            return;
        }

        const correct = this.#input.value === this.#string;

        this.reset(); // always reset after verifying, so you don't get retries on the same one
        
        if (!correct) {
            this.#msg.innerText = "Incorrect. Try again.";
        }

        return correct;
    }
}


export {Captcha}
