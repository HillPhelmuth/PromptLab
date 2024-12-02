// jodit-editor.js
/*import Jodit from '/_content/BlazorJoditEditor/jodit/jodit.min.js';*/
const aiCommandsIcon = `<svg xmlns='http://www.w3.org/2000/svg' viewBox="0 0 270 270">
  <path
     d="M 255.9537,58.150081 237.69527,40.997278 c -1.49414,-1.375593 -3.43653,-2.077427 -5.37891,-2.077427 -1.94239,0 -3.88478,0.701834 -5.37892,2.077427 L 29.919751,226.1128 c -2.988286,2.77926 -2.988286,7.32714 0,10.13447 L 48.148295,253.372 c 1.46426,1.37559 3.406646,2.1055 5.378915,2.1055 1.972268,0 3.884771,-0.72991 5.378914,-2.1055 L 221.34935,100.73732 255.9537,68.284552 c 2.9584,-2.807333 2.9584,-7.355212 0,-10.134471 z M 251.17244,63.79282 221.34935,91.781927 201.89561,73.506191 231.68882,45.48901 c 0.20918,-0.140367 0.38847,-0.224587 0.62754,-0.224587 0.23906,0 0.44824,0.08422 0.59765,0.224587 l 18.25843,17.152803 c 0,0 0.23906,0.33688 0.23906,0.561467 0,0.224586 -0.0896,0.4211 -0.23906,0.58954 z"
     style="stroke-width:2.8964;stroke-opacity:1" />
  <path
     d="m 48.626421,116.87948 10.578532,23.10435 c 0.83672,1.82477 3.615826,1.85284 4.452546,0 l 10.937126,-23.52545 c 2.629692,-5.69888 7.470715,-10.24676 13.536935,-12.71722 l 25.07172,-10.274833 c 1.94239,-0.786053 1.94239,-3.396873 0,-4.182926 L 88.13156,79.008563 C 82.06534,76.53811 77.224317,71.990231 74.594625,66.291346 L 63.657499,42.737824 c -0.83672,-1.824766 -3.615826,-1.824766 -4.452546,0 L 48.267826,66.291346 C 45.638135,71.990231 40.797112,76.53811 34.730891,79.008563 L 9.6292894,89.311474 c -1.9423859,0.786054 -1.9423859,3.3688 0,4.182926 l 25.5498446,10.61172 c 6.036338,2.49852 10.847478,7.07448 13.477169,12.77336 z"
     style="stroke-width:2.8964;fill-opacity:1;stroke-opacity:1" />
  <path
     d="m 111.79878,33.136746 13.56682,5.642739 c 3.19747,1.319446 5.76739,3.761826 7.14201,6.793745 l 5.61797,12.268044 c 0.44825,0.982567 1.91251,0.982567 2.36075,0 l 5.79727,-12.492631 c 1.4045,-3.031919 3.97442,-5.446225 7.20177,-6.765672 l 13.29788,-5.446225 c 1.0459,-0.4211 1.0459,-1.796693 0,-2.217793 l -13.29788,-5.446225 c -3.22735,-1.319447 -5.79727,-3.733753 -7.20177,-6.765672 L 140.48633,6.2144248 c -0.44824,-0.9825664 -1.9125,-0.9825664 -2.36075,0 l -5.79727,12.4926312 c -1.4045,3.031919 -3.97442,5.446225 -7.20177,6.765672 l -13.32776,5.474298 c -1.01601,0.4211 -1.0459,1.796693 0,2.217793 z"
     style="stroke-width:2.8964;fill-opacity:1" />
  <path
     d="m 233.09331,192.98627 -14.13459,-5.7831 c -3.40665,-1.40367 -6.12599,-3.95834 -7.62013,-7.1587 l -6.15587,-13.27868 c -0.47813,-1.03872 -2.03203,-1.03872 -2.51016,0 l -6.15587,13.27868 c -1.49414,3.20036 -4.21348,5.75503 -7.62013,7.1587 l -14.13459,5.81118 c -1.10567,0.44917 -1.10567,1.90898 0,2.35816 l 14.40354,5.97961 c 3.40664,1.40367 6.12598,3.98642 7.59024,7.21485 l 5.97658,13.02602 c 0.47812,1.03872 2.03203,1.03872 2.51016,0 l 6.15586,-13.25061 c 1.49415,-3.20036 4.21349,-5.75503 7.62013,-7.1587 l 14.1346,-5.7831 c 1.10566,-0.44917 1.10566,-1.90899 0,-2.35816 z"
     style="stroke-width:2.8964;stroke-opacity:1" />
</svg>`;
const aiAssistantIcon = `<svg xmlns='http://www.w3.org/2000/svg' viewBox="0 0 24 24">
        <g
            transform="scale(1.2 1.2) translate(-2 -0.5)">
            <path
                d="M 22,12.5 A 1.49995,1.49995 0 0 0 20.50006,11 H 20 V 10 A 3,3 0 0 0 17,7 H 13 V 5.7226 a 2,2 0 1 0 -2,0 V 7 H 7 a 3,3 0 0 0 -3,3 v 1 H 3.49994 a 1.5,1.5 0 0 0 0,3 H 4 v 1 A 3.00128,3.00128 0 0 0 6.20251,17.89282 1.03113,1.03113 0 0 1 7,18.86975 v 0.716 a 0.99928,0.99928 0 0 0 1.00726,1.002 0.9792,0.9792 0 0 0 0.69983,-0.29486 l 2,-2 A 1,1 0 0 1 11.41425,18 H 17 a 3,3 0 0 0 3,-3 v -1 h 0.50006 A 1.49995,1.49995 0 0 0 22,12.5 Z M 19,15 a 2.00226,2.00226 0 0 1 -2,2 H 11.41425 A 1.987,1.987 0 0 0 10,17.58575 l -2,2 v -0.716 A 2.02082,2.02082 0 0 0 6.46771,16.92865 2.00439,2.00439 0 0 1 5,15 V 10 A 2.00226,2.00226 0 0 1 7,8 h 10 a 2.00222,2.00222 0 0 1 2,2 z M 10,12.5 A 1.5,1.5 0 1 1 8.5,11 1.5,1.5 0 0 1 10,12.5 Z m 7,0 A 1.5,1.5 0 1 1 15.5,11 1.5,1.5 0 0 1 17,12.5 Z" />
        </g>
</svg >`;
const saveIcon = `<svg xmlns='http://www.w3.org/2000/svg' viewBox="0 0 1792 1792">
	<path
		d="M512 1536h768v-384h-768v384zm896 0h128v-896q0-14-10-38.5t-20-34.5l-281-281q-10-10-34-20t-39-10v416q0 40-28 68t-68 28h-576q-40 0-68-28t-28-68v-416h-128v1280h128v-416q0-40 28-68t68-28h832q40 0 68 28t28 68v416zm-384-928v-320q0-13-9.5-22.5t-22.5-9.5h-192q-13 0-22.5 9.5t-9.5 22.5v320q0 13 9.5 22.5t22.5 9.5h192q13 0 22.5-9.5t9.5-22.5zm640 32v928q0 40-28 68t-68 28h-1344q-40 0-68-28t-28-68v-1344q0-40 28-68t68-28h928q40 0 88 20t76 48l280 280q28 28 48 76t20 88z"/>
</svg>`;
const uploadIcon = `<svg xmlns='http://www.w3.org/2000/svg' viewBox="0 0 1792 1792">
	<path
		d="M1344 1472q0-26-19-45t-45-19-45 19-19 45 19 45 45 19 45-19 19-45zm256 0q0-26-19-45t-45-19-45 19-19 45 19 45 45 19 45-19 19-45zm128-224v320q0 40-28 68t-68 28h-1472q-40 0-68-28t-28-68v-320q0-40 28-68t68-28h427q21 56 70.5 92t110.5 36h256q61 0 110.5-36t70.5-92h427q40 0 68 28t28 68zm-325-648q-17 40-59 40h-256v448q0 26-19 45t-45 19h-256q-26 0-45-19t-19-45v-448h-256q-42 0-59-40-17-39 14-69l448-448q18-19 45-19t45 19l448 448q31 30 14 69z"/>
</svg>`;
const editorInstances = new WeakMap();

