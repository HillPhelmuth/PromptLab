/*!
 * jodit - Jodit is an awesome and useful wysiwyg editor with filebrowser
 * Author: Chupurnov <chupurnov@gmail.com> (https://xdsoft.net/jodit/)
 * Version: v4.2.24
 * Url: https://xdsoft.net/jodit/
 * License(s): MIT
 */
	
(function webpackUniversalModuleDefinition(root, factory) {
	if(typeof exports === 'object' && typeof module === 'object')
		module.exports = factory();
	else if(typeof define === 'function' && define.amd)
		define([], factory);
	else {
		var a = factory();
		for(var i in a) (typeof exports === 'object' ? exports : root)[i] = a[i];
	}
})(self, function() {
return (self["webpackChunkjodit"] = self["webpackChunkjodit"] || []).push([[882],{

/***/ 71096:
/***/ (function(__unused_webpack_module, __unused_webpack___webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var jodit_core_helpers_checker_is_boolean__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(9810);
/* harmony import */ var jodit_core_helpers_checker_is_string__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(98253);
/* harmony import */ var jodit_core_helpers_utils_attr__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(26150);
/* harmony import */ var jodit_core_helpers_utils_data_bind__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(72197);
/* harmony import */ var jodit_core_ui_icon__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(931);
/* harmony import */ var jodit_config__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(36115);
/* harmony import */ var _helpers_api__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(71722);
/* harmony import */ var _helpers_recognize_manager__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(93676);
/* harmony import */ var _speech_recognize_svg__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(22227);
/* harmony import */ var _speech_recognize_svg__WEBPACK_IMPORTED_MODULE_6___default = /*#__PURE__*/__webpack_require__.n(_speech_recognize_svg__WEBPACK_IMPORTED_MODULE_6__);
/*!
 * Jodit Editor (https://xdsoft.net/jodit/)
 * Released under MIT see LICENSE.txt in the project root for license information.
 * Copyright (c) 2013-2024 Valeriy Chupurnov. All rights reserved. https://xdsoft.net
 */









jodit_config__WEBPACK_IMPORTED_MODULE_3__/* .Config */ .T.prototype.speechRecognize = {
    api: typeof _helpers_api__WEBPACK_IMPORTED_MODULE_4__/* .SpeechRecognition */ .o !== 'undefined' ? _helpers_api__WEBPACK_IMPORTED_MODULE_4__/* .SpeechRecognition */ .o : null,
    sound: true,
    continuous: false,
    interimResults: true,
    commands: {
        'newline|enter': 'enter',
        'delete|remove word|delete word': 'backspaceWordButton',
        comma: 'inserthtml::,',
        underline: 'inserthtml::_',
        hyphen: 'inserthtml::-',
        space: 'inserthtml:: ',
        question: 'inserthtml::?',
        dot: 'inserthtml::.',
        'quote|quotes|open quote': "inserthtml::'",
        'header|header h1': 'formatblock::h1',
        'select all': 'selectall'
    }
};
jodit_core_ui_icon__WEBPACK_IMPORTED_MODULE_2__/* .Icon */ .I.set('speech-recognize', (_speech_recognize_svg__WEBPACK_IMPORTED_MODULE_6___default()));
jodit_config__WEBPACK_IMPORTED_MODULE_3__/* .Config */ .T.prototype.controls.speechRecognize = {
    isVisible(j) {
        return Boolean(j.o.speechRecognize.api);
    },
    isActive(jodit, _) {
        const api = (0,jodit_core_helpers_utils_data_bind__WEBPACK_IMPORTED_MODULE_1__/* .dataBind */ .m)(jodit, 'speech');
        return Boolean(api?.isEnabled);
    },
    isDisabled(jodit) {
        return !jodit.o.speechRecognize.api;
    },
    exec(jodit, current, { button, control }) {
        const { api: ApiConstructor, lang, continuous, interimResults, sound } = jodit.o.speechRecognize;
        if (!ApiConstructor) {
            jodit.alert('Speech recognize API unsupported in your browser');
            return;
        }
        let api = (0,jodit_core_helpers_utils_data_bind__WEBPACK_IMPORTED_MODULE_1__/* .dataBind */ .m)(jodit, 'speech');
        if (!api) {
            const nativeApi = new ApiConstructor();
            api = new _helpers_recognize_manager__WEBPACK_IMPORTED_MODULE_5__/* .RecognizeManager */ .g(jodit.async, nativeApi);
            api.lang = (0,jodit_core_helpers_checker_is_string__WEBPACK_IMPORTED_MODULE_7__/* .isString */ .K)(lang)
                ? lang
                : (0,jodit_core_helpers_utils_attr__WEBPACK_IMPORTED_MODULE_0__/* .attr */ .C)(jodit.od.documentElement, 'lang') ?? undefined;
            api.continuous = continuous;
            api.interimResults = interimResults;
            api.sound = sound;
            (0,jodit_core_helpers_utils_data_bind__WEBPACK_IMPORTED_MODULE_1__/* .dataBind */ .m)(jodit, 'speech', api);
            api.on('pulse', (enable) => {
                button.setMod('pulse', enable);
            });
            api.on('result', (text) => jodit.e.fire('speechRecognizeResult', text));
            api.on('progress', (text) => jodit.e.fire('speechRecognizeProgressResult', text));
            api.on('error', (text) => jodit.message.error(text));
            button.hookStatus('beforeDestruct', () => {
                (0,jodit_core_helpers_utils_data_bind__WEBPACK_IMPORTED_MODULE_1__/* .dataBind */ .m)(jodit, 'speech', null);
                api.destruct();
            });
        }
        if (control.args) {
            const key = control.args[0];
            if ((0,jodit_core_helpers_checker_is_boolean__WEBPACK_IMPORTED_MODULE_8__/* .isBoolean */ .L)(api[key])) {
                api[key] = !api[key];
                if (api.isEnabled) {
                    api.restart();
                }
                return;
            }
        }
        api.toggle();
        if (api.isEnabled) {
            button.setMod('pulse', true);
        }
        button.state.activated = api.isEnabled;
    },
    name: 'speechRecognize',
    command: 'toggleSpeechRecognize',
    tooltip: 'Speech Recognize',
    list: {
        sound: 'Sound',
        interimResults: 'Interim Results'
    },
    childTemplate(jodit, key, value) {
        const api = (0,jodit_core_helpers_utils_data_bind__WEBPACK_IMPORTED_MODULE_1__/* .dataBind */ .m)(jodit, 'speech'), checked = api?.[key] ?? jodit.o.speechRecognize[key];
        return `<span class='jodit-speech-recognize__list-item'><input ${checked ? 'checked' : ''} class='jodit-checkbox' type='checkbox'>&nbsp;${value}</span>`;
    },
    mods: {
        stroke: false
    }
};


/***/ }),

/***/ 81629:
/***/ (function(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   N: function() { return /* binding */ PII; },
/* harmony export */   Z: function() { return /* binding */ WARN; }
/* harmony export */ });
/*!
 * Jodit Editor (https://xdsoft.net/jodit/)
 * Released under MIT see LICENSE.txt in the project root for license information.
 * Copyright (c) 2013-2024 Valeriy Chupurnov. All rights reserved. https://xdsoft.net
 */
/**
 * @module plugins/speech-recognize
 * @internal
 */
const PII = 440;
const WARN = 940;


/***/ }),

/***/ 71722:
/***/ (function(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   o: function() { return /* binding */ SpeechRecognition; }
/* harmony export */ });
/*!
 * Jodit Editor (https://xdsoft.net/jodit/)
 * Released under MIT see LICENSE.txt in the project root for license information.
 * Copyright (c) 2013-2024 Valeriy Chupurnov. All rights reserved. https://xdsoft.net
 */
const SpeechRecognition = window.SpeechRecognition ||
    window.webkitSpeechRecognition;


/***/ }),

/***/ 50834:
/***/ (function(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   F: function() { return /* binding */ execSpellCommand; }
/* harmony export */ });
/*!
 * Jodit Editor (https://xdsoft.net/jodit/)
 * Released under MIT see LICENSE.txt in the project root for license information.
 * Copyright (c) 2013-2024 Valeriy Chupurnov. All rights reserved. https://xdsoft.net
 */
function execSpellCommand(jodit, commandSentence) {
    const [command, value] = commandSentence.split('::');
    jodit.execCommand(command, null, value);
}


/***/ }),

