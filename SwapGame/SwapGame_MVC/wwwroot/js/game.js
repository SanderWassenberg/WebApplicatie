import { Array2D } from "./util.js"

const c = (x,y) => ({x, y});
class PieceType {
    name; href; valid_moves;
    constructor(name, href, valid_moves) {
        this.name = name;
        this.href = href;
        this.valid_moves = valid_moves;
    }

    static Cross  = new PieceType("cross",  "#piece_cross",  [c(1, 1), c(-1, 1), c(1, -1), c(-1, -1)] );
    static Plus   = new PieceType("plus",   "#piece_plus",   [c(0, 1), c(0, -1), c(1, 0),  c(-1, 0)] );
    static Circle = new PieceType("circle", "#piece_circle", [c(0, 2), c(0, -2), c(2, 0),  c(-2, 0)] );
}

class Piece {
    type; side;
    constructor(type, side = 0) {
        this.type = type;
        this.side = side;
    }
    static my(type) {
        return new Piece(type, 0)
    }
    static opponent(type) {
        return new Piece(type, 1)
    }
}

class SwapGame {
    #__board;
    #update_callbacks = [];

    get w() { return this.#__board.w }
    get h() { return this.#__board.h }

    constructor(w,h) {
        this.#__board = new Array2D(w,h)
    }

    loop_over_valid_moves(x, y, callback) {
        const moves = this.get(x,y)?.type.valid_moves ?? [];
        for (const offset of moves) {
            const move_x = x + offset.x;
            const move_y = y + offset.y;
            if (this.is_on_board(move_x, move_y)) {
                callback(move_x, move_y)
            }
        }
    }

    is_on_board(x,y) { return x >= 0 && x < this.#__board.w && y >= 0 && y < this.#__board.h; }

    get(x, y) { return this.#__board.get(x,y); }
    set(x, y, value) {
        if (this.#__board.get(x, y) === value) return;
        this.#__board.set(x, y, value);

        for (const cb of this.#update_callbacks) cb(x, y)
    }

    move(from_x, from_y, to_x, to_y) {

        if (!this.is_on_board(from_x, from_y) || !this.is_on_board(to_x, to_y)) 
            throw console.error("Move out of bounds", from_x, from_y, "->", to_x, to_y);

        const moving_piece = this.get(from_x, from_y)

        if (!moving_piece) 
            throw console.error("There is no piece on", from_x, from_y)

        const offset_x = to_x - from_x;
        const offset_y = to_y - from_y;
        if (moving_piece.type.valid_moves.findIndex(e => e.x === offset_x && e.y === offset_y) === -1)
            throw console.error(to_x, to_y, "is not a valid move for", from_x, from_y)

        const target_piece = this.get(to_x, to_y);
        const move_type = target_piece ? "swap" : "move";

        this.set(from_x, from_y, target_piece)
        this.set(to_x, to_y, moving_piece)

        
    }

    add_update_callback(cb) { this.#update_callbacks.push(cb); }
    remove_update_callback(cb) { 
        const idx = this.#update_callbacks.indexOf(cb);
        if (idx === -1) return
        this.#update_callbacks.splice(idx, 1); 
    }
}



export { SwapGame, Piece, PieceType }