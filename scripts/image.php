<?php

include_once APP_PATH . "/framework/Safe/lib/DateTime.php";

include_once APP_PATH . "/framework/Safe/generated/fileinfo.php";
include_once APP_PATH . "/framework/Safe/generated/filesystem.php";
include_once APP_PATH . "/framework/Safe/generated/image.php";
include_once APP_PATH . "/framework/Safe/generated/pcre.php";
include_once APP_PATH . "/framework/Safe/generated/datetime.php";

include_once APP_PATH . "/framework/Utf8/ForceUTF8/Encoding.php";

include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Contracts/AdapterInterface.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Contracts/HydratorInterface.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Contracts/MapperInterface.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Contracts/ReaderInterface.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Enum/ReaderType.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Exif.php";

include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Adapter/AbstractAdapter.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Adapter/Exiftool.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Adapter/FFprobe.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Adapter/ImageMagick.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Adapter/Native.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Hydrator/Mutator.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Mapper/AbstractMapper.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Mapper/Exiftool.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Mapper/FFprobe.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Mapper/ImageMagick.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Mapper/Native.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Reader/PhpExifReaderException.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Reader/Reader.php";

include_once APP_PATH . "/framework/SimpleImage.php";

class image {

    public static function process_upload($raw, $upload_raw, $name, $size, $type, $album_id) {
        include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Reader/Reader.php";

        // reader with Native adapter
        $reader = \PHPExif\Reader\Reader::factory(\PHPExif\Enum\ReaderType::NATIVE);
        $exif = $reader->read($raw);
        $exif_tags = [
            "Aperture",
            "Author",
            "Headline",
            "Credit",
            "Source",
            "Jobtitle",
            "Iso",
            "Exposure",
            "ExposureMilliseconds",
            "FocusDistance",
            "Width",
            "Height",
            "Title",
            "Caption",
            "Copyright",
            "Keywords",
            "Camera",
            "HorizontalResolution",
            "VerticalResolution",
            "Software",
            "FocalLength",
            "CreationDate",
            "ColorSpace",
            "MimeType",
            "FileSize",
            "FileName",
            "Orientation",
            "GPS",
            "Description",
            "Make",
            "Altitude",
            "Longitude",
            "Latitude",
            "ImgDirection",
            "Lens",
            "ContentIdentifier",
            "Framerate",
            "Duration",
            "MicroVideoOffset",
            "Sublocation",
            "City",
            "State",
            "Country"
        ];
        $exif_values = [];

        foreach($exif_tags as $attr) {
            $exif_values[strtolower($attr)] = $exif->{"get$attr"}();
        }

        $filename = $exif_values["filename"];
        $filename = "/" . date("Y") . "/" . substr($filename, 3, 3) . "/" . $filename;
        $upload = DotNetRegistry::Read("UPLOAD_DATA");
        $large = $upload . "/large" . $filename;
        $small = $upload . "/thumbs" . $filename;

        mkdir(dirname($large), 0777, true);
        mkdir(dirname($small), 0777, true);

        self::rescale_image($raw, $large, 1600);
        self::rescale_image($raw, $small, 500);

        $photo = new Table("photo");
        $group = new Table("photo_groups");
        $album = new Table("album");

        $img_id = $photo->add([
            "name" => $name,
            "add_time" => Utils::Now(),
            "config_id" => 0,
            "raw" => $upload_raw,
            "large" => "/large" . $filename,
            "thumbnail" => "/thumbs" . $filename,
            "description" => "",
            "exif" => json_encode($exif_values)
        ]);
        $group->add([
            "photo_id" => $img_id,
            "album_id" => $album_id,
            "add_time" => Utils::Now()
        ]);
        $album->where([
            "id" => $album_id
        ])->limit(1)
          ->save([
            "photos" => "~photos+1"
          ])
          ;

        return null;
    }

    private static function rescale_image($raw, $scaled_file, $max_pixels) {
        $img = new SimpleImage();
        $img->load($raw);
        $w = $img->getWidth();
        $h = $img->getHeight();
        $r = $w / $h;

        if ($r >= 1) {
            $w = $max_pixels;
            $h = $max_pixels / $r;
        } else {
            $w = $max_pixels / $r;
            $h = $max_pixels;
        }

        $img->resize($w, $h);
        $img->save($scaled_file);
    }
}