// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

export function scrollDown(element) {
	if (!element) return;
	element.scrollTop = element.scrollHeight;
}
export function addCodeStyle(element) {
	if (!element) {
		console.log("No element in addCodeStyle");
		return;
	}
	if (!Prism) {
		console.log("No Prism in addCodeStyle");
		return;
	}

	Prism.highlightAllUnder(element);
}

export function setTheme(theme) {
	const link = document.getElementById('radzen-style');

	// Construct the new href value based on the theme parameter
	const newHref = `_content/Radzen.Blazor/css/${theme}.css`;

	// Update the link element's href attribute to the new value
	link.href = newHref;
}
export function showJsonViewer(element, jsonObj) {
	if (!jsonObj) {
		console.log("No json object in addJsonToViewer");
		return;
	}
	if (!element) {
		console.log("No element in addJsonToViewer");
		return;
	}
	console.log(`Json object: ${JSON.stringify(jsonObj)}`);
	element.data = jsonObj;

}