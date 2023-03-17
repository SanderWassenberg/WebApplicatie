import {SwapGame_VisualsManager} from "./visualsmanager.js"
import {SwapGame, Piece, PieceType} from "./game.js"

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
		sheet.insertRule("#board *{transition: unset }");
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
	let bar_shown = false;

	show_sidebar(false);

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

	redirect_to_login.addEventListener("click", e=>{
		login_elem.classList.remove("hide")
		signup_elem.classList.add("hide")
	})
	
	redirect_to_signup.addEventListener("click", e=>{
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



// init game
const game    = new SwapGame(8, 8)

game.set(0, 0, Piece.my(PieceType.Plus));
game.set(0, 1, Piece.my(PieceType.Cross));
game.set(0, 2, Piece.opponent(PieceType.Circle));

// init visuals
const visuals = document.querySelector("swapgame-board")
visuals.set_game(game)
