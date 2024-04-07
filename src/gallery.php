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

        $upload = DotNetRegistry::Read("UPLOAD_DATA");
        $upload = $upload . "/raw";
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
        $upload = DotNetRegistry::Read("UPLOAD_DATA");
        $upload = $upload . "/raw";
        $raw = $upload . "/" . $year . "/" . $file;

        $err = image::process_upload($raw, "/" . $year . "/" . $file, $name, $size, $type, $album_id);

        if (Utils::isDbNull($err)) {
            controller::success(1);
        } else {
            controller::error(1);
        }
    }

    /**
     * @uses api
     * @method GET
     * @access *
    */
    public function get_images($album_id) {
        $album = new Table("photo_groups");
        $images = $album
            ->left_join("photo")
            ->on(["photo" => "id", "photo_groups" => "photo_id"])
            ->where(["album_id" => $album_id])
            ->select([
                "`photo_id` as `id`","`description` as `desc`","`name` as `alt`"
            ]);
        
        controller::success($images);
    }

    /**
     * get image file by id
     * 
     * @uses file
     * @method GET
     * @access *
    */
    public function image($id, $q = "large") {
        # thumbnail
        $photo = new Table("photo");
        $img = $photo->where(["id" => $id])->find();
        $filename = $img[$q];
        $filepath = DotNetRegistry::Read("UPLOAD_DATA") . "/" . $filename;
        
        Utils::PushDownload($filepath);
    }
}