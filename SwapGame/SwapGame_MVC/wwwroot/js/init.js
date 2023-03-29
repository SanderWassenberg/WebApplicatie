import {SwapGame_VisualsManager} from "./visualsmanager.js" // required for attaching the game to the visuals.
import {SwapGame, Piece, PieceType} from "./game.js"
import {api_post, formdata_obj, update_list} from "./util.js"

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
		sheet.insertRule("swapgame-board *{transition: unset }");
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
			document.documentElement.dataset.sidebar = "shown";
		} else {
			button.innerHTML = "&#9776;" // burger
			sidebar.classList.add("collapse-vertical");
			document.documentElement.dataset.sidebar = "hidden";
		}
	}
}

// redirection gedoe
const login_elem = document.querySelector(".login-area")
const signup_elem = document.querySelector(".signup-area")
const game_elem = document.querySelector("swapgame-board")
const main = document.querySelector("main");

document.querySelectorAll("[data-redirect]")
	.forEach(elem => elem.addEventListener("click", e => redirect(elem.dataset.redirect)))


function redirect(where) {
	switch(where) {
		case "login": 
			switch_main_content(login_elem)
		break;
		case "signup": 
			switch_main_content(signup_elem);
		break;
		case "game": 
			switch_main_content(game_elem);
		break;
		default: console.error("unknown redirect target", where)
	}
}

function switch_main_content(elem_to_show) {
	for (const elem of main.children) {
		if (elem === elem_to_show)
			elem.classList.remove("hide")
		else
			elem.classList.add("hide")
	}
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





document.querySelector("#test_button").addEventListener("click", async e => {
	const response = await api_post('/test', {Name: "CoolName", Num:56});
	console.log(response)
})

{
	const form = document.querySelector("#signup-form");
	const error_list = form.querySelector(".signup-area__errors")
	const submit = form.querySelector("input[type='submit']")
	
	const update = (e)=>{
		if (e.key === "Enter") return;
		const data = formdata_obj(form);
		if (data.Password !== data.ConfirmPassword) {
			update_list(error_list, "Passwords do not match.")
			submit.disabled = true;
		} else {
			update_list(error_list)
			submit.disabled = false;
		}
	}
	form.querySelector("[name='Password']").addEventListener("keyup", update)
	form.querySelector("[name='ConfirmPassword']").addEventListener("keyup", update)

	form.addEventListener("submit", async e => {
		e.preventDefault();
		e.stopImmediatePropagation();

		const data = formdata_obj(form);

		if (data.Password !== data.ConfirmPassword) return;

		delete data.ConfirmPassword // do not send this, remove it from the object.

		const response = await api_post('/signup', data);

		if (response.ok) {
			update_list(error_list)
			return
		}
		
		try {
			const obj = await response.json()
			update_list(error_list, obj.map(o => o.description))
			redirect("login")
		} catch (err) {
			console.log("json parse error:", err)
			update_list(error_list, ["Something went wrong when parsing the response from the server", err])
		}
	})
}