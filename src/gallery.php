<?php

include __DIR__ . "/../etc/bootstrap.php";

class App {

    /**
     * create new album
     * 
     * @uses api
     * @method POST
    */
    public function new_album($name, $parent_id = 0, $description = "") {
        $album = new Table("album");
        $album->add([
            "name" => $name,
            "photos" => 0,
            "parent_id" => $parent_id,
            "note" => $description,
            "config_id" => 0
        ]);
        $check = ["name" => $name, "parent_id" => $parent_id];
        $check = $album->where($check)->find();

        if (Utils::isDbNull($check)) {
            controller::error($album->getLastMySqlError());
        } else {
            controller::success($check["id"]);
        }
    }

    /**
     * upload image
     * 
     * @uses api
     * @method POST
    */
    public function upload() {
        include APP_PATH . "/framework/php-webuploader/src/Upload.php";

        $upload = APP_PATH . "/data/raw/";
        $upload_temp = dotnet::getMyTempDirectory() . "/upload_temp/";

        //调用
        $demo = new Upload();
        $demo->uploadVideo($upload, $upload_temp);
    }

    /**
     * @uses api
     * @method POST
    */
    public function save_image($file, $name, $size, $type, $album_id) {
        include APP_PATH . "/scripts/image.php";

        $year = substr($file, 0, 4);
        $upload = APP_PATH . "/data/raw/";
        $raw = $upload . "/" . $year . "/" . $file;

        image::process_upload($raw);
    }
}