import { Template, Array2D, remove_all_children } from "./util.js"

class Tile extends HTMLElement {
    x; y;
    #use; #svg;
    #manager;
    #__piece;

    static { customElements.define('swapgame-tile', Tile); }
    static #make_body = Template.new('<svg viewBox="-5 -5 110 110"><use href=""></use></svg>')

    constructor(manager, x, y, piece) {
        super()

        this.#manager = manager;
        this.x = x;
        this.y = y;

        this.append(Tile.#make_body());
        this.#use = this.querySelector("use");
        this.#svg = this.querySelector("svg");
        this.addEventListener("click", e => this.#manager.click(this, e))
        
        this.piece = piece; // must set AFTER initializing #use and #svg
    }

    get piece() { return this.#__piece; }
    set piece(piece) {
        this.#__piece = piece;
        this.#use.setAttribute("href", piece?.type.href)
        this.#svg.setAttribute("stroke", ["var(--side-0-color)", "var(--side-1-color)"][piece?.side ?? 0])
    }

    get is_selected() { return this.classList.contains("selected") }
    set is_selected(v) { this.classList[v ? "add" : "remove"]("selected") }
    get is_highlighted() { return this.classList.contains("highlighted") }
    set is_highlighted(v) { this.classList[v ? "add" : "remove"]("highlighted") }
}

class SwapGame_VisualsManager extends HTMLElement {
    #game;
    #tiles;
    #grid

    static #make_body = Template.new(`
    <div class="width-limits-the-height center-content">
        <div class="square-from-height center-content">
            <div class="grid"></div>
        </div>
    </div>`)

    static { customElements.define("swapgame-board", SwapGame_VisualsManager); }

    constructor() {
        super();
        this.append(SwapGame_VisualsManager.#make_body())
        this.#grid = this.querySelector(".grid");
    }

    set_game(game) {
        this.#game = game;
        this.#tiles = new Array2D(game.w, game.h)

        remove_all_children(this.#grid)
        this.#grid.style.gridTemplateColumns = `repeat(${game.w}, auto)`
        this.#grid.style.gridTemplateRows    = `repeat(${game.h}, auto)`

        for (let y = 0; y < game.h; y++) 
        for (let x = 0; x < game.w; x++) {
            const t = new Tile(this, x, y, game.get(x,y))
            this.#grid.append(t)
            this.#tiles.set(x, y, t)
        }

        game.add_update_callback((x,y) => {
            this.#tiles.get(x,y).piece = game.get(x,y)
        })
    }

    #selected_tile;
    get selected_tile() { return this.#selected_tile }
    set selected_tile(new_selection) {
        const update_select_status = (tile, state) => {
            if (!tile) return;
            tile.is_selected = state
            this.#game.loop_over_valid_moves(tile.x, tile.y, (x,y) => {
                this.#tiles.get(x,y).is_highlighted = state
            })
        }
        update_select_status(this.#selected_tile, false)
        this.#selected_tile = new_selection
        update_select_status(this.#selected_tile, true)
    }

    click(clicked_tile, event) {
        if (clicked_tile.is_selected) {
            this.selected_tile = undefined;
        } else if (clicked_tile.is_highlighted) {
            const from_tile = this.selected_tile;
            this.selected_tile = undefined; // unselect before doing move
            this.#game.move(from_tile.x, from_tile.y, clicked_tile.x, clicked_tile.y)
        } else {
            this.selected_tile = clicked_tile
        }
    }
}

export {SwapGame_VisualsManager}