/***/ 93676:
/***/ (function(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   g: function() { return /* binding */ RecognizeManager; }
/* harmony export */ });
/* harmony import */ var tslib__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(31635);
/* harmony import */ var jodit_core_decorators__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(22664);
/* harmony import */ var jodit_core_event_emitter_eventify__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(43431);
/* harmony import */ var _sound__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(95473);
/* harmony import */ var jodit_plugins_speech_recognize_constants__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(81629);
/*!
 * Jodit Editor (https://xdsoft.net/jodit/)
 * Released under MIT see LICENSE.txt in the project root for license information.
 * Copyright (c) 2013-2024 Valeriy Chupurnov. All rights reserved. https://xdsoft.net
 */
var RecognizeManager_1;





let RecognizeManager = RecognizeManager_1 = class RecognizeManager extends jodit_core_event_emitter_eventify__WEBPACK_IMPORTED_MODULE_1__/* .Eventify */ .h {
    set lang(v) {
        this._lang = v;
        this._api.lang = v;
    }
    get lang() {
        return this._lang;
    }
    set continuous(v) {
        this._continuous = v;
        this._api.continuous = v;
    }
    get continuous() {
        return this._continuous;
    }
    set interimResults(v) {
        this._interimResults = v;
        this._api.interimResults = v;
    }
    get interimResults() {
        return this._interimResults;
    }
    constructor(async, api) {
        super();
        this.async = async;
        this._continuous = false;
        this._interimResults = false;
        this.sound = true;
        this._isEnabled = false;
        this._restartTimeout = 0;
        this._onSpeechStart = (e) => {
            if (!this._isEnabled) {
                return;
            }
            this.async.clearTimeout(this._restartTimeout);
            this._restartTimeout = this.async.setTimeout(() => {
                this.restart();
                this.emit('pulse', false);
                this._makeSound(jodit_plugins_speech_recognize_constants__WEBPACK_IMPORTED_MODULE_2__/* .WARN */ .Z);
            }, 5000);
            this.emit('pulse', true);
        };
        this.__interimResults = '';
        this._api = api;
        RecognizeManager_1._instances.add(this);
    }
    destruct() {
        this.stop();
        RecognizeManager_1._instances.delete(this);
        super.destruct();
    }
    get isEnabled() {
        return this._isEnabled;
    }
    start() {
        if (this._isEnabled) {
            return;
        }
        this._isEnabled = true;
        RecognizeManager_1._instances.forEach(instance => {
            if (instance !== this) {
                instance.stop();
            }
        });
        try {
            this._api.start();
        }
        catch (e) {
            this._onError(e);
            this.stop();
            return;
        }
        this.__on('speechstart', this._onSpeechStart)
            .__on('error', this._onError)
            .__on('result', this._onProgress)
            .__on('end', this._onResults);
    }
    stop() {
        if (!this._isEnabled) {
            return;
        }
        this._api.abort();
        this._api.stop();
        this.__off('speechstart', this._onSpeechStart)
            .__off('error', this._onError)
            .__off('result', this._onProgress)
            .__off('end', this._onResults);
        this.async.clearTimeout(this._restartTimeout);
        this._isEnabled = false;
        this.emit('pulse', false);
    }
    toggle() {
        if (!this._isEnabled) {
            this.start();
        }
        else {
            this.stop();
        }
    }
    restart() {
        this.stop();
        this.start();
    }
    __on(event, callback) {
        this._api.addEventListener(event, callback);
        return this;
    }
    __off(event, callback) {
        this._api.removeEventListener(event, callback);
        return this;
    }
    _onResults(e) {
        this.emit('pulse', false);
        this.emit('result', this.__interimResults);
        this.__interimResults = '';
        this._makeSound(jodit_plugins_speech_recognize_constants__WEBPACK_IMPORTED_MODULE_2__/* .PII */ .N);
        this.restart();
    }
    _onProgress(e) {
        if (!this._isEnabled) {
            return;
        }
        this.__interimResults = '';
        if (!e.results) {
            return;
        }
        for (let i = 0; i < e.results.length; i++) {
            const resultItem = e.results.item(i);
            if (resultItem.length) {
                const { transcript } = resultItem.item(0);
                this.__interimResults += transcript;
            }
        }
        if (this.__interimResults) {
            this.emit('progress', this.__interimResults);
        }
    }
    _onError(e) {
        if (e.error === 'voice-unavailable') {
            this.emit('error', 'Voice unavailable');
        }
        if (e.error === 'not-allowed') {
            this.emit('error', 'Not allowed');
        }
        if (e.error === 'language-unavailable' ||
            // @ts-ignore
            e.error === 'language-not-supported') {
            this.emit('error', 'Language unavailable');
        }
        this._makeSound(jodit_plugins_speech_recognize_constants__WEBPACK_IMPORTED_MODULE_2__/* .WARN */ .Z);
        this.emit('pulse', false);
        this.stop();
    }
    _makeSound(frequency) {
        if (this.sound) {
            (0,_sound__WEBPACK_IMPORTED_MODULE_3__/* .sound */ .s)({ frequency });
        }
    }
};
RecognizeManager._instances = new Set();
RecognizeManager = RecognizeManager_1 = (0,tslib__WEBPACK_IMPORTED_MODULE_4__/* .__decorate */ .Cg)([
    jodit_core_decorators__WEBPACK_IMPORTED_MODULE_0__.autobind
], RecognizeManager);



