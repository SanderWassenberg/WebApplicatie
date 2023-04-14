import {Captcha} from "./captcha.js"
import {SwapGame, Piece, PieceType} from "./game.js"
import {api_post, set_ul_content, bind_inner_text, get_error_messages_from_response} from "./util.js"
import "./visualsmanager.js" // required for attaching the game to the visuals.

class Page {
	
	static main = document.querySelector("main");
	static {
		Page.redirect("signup")
	}


	jwt_token;
	static #username;

	static set_user(username) {
		Page.#username = username;
		bind_inner_text("username", username);
	}

	static redirect(where) {
		for (const elem of Page.main.children) {
			if (elem.dataset.page === where)
				elem.classList.remove("hide")
			else
				elem.classList.add("hide")
		}
	}
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
		sheet.insertRule("swapgame-board *{transition: unset }");
		document.adoptedStyleSheets = [sheet];
	}, 1)

	function set_theme(theme) {
		document.documentElement.setAttribute("data-theme", theme);
		localStorage.setItem("theme", theme);
	}
}

{ // Sidebar gedoe
	const button = document.querySelector("#sidebar-button");
	const sidebar = document.querySelector("#sidebar");
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

{
	// redirection gedoe
	const login_elem = document.querySelector(".login-area")
	const signup_elem = document.querySelector(".signup-area")
	const game_elem = document.querySelector("swapgame-board")
	const main = document.querySelector("main");

	document.querySelectorAll("[data-redirect]")
		.forEach(elem => elem.addEventListener("click", e => Page.redirect(elem.dataset.redirect)))
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
	const response = await api_post('/ping');
	console.log(response)
})

{
	const form   = document.querySelector("#signup-form");
	const submit_btn = form.querySelector("input[type='submit']");
	const error_list = form.querySelector(".signup-area__errors");
	const captcha    = form.querySelector(Captcha.tag_name);

	captcha.onsubmit = () => submit_btn.click();

	const signup_submit_event = async e => {
		e.preventDefault();

		const is_human = captcha.verify()
		if (!is_human) {
			captcha.focus();
			return;
		}

		const data = new FormData(form);

		const pw_match = data.get("Password") === data.get("ConfirmPassword")

		if (!pw_match) {
			set_ul_content(error_list, "Passwords do not match.") 
			return; 
		}

		set_ul_content(error_list) // clear errors upon submitting

		data.delete("ConfirmPassword")

		submit_btn.disabled = true;
		let response;
		try {
			response = await api_post('/signup', data);
		} catch(e) {
			set_ul_content(error_list, "Failed to reach API")
			return // finally-block runs before returning, don't worry
		} finally {
			submit_btn.disabled = false;
		}

		if (response.ok) {
			form.reset();
			set_ul_content(error_list);
			Page.redirect("login")
			return
		}

		let errors = await get_error_messages_from_response(response, result => {
			if (result.identity_errors) return result.identity_errors.map(o => o.description)
			if (result.error_message)   return result.error_message
		})

		set_ul_content(error_list, errors)
	}

	form.addEventListener("submit", signup_submit_event)
}

{
	const form   = document.querySelector("#login-form");
	const submit_btn = form.querySelector("input[type='submit']");
	const error_list = form.querySelector(".login-area__errors")

	const login_submit_event = async e => {
		e.preventDefault();

		const data = new FormData(form);

		submit_btn.disabled = true;

		let response;
		try {
			response = await api_post('/login', data);
		} catch(e) {
			set_ul_content(error_list, "Failed to reach API");
			return // finally-block runs before returning, dont worry.
		} finally {
			submit_btn.disabled = false;
		}

		if (response.ok) {
			Page.set_user(data.get("Name"))
			Page.jwt_token = (await response.json()).jwt_token;

			form.reset();
			set_ul_content(error_list);
			Page.redirect("welcome")
			return
		}

		const errors = await get_error_messages_from_response(response, result => {
			if (result.error_message) return result.error_message;
		})

		set_ul_content(error_list, errors)
	}

	form.addEventListener("submit", login_submit_event);
}