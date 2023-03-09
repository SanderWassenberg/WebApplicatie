"use strict";

// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.


{
	class Piece {
		constructor(type_id, side) {
			this.type = piece_types[type_id];
			this.side = side;
		}
	}
	class Coord {
		constructor(x, y) {
			this.x = x;
			this.y = y;
		}
	}

	const tile_template = document.createElement("div");
	tile_template.classList.add("tile");

	class Tile extends Coord {
		constructor(x, y) {
			super(x, y)
			this.elem = tile_template.cloneNode();
			this.elem.addEventListener("click", this)
			this.piece = undefined;
		}

		// eventlisteners call this function
		handleEvent(e) {
			console.log(`clicked tile (${this.x}, ${this.y})`)
		}
	}
	const c = (x, y) => new Coord(x, y);
	const piece_types = [
		{ name: "cross", valid_moves: [c(1, 1), c(-1, 1), c(1, -1), c(-1, -1)] },
		{ name: "plus",  valid_moves: [c(0, 1), c(0, -1), c(0, 1), c(0, -1)] },
		{ name: "dot",   valid_moves: [c(0, 2), c(0, -2), c(0, 2), c(0, -2)] },
	]
	const PIECE_CROSS = 0;
	const PIECE_PLUS = 1;
	const PIECE_DOT = 2;

	const board_width = 8;
	const board_height = 8;

	// init grid
	const board_elem = document.getElementById("board");
	board_elem.style.gridTemplateColumns = `repeat(${board_width}, auto)`
	board_elem.style.gridTemplateRows = `repeat(${board_height}, auto)`

	// make board
	const board = Array(board_height);
	for (let i = 0; i < board_height; i++) board[i] = Array(board_width);

	// fill board
	for (let y = 0; y < board_height; y++)
		for (let x = 0; x < board_width; x++) {
			board[y][x] = new Tile(x, y)
			board_elem.append(board[y][x].elem)
		}

	// init pieces
	board[0][0].piece = new Piece(PIECE_PLUS, "black")
	board[1][0].piece = new Piece(PIECE_CROSS, "black")
	board[2][0].piece = new Piece(PIECE_DOT, "black")
}



{ // THEME gedoe
	const theme_switch = document.querySelector('#theme_switch input[type="checkbox"]');
	const theme = localStorage.getItem("theme")

	set_theme(theme ?? "light");
	theme_switch.checked = theme === "dark";
	theme_switch.addEventListener("change", e => set_theme(e.target.checked ? "dark" : "light"));

	// Create a theme transition animation, as a CSS rule. The reason this isn't a rule in the CSS file is because that would trigger the transition from the default theme to the theme remembered by localstorage every time the page is reloaded. The timeout is also necessary, otherwise the rule is added so fast it still activates the transition on page load.
	setTimeout(() => {
		const sheet = new CSSStyleSheet();
		sheet.insertRule("*{transition: .2s }");
		document.adoptedStyleSheets = [sheet];
	}, 1)

	function set_theme(theme) {
		document.documentElement.setAttribute("data-theme", theme);
		localStorage.setItem("theme", theme);
	}
}

{ // Sidebar gedoe
	const button = document.getElementById("sidebar-button");
	const sidebar = document.getElementById("sidebar");
	const overlay = document.querySelector(".focus-overlay");

	let bar_shown = !sidebar.classList.contains("collapse-vertical");
	show_sidebar(bar_shown);

	button.addEventListener("click", toggle_sidebar)
	overlay.addEventListener("click", () => show_sidebar(false))

	function toggle_sidebar() {
		show_sidebar(!bar_shown);
	}

	function show_sidebar(show) {
		bar_shown = show;
		if (show) {
			button.innerHTML = "&#10006;" // cross
			sidebar.classList.remove("collapse-vertical");
			document.documentElement.setAttribute("data-sidebar", "shown");
		} else {
			button.innerHTML = "&#9776;" // burger
			sidebar.classList.add("collapse-vertical");
			document.documentElement.setAttribute("data-sidebar", "hidden");
		}
	}
}

{ // login/signup gedoe
	const login_elem = document.querySelector(".login-area")
	const signup_elem = document.querySelector(".signup-area")
	const redirect_to_login = document.getElementById("signup-redirect-to-login")
	const redirect_to_signup = document.getElementById("login-redirect-to-signup")

	redirect_to_login.addEventListener("click", e => {
		login_elem.classList.remove("hide")
		signup_elem.classList.add("hide")
	})

	redirect_to_signup.addEventListener("click", e => {
		signup_elem.classList.remove("hide")
		login_elem.classList.add("hide")
	})
}

{ // GDPR gedoe
	const gdpr = document.querySelector(".gdpr-popup");
	const hide_gdpr = () => gdpr.classList.add("hide");
	const show_gdpr = () => gdpr.classList.remove("hide");
	const get_cookie_status = () => localStorage.getItem("use_cookies");
	const set_cookie_status = status => localStorage.setItem("use_cookies", status);

	// Init buttons
	for (const option of ["accept", "reject", "postpone"]) {
		gdpr.querySelector(`.button.${option}`).addEventListener("click", e => {
			set_cookie_status(option)
			hide_gdpr()
		})
	}

	// GDPR is hidden by default.
	const choice = get_cookie_status() ?? "postpone";
	const made_choice = ["accept", "reject"].includes(choice)
	if (!made_choice) show_gdpr();
}