/***/ }),

/***/ 95473:
/***/ (function(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   s: function() { return /* binding */ sound; }
/* harmony export */ });
/* harmony import */ var jodit_plugins_speech_recognize_constants__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(81629);
/*!
 * Jodit Editor (https://xdsoft.net/jodit/)
 * Released under MIT see LICENSE.txt in the project root for license information.
 * Copyright (c) 2013-2024 Valeriy Chupurnov. All rights reserved. https://xdsoft.net
 */
/**
 * @module plugins/speech-recognize
 */

/**
 * @internal
 */
function sound({ sec = 0.1, frequency = jodit_plugins_speech_recognize_constants__WEBPACK_IMPORTED_MODULE_0__/* .PII */ .N, gain = 0.1, type = 'sine' } = {}) {
    if (typeof window.AudioContext === 'undefined' &&
        typeof window.webkitAudioContext === 'undefined') {
        return;
    }
    // one context per document
    const context = new (window.AudioContext ||
        window.webkitAudioContext)();
    const vol = context.createGain();
    const osc = context.createOscillator();
    osc.type = type;
    osc.frequency.value = frequency; // Hz
    osc.connect(vol);
    vol.connect(context.destination);
    osc.start(); // start the oscillator
    osc.stop(context.currentTime + sec);
    vol.gain.value = gain;
}


