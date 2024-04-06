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
        };
        album.prototype.create_onclick = function () {
            modals.create_album();
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
        };
        gallery.prototype.create_onclick = function () {
            modals.create_album();
        };
        return gallery;
    }(Bootstrap));
    pages.gallery = gallery;
})(pages || (pages = {}));
var pages;
(function (pages) {
    var login = /** @class */ (function (_super) {
        __extends(login, _super);
        function login() {
            return _super !== null && _super.apply(this, arguments) || this;
        }
        Object.defineProperty(login.prototype, "appName", {
            get: function () {
                return "login";
            },
            enumerable: false,
            configurable: true
        });
        login.prototype.init = function () {
        };
        login.prototype.login_onclick = function () {
            var account_data = {
                email: $ts.value("#email"),
                passwd: $ts.value("#password")
            };
            if (Strings.Empty(account_data.email)) {
                return;
            }
            else if (Strings.Empty(account_data.passwd)) {
                return;
            }
            else {
                account_data.passwd = md5(account_data.passwd);
            }
            $ts.post("/access/login", account_data, function (result) {
                if (result.code == 0) {
                    $goto("/gallery");
                }
                else {
                }
            });
        };
        return login;
    }(Bootstrap));
    pages.login = login;
})(pages || (pages = {}));
///<reference path="./pages/album.ts" />
///<reference path="./pages/gallery.ts" />
///<reference path="./pages/login.ts" />
var app;
(function (app) {
    function run() {
        Router.AddAppHandler(new pages.album());
        Router.AddAppHandler(new pages.gallery());
        Router.AddAppHandler(new pages.login());
        Router.RunApp();
    }
    app.run = run;
})(app || (app = {}));
$ts(app.run);
var modals;
(function (modals) {
    function album_parent() {
        var url = $ts.location.url;
        console.log("try to get current album reference from url:");
        console.log(url);
        if (url.path == "/gallery" || url.path == "/gallery/") {
            return "0";
        }
        else {
            return url.getArgument("id");
        }
    }
    function create_album() {
        var album_name = $ts.value("#album-name");
        var desc = $ts.value("#desc-text");
        var parent_id = album_parent();
        var new_album = {
            name: album_name,
            description: desc,
            parent_id: parent_id
        };
        console.log("view of the arguments for new album:");
        console.log(new_album);
        if (Strings.Empty(album_name)) {
        }
        $ts.post("/album/new", new_album, function (result) {
            if (result.code == 0) {
                $goto("/album?id=".concat(result.info));
            }
            else {
            }
        });
    }
    modals.create_album = create_album;
})(modals || (modals = {}));
var modals;
(function (modals) {
    function upload_images() {
    }
    modals.upload_images = upload_images;
})(modals || (modals = {}));
//# sourceMappingURL=lychee.js.map