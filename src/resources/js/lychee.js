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
var modals;
(function (modals) {
    modals.$ = window.$;
    function CreateWebUploaderUi() {
        return window.WebUploader.create({
            // 选完文件后，是否自动上传。
            auto: false,
            // swf文件路径
            swf: "/resources/vendor/webuploader/Uploader.swf",
            // 文件接收服务端。
            server: "/gallery/upload",
            accept: {
                title: "Upload Image Files",
                // extensions: "dat,asf,rm,ram,3gp,mov,m4v,dvix,dv,qt,divx,cpk,fli,flc,mod,mp4,wmv,flv,avi,mkv,vob,mpg,rmvb,mpeg,mov,mts",
            },
            // 选择文件的按钮。可选。
            // 内部根据当前运行是创建，可能是input元素，也可能是flash.
            pick: '#picker',
            // mulitple:true,//选择多个
            chunked: true,
            chunkSize: 1 * 1024 * 1024,
            threads: 4,
            method: 'POST'
        });
    }
    modals.CreateWebUploaderUi = CreateWebUploaderUi;
    function showFileInfo(file) {
        var $list = $ts("#thelist");
        var info_str = "\n            <div id=\"".concat(file.id, "\" class=\"item\">\n                <h4 class=\"info\">").concat(file.name, "</h4>\n                <p class=\"info\">FileSize: ").concat(Strings.Lanudry(file.size), "</p>\n                <p class=\"state\">Pending Upload ...</p>\n            </div>");
        console.log(file);
        // webuploader事件.当选择文件后，文件被加载到文件队列中，触发该事件。
        // 等效于 uploader.onFileueued = function(file){...} ，类似js的事件定义。
        $list.appendElement($ts("<div>").display(info_str));
    }
    modals.showFileInfo = showFileInfo;
    function on_progress(file, percentage) {
        var $li = modals.$('#' + file.id);
        var $percent = $li.find('.progress .progress-bar');
        // 避免重复创建
        if (!$percent.length) {
            $percent = modals.$('<div class="progress progress-striped active">' +
                '<div class="progress-bar" role="progressbar" style="width: 0%">' +
                '</div>' +
                '</div>').appendTo($li).find('.progress-bar');
        }
        $li.find('p.state').text("Upload ... ".concat(Strings.round(percentage * 100, 2), "%"));
        $percent.css('width', percentage * 100 + '%');
    }
    modals.on_progress = on_progress;
    function on_success(file, response) {
        var urls = response.data;
        var info = {
            file: "".concat(urls.dir, "/").concat(urls.name),
            name: $ts.baseName(file.name),
            size: file.size,
            type: file.type,
            album_id: modals.album_parent()
        };
        modals.$('#' + file.id).addClass('upload-state-done');
        console.log("video file upload success:");
        console.log(urls);
        // write database
        $ts.post("/gallery/save_image", info, function (result) {
        });
    }
    modals.on_success = on_success;
    function on_complete(file) {
        // alert(file.id)
        // alert(file);
        modals.$('#' + file.id).find('.progress').remove();
        modals.$('#' + file.id).find('p.state').text('已上传');
        // // $('.layui-video-box').html(Help.videoHtml(url, key));
        // Help.video_read();
        // location.href="http://www.xiaosan.com/tp5/public/index.php/index/backstage/vioshow";
    }
    modals.on_complete = on_complete;
    function on_error(file) {
        modals.$('#' + file.id).find('p.state').text('上传出错');
    }
    modals.on_error = on_error;
})(modals || (modals = {}));
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
    modals.album_parent = album_parent;
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
        $ts.post("/gallery/new_album", new_album, function (result) {
            if (result.code == 0) {
                $goto("/album?id=".concat(result.info));
            }
            else {
            }
        });
    }
    modals.create_album = create_album;
})(modals || (modals = {}));
///<reference path="../modals/upload_images.ts"/>
///<reference path="../modals/create_album.ts"/>
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
            var vm = this;
            this.uploader = modals.CreateWebUploaderUi();
            // 当有文件添加进来的时候
            this.uploader.on('fileQueued', function (file) { return modals.showFileInfo(file); });
            // 文件上传过程中创建进度条实时显示。
            this.uploader.on('uploadProgress', function (file, percentage) { return modals.on_progress(file, percentage); });
            // 文件上传成功，给item添加成功class, 用样式标记上传成功。
            this.uploader.on('uploadSuccess', function (file, response) { return modals.on_success(file, response); });
            // 文件上传失败，显示上传出错。
            this.uploader.on('uploadError', function (file) { return modals.on_error(file); });
            // 完成上传完了，成功或者失败，先删除进度条。
            this.uploader.on('uploadComplete', function (file) { return modals.on_complete(file); });
            for (var _i = 0, _a = utils.getObsoletes(); _i < _a.length; _i++) {
                var menu = _a[_i];
                utils.removeElement(menu);
            }
            $ts.get("/gallery/get_images?album_id=".concat(modals.album_parent()), function (result) {
                if (result.code == 0) {
                    vm.show_album(result.info);
                }
            });
        };
        album.prototype.show_album = function (list) {
            var div = $ts("#animated-thumbnails-gallery").clear();
            for (var _i = 0, list_1 = list; _i < list_1.length; _i++) {
                var img = list_1[_i];
                var lbox = $ts("<img>", {
                    class: "img-responsive",
                    alt: img.alt,
                    src: "/gallery/image?id=".concat(img.id, "&q=thumbnail")
                });
                var link = $ts("<a>", {
                    class: "gallery-item",
                    "data-src": "/gallery/image?id=".concat(img.id, "&q=large"),
                    "data-sub-html": img.desc
                }).display(lbox);
                div.appendElement(link);
            }
        };
        album.prototype.file_picker_onclick = function () {
            var inputs = $ts("$file");
            var file = inputs.First;
            console.log(file);
            file.click();
        };
        album.prototype.upload_onclick = function () {
            this.uploader.upload();
        };
        album.prototype.create_onclick = function () {
            modals.create_album();
        };
        return album;
    }(Bootstrap));
    pages.album = album;
})(pages || (pages = {}));
var utils;
(function (utils) {
    function removeElement(node) {
        node.parentNode.removeChild(node);
    }
    utils.removeElement = removeElement;
    function getObsoletes() {
        return $ts.select(".delete").ToArray();
    }
    utils.getObsoletes = getObsoletes;
})(utils || (utils = {}));
/// <reference path="../utils.ts" />
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
            utils.removeElement($ts("#menu_upload"));
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
//# sourceMappingURL=lychee.js.map