/***/ }),

/***/ 25202:
/***/ (function(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   ar: function() { return /* reexport module object */ _ar_js__WEBPACK_IMPORTED_MODULE_0__; },
/* harmony export */   cs_cz: function() { return /* reexport module object */ _ar_js__WEBPACK_IMPORTED_MODULE_0__; },
/* harmony export */   de: function() { return /* reexport module object */ _ar_js__WEBPACK_IMPORTED_MODULE_0__; },
/* harmony export */   es: function() { return /* reexport module object */ _ar_js__WEBPACK_IMPORTED_MODULE_0__; },
/* harmony export */   fa: function() { return /* reexport module object */ _ar_js__WEBPACK_IMPORTED_MODULE_0__; },
/* harmony export */   fi: function() { return /* reexport module object */ _ar_js__WEBPACK_IMPORTED_MODULE_0__; },
/* harmony export */   fr: function() { return /* reexport module object */ _ar_js__WEBPACK_IMPORTED_MODULE_0__; },
/* harmony export */   he: function() { return /* reexport module object */ _ar_js__WEBPACK_IMPORTED_MODULE_0__; },
/* harmony export */   hu: function() { return /* reexport module object */ _ar_js__WEBPACK_IMPORTED_MODULE_0__; },
/* harmony export */   id: function() { return /* reexport module object */ _ar_js__WEBPACK_IMPORTED_MODULE_0__; },
/* harmony export */   it: function() { return /* reexport module object */ _ar_js__WEBPACK_IMPORTED_MODULE_0__; },
/* harmony export */   ja: function() { return /* reexport module object */ _ar_js__WEBPACK_IMPORTED_MODULE_0__; },
/* harmony export */   ko: function() { return /* reexport module object */ _ar_js__WEBPACK_IMPORTED_MODULE_0__; },
/* harmony export */   mn: function() { return /* reexport module object */ _ar_js__WEBPACK_IMPORTED_MODULE_0__; },
/* harmony export */   nl: function() { return /* reexport module object */ _ar_js__WEBPACK_IMPORTED_MODULE_0__; },
/* harmony export */   pl: function() { return /* reexport module object */ _ar_js__WEBPACK_IMPORTED_MODULE_0__; },
/* harmony export */   pt_br: function() { return /* reexport module object */ _ar_js__WEBPACK_IMPORTED_MODULE_0__; },
/* harmony export */   ru: function() { return /* reexport module object */ _ar_js__WEBPACK_IMPORTED_MODULE_0__; },
/* harmony export */   tr: function() { return /* reexport module object */ _ar_js__WEBPACK_IMPORTED_MODULE_0__; },
/* harmony export */   zh_cn: function() { return /* reexport module object */ _ar_js__WEBPACK_IMPORTED_MODULE_0__; },
/* harmony export */   zh_tw: function() { return /* reexport module object */ _ar_js__WEBPACK_IMPORTED_MODULE_0__; }
/* harmony export */ });
/* harmony import */ var _ar_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(41324);
/* harmony import */ var _ar_js__WEBPACK_IMPORTED_MODULE_0___default = /*#__PURE__*/__webpack_require__.n(_ar_js__WEBPACK_IMPORTED_MODULE_0__);
/*!
 * Jodit Editor (https://xdsoft.net/jodit/)
 * Released under MIT see LICENSE.txt in the project root for license information.
 * Copyright (c) 2013-2024 Valeriy Chupurnov. All rights reserved. https://xdsoft.net
 */