export function initializeJoditEditor(element, initialContent, dotNetHelper) {
    // Ensure Jodit script is loaded
    if (typeof Jodit === 'undefined') {
        console.error('Jodit library not loaded');
        return;
    }
    Jodit.modules.Icon.set('ai-assistant', aiAssistantIcon);
    Jodit.modules.Icon.set('ai-commands', aiCommandsIcon);
    console.log(Jodit.modules.Icon.get('ai-assistant'));
    const editor = Jodit.make(element, {
        autofocus: true,
        spellcheck: true,
        toolbarButtonSize: "xsmall",
        defaultMode: "1",
        //inline: true,
        toolbarInlineForSelection: true,
        showPlaceholder: false,
        height: 650,
        toolbarAdaptive: false,
        toolbarStickyOffset: 10,
        buttons: "bold,italic,underline,ul,ol,paragraph,|,undo,redo,|,spellcheck,|,cut,copy,paste,selectall,|,table,symbols,indent,outdent",
        extraButtons: [
            {
                name: 'improve',
                icon: Jodit.modules.Icon.get('ai-assistant'),
                tooltip: 'Ask Expert Prompt Engineer To Improve',
                exec: function (editor) {
                    dotNetHelper.invokeMethodAsync('OnEventInvoked', 'Improve');
                }
            },
            {
                name: 'eval',
                tooltip: 'Evaluate Prompt',
                icon: Jodit.modules.Icon.get('ai-commands'),
                exec: function (editor) {
                    dotNetHelper.invokeMethodAsync('OnEventInvoked', 'Eval');
                }
            },
            {
                name: 'save',
                tooltip: 'Save Prompt',
                icon: Jodit.modules.Icon.get('save'),
                exec: function (editor) {
                    dotNetHelper.invokeMethodAsync('OnEventInvoked', 'Save');
                }
            },
            {
                name: 'upload',
                tooltip: 'Load Saved Prompt',
                icon: Jodit.modules.Icon.get('upload'),
                exec: function (editor) {
                    dotNetHelper.invokeMethodAsync('OnEventInvoked', 'Load');
                }
            }
        ]
    });
   
    // Set initial content
    if (initialContent) {
        editor.value = initialContent;
    }

    // Store the editor instance
    editorInstances.set(element, editor);

    // Add event listener for content changes
    editor.events.on('change', (newContent) => {
        // Invoke C# method when content changes
        dotNetHelper.invokeMethodAsync('OnContentChanged', newContent);
    });
}

export function setJoditContent(element, content) {
    const editor = editorInstances.get(element);
    if (editor) {
        editor.value = content;
    }
}

export function getJoditContent(element) {
    const editor = editorInstances.get(element);
    return editor ? editor.value : '';
}

export function destroyJoditEditor(element) {
    const editor = editorInstances.get(element);
    if (editor) {
        editor.destruct();
        editorInstances.delete(element);
    }
}
