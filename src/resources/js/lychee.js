var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (Object.prototype.hasOwnProperty.call(b, p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        if (typeof b !== "function" && b !== null)
            throw new TypeError("Class extends value " + String(b) + " is not a constructor or null");
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var pages;
(function (pages) {
    var album = /** @class */ (function (_super) {
        __extends(album, _super);
        function album() {
            return _super !== null && _super.apply(this, arguments) || this;
        }
        Object.defineProperty(album.prototype, "appName", {
            get: function () {
                return "album";
            },
            enumerable: false,
            configurable: true
        });
        album.prototype.init = function () {
            throw new Error("Method not implemented.");
        };
        return album;
    }(Bootstrap));
    pages.album = album;
})(pages || (pages = {}));
var pages;
(function (pages) {
    var gallery = /** @class */ (function (_super) {
        __extends(gallery, _super);
        function gallery() {
            return _super !== null && _super.apply(this, arguments) || this;
        }
        Object.defineProperty(gallery.prototype, "appName", {
            get: function () {
                return "gallery";
            },
            enumerable: false,
            configurable: true
        });
        gallery.prototype.init = function () {
            throw new Error("Method not implemented.");
        };
        return gallery;
    }(Bootstrap));
    pages.gallery = gallery;
})(pages || (pages = {}));
///<reference path="./pages/album.ts" />
///<reference path="./pages/gallery.ts" />
var app;
(function (app) {
    function run() {
        Router.AddAppHandler(new pages.album());
        Router.AddAppHandler(new pages.gallery());
        Router.RunApp();
    }
    app.run = run;
})(app || (app = {}));
$ts(app.run);
var modals;
(function (modals) {
    function create_album() {
    }
    modals.create_album = create_album;
})(modals || (modals = {}));
//# sourceMappingURL=lychee.js.map