// @ts-nocheck
























/***/ }),

/***/ 37873:
/***/ (function(__unused_webpack_module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   SpeechRecognizeNative: function() { return /* binding */ SpeechRecognizeNative; }
/* harmony export */ });
/* harmony import */ var tslib__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(31635);
/* harmony import */ var jodit_core_decorators_watch_watch__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(66927);
/* harmony import */ var jodit_core_dom_dom__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(55186);
/* harmony import */ var jodit_core_global__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(56298);
/* harmony import */ var jodit_core_helpers_utils_utils__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(84976);
/* harmony import */ var jodit_core_plugin__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(71005);
/* harmony import */ var _config__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(71096);
/* harmony import */ var _jodit__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(46173);
/* harmony import */ var _helpers_exec_spell_command__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(50834);
/* harmony import */ var _langs__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(25202);
/*!
 * Jodit Editor (https://xdsoft.net/jodit/)
 * Released under MIT see LICENSE.txt in the project root for license information.
 * Copyright (c) 2013-2024 Valeriy Chupurnov. All rights reserved. https://xdsoft.net
 */











class SpeechRecognizeNative extends jodit_core_plugin__WEBPACK_IMPORTED_MODULE_4__/* .Plugin */ .k {
    constructor() {
        super(...arguments);
        this.buttons = [
            {
                group: 'state',
                name: 'speechRecognize'
            }
        ];
        this._commandToWord = {};
    }
    afterInit(jodit) {
        const { commands } = jodit.o.speechRecognize;
        if (commands) {
            (0,jodit_core_global__WEBPACK_IMPORTED_MODULE_2__/* .extendLang */ .JW)(_langs__WEBPACK_IMPORTED_MODULE_7__);
            (0,jodit_core_helpers_utils_utils__WEBPACK_IMPORTED_MODULE_3__/* .keys */ .HP)(commands, false).forEach(words => {
                const keys = words.split('|');
                keys.forEach(key => {
                    key = key.trim().toLowerCase();
                    this._commandToWord[key] = commands[words];
                    const translatedKeys = jodit.i18n(key);
                    if (translatedKeys !== key) {
                        translatedKeys.split('|').forEach(translatedKey => {
                            this._commandToWord[translatedKey.trim().toLowerCase()] = commands[words].trim();
                        });
                    }
                });
            });
        }
    }
    beforeDestruct(jodit) {
        jodit_core_dom_dom__WEBPACK_IMPORTED_MODULE_1__/* .Dom */ .J.safeRemove(this.messagePopup);
    }
    onSpeechRecognizeProgressResult(text) {
        if (!this.messagePopup) {
            this.messagePopup = this.j.create.div('jodit-speech-recognize__popup');
        }
        this.j.workplace.appendChild(this.messagePopup);
        this.j.async.clearTimeout(this.__hidePopupTimeout);
        this.__hidePopupTimeout = this.j.async.setTimeout(() => {
            jodit_core_dom_dom__WEBPACK_IMPORTED_MODULE_1__/* .Dom */ .J.safeRemove(this.messagePopup);
        }, 1000);
        this.messagePopup.innerText = text + '|';
    }
    onSpeechRecognizeResult(text) {
        this.j.async.clearTimeout(this.__hidePopupTimeout);
        jodit_core_dom_dom__WEBPACK_IMPORTED_MODULE_1__/* .Dom */ .J.safeRemove(this.messagePopup);
        const { j } = this;
        const { s } = j;
        if (!this._checkCommand(text)) {
            const { range } = s, node = s.current();
            if (s.isCollapsed() &&
                jodit_core_dom_dom__WEBPACK_IMPORTED_MODULE_1__/* .Dom */ .J.isText(node) &&
                jodit_core_dom_dom__WEBPACK_IMPORTED_MODULE_1__/* .Dom */ .J.isOrContains(j.editor, node) &&
                node.nodeValue) {
                const sentence = node.nodeValue;
                node.nodeValue =
                    sentence +
                        (/[\u00A0 ]\uFEFF*$/.test(sentence) ? '' : ' ') +
                        text;
                range.setStartAfter(node);
                s.selectRange(range);
                j.synchronizeValues();
            }
            else {
                s.insertHTML(text);
            }
        }
    }
    _checkCommand(command) {
        command = command.toLowerCase().replace(/\./g, '');
        if (this._commandToWord[command]) {
            (0,_helpers_exec_spell_command__WEBPACK_IMPORTED_MODULE_8__/* .execSpellCommand */ .F)(this.j, this._commandToWord[command]);
            return true;
        }
        return false;
    }
}
(0,tslib__WEBPACK_IMPORTED_MODULE_9__/* .__decorate */ .Cg)([
    (0,jodit_core_decorators_watch_watch__WEBPACK_IMPORTED_MODULE_0__/* .watch */ .w)(':speechRecognizeProgressResult')
], SpeechRecognizeNative.prototype, "onSpeechRecognizeProgressResult", null);
(0,tslib__WEBPACK_IMPORTED_MODULE_9__/* .__decorate */ .Cg)([
    (0,jodit_core_decorators_watch_watch__WEBPACK_IMPORTED_MODULE_0__/* .watch */ .w)(':speechRecognizeResult')
], SpeechRecognizeNative.prototype, "onSpeechRecognizeResult", null);
_jodit__WEBPACK_IMPORTED_MODULE_6__/* .Jodit */ .x.plugins.add('speech-recognize', SpeechRecognizeNative);


/***/ }),

/***/ 22227:
/***/ (function(module) {

module.exports = "<svg viewBox=\"0 0 16 16\" xml:space=\"preserve\" xmlns=\"http://www.w3.org/2000/svg\"> <path d=\"M8,11c1.657,0,3-1.343,3-3V3c0-1.657-1.343-3-3-3S5,1.343,5,3v5C5,9.657,6.343,11,8,11z\"/> <path d=\"M13,8V6h-1l0,1.844c0,1.92-1.282,3.688-3.164,4.071C6.266,12.438,4,10.479,4,8V6H3v2c0,2.414,1.721,4.434,4,4.899V15H5v1h6 v-1H9v-2.101C11.279,12.434,13,10.414,13,8z\"/> </svg>"

/***/ })

},
/******/ function(__webpack_require__) { // webpackRuntimeModules
/******/ var __webpack_exec__ = function(moduleId) { return __webpack_require__(__webpack_require__.s = moduleId); }
/******/ var __webpack_exports__ = (__webpack_exec__(37873));
/******/ return __webpack_exports__;
/******/ }
]);
});